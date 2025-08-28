using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static FrogBattle.Classes.StatusEffect;

namespace FrogBattle.Classes
{
    // rewrite #7 gazillion lmfao
    internal abstract class Ability
    {
        protected Ability(Character parent, Properties properties)
        {
            Parent = parent;
            Props = properties;
        }
        public Character Parent { get; }
        public Properties Props { get; }
        /// <summary>
        /// Pool changes that execute after the ability is checked, but before it is launched.
        /// </summary>
        public Dictionary<Pools, Cost> Costs { get; }
        /// <summary>
        /// Pool changes only execute if the ability is launched successfully (e.g. not a miss).
        /// </summary>
        public Dictionary<Pools, Reward> Rewards { get; }
        public Dictionary<object, Condition> Conditions { get; }
        protected Dictionary<string, object[]> FlavourTextBuilder { get; }
        public static string GenericDamage { get; } = "character.generic.damage";
        public static string GenericMiss { get; } = "character.generic.miss";
        public record Properties
        (
            string Name,
            bool RepeatsTurn
        );
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
        protected abstract bool Use();
        /// <summary>
        /// Creates untranslated flavour text keys.
        /// </summary>
        /// <returns>An enumerator that iterates through every available key for this ability.</returns>
        public Dictionary<TextTypes, string> FlavourText()
        {
            FlavourTextBuilder.Clear();
            var result = new Dictionary<TextTypes, string>();
            string baseName = string.Join('.', Parent._internalName, typeof(Ability).Name.camelCase(), GetType().Name.camelCase(), "text");
            foreach (var item in Enum.GetValues(typeof(TextTypes)))
                if (Localization.strings.ContainsKey(string.Join('.', baseName, item.ToString().camelCase())))
                     result.Add((TextTypes)item, string.Join('.', baseName, item.ToString().camelCase()));
            return result;
        }
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
            var cost = new PoolAmountCondition(this, -1 * amount, Pools.Mana, Operators.Additive);
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
    internal abstract class SingleTargetAttack(Character source, Ability.Properties properties, Character target, double ratio, Stats scalar, double? hitRate, bool independentHitRate, Damage.Properties props, uint[] split) : Ability(source, properties), IAttack
    {
        public Character MainTarget { get; } = target;
        public double Ratio { get; } = ratio;
        public Stats Scalar { get; } = scalar;
        public double? HitRate { get; } = hitRate;
        public bool IndependentHitRate { get; } = independentHitRate;
        public DamageTypes Type { get; } = props.Type;
        public double DefenseIgnore { get; }
        public double TypeResPen { get; }
        public uint[] Split { get; } = split;

        protected override bool Use()
        {
            var text = FlavourText();
            var damages = this.AttackDamage(MainTarget);
            if (text.TryGetValue(TextTypes.Start, out var startKey)) FlavourTextBuilder.Add(startKey, [Parent.Name, MainTarget.Name]);
            if (damages == null)
            {
                if (text.TryGetValue(TextTypes.Miss, out var key)) FlavourTextBuilder.Add(key, [Parent.Name, MainTarget.Name]);
                else FlavourTextBuilder.Add(GenericMiss, [Parent.Name, MainTarget.Name]);
                return false;
            }
            double finalDamage = 0;
            bool allMiss = true;
            int index = 0;
            foreach (var item in damages)
            {
                if (item == null)
                    if (text.TryGetValue(TextTypes.Miss, out var key)) FlavourTextBuilder.Add(key, [Parent.Name, MainTarget.Name]);
                    else FlavourTextBuilder.Add(GenericMiss, [Parent.Name, MainTarget.Name]);
                else
                {
                    index += 1;
                    finalDamage += item.Amount;
                    if (text.TryGetValue((TextTypes)index, out var key)) FlavourTextBuilder.Add(key, [Parent.Name, MainTarget.Name, item.Amount]);
                    else FlavourTextBuilder.Add(GenericDamage, [Parent.Name, MainTarget.Name, item.Amount]);
                    MainTarget.TakeDamage(item);
                    allMiss = false;
                }
            }
            if (text.TryGetValue(TextTypes.End, out var endKey)) FlavourTextBuilder.Add(endKey, [Parent.Name, MainTarget.Name]);
            foreach (var item in FlavourTextBuilder)
            {
                Parent.ParentBattle.BattleText.AppendLine(Localization.Translate(item));
            }
            return !allMiss;
        }
    }

    internal abstract class BlastAttack(Character source, Ability.Properties properties, Character mainTarget, double ratio, Stats scalar, double? hitRate, bool independentHitRate, Damage.Properties props, uint[] split, double falloff) : Ability(source, properties), IAttack
    {
        public Character MainTarget { get; } = mainTarget;
        public double Ratio { get; } = ratio;
        public Stats Scalar { get; } = scalar;
        public double? HitRate { get; } = hitRate;
        public bool IndependentHitRate { get; } = independentHitRate;
        public DamageTypes Type { get; } = props.Type;
        public double DefenseIgnore { get; }
        public double TypeResPen { get; }
        public uint[] Split { get; } = split;
        public double Falloff { get; } = falloff;
        protected override bool Use()
        {
            var text = FlavourText();
            var centerDamages = this.AttackDamage(MainTarget);
            // 2 more sets of damage for side targets
            if (text.TryGetValue(TextTypes.Start, out var startKey)) FlavourTextBuilder.Add(startKey, [Parent.Name, MainTarget.Name]);
            if (centerDamages == null)
            {
                if (text.TryGetValue(TextTypes.Miss, out var key)) FlavourTextBuilder.Add(key, [Parent.Name, MainTarget.Name]);
                else FlavourTextBuilder.Add(GenericMiss, [Parent.Name, MainTarget.Name]);
                return false;
            }
            double finalDamage = 0;
            bool allMiss = true;
            int index = 0;
            foreach (var item in centerDamages)
            {
                if (item == null)
                    if (text.TryGetValue(TextTypes.Miss, out var key)) FlavourTextBuilder.Add(key, [Parent.Name, MainTarget.Name]);
                    else FlavourTextBuilder.Add(GenericMiss, [Parent.Name, MainTarget.Name]);
                else
                {
                    index += 1;
                    finalDamage += item.Amount;
                    if (text.TryGetValue((TextTypes)index, out var key)) FlavourTextBuilder.Add(key, [Parent.Name, MainTarget.Name, item.Amount]);
                    else FlavourTextBuilder.Add(GenericDamage, [Parent.Name, MainTarget.Name, item.Amount]);
                    MainTarget.TakeDamage(item);
                    // Implement damage dealing for side targets incorporating falloff
                    allMiss = false;
                }
            }
            if (text.TryGetValue(TextTypes.End, out var endKey)) FlavourTextBuilder.Add(endKey, [Parent.Name, MainTarget.Name]);
            foreach (var item in FlavourTextBuilder)
            {
                Parent.ParentBattle.BattleText.AppendLine(Localization.Translate(item));
            }
            return !allMiss;
        }
    }

    internal abstract class AoEAttack(Character source, Ability.Properties properties, Character mainTarget, double ratio, Stats scalar, double? hitRate, Damage.Properties props, uint[] split) : Ability(source, properties), IAttack
    {
        public Character MainTarget { get; } = mainTarget;
        public double Ratio { get; } = ratio;
        public Stats Scalar { get; } = scalar;
        public double? HitRate { get; } = hitRate;
        public bool IndependentHitRate { get; } = true;
        public DamageTypes Type { get; } = props.Type;
        public double DefenseIgnore { get; }
        public double TypeResPen { get; }
        public uint[] Split { get; } = split;
        public List<Character> Targets => MainTarget.Team;
        // this definitely needs a review lmfao
        // but i also need sleep so uh
        protected override bool Use()
        {
            var text = FlavourText();
            var damages = new List<List<Damage>>();
            foreach (var item in Targets)
            {
                damages.Add(this.AttackDamage(item));
            }
            if (text.TryGetValue(TextTypes.Start, out var startKey)) FlavourTextBuilder.Add(startKey, [Parent.Name, .. Targets.Select(x => x.Name)]);
            if (damages == null)
            {
                if (text.TryGetValue(TextTypes.Miss, out var key)) FlavourTextBuilder.Add(key, [Parent.Name, .. Targets.Select(x => x.Name)]);
                else FlavourTextBuilder.Add(GenericMiss, [Parent.Name, .. Targets.Select(x => x.Name)]);
                return false;
            }
            double finalDamage = 0;
            bool allMiss = true;
            int index = 0;
            foreach (var item in damages)
            {
                if (item == null)
                    if (text.TryGetValue(TextTypes.Miss, out var key)) FlavourTextBuilder.Add(key, [Parent.Name, .. Targets.Select(x => x.Name)]);
                    else FlavourTextBuilder.Add(GenericMiss, [Parent.Name, .. Targets.Select(x => x.Name)]);
                else
                {
                    index += 1;
                    finalDamage += item.Amount;
                    if (text.TryGetValue((TextTypes)index, out var key)) FlavourTextBuilder.Add(key, [Parent.Name, .. Targets.Select(x => x.Name), item.Amount]);
                    else FlavourTextBuilder.Add(GenericDamage, [Parent.Name, .. Targets.Select(x => x.Name), item.Amount]);
                    Targets.ForEach(x => x.TakeDamage(item));
                    allMiss = false;
                }
            }
            if (text.TryGetValue(TextTypes.End, out var endKey)) FlavourTextBuilder.Add(endKey, [Parent.Name, .. Targets.Select(x => x.Name)]);
            foreach (var item in FlavourTextBuilder)
            {
                Parent.ParentBattle.BattleText.AppendLine(Localization.Translate(item));
            }
            return !allMiss;
        }
    }

    internal abstract class BuffSelf : Ability
    {
        public BuffSelf(Character source, Properties properties, StatusEffect buff) : base(source, properties)
        {
            Buff = buff;
        }
        public StatusEffect Buff { get; }
        protected override bool Use()
        {

        }
    }

    internal abstract class BuffTeam : Ability
    {
        public BuffTeam(Character source, Properties properties, StatusEffect buff) : base(source, properties)
        {
            Buff = buff;
        }
        public List<Character> Targets { get => Parent.Team; }
        public StatusEffect Buff { get; }
        protected override bool Use()
        {

        }
    }

    internal abstract class Heal : Ability
    {
        public Heal(Character source, Properties properties, double amount, Character target) : base(source, properties)
        {
            Target = target;
        }
        public Character Target { get; }
        protected override bool Use()
        {

        }
    }
}
