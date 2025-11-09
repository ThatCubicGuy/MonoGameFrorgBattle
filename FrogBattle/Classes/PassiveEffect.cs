using System.Collections.Generic;
using System.Linq;

namespace FrogBattle.Classes
{
    /// <summary>
    /// PassiveEffects are conditional StatusEffects that aren't displayed and are turn independent.
    /// </summary>
    internal abstract class PassiveEffect : IAttributeModifier
    {
        private readonly object _uid;
        public Dictionary<object, SubeffectInstance> Subeffects { get; } = [];
        public PassiveEffect()
        {
            _uid = GetType();
        }
        public Character Source { get; init; }
        public Character Target { get => Source; }
        Character IHasTarget.User => Source;
        public Condition Condition { protected get; init; }
        public uint GetStacks(Character target) => target == null ? 0 : Condition.Get(target);
    }
}
