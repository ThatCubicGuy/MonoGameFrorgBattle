using System;
using System.Collections.Generic;
using FrogBattle.Classes.Effects;

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
        double BaseActionValue { get; }
        void EndTurn();
    }
    /// <summary>
    /// Implements a Source and a Target for various purposes.
    /// </summary>
    internal interface ISourceTargetContext
    {
        Character Source { get; }
        Character Target { get; }
    }
    /// <summary>
    /// Implements a User and a Target. This interface implements <see cref="ISourceTargetContext"/>.
    /// </summary>
    internal interface IHasUser : ISourceTargetContext
    {
        Character User { get; }
        Character ISourceTargetContext.Source => User;
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
        ISubeffectInstance GetInstance(IAttributeModifier ctx);
        double GetAmount(ISubeffectInstance ctx);
        string GetLocalizedText(ISubeffectInstance ctx);
        object GetKey();
    }

    internal interface IMutableEffect : ISubeffect
    {
        double BaseAmount { get; }
        double MaxAmount { get; }
        double GetMaxAmount(ISubeffectInstance ctx);
    }

    internal interface ISubeffectInstance : ISourceTargetContext
    {
        IAttributeModifier Parent { get; }
        Character ISourceTargetContext.Source => Parent.Source;
        Character ISourceTargetContext.Target => Parent.Target;
        ISubeffect Definition { get; }
        double Amount { get; }
    }

    internal interface IMutableSubeffectInstance : ISubeffectInstance
    {
        new double Amount { get; set; }
        double ISubeffectInstance.Amount => Amount;
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
    internal interface IAttributeModifier : ISourceTargetContext
    {
        Dictionary<object, ISubeffectInstance> Subeffects { get; }
    }

}
