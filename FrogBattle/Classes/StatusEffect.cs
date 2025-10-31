using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    /// <summary>
    /// StatusEffects are attribute modifiers (that have to be attached to entities) that influence the entity's stat in some way, and some more. 
    /// </summary>
    internal abstract class StatusEffect : IAttributeModifier
    {
        public static readonly StatusEffect Empty = new EmptyStatusEffect();
        private sealed class EmptyStatusEffect : StatusEffect
        {
            public override StatusEffect Init() => throw new InvalidOperationException("Cannot initialise StatusEffect.Empty");
        }
        public bool IsEmpty() => this is EmptyStatusEffect;
        /// <summary>
        /// Unique EffectID used in combination with the source fighter to determine whether two StatusEffects are equal.
        /// </summary>
        private readonly object _uid;
        private uint stacks = 1;
        /// <summary>
        /// Base constructor for any StatusEffect.
        /// </summary>
        /// <param name="source">The character that applied this effect.</param>
        /// <param name="target">The character to which this effect is applied.</param>
        /// <param name="turns">The amount of turns this StatusEffect should be applied for.</param>
        /// <param name="maxStacks">The maximum amount of stacks this StatusEffect can have.</param>
        /// <param name="properties">Properties such as invisibility or unremovability.</param>
        /// <param name="effects">Subeffects that this StatusEffect has.</param>
        public StatusEffect()
        {
            _uid = GetType();
        }
        /// <summary>
        /// This is where the StatusEffect initializes the actual subeffects, because many of them require the source or target for the value.
        /// </summary>
        public abstract StatusEffect Init();
        /// <summary>
        /// Adds turns, stacks and mutable effects from another StatusEffect as necessary.
        /// </summary>
        /// <param name="New">The new status effect to take from.</param>
        public StatusEffect AddMutables(StatusEffect New)
        {
            Stacks += New.Stacks;
            if (New.Properties.HasFlag(Flags.StackTurns)) Turns += New.Turns;
            foreach (var item in Subeffects.Values.OfType<IMutableEffect>())
            {
                if (New.Subeffects.TryGetValue(((Subeffect)item).GetKey(), out var effect)) AddEffect(effect);
            }
            return this;
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
            if (obj is not StatusEffect eff) return false;
            return GetHashCode() == eff.GetHashCode();
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(_uid, Source);
        }

        public StatusEffect Clone()
        {
            var clone = MemberwiseClone() as StatusEffect;
            clone.Subeffects = new(Subeffects);
            foreach (var item in clone.Subeffects.Values)
            {
                item.SetParent(clone);
            }
            return clone;
        }

        public string Name { get; init; }
        // EVERY SINGLE TIME THAT I THINK I MANAGED TO FIND A WAY TO INCCORPORATE required INTO MY CODE
        // EVERY. SINGLE. TIME. IT GETS REMOVED
        // CUZ CSHARP IS DUM >:(
        /// <summary>
        /// The character that applied this effect.
        /// </summary>
        public virtual Character Source { get; init; }
        /// <summary>
        /// The character to which this effect is applied.
        /// </summary>
        public virtual Character Target { get; init; }
        public Dictionary<object, Subeffect> Subeffects { get; private set; } = [];
        public uint Turns { get; set; }
        public uint Stacks
        {
            get => stacks;
            set
            {
                stacks = Math.Min(value, MaxStacks);
            }
        }
        public uint MaxStacks { get; init; } = 1;
        public Flags Properties { get; init; }

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
        /// Deducts a turn from the StatusEffect and returns true if it has reached the end of its lifetime.
        /// </summary>
        /// <returns>True if the StatusEffect has run out of turns and should be removed, false otherwise.</returns>
        public bool Expire()
        {
            return !(Is(Flags.Infinite) || (--Turns > 0));
        }

        public StatusEffect AddEffect(Subeffect effect)
        {
            if (effect is IMutableEffect mutable) UpdateEffect(mutable);
            else Subeffects[effect.GetKey()] = effect.SetParent(this);
            return this;
        }
        public void UpdateEffect(IMutableEffect effect)
        {
            if (Subeffects.TryGetValue(effect.GetKey(), out var result) && result is IMutableEffect mutable) mutable.Amount += effect.Amount;
        }

        /// <summary>
        /// Get all subeffects of type <typeparamref name="TResult"/> within this <see cref="StatusEffect"/>.
        /// </summary>
        /// <returns>A dictionary containing every effect of type <typeparamref name="TResult"/>.</returns>
        public Dictionary<object, TResult> GetSubeffectsOfType<TResult>() where TResult : Subeffect
        {
            return Subeffects.Values.OfType<TResult>().ToDictionary(x => x.GetKey());
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
            return Subeffects.Values.OfType<TResult>().SingleOrDefault();
        }

        /// <summary>
        /// Gets the subeffect that modifies <paramref name="stat"/> in some way.
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public Modifier GetModifier(Stats stat)
        {
            return GetSubeffectsOfType<Modifier>().TryGetValue((typeof(Modifier), stat), out var result) ? result : null;
        }
    }
}
