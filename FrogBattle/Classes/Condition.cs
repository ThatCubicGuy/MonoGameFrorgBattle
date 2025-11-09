using System;
using System.Linq;

namespace FrogBattle.Classes
{
    internal abstract record class Condition(ConditionTypes ConditionType)
    {
        public abstract uint Get(Character user);
        public abstract object GetKey();
    }
    internal record StatValue(Stats Stat, double Step, PositiveInterval EffectiveInterval) : Condition(ConditionTypes.StatValue)
    {
        public override uint Get(Character user) => (uint)Math.Floor(Math.Min(Math.Max(0, user.GetStat(Stat) - EffectiveInterval.Min), EffectiveInterval.Width) / Step);
        public override object GetKey() => (typeof(StatValue), Stat);
    }
    /// <summary>
    /// Checks whether a stat's current value is within a given interval. Returns 1 or 0.
    /// </summary>
    /// <param name="Stat">The stat to check.</param>
    /// <param name="Interval">The interval within which the stat value has to be in order to return 1.</param>
    internal record StatThreshold(Stats Stat, PositiveInterval Interval) : Condition(ConditionTypes.StatThreshold)
    {
        public override uint Get(Character user) => Interval.Contains(user.GetStat(Stat)) ? 1U : 0U;
        public override object GetKey() => (typeof(StatThreshold), Stat);
    }
    internal record PoolValue(Pools Pool, double Step, PositiveInterval EffectiveInterval) : Condition(ConditionTypes.PoolValue)
    {
        public override uint Get(Character user) => (uint)Math.Floor(Math.Min(Math.Max(0, user.Resolve(Pool) - EffectiveInterval.Min), EffectiveInterval.Width) / Step);
        public override object GetKey() => (typeof(PoolValue), Pool);
    }
    /// <summary>
    /// Checks whether a pool is within a given interval. Returns 1 or 0.
    /// </summary>
    /// <param name="Pool">The pool to check.</param>
    /// <param name="Interval">The interval within which the pool value has to be in order to return 1.</param>
    internal record PoolThreshold(Pools Pool, PositiveInterval Interval) : Condition(ConditionTypes.PoolThreshold)
    {
        public override uint Get(Character user) => Interval.Contains(user.Resolve(Pool)) ? 1U : 0U;
        public override object GetKey() => (typeof(PoolThreshold), Pool);
    }
    internal record EffectStacks(StatusEffectDefinition Effect, PositiveInterval EffectiveInterval) : Condition(ConditionTypes.EffectStacks)
    {
        public override uint Get(Character user) => (uint)Math.Min(Math.Max(0, (user.ActiveEffects.FirstOrDefault(x => x == Effect)?.Stacks ?? 0) - EffectiveInterval.Min), EffectiveInterval.Width);
        public override object GetKey() => (typeof(EffectStacks), Effect);
    }
    internal record EffectsTypeCount<TEffect>(PositiveInterval EffectiveInterval) : Condition(ConditionTypes.EffectsTypeCount) where TEffect : SubeffectDefinition
    {
        public override uint Get(Character user) => (uint)Math.Min(Math.Max(0, user.GetActives<TEffect>().Count - EffectiveInterval.Min), EffectiveInterval.Width);
        public override object GetKey() => (typeof(EffectsTypeCount<TEffect>), typeof(TEffect));
    }
    internal record EffectTypeStacks<TEffect>(PositiveInterval EffectiveInterval) : Condition(ConditionTypes.EffectTypeStacks) where TEffect : SubeffectDefinition
    {
        public override uint Get(Character user) => (uint)Math.Min(Math.Max(0, user.GetActives<TEffect>().Sum(x => (x.Parent as StatusEffectDefinition).Stacks) - EffectiveInterval.Min), EffectiveInterval.Width);
        public override object GetKey() => (typeof(EffectTypeStacks<TEffect>), typeof(TEffect));
    }
}
