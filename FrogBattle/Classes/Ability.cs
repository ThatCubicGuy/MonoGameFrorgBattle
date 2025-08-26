using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
        public Dictionary<Pools, PoolChange> PoolChanges { get; }
        public Dictionary<object, Condition> Conditions { get; }
        public record Properties
        (
            string Name,
            bool RepeatsTurn
        );
        public bool TryUse()
        {
            foreach (var item in Conditions)
            {
                if (!item.Value.Check()) return false;
            }
            Use();
            foreach (var item in PoolChanges)
            {
                Parent.ApplyChange(item.Value);
            }
            return true;
        }
        public abstract void Use();
        protected Ability WithPoolChange(PoolChange change)
        {
            PoolChanges[change.Pool] = change;
            return this;
        }
        protected Ability WithCondition(Condition condition)
        {
            Conditions[condition.GetKey()] = condition;
            return this;
        }
        /// <summary>
        /// Attaches a PoolAmount condition to the ability and its corresponding change.
        /// </summary>
        /// <returns>This ability.</returns>
        protected Ability WithCost(PoolAmount cost)
        {
            return WithCondition(cost).WithPoolChange(cost.GetCost());
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
        internal class StatThreshold : Condition
        {
            private readonly double? _min;
            private readonly double? _max;
            public StatThreshold(Ability parent, double? minAmount, double? maxAmount, Stats stat, Operators op) : base(parent)
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
        internal class PoolAmount : Condition
        {
            private readonly double _amount;
            public PoolAmount(Ability parent, double amount, Pools pool, Operators op) : base(parent)
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
            /// <returns>A <see cref="PoolChange"/> with the stat requirements of this condition.</returns>
            public PoolChange GetCost()
            {
                return new(-1 * _amount, Pool, Op);
            }
            public override bool Check()
            {
                return ParentFighter.Resolve(Pool) >= Amount;
            }
            public override object GetKey() => Pool;
        }
        internal record PoolChange
        (
            double Amount,
            Pools Pool,
            Operators Op
        );
    }
    internal abstract class SingleTargetAttack(Character source, Ability.Properties properties, Character target, double ratio, Stats scalar, uint[] split) : Ability(source, properties), IAttack
    {
        public Character MainTarget { get; } = target;
        public double Ratio { get; } = ratio;
        public Stats Scalar { get; } = scalar;
        public uint[] Split { get; } = split;
        public override void Use()
        {

        }
    }

    internal abstract class BlastAttack(Character source, Ability.Properties properties, Character mainTarget, double ratio, Stats scalar, uint[] split, double falloff) : Ability(source, properties), IAttack
    {
        public Character MainTarget { get; } = mainTarget;
        public double Ratio { get; } = ratio;
        public Stats Scalar { get; } = scalar;
        public uint[] Split { get; } = split;
        public double Falloff { get; } = falloff;
        public override void Use()
        {

        }
    }

    internal abstract class AoEAttack(Character source, Ability.Properties properties, Character mainTarget, double ratio, Stats scalar, uint[] split) : Ability(source, properties), IAttack
    {
        public Character MainTarget { get; } = mainTarget;
        public double Ratio { get; } = ratio;
        public Stats Scalar { get; } = scalar;
        public uint[] Split { get; } = split;
        public List<Character> Targets => MainTarget.Team;
        public override void Use()
        {

        }
    }

    internal abstract class BuffSelf : Ability
    {
        public BuffSelf(Character source, Properties properties, StatusEffect buff) : base(source, properties)
        {
            Buff = buff;
        }
        public StatusEffect Buff { get; }
        public override void Use()
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
        public override void Use()
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
    }

}
