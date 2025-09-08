using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace FrogBattle.Classes
{
    // rewrite #7 gazillion lmfao
    internal abstract class Ability : IHasTarget
    {
        public Ability(Character parent, Character target, AbilityInfo props)
        {
            Parent = parent;
            Target = target;
            Properties = props;
        }
        public Character Parent { get; }
        public Character Target { get; }
        public AbilityInfo Properties { get; init; }
        public Dictionary<Pools, Cost> Costs { get; } = [];
        public Dictionary<Pools, Reward> Rewards { get; } = [];
        public Dictionary<object, Ability.Condition> Conditions { get; } = [];
        /// <summary>
        /// Tries using the ability. If conditions are not met returns false.
        /// Whether the ability was used successfully or missed does not influence the return value.
        /// </summary>
        /// <returns>True if the turn can continue, false otherwise.</returns>
        public bool TryUse()
        {
            foreach (var item in Conditions)
            {
                if (!item.Value.Check())
                {
                    AddText("conditions.missing.generic", Localization.Translate(item.Value switch
                    {
                        StatThresholdCondition st => "stats." + st.Stat.ToString().camelCase(),
                        PoolAmountCondition pl => "pools." + pl.Pool.ToString().camelCase(),
                        Condition cn => "generic"
                    }));
                    return false;
                }
            }
            foreach (var item in Costs)
            {
                Parent.ApplyChange(item.Value);
            }
            if (!Use()) return true;
            foreach (var item in Rewards)
            {
                Parent.ApplyChange(item.Value);
            }
            return true;
        }
        /// <summary>
        /// Use the ability.
        /// </summary>
        /// <returns>True if used successfully, false if missed.</returns>
        private protected abstract bool Use();
        /// <summary>
        /// Display the function. Idfk how you animate shit. But you do or something. So have fun.
        /// </summary>
        /// <param name="args"></param>
        public virtual void Display(object[] args)
        {

        }
        /// <summary>
        /// Creates untranslated flavour text keys.
        /// </summary>
        /// <returns>A dictionary with every available line for this ability.</returns>
        public Dictionary<TextTypes, string> FlavourText()
        {
            /*
            // i'm sorry but honest to god replacing four lines of code with the most hideous monstrosity of a single line return known to man was way too funny to pass up
            var result = new Dictionary<TextTypes, string>();
            string baseName = string.Join('.', Parent._internalName, typeof(Ability).Name.camelCase(), GetType().Name.camelCase(), "text");
            foreach (var item in Enum.GetValues(typeof(TextTypes)))
                if (Localization.strings.ContainsKey(string.Join('.', baseName, item.ToString().camelCase())))
                    result.Add((TextTypes)item, string.Join('.', baseName, item.ToString().camelCase()));
            return result;
            */
            return Enum.GetValues(typeof(TextTypes)).Cast<TextTypes>().Select(x => new KeyValuePair<TextTypes, string>(x, string.Join('.', Parent._internalName, typeof(Ability).Name.camelCase(), GetType().Name.camelCase(), "text", x.ToString().camelCase()))).Where(x => Localization.strings.ContainsKey(x.Value)).ToDictionary();
            // Goodbye horrid monstrosity... you were good, son, real good... maybe even the best.
            //return Enum.GetValues(typeof(TextTypes)).Cast<object>().ToList().FindAll(x => Localization.strings.ContainsKey(string.Join('.', string.Join('.', Parent._internalName, typeof(Ability).Name.camelCase(), GetType().Name.camelCase(), "text"), x.ToString().camelCase()))).Select<object, KeyValuePair<TextTypes, string>>(x => new((TextTypes)x, string.Join('.', string.Join('.', Parent._internalName, typeof(Ability).Name.camelCase(), GetType().Name.camelCase(), "text"), x.ToString().camelCase()))).ToDictionary();
        }
        /// <summary>
        /// Shorthand AddText method so i don't write five vigintillion characters each time.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        protected void AddText(string key, params object[] args)
        {
            Parent.ParentBattle.BattleText.AppendLine(Localization.Translate(key, args));
        }

        public bool ApplyEffect(Character target, EffectInfo effect)
        {
            if (this is not IAppliesEffect ab) throw new InvalidOperationException("Cannot call ApplyEffect from an ability that does not implement IAppliesEffect.");
            if (effect == null) return false;
            if (target == null) return false;
            if (BattleManager.RNG < (effect.ChanceType switch
            {
                ChanceTypes.Fixed => effect.Chance,
                // Base chance takes into account your EHR and the enemy's EffectRES
                ChanceTypes.Base => effect.Chance + ab.Parent.GetStatVersus(Stats.EffectHitRate, target) - target.GetStatVersus(Stats.EffectRES, ab.Parent),
                _ => throw new InvalidDataException($"Unknown chance type: {effect.ChanceType}")
            }))
            {
                target.AddEffect(effect.AppliedEffect);
                return true;
            }
            return false;
        }
        public Ability WithCost(Cost cost)
        {
            Costs[cost.Pool] = cost;
            return this;
        }
        public Ability WithReward(Reward reward)
        {
            Rewards[reward.Pool] = reward;
            return this;
        }
        public Ability WithCondition(Ability.Condition condition)
        {
            Conditions[condition.GetKey()] = condition;
            return this;
        }
        /// <summary>
        /// Attaches a PoolAmount condition to the ability and its corresponding cost.
        /// </summary>
        /// <returns>This ability.</returns>
        protected Ability WithGenericCost(PoolAmountCondition cost)
        {
            return WithCondition(cost).WithCost(cost.GetCost());
        }
        /// <summary>
        /// Attaches a <see cref="Pools.Mana"/> amount condition, its corresponding cost, and energy generation. Percentage configurable.
        /// </summary>
        /// <param name="cost"></param>
        /// <returns></returns>
        protected Ability WithGenericManaCost(double amount, double energyGenPercent = 0.2)
        {
            var cost = new PoolAmountCondition(this, amount, Pools.Mana, Operators.Additive);
            return WithCondition(cost).WithCost(cost.GetCost()).WithReward(new Reward(Parent, Parent, amount * energyGenPercent, Pools.Energy, Operators.Additive));
        }

        internal abstract class Condition(Ability parent)
        {
            public Ability Parent { get; } = parent;
            public Character ParentFighter => Parent.Parent;
            public abstract bool Check();
            public abstract object GetKey();
        }
        /// <summary>
        /// Check whether a stat is within an inclusive interval [Min, Max].
        /// </summary>
        internal class StatThresholdCondition : Condition
        {
            private readonly double? _min;
            private readonly double? _max;
            public StatThresholdCondition(Ability parent, double? minAmount, double? maxAmount, Stats stat, Operators op) : base(parent)
            {
                if (minAmount == null && maxAmount == null) throw new ArgumentNullException(string.Join(", ", [nameof(minAmount), nameof(maxAmount)]), "Condition requires at least one bound.");
                _min = minAmount;
                _max = maxAmount;
                Stat = stat;
                Op = op;
            }
            public double Min => Op.Apply(_min, ParentFighter.Base[Stat]) ?? double.NegativeInfinity;
            public double Max => Op.Apply(_max, ParentFighter.Base[Stat]) ?? double.PositiveInfinity;
            public Stats Stat { get; }
            private Operators Op { get; }
            public override bool Check()
            {
                return ParentFighter.GetStat(Stat).IsWithinBounds(Min, Max);
            }
            public override object GetKey() => Stat;
        }
        /// <summary>
        /// Checks whether a pool's value is above or equal to Amount.
        /// </summary>
        internal class PoolAmountCondition : Condition
        {
            private readonly double _amount;
            public PoolAmountCondition(Ability parent, double amount, Pools pool, Operators op) : base(parent)
            {
                if (op == Operators.Multiplicative && pool.Max() == Stats.None) throw new ArgumentException("Cannot have percentage conditions for pools that do not have a max value.", nameof(op));
                _amount = amount;
                Pool = pool;
                Op = op;
            }
            /// <summary>
            /// Automatically applies the operator based on the fighter it is attached to.
            /// </summary>
            public double Amount
            {
                get => Op.Apply(_amount, Pool.Max() != Stats.None ? ParentFighter.GetStat(Pool.Max()) : _amount);
            }
            public Pools Pool { get; }
            private Operators Op { get; }
            /// <summary>
            /// Constructs a pool change corresponding to the amount and stat required by this condition.
            /// </summary>
            /// <returns>A <see cref="Reward"/> with the stat requirements of this condition.</returns>
            public Cost GetCost()
            {
                return new(ParentFighter, ParentFighter, _amount, Pool, Op);
            }
            public override bool Check()
            {
                return ParentFighter.Resolve(Pool) >= Amount;
            }
            public override object GetKey() => Pool;
        }
    }
    // oh god
    internal record class AttackHelper : IAttack, IAppliesEffect
    {
        public AttackHelper(IAttack src, double falloff = 0) : this(src.Parent, src.Target, src.AttackInfo with { Ratio = src.AttackInfo.Ratio - falloff}, src is IAppliesEffect ef ? ef.EffectInfos : null) { }
        public AttackHelper(Character parent, Character target, AttackInfo attackInfo, EffectInfo[] effectInfo)
        {
            Parent = parent;
            Target = target;
            AttackInfo = attackInfo;
            EffectInfos = effectInfo;
        }
        public Character Parent { get; init; }
        public Character Target { get; init; }
        public AttackInfo AttackInfo { get; init; }
        public EffectInfo[] EffectInfos { get; init; }
        private bool IsHit()
        {
            return AttackInfo.HitRate == null || BattleManager.RNG < AttackInfo.HitRate + Parent.GetStatVersus(Stats.HitRateBonus, Target) - Target.GetStatVersus(Stats.Dex, Parent) / 100;
        }

        /// <summary>
        /// Creates a series of damages for the given target.
        /// </summary>
        /// <returns>An enumerator. Advance </returns>
        public IEnumerator<Damage> Init()
        {
            if (Target == null) yield break;
            if (!AttackInfo.IndependentHitRate && !IsHit()) yield break;
            if (AttackInfo.Split == null || AttackInfo.Split.Length == 0)
            {
                yield return (!AttackInfo.IndependentHitRate || IsHit()) ? new Damage(Parent, Target, AttackInfo.Ratio * Parent.GetStatVersus(AttackInfo.Scalar, Target), AttackInfo.DamageInfo) : null;
                yield break;
            }
            long sum = AttackInfo.Split.Sum(x => x);
            foreach (var i in AttackInfo.Split)
            {
                yield return (!AttackInfo.IndependentHitRate || IsHit()) ? new Damage(Parent, Target, AttackInfo.Ratio * Parent.GetStatVersus(AttackInfo.Scalar, Target) * i / sum, AttackInfo.DamageInfo) : null;
            }
            yield break;
        }
    }

    internal abstract class SingleTargetAttack(Character source, Character target, AbilityInfo properties, AttackInfo attackInfo, EffectInfo[] effectInfos) : Ability(source, target, properties), IAttack, IAppliesEffect
    {
        public AttackInfo AttackInfo { get; } = attackInfo;
        public EffectInfo[] EffectInfos { get; } = effectInfos ?? [];

        private protected override bool Use()
        {
            // Get the lists for flavour text
            var text = FlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, Parent, Target);
            // Create attack helper instance
            var helper = new AttackHelper(this);
            var attack = helper.Init();
            // If there is no first element to the collection, means we missed without IHR, so it's over
            if (!attack.MoveNext())
            {
                // Ability miss text
                if (text.TryGetValue(TextTypes.Miss, out var missKey)) AddText(missKey, Parent, Target);
                else AddText(Generic.Miss, Parent, Target);
                return false;
            }
            // Initialize final damage
            double finalDamage = 0;
            // Index for displaying damage text
            int index = 0;
            // Check to make sure we hit anything
            var hit = false;
            // Use do-while so we can freely check the first element for a miss. Perfect!
            do
            {
                if (attack.Current != null)
                {
                    hit = true;
                    // Ability damage text
                    if (text.TryGetValue((TextTypes)(++index), out var damageKey)) AddText(damageKey, Parent, Target, attack.Current);
                    else AddText(Generic.Damage, Parent, Target, attack.Current);
                    finalDamage += attack.Current.Take();
                }
                else
                {
                    // Ability miss text
                    if (text.TryGetValue(TextTypes.Miss, out var missKey)) AddText(missKey, Parent, Target);
                    else AddText(Generic.Miss, Parent, Target);
                }
            } while (attack.MoveNext());
            // If we missed everything, tough luck.
            if (!hit) return false;
            // Try to apply the effects to the target after dealing the damage, knowing we hit at least once
            foreach (var item in EffectInfos)
            {
                if (ApplyEffect(Target, item))
                {
                    // Ability buff/debuff application text
                    if (text.TryGetValue(TextTypes.ApplyEffect, out var effectKey)) AddText(effectKey, Parent, Target, item.AppliedEffect);
                    else AddText(Generic.ApplyEffect, Parent, Target, item.AppliedEffect);
                }
            }
            // Ability end text
            if (text.TryGetValue(TextTypes.End, out var endKey)) AddText(endKey, Parent, Target, finalDamage);
            return true;
            // dammit i need to solve text cause its too varied
        }
    }

    internal abstract class BlastAttack(Character source, Character mainTarget, AbilityInfo properties, AttackInfo attackInfo, EffectInfo[] effectInfos, double falloff) : Ability(source, mainTarget, properties), IAttack, IAppliesEffect
    {
        public double Falloff { get; init; } = falloff;
        public AttackInfo AttackInfo { get; } = attackInfo;
        public EffectInfo[] EffectInfos { get; } = effectInfos ?? [];
        private protected override bool Use()
        {
            // Get the lists for flavour text
            var text = FlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, Parent, Target);

            double finalDamage = 0;
            // Create new attacks for the side targets with falloff
            var helperM = new AttackHelper(this);
            var helperR = new AttackHelper(this, Falloff) { Target = Target.RightTeammate };
            var helperL = new AttackHelper(this, Falloff) { Target = Target.LeftTeammate };
            var middleAttack = helperM.Init();
            var rightAttack = helperR.Init();
            var leftAttack = helperL.Init();
            // If there is no first element to the collection, means we missed without IHR, so it's over
            // I made Blast attacks care about the middle target first and foremost, so that's the one whose miss is calculated.
            if (!middleAttack.MoveNext())
            {
                // Ability miss text
                if (text.TryGetValue(TextTypes.Miss, out var missKey)) AddText(missKey, Parent, Target);
                else AddText(Generic.Miss, Parent, Target);
                return false;
            }
            // Manually move the other two because yknow. Lmao
            rightAttack.MoveNext();
            leftAttack.MoveNext();
            // Index and checks
            int index = 0;
            var hitMid = false;
            var hitLeft = false;
            var hitRight = false;
            do
            {
                // Practically start indexing from 1, for damage text
                ++index;
                if (middleAttack.Current != null)
                {
                    hitMid = true;
                    // Ability damage text
                    if (text.TryGetValue((TextTypes)index, out var damageKey)) AddText(damageKey, Parent, middleAttack.Current.Target, middleAttack.Current);
                    else AddText(Generic.Damage, Parent, middleAttack.Current.Target, middleAttack.Current);
                    finalDamage += middleAttack.Current.Take();
                }
                if (Target.RightTeammate != null && rightAttack.Current != null)
                {
                    hitRight = true;
                    // Ability damage text
                    if (text.TryGetValue((TextTypes)index, out var damageKey)) AddText(damageKey, Parent, rightAttack.Current.Target, rightAttack.Current);
                    else AddText(Generic.Damage, Parent, rightAttack.Current.Target, rightAttack.Current);
                    finalDamage += rightAttack.Current.Take();
                }
                if (Target.LeftTeammate != null && leftAttack.Current != null)
                {
                    hitLeft = true;
                    // Ability damage text
                    if (text.TryGetValue((TextTypes)index, out var damageKey)) AddText(damageKey, Parent, leftAttack.Current.Target, leftAttack.Current);
                    else AddText(Generic.Damage, Parent, leftAttack.Current.Target, leftAttack.Current);
                    finalDamage += leftAttack.Current.Take();
                }
                // Move through all lists of damage instances for targets at the same time (using | instead of || here to avoid shortcircuiting)
            } while (middleAttack.MoveNext() | rightAttack.MoveNext() | leftAttack.MoveNext());
            if (hitMid)
            {
                // Try to apply the effects to the target after dealing the damage, knowing we hit at least once
                foreach (var item in EffectInfos)
                {
                    if (ApplyEffect(Target, item))
                    {
                        // Ability buff/debuff application text
                        if (text.TryGetValue(TextTypes.ApplyEffect, out var effectKey)) AddText(effectKey, Parent, Target, item.AppliedEffect);
                        else AddText(Generic.ApplyEffect, Parent, Target, item.AppliedEffect);
                    }
                }
            }
            if (hitLeft)
            {
                // Try to apply the effects to the target after dealing the damage, knowing we hit at least once
                foreach (var item in EffectInfos)
                {
                    if (ApplyEffect(Target.LeftTeammate, item))
                    {
                        // Ability buff/debuff application text
                        if (text.TryGetValue(TextTypes.ApplyEffect, out var effectKey)) AddText(effectKey, Parent, Target.LeftTeammate, item.AppliedEffect);
                        else AddText(Generic.ApplyEffect, Parent, Target.LeftTeammate, item.AppliedEffect);
                    }
                }
            }
            if (hitRight)
            {
                // Try to apply the effects to the target after dealing the damage, knowing we hit at least once
                foreach (var item in EffectInfos)
                {
                    if (ApplyEffect(Target.RightTeammate, item))
                    {
                        // Ability buff/debuff application text
                        if (text.TryGetValue(TextTypes.ApplyEffect, out var effectKey)) AddText(effectKey, Parent, Target.RightTeammate, item.AppliedEffect);
                        else AddText(Generic.ApplyEffect, Parent, Target.RightTeammate, item.AppliedEffect);
                    }
                }
            }
            // Ability end text
            if (text.TryGetValue(TextTypes.End, out var endKey)) AddText(endKey, Parent, Target, finalDamage);
            return true;
        }
    }

    internal abstract class BounceAttack(Character source, Character mainTarget, AbilityInfo properties, AttackInfo attackInfo, EffectInfo[] effectInfos, uint count) : Ability(source, mainTarget, properties), IAttack, IAppliesEffect
    {
        public uint Count { get; } = count;
        public AttackInfo AttackInfo { get; } = attackInfo;
        public EffectInfo[] EffectInfos { get; } = effectInfos ?? [];
        public List<Character> Targets => Target.Team;

        private protected override bool Use()
        {
            // Get the lists for flavour text
            var text = FlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, Parent, Target);

            double finalDamage = 0;
            var helper = new AttackHelper(this);
            for (int i = 0; i < Count; ++i)
            {
                var attack = helper.Init();
                // If there is no first element to the collection, means we missed without IHR, so we skip to the next bounce
                if (!attack.MoveNext())
                {
                    // Ability miss text
                    if (text.TryGetValue(TextTypes.Miss, out var missKey)) AddText(missKey, Parent, attack.Current.Target);
                    else AddText(Generic.Miss, Parent, attack.Current.Target);
                    continue; // Changing from continue; to break; here is a simple way to prevent misses from bouncing again.
                }
                // Index for displaying damage text
                int index = 0;
                // Hit check
                var hit = false;
                // Use do-while so we can freely check the first element for a miss. Perfect!
                do
                {
                    if (attack.Current != null)
                    {
                        hit = true;
                        // Ability damage text
                        if (text.TryGetValue((TextTypes)(++index), out var damageKey)) AddText(damageKey, Parent, attack.Current.Target, attack.Current);
                        else AddText(Generic.Damage, Parent, attack.Current.Target, attack.Current);
                        finalDamage += attack.Current.Take();
                    }
                    // No miss text here because it would clog too much. If there were misses when you cast the ability,
                    // on each bounce, AND on each damage instance of each bounce, that would be too many.
                } while (attack.MoveNext());
                // Apply effects
                if (hit)
                {
                    // Try to apply the effects to the target after dealing the damage, knowing we hit at least once
                    foreach (var item in EffectInfos)
                    {
                        if (ApplyEffect(helper.Target, item))
                        {
                            // Ability buff/debuff application text
                            if (text.TryGetValue(TextTypes.ApplyEffect, out var effectKey)) AddText(effectKey, Parent, helper.Target, item.AppliedEffect);
                            else AddText(Generic.ApplyEffect, Parent, helper.Target, item.AppliedEffect);
                        }
                    }
                }
                helper = helper with { Target = Targets.OrderBy(x => BattleManager.RNG).First() };
            }
            // Check whether we hit anybody
            if (finalDamage != 0)
            {
                // Ability end text
                if (text.TryGetValue(TextTypes.End, out var endKey)) AddText(endKey, Parent, Target, finalDamage);
                return true;
            }
            else
            {
                // Ability miss text
                if (text.TryGetValue(TextTypes.Miss, out var missKey)) AddText(missKey, Parent, Target);
                else AddText(Generic.Miss, Parent, Target);
                return false;
            }
        }
    }

    internal abstract class AoEAttack(Character source, Character mainTarget, AbilityInfo properties, AttackInfo attackInfo, EffectInfo[] effectInfos) : Ability(source, mainTarget, properties), IAttack, IAppliesEffect
    {
        public List<Character> Targets => Target.Team;
        public AttackInfo AttackInfo { get; } = attackInfo;
        public EffectInfo[] EffectInfos { get; } = effectInfos ?? [];

        private protected override bool Use()
        {
            // Get the lists for flavour text
            var text = FlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, Parent, Target);
            double finalDamage = 0;
            var attacks = new List<IEnumerator<Damage>>();
            var helpers = new List<AttackHelper>();
            foreach (var character in Targets)
            {
                helpers.Add(new AttackHelper(this) with { Target = character });
                attacks.Add(helpers.Last().Init());
            }
            // If there is no first element to the collection, means we missed without IHR, so it's over for that enemy
            foreach (var attack in attacks)
            {
                if (!attack.MoveNext())
                {
                    // Ability miss text
                    if (text.TryGetValue(TextTypes.Miss, out var missKey)) AddText(missKey, Parent, helpers[attacks.IndexOf(attack)].Target);
                    else AddText(Generic.Miss, Parent, helpers[attacks.IndexOf(attack)].Target);
                    // Could do something here to clear the attacks list tbh. Micro optimisation though. Not worth my motivation.
                }
            }
            // Index for displaying damage text
            int index = 0;
            // Funnier hit check
            var hit = new List<Character>();
            // Use do-while so we can freely check the first elements for a miss. Perfect!
            do
            {
                foreach (var attack in attacks)
                {
                    if (attack.Current != null)
                    {
                        hit.Add(attack.Current.Target);
                        // Ability damage text
                        if (text.TryGetValue((TextTypes)(++index), out var damageKey)) AddText(damageKey, Parent, attack.Current.Target, attack.Current);
                        else AddText(Generic.Damage, Parent, attack.Current.Target, attack.Current);
                        finalDamage += attack.Current.Take();
                    }
                }
                // Iterate while there's still damage instances to be done to at least one enemy.
            } while (attacks.Select(x => x.MoveNext()).Any(x => x));
            // Apply effects in a funny way
            foreach (var target in hit)
            {
                foreach (var item in EffectInfos)
                {
                    if (ApplyEffect(target, item))
                    {
                        // Ability buff/debuff application text
                        if (text.TryGetValue(TextTypes.ApplyEffect, out var effectKey)) AddText(effectKey, Parent, target, item.AppliedEffect);
                        else AddText(Generic.ApplyEffect, Parent, target, item.AppliedEffect);
                    }
                }
            }
            // Ability end text
            if (text.TryGetValue(TextTypes.End, out var endKey)) AddText(endKey, Parent, Target, finalDamage);
            return true;
        }
    }

    internal abstract class Buff(Character source, Character target, AbilityInfo properties, EffectInfo[] effectInfos) : Ability(source, target, properties), IAppliesEffect
    {
        public EffectInfo[] EffectInfos { get; } = effectInfos;
        private protected override bool Use()
        {
            var text = FlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, Parent, Target);
            foreach (var item in EffectInfos)
            {
                if (ApplyEffect(Target, item))
                {
                    // Ability buff application text
                    if (text.TryGetValue(TextTypes.ApplyEffect, out var effectKey)) AddText(effectKey, Parent, Target, item.AppliedEffect);
                    else AddText(Generic.ApplyEffect, Parent, Target, item.AppliedEffect);
                }
            }
            return true;
        }
    }

    internal abstract class BuffTeam(Character source, AbilityInfo properties, EffectInfo[] effectInfos) : Ability(source, source, properties), IAppliesEffect
    {
        public EffectInfo[] EffectInfos { get; } = effectInfos;
        public List<Character> Targets { get => Target.Team; }
        private protected override bool Use()
        {
            var text = FlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, Parent, Target);
            foreach (var target in Target.Team)
            {
                foreach (var item in EffectInfos)
                {
                    if (ApplyEffect(target, item))
                    {
                        // Ability buff application text
                        if (text.TryGetValue(TextTypes.ApplyEffect, out var effectKey)) AddText(effectKey, Parent, target, item.AppliedEffect);
                        else AddText(Generic.ApplyEffect, Parent, target, item.AppliedEffect);
                    }
                }
            }
            // Ability end text
            if (text.TryGetValue(TextTypes.Start, out var endKey)) AddText(endKey, Parent, Target);
            return true;
        }
    }

    internal abstract class Heal(Character source, Character target, AbilityInfo properties, HealingInfo healingInfo, EffectInfo[] effectInfos) : Ability(source, target, properties), IHealing, IAppliesEffect
    {
        public HealingInfo HealingInfo { get; } = healingInfo;
        public EffectInfo[] EffectInfos { get; } = effectInfos ?? [];
        private protected override bool Use()
        {
            var text = FlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, Parent, Target);
            // Ability heal text
            var healing = new Healing(Parent, Target, HealingInfo);
            if (text.TryGetValue(TextTypes.Healing, out var healingKey)) AddText(healingKey, Parent, Target, healing);
            else AddText(Generic.Healing, Parent, Target, healing);
            double finalHealing = healing.Take();
            foreach (var item in EffectInfos)
            {
                if (ApplyEffect(Target, item))
                {
                    // Ability buff application text
                    if (text.TryGetValue(TextTypes.ApplyEffect, out var effectKey)) AddText(effectKey, Parent, Target, item.AppliedEffect);
                    else AddText(Generic.ApplyEffect, Parent, Target, item.AppliedEffect);
                }
            }
            // Ability end text
            if (text.TryGetValue(TextTypes.Start, out var endKey)) AddText(endKey, Parent, Target, finalHealing);
            return true;
        }
    }

    internal abstract class HealTeam(Character source, AbilityInfo properties, HealingInfo healingInfo, EffectInfo[] effectInfos) : Ability(source, source, properties), IHealing, IAppliesEffect
    {
        public HealingInfo HealingInfo { get; } = healingInfo;
        public EffectInfo[] EffectInfos { get; } = effectInfos ?? [];
        public List<Character> Targets { get => Target.Team; }
        private protected override bool Use()
        {
            var text = FlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, Parent, Target);
            double finalHealing = 0;
            foreach (var target in Target.Team)
            {
                var healing = new Healing(Parent, target, HealingInfo);
                if (text.TryGetValue(TextTypes.Healing, out var healingKey)) AddText(healingKey, Parent, Target, healing);
                else AddText(Generic.Healing, Parent, Target, healing);
                finalHealing = healing.Take();
                foreach (var item in EffectInfos)
                {
                    if (ApplyEffect(target, item))
                    {
                        // Ability buff application text
                        if (text.TryGetValue(TextTypes.ApplyEffect, out var effectKey)) AddText(effectKey, Parent, target, item.AppliedEffect);
                        else AddText(Generic.ApplyEffect, Parent, target, item.AppliedEffect);
                    }
                }
            }
            // Ability end text
            if (text.TryGetValue(TextTypes.Start, out var endKey)) AddText(endKey, Parent, Target, finalHealing);
            return true;
        }
    }

    internal sealed class SkipTurn : Ability
    {
        public SkipTurn(Character source) : base(source, source, new())
        {
            WithReward(new(source, source, 5, Pools.Mana, Operators.Additive));
        }
        private protected override bool Use()
        {
            Parent.ParentBattle.BattleText.AppendLine(Localization.Translate("character.generic.skip", Parent, Rewards.Single().Value.Amount));
            return true;
        }
    }
}
