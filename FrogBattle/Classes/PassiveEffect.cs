using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal class PassiveEffect : IAttributeModifier
    {
        private readonly Dictionary<object, Subeffect> _effects = [];
        public PassiveEffect(Character parent)
        {
            Source = parent;
        }
        public Character Source { get; }
        public Character Target { get; set; }
        /// <summary>
        /// Function that determined whether (and how much) the condition is fulfilled.
        /// </summary>
        public Func<Character[], int> ConditionFulfill { get; init; }
        public Dictionary<object, Subeffect> GetSubeffects()
        {
            return _effects;
        }

        /// <summary>
        /// Get all subeffects of type <typeparamref name="TResult"/> within this <see cref="PassiveEffect"/>.
        /// </summary>
        /// <returns>A dictionary containing every effect of type <typeparamref name="TResult"/>.</returns>
        public Dictionary<object, TResult> GetSubeffects<TResult>() where TResult : Subeffect
        {
            return GetSubeffects().Values.OfType<TResult>().ToDictionary(x => x.GetKey());
        }

        /// <summary>
        /// Gets the subeffect that modifies <paramref name="stat"/> in some way.
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public Modifier GetModifier(Stats stat)
        {
            return _effects.TryGetValue((typeof(Modifier), stat), out var result) ? result as Modifier : null;
        }
    }
}
