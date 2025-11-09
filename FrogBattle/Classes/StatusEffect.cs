using System;
using System.Collections.Generic;
using System.Linq;

namespace FrogBattle.Classes
{
    /// <summary>
    /// StatusEffects are attribute modifiers (that have to be attached to entities) that influence the entity's stat in some way, and some more. 
    /// </summary>
    internal abstract class StatusEffectDefinition
    {
        /// <summary>
        /// Unique EffectID used to determine whether two StatusEffects are equal.
        /// </summary>
        private readonly object _uid;
        /// <summary>
        /// Base constructor for any StatusEffect.
        /// </summary>
        /// <param name="source">The character that applied this effect.</param>
        /// <param name="target">The character to which this effect is applied.</param>
        /// <param name="turns">The amount of turns this StatusEffect should be applied for.</param>
        /// <param name="maxStacks">The maximum amount of stacks this StatusEffect can have.</param>
        /// <param name="properties">Properties such as invisibility or unremovability.</param>
        /// <param name="effects">Subeffects that this StatusEffect has.</param>
        public StatusEffectDefinition(params SubeffectDefinition[] subeffects)
        {
            _uid = GetType();
            Subeffects = subeffects.ToDictionary(x => x.GetKey());
        }

        public string Name { get; protected init; }
        public uint MaxStacks { get; protected init; }
        public uint BaseTurns { get; protected init; }
        public Flags Properties { get; protected init; }
        public Dictionary<object, SubeffectDefinition> Subeffects { get; protected init; } = [];

        [Flags] public enum Flags
        {
            None = 0,
            Debuff      = 1 << 0,
            Unremovable = 1 << 1,
            Hidden      = 1 << 2,
            Infinite    = 1 << 3,
            StartTick   = 1 << 4,
            StackTurns  = 1 << 5,
            RemoveStack = 1 << 6,
        }

        public override bool Equals(object obj)
        {
            if (obj is not StatusEffectDefinition eff) return false;
            return GetHashCode() == eff.GetHashCode();
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(_uid, MaxStacks, BaseTurns, Properties);
        }

        public StatusEffectInstance GetInstance(Character source, Character target) => new(this, source, target);
        public StatusEffectInstance GetInstance(IHasTarget ctx) => GetInstance(ctx.User, ctx.Target);
    }
    internal class StatusEffectInstance : IAttributeModifier
    {

        private readonly StatusEffectDefinition _definition;
        private uint stacks;
        public StatusEffectInstance(StatusEffectDefinition definition, Character source, Character target)
        {
            Source = source;
            Target = target;
            _definition = definition;
            Turns = _definition.BaseTurns;
            foreach (var item in _definition.Subeffects)
            {
                Subeffects.Add(item.Key, item.Value.GetInstance(this));
            }
        }
        /// <summary>
        /// The character that applied this effect.
        /// </summary>
        public Character Source { get; }
        Character IHasTarget.User => Source;

        /// <summary>
        /// The character to which this effect is applied.
        /// </summary>
        public Character Target { get; }
        public StatusEffectDefinition Definition => _definition;
        public string Name => Definition.Name;
        public Dictionary<object, SubeffectInstance> Subeffects { get; }
        public uint Turns { get; private set; }
        public uint Stacks
        {
            get => stacks;
            set => stacks = Math.Min(value, _definition.MaxStacks);
        }

        public bool Is(StatusEffectDefinition.Flags p)
        {
            return _definition.Properties.HasFlag(p);
        }

        public void UpdateEffect(SubeffectInstance effect)
        {
            if (Subeffects.TryGetValue(effect.GetKey(), out var result)) result.Amount += effect.Amount;
        }

        public void Renew(StatusEffectInstance effect)
        {
            if (Is(StatusEffectDefinition.Flags.StackTurns)) Turns += effect.Turns;
            else Turns = effect.Turns;
            Stacks += effect.Stacks;
            foreach (var item in effect.Subeffects.Values)
            {
                UpdateEffect(item);
            }
        }

        /// <summary>
        /// Deducts a turn from the StatusEffect and returns true if it has reached the end of its lifetime.
        /// </summary>
        /// <returns>True if the StatusEffect has run out of turns and should be removed, false otherwise.</returns>
        public bool Expire()
        {
            return !(Is(StatusEffectDefinition.Flags.Infinite) || (--Turns > 0));
        }

        public override bool Equals(object obj)
        {
            return _definition.Equals((obj as StatusEffectInstance)?._definition);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(_definition);
        }
    }
}
