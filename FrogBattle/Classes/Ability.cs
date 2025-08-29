using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static FrogBattle.Classes.StatusEffect;

namespace FrogBattle.Classes
{
    // rewrite #7 gazillion lmfao
    internal abstract class Ability : IHasTarget
    {
        protected Ability(Character parent, Character target, AbilityInfo properties)
        {
            Parent = parent;
            Target = target;
            Props = properties;
        }
        public Character Parent { get; }
        public Character Target { get; protected set; }
        public AbilityInfo Props { get; }
        /// <summary>
        /// Pool changes that execute after the ability is checked, but before it is launched.
        /// </summary>
        public Dictionary<Pools, Cost> Costs { get; } = [];
        /// <summary>
        /// Pool changes only execute if the ability is launched successfully (e.g. not a miss).
        /// </summary>
        public Dictionary<Pools, Reward> Rewards { get; } = [];
        public Dictionary<object, Condition> Conditions { get; } = [];
        public static string GenericDamage { get; } = "character.generic.damage";
        public static string GenericMiss { get; } = "character.generic.miss";
        /// <summary>
        /// Tries using the ability. If conditions are not met, or if the ability should repeat the turn, returns false.
        /// Whether the ability was used successfully or missed does not influence the return value.
        /// </summary>
        /// <returns>True if the turn can continue, false otherwise.</returns>
        public bool TryUse()
        {
            foreach (var item in Conditions)
            {
                if (!item.Value.Check()) return false;
            }
            foreach (var item in Costs)
            {
                Parent.ApplyChange(item.Value);
            }
            if (!Use()) return !Props.RepeatsTurn;
            foreach (var item in Rewards)
            {
                Parent.ApplyChange(item.Value);
            }
            return !Props.RepeatsTurn;
        }
        /// <summary>
        /// Use the ability.
        /// </summary>
        /// <returns>True if used successfully, false if missed.</returns>
        private protected abstract bool Use();
        /// <summary>
        /// Creates untranslated flavour text keys.
        /// </summary>
        /// <returns>An enumerator that iterates through every available key for this ability.</returns>
        public Dictionary<TextTypes, string> FlavourText()
        {
            var result = new Dictionary<TextTypes, string>();
            string baseName = string.Join('.', Parent._internalName, typeof(Ability).Name.camelCase(), GetType().Name.camelCase(), "text");
            foreach (var item in Enum.GetValues(typeof(TextTypes)))
                if (Localization.strings.ContainsKey(string.Join('.', baseName, item.ToString().camelCase())))
                     result.Add((TextTypes)item, string.Join('.', baseName, item.ToString().camelCase()));
            return result;
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

        public bool ApplyEffect(Character target)
        {
            if (this is not IAppliesEffect ab) throw new InvalidOperationException("Cannot call ApplyEffect from an ability that does not implement IAppliesEffect.");
            if (ab.EffectInfo == null) return false;
            if (target == null) return false;
            if (BattleManager.RNG < ab.EffectInfo.ChanceType switch
            {
                ChanceTypes.Fixed => ab.EffectInfo.Chance,
                // Base chance takes into account your EHR and the enemy's EffectRES
                ChanceTypes.Base => ab.EffectInfo.Chance + ab.Parent.GetStat(Stats.EffectHitRate) - target.GetStat(Stats.EffectRES),
                _ => throw new InvalidDataException($"Unknown chance type: {ab.EffectInfo.ChanceType}")
            })
            {
                target.AddEffect(ab.EffectInfo.AppliedEffect);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Automatically deals damage to the targets given and returns
        /// the final damage dealt as a double. Null means everything was a miss.
        /// If the Ability does not implement IAttack, throws an exception.
        /// </summary>
        /// <param name="Targets">The targets to attack.</param>
        /// <returns>Total damage dealt. Null means every instance was missed.</returns>
        /// <exception cref="InvalidOperationException">IAttack is required for this method.</exception>
        protected internal double? SimpleAttack(params Character[] Targets)
        {
            if (this is not IAttack ab) throw new InvalidOperationException("Cannot call SimpleAttack from an ability that does not implement IAttack.");
            // Get the lists for flavour text and initialise damages matrix
            var text = FlavourText();
            var allDamages = new List<List<Damage>>();
            // Populate damage matrix - List by target above damage
            foreach (var item in Targets)
            {
                if (item == null) continue;
                allDamages.Add(ab.AttackDamage(item));
            }
            // Turn from list full of null into null reference if everything was a miss
            if (allDamages.All(x => x == null)) allDamages = null;
            if (allDamages == null)
            {
                return null;
            }
            double finalDamage = 0;
            // Traverse the matrix vertically (Damage 1 for target 1-4, damage 2 for target 1-4, ...)
            for (int index = 0; index < allDamages.Count; ++index)
            {
                // Each item is a list of damages for a target.
                // We use the above index to get which damage we want first.
                foreach (var item in allDamages)
                {
                    if (item[index] == null)
                    {
                        // Ability miss text
                        if (text.TryGetValue(TextTypes.Miss, out var missKey)) AddText(missKey, [Parent.Name, Target.Name]);
                        else AddText(GenericMiss, [Parent.Name, Target.Name]);
                    }
                    else
                    {
                        finalDamage += item[index].Amount;
                        // Index is 0 based while TextType Damage(1 : 16) index is 1 based, so we increment by 1
                        if (text.TryGetValue((TextTypes)(index + 1)/*here*/, out var damageKey)) AddText(damageKey, [Parent.Name, Target.Name, item[index].Amount]);
                        else AddText(GenericDamage, [Parent.Name, Target.Name, item[index].Amount]);
                        // Apply damage and effects to targets
                        Array.ForEach(Targets, x => { x.TakeDamage(item[index]); ApplyEffect(x); });
                    }
                }
            }
            return finalDamage;
        } // ----PENDING REFACTOR LOL----
        protected Ability WithCost(Cost change)
        {
            Costs[change.Pool] = change;
            return this;
        }
        protected Ability WithReward(Reward change)
        {
            Rewards[change.Pool] = change;
            return this;
        }
        protected Ability WithCondition(Condition condition)
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

        internal abstract class Condition
        {
            public Condition(Ability parent)
            {
                Parent = parent;
            }
            public Ability Parent { get; }
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
    // a bit botched to just remove abstract modifier on SingleTargetAttack, but i'll do this for now and refactor later
    internal class SingleTargetAttack(Character source, Character target, AbilityInfo properties, AttackInfo attackInfo, EffectInfo effectInfo = null) : Ability(source, target, properties), IAttack, IAppliesEffect
    {
        public AttackInfo AttackInfo { get; } = attackInfo;
        public EffectInfo EffectInfo { get; } = effectInfo;

        private protected override bool Use()
        {
            // Get the lists for flavour text
            var text = FlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, [Parent.Name, Target.Name]);
            var finalDamage = SimpleAttack(Target);
            if (finalDamage != null)
            {
                // Ability end text
                if (text.TryGetValue(TextTypes.End, out var endKey)) AddText(endKey, [Parent.Name, Target.Name, finalDamage, EffectInfo?.AppliedEffect.CreateArgs()]);
                return true;
            }
            else
            {
                // Ability miss text
                if (text.TryGetValue(TextTypes.Miss, out var missKey)) AddText(missKey, [Parent.Name, Target.Name]);
                else AddText(GenericMiss, [Parent.Name, Target.Name]);
                return false;
            }
        }
    }

    internal abstract class BlastAttack(Character source, Character mainTarget, AbilityInfo properties, double falloff, AttackInfo attackInfo, EffectInfo effectInfo = null) : Ability(source, mainTarget, properties), IAttack, IAppliesEffect
    {
        public double Falloff { get; } = falloff;
        public AttackInfo AttackInfo { get; } = attackInfo;
        public EffectInfo EffectInfo { get; } = effectInfo;
        // inclomplete lmao i need to get left-right targets
        private protected override bool Use()
        {
            // Get the lists for flavour text
            var text = FlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, [Parent.Name, Target.Name]);
            var finalDamage = SimpleAttack(Target);
            if (finalDamage != null)
            {
                // Create a new attack for the side targets with falloff
                var sideAttack = new SingleTargetAttack(Parent, Target, Props, AttackInfo with { Ratio = AttackInfo.Ratio - Falloff });
                // Calculate damage for side targets
                finalDamage += sideAttack.SimpleAttack(Target.LeftTeammate) ?? 0;
                finalDamage += sideAttack.SimpleAttack(Target.RightTeammate) ?? 0;
                // Ability end text
                if (text.TryGetValue(TextTypes.End, out var endKey)) AddText(endKey, [Parent.Name, Target.Name, finalDamage,EffectInfo?.AppliedEffect.CreateArgs()]);
                return true;
            }
            else
            {
                // Ability miss text
                if (text.TryGetValue(TextTypes.Miss, out var missKey)) AddText(missKey, [Parent.Name, Target.Name]);
                else AddText(GenericMiss, [Parent.Name, Target.Name]);
                return false;
            }
        }
    }

    internal abstract class BounceAttack(Character source, Character mainTarget, AbilityInfo properties, uint count, AttackInfo attackInfo, EffectInfo effectInfo = null) : Ability(source, mainTarget, properties), IAttack, IAppliesEffect
    {
        public uint Count { get; } = count;
        public AttackInfo AttackInfo { get; } = attackInfo;
        public EffectInfo EffectInfo { get; } = effectInfo;
        public List<Character> Targets => Target.Team;

        private protected override bool Use()
        {
            // Get the lists for flavour text
            var text = FlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, [Parent.Name, Target.Name]);

            double finalDamage = 0;
            for (int index = 0; index < Count; ++index)
            {
                finalDamage += SimpleAttack(Targets.OrderBy(x => BattleManager.RNG).First()) ?? 0;

            }

            if (finalDamage != 0)
            {
                // Ability end text
                if (text.TryGetValue(TextTypes.End, out var endKey)) AddText(endKey, [Parent.Name, Target.Name, finalDamage, EffectInfo?.AppliedEffect.CreateArgs()]);
                return true;
            }
            else
            {
                // Ability miss text
                if (text.TryGetValue(TextTypes.Miss, out var missKey)) AddText(missKey, [Parent.Name, Target.Name]);
                else AddText(GenericMiss, [Parent.Name, Target.Name]);
                return false;
            }
        }
    }

    internal abstract class AoEAttack(Character source, AbilityInfo properties, Character mainTarget, AttackInfo attackInfo, EffectInfo effectInfo = null) : Ability(source, mainTarget, properties), IAttack, IAppliesEffect
    {
        public List<Character> Targets => Target.Team;
        public AttackInfo AttackInfo { get; } = attackInfo;
        public EffectInfo EffectInfo { get; } = effectInfo;

        private protected override bool Use()
        {
            // Get the lists for flavour text
            var text = FlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) AddText(startKey, [Parent.Name, Target.Name]);
            double finalDamage = 0;
            foreach (var character in Targets)
            {
                finalDamage += SimpleAttack(Target) ?? 0;
            }
            if (finalDamage != 0)
            {

                // Ability end text
                if (text.TryGetValue(TextTypes.End, out var endKey)) AddText(endKey, [Parent.Name, Target.Name, finalDamage, EffectInfo?.AppliedEffect.CreateArgs()]);
                return true;
            }
            else
            {
                // Ability miss text
                if (text.TryGetValue(TextTypes.Miss, out var missKey)) AddText(missKey, [Parent.Name, Target.Name]);
                else AddText(GenericMiss, [Parent.Name, Target.Name]);
                return false;
            }
        }
    }

    internal abstract class BuffSelf(Character source, AbilityInfo properties, EffectInfo effectInfo) : Ability(source, source, properties), IAppliesEffect
    {
        public EffectInfo EffectInfo { get; } = effectInfo;
        private protected override bool Use()
        {
            var text = FlavourText();

            return true;
        }
    }

    internal abstract class BuffTeam(Character source, AbilityInfo properties, EffectInfo effectInfo) : Ability(source, source, properties), IAppliesEffect
    {
        public EffectInfo EffectInfo { get; } = effectInfo;
        public List<Character> Targets { get => Target.Team; }
        private protected override bool Use()
        {
            var text = FlavourText();

            return true;
        }
    }

    internal abstract class Heal(Character source, Character target, AbilityInfo properties, double amount) : Ability(source, target, properties)
    {
        public double Amount { get; } = amount;
        private protected override bool Use()
        {

            return true;
        }
    }

    internal sealed class SkipTurn : Ability
    {
        public SkipTurn(Character source) : base(source, source, new("Skip turn", false))
        {
            WithReward(new(source, source, 5, Pools.Mana, Operators.Additive));
        }
        private protected override bool Use()
        {
            Parent.ParentBattle.BattleText.AppendLine(Localization.Translate("character.generic.skip", Parent.Name, Rewards.Single().Value.Amount));
            return true;
        }
    }
}
