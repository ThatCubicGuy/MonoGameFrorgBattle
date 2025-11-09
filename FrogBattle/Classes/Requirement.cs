namespace FrogBattle.Classes
{
    /// <summary>
    /// Requirements for casting abilities. These can only be true or false.
    /// </summary>
    internal abstract class Requirement
    {
        public abstract bool Check(IHasTarget ctx);
        public abstract object GetKey();
    }
    internal class PoolRequirement : Requirement
    {
        private readonly double _amount;
        public PoolRequirement(double amount, Pools pool, Operators op)
        {
            _amount = amount;
            Pool = pool;
            Op = op;
        }

        public Pools Pool { get; }
        public Operators Op { get; }

        public Cost GetCost() => new(_amount, Pool, Op);

        public override bool Check(IHasTarget ctx)
        {
            return ctx.User.Resolve(Pool) >= Op.Apply(_amount, ctx.User.GetStat(Pool.Max()));
        }
        public override object GetKey() => (typeof(PoolRequirement), Pool);
    }
    internal class StatRequirement : Requirement
    {
        private readonly double _amount;

        public StatRequirement(double amount, Stats stat, Operators op)
        {
            _amount = amount;
            Stat = stat;
            Op = op;
        }

        public Stats Stat { get; }
        public Operators Op { get; }

        public override bool Check(IHasTarget ctx)
        {
            return ctx.User.GetStat(Stat) >= Op.Apply(_amount, ctx.User.Base[Stat]);
        }
        public override object GetKey() => (typeof(StatRequirement), Stat);
    }
}
