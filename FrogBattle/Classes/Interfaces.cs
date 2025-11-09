using System;
using System.Collections.Generic;

namespace FrogBattle.Classes
{
    internal interface ITakesAction
    {
        /// <summary>
        /// The action of the ITakesAction entity.
        /// </summary>
        /// <returns>True if the turn should end, false otherwise.</returns>
        bool StartTurn();
    }

    // anything that has a turn in the ActionBar
    internal interface IHasTurn : ITakesAction
    {
        void EndTurn();
        double BaseActionValue { get; }
    }

    internal interface IHasTarget
    {
        Character User { get; }
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
        List<StatusEffectInstance> ActiveEffects { get; }
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

    internal interface IAttack
    {
        AttackInfo AttackInfo { get; }
    }

    internal interface IAppliesEffect
    {
        EffectInfo[] EffectInfos { get; }
    }

    internal interface ISubeffect
    {
        double GetAmount(IAttributeModifier ctx);
        object GetKey();
    }

    internal interface IMutableEffect : ISubeffect
    {
        void SetAmount(double value);
    }

    internal interface ISubeffectInstance
    {
        double Amount { get; }
    }
    internal interface IMutableSubeffectInstance : ISubeffectInstance
    {
        new double Amount { get; set; }
    }

    internal interface IEvent
    {
        static abstract void Event(object sender, Damage.Snapshot e);
    }

    internal interface ITrigger;

    internal interface IHealing
    {
        HealingInfo HealingInfo { get; }
    }

    /// <summary>
    /// An instance type of an attribute modifier. Has Source, Target, and a list of SubeffectInstances.
    /// </summary>
    internal interface IAttributeModifier : IHasTarget
    {
        Character Source => User;
        Dictionary<object, SubeffectInstance> Subeffects { get; }
    }

    internal interface ISubeffectInstance
    {
        double Amount { get; }
    }
}
