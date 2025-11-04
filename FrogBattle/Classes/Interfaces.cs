using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrogBattle.Classes
{
    internal interface ITakesAction
    {
        /// <summary>
        /// The action of the ITakesAction entity.
        /// </summary>
        /// <returns>True if the turn should end, false otherwise.</returns>
        void TakeAction();
    }

    // anything that has a turn in the ActionBar
    internal interface IHasTurn : ITakesAction
    {
        double BaseActionValue { get; }
    }

    internal interface IHasTarget
    {
        Character Parent { get; }
        Character Target { get; }
    }

    internal interface IHasStats
    {

    }

    internal interface IDamageable
    {
        double Hp { get; }
        void TakeDamage(Damage.Snapshot damage);
        event EventHandler<Damage.Snapshot> DamageTaken;  // FINALLY I UNDERSTAND HOW THIS PMO SHIT WORKS
    }
    
    internal interface IDamageSource
    {
        bool IsCrit(IDamageable target);
        void DealDamage(Damage.Snapshot damage);
        event EventHandler<Damage.Snapshot> DamageDealt;
    }

    internal interface ICanHaveActives
    {
        List<StatusEffect> ActiveEffects { get; }
    }

    internal interface ICanHavePassives
    {
        List<PassiveEffect> PassiveEffects { get; }
    }

    internal interface ISupportsEffects : ICanHaveActives, ICanHavePassives
    {
        void AddEffect(IAttributeModifier effect);
        void RemoveEffect(IAttributeModifier effect);
    }

    internal interface IAttack : IHasTarget
    {
        AttackInfo AttackInfo { get; }
    }

    internal interface ISubeffect
    {
        IAttributeModifier Parent { get; init; }
        object GetKey();
    }

    internal interface IMutableEffect : ISubeffect
    {
        double Amount { get; set; }
    }

    internal interface IEvent
    {
        static abstract void Event(object sender, Damage.Snapshot e);
    }

    internal interface ITrigger;

    internal interface IHealing : IHasTarget
    {
        HealingInfo HealingInfo { get; }
    }

    internal interface IAppliesEffect : IHasTarget
    {
        EffectInfo[] EffectInfos { get; }
    }

    internal interface IPoolChange
    {
        public Character Source { get; }
        public Character Target { get; }
        /// <summary>
        /// Automatically calculates the final value of the amount through the attached fighters.
        /// </summary>
        public double Amount { get; }
        public Pools Pool { get; }
        public Operators Op { get; }
    }

    internal interface IAttributeModifier
    {
        Character Source { get; }
        Character Target { get; }
        Dictionary<object, Subeffect> Subeffects { get; }
    }
}
