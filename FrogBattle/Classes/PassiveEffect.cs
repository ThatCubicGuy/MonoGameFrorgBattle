using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    /// <summary>
    /// PassiveEffects are conditional StatusEffects that aren't displayed and are turn independent.
    /// </summary>
    internal abstract class PassiveEffect : IAttributeModifier
    {
        private readonly object _uid;
        public Dictionary<object, Subeffect> Subeffects { get; } = [];
        public PassiveEffect()
        {
            _uid = GetType();
        }
        public Character Source { get; }
        public Character Target { get => Source; }
        public Condition Condition { protected get; init; }
        public uint GetStacks(Character target) => target == null ? 0 : Condition.Get(target);

        /// <summary>
        /// Get all subeffects of type <typeparamref name="TResult"/> within this <see cref="PassiveEffect"/>.
        /// </summary>
        /// <returns>A dictionary containing every effect of type <typeparamref name="TResult"/>.</returns>
        public Dictionary<object, TResult> GetSubeffectsOfType<TResult>() where TResult : Subeffect
        {
            return Subeffects.Values.OfType<TResult>().ToDictionary(x => x.GetKey());
        }

        /// <summary>
        /// Gets the subeffect that modifies <paramref name="stat"/> in some way.
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public Modifier GetModifier(Stats stat)
        {
            return Subeffects.TryGetValue((typeof(Modifier), stat), out var result) ? result as Modifier : null;
        }
        // Builder methods
        public void AddEffect(Subeffect effect)
        {
            Subeffects[effect.GetKey()] = effect.SetParent(this);
        }
    }
}
