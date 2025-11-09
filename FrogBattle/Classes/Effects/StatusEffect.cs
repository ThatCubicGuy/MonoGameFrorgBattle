using System;
using System.Collections.Generic;
using System.Linq;

namespace FrogBattle.Classes.Effects
{
    /// <summary>
    /// StatusEffects are attribute modifiers (that have to be attached to entities) that influence the entity's stat in some way, and some more. 
    /// </summary>
    internal abstract record class StatusEffectDefinition
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
        public uint MaxStacks { get; init; }
        public uint BaseTurns { get; init; }
        public EffectFlags Properties { get; protected init; }
        public Dictionary<object, SubeffectDefinition> Subeffects { get; protected init; } = [];

        public StatusEffectInstance GetInstance(Character source, Character target) => new(this, source, target);
        public StatusEffectInstance GetInstance(ISourceTargetContext ctx) => GetInstance(ctx.Source, ctx.Target);
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

        /// <summary>
        /// The character to which this effect is applied.
        /// </summary>
        public Character Target { get; }
        public StatusEffectDefinition Definition => _definition;
        public string Name => Definition.Name;
        public Dictionary<object, ISubeffectInstance> Subeffects { get; }
        public uint Turns { get; private set; }
        public uint Stacks
        {
            get => stacks;
            set => stacks = Math.Min(value, _definition.MaxStacks);
        }

        public bool Is(EffectFlags p)
        {
            return _definition.Properties.HasFlag(p);
        }

        public void UpdateEffect(IMutableSubeffectInstance effect)
        {
            if (Subeffects.TryGetValue(effect.Definition.GetKey(), out var result)) ((IMutableSubeffectInstance)result).Amount += effect.Amount;
        }

        public void Renew(StatusEffectInstance effect)
        {
            if (Is(EffectFlags.StackTurns)) Turns += effect.Turns;
            else Turns = effect.Turns;
            Stacks += effect.Stacks;
            foreach (var item in effect.Subeffects.Values.OfType<IMutableSubeffectInstance>())
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
            return !(Is(EffectFlags.Infinite) || (--Turns > 0));
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
