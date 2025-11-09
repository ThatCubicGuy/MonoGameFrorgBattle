using System.Collections.Generic;
using System.Linq;

namespace FrogBattle.Classes.Effects
{
    /// <summary>
    /// PassiveEffects are conditional StatusEffects that aren't displayed and are turn independent.
    /// </summary>
    internal abstract record class PassiveEffect : IAttributeModifier
    {
        private readonly object _uid;
        private readonly Dictionary<object, ISubeffect> _definitions = [];
        public PassiveEffect(params SubeffectDefinition[] subeffects)
        {
            _uid = GetType();
            foreach (var item in subeffects)
            {
                _definitions.Add(item.GetKey(), item);
                Subeffects.Add(item.GetKey(), (StaticSubeffectInstance)item.GetInstance(this));
            }
        }
        public Character Source { get; init; }
        public Character Target { get => Source; }
        public Condition Condition { protected get; init; }
        public Dictionary<object, ISubeffectInstance> Subeffects { get; } = [];
        Dictionary<object, ISubeffectInstance> IAttributeModifier.Subeffects => Subeffects;
        public uint GetStacks(Character target) => target == null ? 0 : Condition.Get(target);
    }
}
