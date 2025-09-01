using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal class StatusEffect : IAttributeModifier
    {
        private readonly object _uid;
        private readonly Dictionary<object, Subeffect> _effects = [];
        private uint stacks = 1;
        
        public StatusEffect(Character source, Character target, uint turns, uint maxStacks, Flags properties, params Subeffect[] effects)
        {
            Source = source;
            Target = target;
            Turns = turns;
            MaxStacks = maxStacks;
            Properties = properties;
            foreach (var mod in effects)
            {
                _effects[mod.GetKey()] = mod;
            }
            _uid = GetType();
        }
        public StatusEffect(StatusEffect other) : this(other.Source, other.Target, other.Turns, other.MaxStacks, other.Properties)
        {
            _effects = new(other._effects);
            _uid = other._uid;
        }

        public static bool operator ==(StatusEffect left, StatusEffect right)
        {
            return left?.Equals(right) ?? right is null;
        }
        public static bool operator !=(StatusEffect left, StatusEffect right)
        {
            return !(left == right);
        }
        public override bool Equals(object obj)
        {
            if (this is null) return obj is null;
            if (obj is not StatusEffect eff) return false;
            return _uid.Equals(eff._uid);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(_effects, MaxStacks, Properties);
        }

        public string Name { get; set; }
        /// <summary>
        /// The character that applied this effect.
        /// </summary>
        public Character Source { get; }
        /// <summary>
        /// The character to which this effect is applied.
        /// </summary>
        public Character Target { get; }
        public uint Turns { get; private set; }
        public uint Stacks
        {
            get => stacks;
            set
            {
                stacks = Math.Min(value, MaxStacks);
            }
        }
        public uint MaxStacks { get; private set; }
        public Flags Properties { get; private set; }

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

        public bool Is(Flags p)
        {
            return Properties.HasFlag(p);
        }

        /// <summary>
        /// Deducts a turn from the StatusEffect and returns true if it has reached its end of lifetime.
        /// </summary>
        /// <returns>True if the StatusEffect has run out of turns and should be removed, false otherwise.</returns>
        public bool Expire()
        {
            return !(Is(Flags.Infinite) || (--Turns > 0));
        }

        public StatusEffect AddEffect(Subeffect effect)
        {
            _effects[effect.GetKey()] = effect;
            return this;
        }

        /// <summary>
        /// Get all subeffects within this <see cref="StatusEffect"/>.
        /// </summary>
        /// <returns>A dictionary containing every effect.</returns>
        public Dictionary<object, Subeffect> GetSubeffects()
        {
            return _effects;
        }

        /// <summary>
        /// Get all subeffects of type <typeparamref name="TResult"/> within this <see cref="StatusEffect"/>.
        /// </summary>
        /// <returns>A dictionary containing every effect of type <typeparamref name="TResult"/>.</returns>
        public Dictionary<object, TResult> GetSubeffects<TResult>() where TResult : Subeffect
        {
            return GetSubeffects().Values.OfType<TResult>().ToDictionary(x => x.GetKey());
        }

        /// <summary>
        /// Get the single effect of type <typeparamref name="TResult"/> contained within effects, or
        /// a default value if there are none.
        /// If effects has more than one <typeparamref name="TResult"/> effect, throws an exception.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public TResult SingleEffect<TResult>() where TResult : Subeffect
        {
            return GetSubeffects().Values.OfType<TResult>().SingleOrDefault();
        }

        /// <summary>
        /// Gets the subeffect that modifies <paramref name="stat"/> in some way.
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public Modifier GetModifier(Stats stat)
        {
            return GetSubeffects<Modifier>().TryGetValue((typeof(Modifier), stat), out var result) ? result : null;
        }

        public override string ToString()
        {
            return string.Join(", ", _effects.Values.Select(x => x.GetLocalizedText()));
        }
    }
}
