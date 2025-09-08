using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal abstract record class Condition(ConditionTypes ConditionType)
    {
        public abstract uint Get(Character target);
        public abstract object GetKey();
    }
    internal record StatValue(Stats Stat, double Step, PositiveInterval EffectiveInterval) : Condition(ConditionTypes.StatValue)
    {
        public override uint Get(Character target) => (uint)Math.Floor(Math.Min(target.GetStat(Stat) - EffectiveInterval.Min, EffectiveInterval.Width) / Step);
        public override object GetKey() => (typeof(StatValue), Stat);
    }
    internal record StatThreshold(Stats Stat, PositiveInterval Interval) : Condition(ConditionTypes.StatThreshold)
    {
        public override uint Get(Character target) => Interval.Contains(target.GetStat(Stat)) ? 1U : 0U;
        public override object GetKey() => (typeof(StatThreshold), Stat);
    }
    internal record PoolValue(Pools Pool, double Step, PositiveInterval EffectiveInterval) : Condition(ConditionTypes.PoolValue)
    {
        public override uint Get(Character target) => (uint)Math.Floor(Math.Min(target.Resolve(Pool) - EffectiveInterval.Min, EffectiveInterval.Width) / Step);
        public override object GetKey() => (typeof(PoolValue), Pool);
    }
    internal record PoolThreshold(Pools Pool, PositiveInterval Interval) : Condition(ConditionTypes.PoolThreshold)
    {
        public override uint Get(Character target) => Interval.Contains(target.Resolve(Pool)) ? 1U : 0U;
        public override object GetKey() => (typeof(PoolThreshold), Pool);
    }
    internal record EffectStacks(StatusEffect Effect, PositiveInterval EffectiveInterval) : Condition(ConditionTypes.EffectStacks)
    {
        public override uint Get(Character target) => (uint)Math.Min((target.GetActives().FirstOrDefault(x => x == Effect)?.Stacks ?? 0) - EffectiveInterval.Min, EffectiveInterval.Width);
        public override object GetKey() => (typeof(EffectStacks), Effect);
    }
    internal record EffectsTypeCount<TEffect>(PositiveInterval EffectiveInterval) : Condition(ConditionTypes.EffectsTypeCount) where TEffect : Subeffect
    {
        public override uint Get(Character target) => (uint)Math.Min(target.GetActives<TEffect>().Count - EffectiveInterval.Min, EffectiveInterval.Width);
        public override object GetKey() => (typeof(EffectsTypeCount<TEffect>), typeof(TEffect));
    }
    internal record EffectTypeStacks<TEffect>(PositiveInterval EffectiveInterval) : Condition(ConditionTypes.EffectTypeStacks) where TEffect : Subeffect
    {
        public override uint Get(Character target) => (uint)Math.Min(target.GetActives<TEffect>().Sum(x => (x.Parent as StatusEffect).Stacks) - EffectiveInterval.Min, EffectiveInterval.Width);
        public override object GetKey() => (typeof(EffectTypeStacks<TEffect>), typeof(TEffect));
    }
}
