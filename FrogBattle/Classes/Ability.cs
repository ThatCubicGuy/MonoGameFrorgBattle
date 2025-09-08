using Microsoft.Xna.Framework.Graphics;
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
                        StatThresholdCondition st => "stats." + st.Stat.ToString().FirstLower(),
                        PoolAmountCondition pl => "pools." + pl.Pool.ToString().FirstLower(),
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
            return Enum.GetValues(typeof(TextTypes)).Cast<TextTypes>().Select(x => new KeyValuePair<TextTypes, string>(x, string.Join('.', Parent._internalName, typeof(Ability).Name.FirstLower(), GetType().Name.FirstLower(), "text", x.ToString().FirstLower()))).Where(x => Localization.strings.ContainsKey(x.Value)).ToDictionary();
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
                // Ability buff/debuff application text
                if (FlavourText().TryGetValue(TextTypes.ApplyEffect, out var effectKey)) AddText(effectKey, Parent, target, effect.AppliedEffect);
                else AddText(Generic.ApplyEffect, Parent, target, effect.AppliedEffect);
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
        /// The amount of energy restored has a flat 10 added to it.
        /// </summary>
        /// <param name="amount">Base amount of Mana that this ability requires.</param>
        /// <param name="energyGenPercent">The percentage of base mana to gain in energy.</param>
        /// <returns>This.</returns>
        protected Ability WithGenericManaCost(double amount, double energyGenPercent = 0.4)
        {
            var cost = new PoolAmountCondition(this, amount, Pools.Mana, Operators.Additive);
            return WithGenericCost(cost).WithReward(new Reward(Parent, Parent, amount * energyGenPercent + 10, Pools.Energy, Operators.Additive));
        }
        /// <summary>
        /// Attaches an <see cref="Pools.Energy"/> amount condition and cost corresponding to MaxEnergy.
        /// </summary>
        /// <returns>This.</returns>
        protected Ability WithBurstCost()
        {
            var cost = new PoolAmountCondition(this, Parent.GetStatVersus(Stats.MaxEnergy, Target), Pools.Energy, Operators.Additive);
            return WithGenericCost(cost);
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
    internal record class AttackHelper(Character Parent, Character Target, AttackInfo AttackInfo, EffectInfo[] EffectInfos) : IAttack, IAppliesEffect
    {
        public AttackHelper(IAttack src, double falloff = 0) : this(src.Parent, src.Target, src.AttackInfo with { Ratio = Math.Max(0, src.AttackInfo.Ratio - falloff)}, src is IAppliesEffect ef ? ef.EffectInfos : null) { }
        private bool IsHit()
        {
            return AttackInfo.HitRate == null || BattleManager.RNG < AttackInfo.HitRate + Parent.GetStatVersus(Stats.HitRateBonus, Target) - Target.GetStatVersus(Stats.Dex, Parent) / 100;
        }
        protected void AddText(string key, params object[] args)
        {
            Parent.ParentBattle.BattleText.AppendLine(Localization.Translate(key, args));
        }
        private IEnumerator<StatusEffect> ApplyEffects()
        {
            if (Target == null) yield break;
            if (EffectInfos == null) yield break;
            foreach (var effect in EffectInfos)
            {
                if (BattleManager.RNG < (effect.ChanceType switch
                {
                    ChanceTypes.Fixed => effect.Chance,
                    // Base chance takes into account your EHR and the enemy's EffectRES
                    ChanceTypes.Base => effect.Chance + Parent.GetStatVersus(Stats.EffectHitRate, Target) - Target.GetStatVersus(Stats.EffectRES, Parent),
                    _ => throw new InvalidDataException($"Unknown chance type: {effect.ChanceType}")
                })) yield return effect.AppliedEffect;
            }
            yield break;
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
        /// <summary>
        /// Uses private Init() to create and then apply a series of damages for the given target,
        /// as well as add corresponding battle text and apply effects.
        /// </summary>
        /// <param name="text">Flavour text dictionary.</param>
        /// <returns>Each instance of damage dealt.</returns>
        public IEnumerator<Damage> FancyInit(Dictionary<TextTypes, string> text)
        {
            // Make sure there's anyone to attack
            if (Target == null) yield break;
            // Create attack helper instance
            var attack = Init();
            // If there is no first element to the collection, means we missed without IHR, so it's over
            if (!attack.MoveNext())
            {
                // Ability miss text
                if (text.TryGetValue(TextTypes.Miss, out var missKey)) AddText(missKey, Parent, Target);
                else AddText(Generic.Miss, Parent, Target);
                yield break;
            }
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
                    // Pause only when we deal damage
                    yield return attack.Current;
                    attack.Current.Take();
                }
                else
                {
                    // Ability miss text...?
                    if (text.TryGetValue(TextTypes.Miss, out var missKey)) AddText(missKey, Parent, Target);
                    else AddText(Generic.Miss, Parent, Target);
                }
            } while (attack.MoveNext());
            // If we missed everything, tough luck.
            if (!hit) yield break;
            // Try to apply the effects to the target after dealing the damage, knowing we hit at least once
            var effects = ApplyEffects();
            while (effects.MoveNext())
            {
                Target.AddEffect(effects.Current);
                // Ability buff/debuff application text
                if (text.TryGetValue(TextTypes.ApplyEffect, out var effectKey)) AddText(effectKey, Parent, Target, effects.Current);
                else AddText(Generic.ApplyEffect, Parent, Target, effects.Current);
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
            // Get the list for flavour text
            var text = FlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, Parent, Target);
            // Create attack helper instance
            var helper = new AttackHelper(this);
            var attack = helper.FancyInit(text);
            // Abort if we miss the target
            if (!attack.MoveNext()) return false;
            // Calculate damage
            double finalDamage = 0;
            do { finalDamage += attack.Current.Amount; } while (attack.MoveNext());
            // Ability end text
            if (text.TryGetValue(TextTypes.End, out var endKey)) AddText(endKey, Parent, Target, finalDamage);
            return true;
        }
    }

    internal abstract class BlastAttack(Character source, Character mainTarget, AbilityInfo properties, AttackInfo attackInfo, EffectInfo[] effectInfos, double falloff) : Ability(source, mainTarget, properties), IAttack, IAppliesEffect
    {
        public double Falloff { get; init; } = falloff;
        public AttackInfo AttackInfo { get; } = attackInfo;
        public EffectInfo[] EffectInfos { get; } = effectInfos ?? [];
        private protected override bool Use()
        {
            // Get the list for flavour text
            var text = FlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, Parent, Target);
            double finalDamage = 0;
            // Create new attacks for the side targets with falloff
            var helperM = new AttackHelper(this);
            var helperR = new AttackHelper(this, Falloff) { Target = Target.RightTeammate };
            var helperL = new AttackHelper(this, Falloff) { Target = Target.LeftTeammate };
            var middleAttack = helperM.FancyInit(text);
            var rightAttack = helperR.FancyInit(text);
            var leftAttack = helperL.FancyInit(text);
            // We care mainly about the middle target - miss that and we're cooked. Otherwise continue.
            if (!middleAttack.MoveNext()) return false;
            // Manually move the other two
            rightAttack.MoveNext();
            leftAttack.MoveNext();
            // Use | instead of || to avoid shortcircuiting and exhaust each branch at the same time while adding up their damages
            do { finalDamage += (middleAttack.Current?.Amount ?? 0) + (rightAttack.Current?.Amount ?? 0) + (leftAttack.Current?.Amount ?? 0); } while (middleAttack.MoveNext() | leftAttack.MoveNext() | rightAttack.MoveNext());
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
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, Parent, Target, Count);
            // Create helper instance
            var helper = new AttackHelper(this);
            // Initialise damage for testing whether we hit anything
            double finalDamage = 0;
            for (int i = 0; i < Count; i++)
            {
                var attack = helper.FancyInit(text);
                // Skip if we miss (use break; to end bounces on miss)
                if (!attack.MoveNext()) continue;
                // Add up damage
                do { finalDamage += attack.Current.Amount; } while (attack.MoveNext());
                // Move to random target
                helper = helper with { Target = Targets.OrderBy(x => BattleManager.RNG).First() };
            }
            if (finalDamage == 0) return false;
            // Ability end text
            if (text.TryGetValue(TextTypes.End, out var endKey)) AddText(endKey, Parent, Target);
            return true;
        }
    }

    internal abstract class AoEAttack(Character source, Character mainTarget, AbilityInfo properties, AttackInfo attackInfo, EffectInfo[] effectInfos) : Ability(source, mainTarget, properties), IAttack, IAppliesEffect
    {
        public AttackInfo AttackInfo { get; } = attackInfo;
        public EffectInfo[] EffectInfos { get; } = effectInfos ?? [];
        public List<Character> Targets => Target.Team;

        private protected override bool Use()
        {
            // Get the lists for flavour text
            var text = FlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, Parent, Target);
            // Create attack helper instances for every target
            var attacks = new List<IEnumerator<Damage>>();
            var helpers = new List<AttackHelper>();
            foreach (var item in Targets)
            {
                helpers.Add(new(this) { Target = item });
                attacks.Add(helpers.Last().FancyInit(text));
            }
            double finalDamage = 0;
            do { finalDamage += attacks.Where(x => x.Current != null).Sum(x => x.Current.Amount); } while (attacks.Select(x => x.MoveNext()).Any(x => x));
            if (finalDamage == 0) return false;
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
