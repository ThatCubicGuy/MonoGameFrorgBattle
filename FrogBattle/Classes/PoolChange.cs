namespace FrogBattle.Classes
{
    internal abstract class PoolChange
    {
        private readonly double _baseAmount;

        public PoolChange(double amount, Pools pool, Operators op)
        {
            // You can use the cost / reward system for gaining and consuming HP.
            // Prefer not to use it for actual healing and damage.
            _baseAmount = amount;
            Pool = pool;
            Op = op;
        }

        public Pools Pool { get; }
        public Operators Op { get; }

        /// <summary>
        /// Returns the final value of the amount, after applying the operator.
        /// </summary>
        public double GetAmount(IHasTarget ctx)
        {
            return Op.Apply(_baseAmount, ctx.Target.Resolve(Pool)) * GetBonuses(Pool, ctx);
        }
        protected abstract double GetBonuses(Pools pool, IHasTarget ctx);
    }

    /// <summary>
    /// Rewards generally given after using abilities.
    /// </summary>
    internal class Reward : PoolChange
    {
        /// <summary>
        /// Initialises a new instance of a <see cref="Reward"/>.
        /// </summary>
        /// <param name="amount">Base amount of the reward.</param>
        /// <param name="pool">The pool to which it will be credited.</param>
        /// <param name="op">The operator to apply between the reward amount and base pool value.</param>
        public Reward(double amount, Pools pool, Operators op) : base(amount, pool, op) { }
        protected override double GetBonuses(Pools pool, IHasTarget ctx)
        {
            return pool switch
            {
                Pools.Mana => ctx.Target.GetStatVersus(Stats.ManaRegen, ctx.User),
                Pools.Energy => ctx.Target.GetStatVersus(Stats.EnergyRecharge, ctx.User),
                _ => 1
            };
        }
    }

    /// <summary>
    /// The opposite of rewards. Positive values are deducted. Taxed before using the ability.
    /// </summary>
    internal class Cost : PoolChange
    {
        /// <summary>
        /// Initialises a new instance of a <see cref="Cost"/>.
        /// </summary>
        /// <param name="amount">Base amount of the cost.</param>
        /// <param name="pool">The pool from which it will be deducted.</param>
        /// <param name="op">The operator to apply between the cost amount and base pool value.</param>
        public Cost(double amount, Pools pool, Operators op) : base(-1 * amount, pool, op) { }
        protected override double GetBonuses(Pools pool, IHasTarget ctx)
        {
            return pool switch
            {
                Pools.Mana => ctx.Target.GetStatVersus(Stats.ManaCost, ctx.User),
                _ => 1
            };
        }
    }
}
