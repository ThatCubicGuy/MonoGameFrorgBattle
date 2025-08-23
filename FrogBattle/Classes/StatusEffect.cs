using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    // really not sure what the best course of action is here.
    // shield statuseffects will only ever have one shield effect, but it feels weird to add this
    // special case and leave it as is. maybe i could add different kinds of effects, but then
    // that's redundant. idk tbh.
    internal class StatusEffect : ICloneable
    {
        protected List<Modifier> modifiers;
        protected List<Adder> adders;
        public readonly uint maxStacks;
        public readonly Properties props;
        public StatusEffect(Fighter src, uint maxStacks, Properties props, Texture2D icon, params IEffect[] effects)
        {
            this.maxStacks = maxStacks;
            this.props = props;
            Icon = icon;
            modifiers = (List<Modifier>)effects.ToList().OfType<Modifier>();
            adders = (List<Adder>)effects.ToList().OfType<Adder>();
        }
        public StatusEffect(Fighter src, uint maxStacks, Properties props, Texture2D icon, params Modifier[] effects)
        {
            this.maxStacks = maxStacks;
            this.props = props;
            Icon = icon;
            modifiers = [.. effects];
        }
        public StatusEffect(Fighter src, uint maxStacks, Properties props, Texture2D icon, params Adder[] effects)
        {
            this.maxStacks = maxStacks;
            this.props = props;
            Icon = icon;
            adders = [.. effects];
        }
        public object Clone()
        {
            return new StatusEffect(Source, maxStacks, props, Icon, [..adders.GetRange(0, adders.Count), ..modifiers.GetRange(0, modifiers.Count)]);
        }
        public virtual bool AddStacks(uint stacks)
        {
            if (Stacks == maxStacks) return false;
            Stacks += stacks;
            if (Stacks > maxStacks) Stacks = maxStacks;
            return true;
        }
        public Texture2D Icon { get; private set; }
        public uint Stacks { get; private set; }
        public Fighter Source { get; }
        [Flags] public enum Properties
        {
            None,
            Unremovable,
            Hidden,
            StackTurns,
            IndependentStackDuration,
        }
        internal class Modifier : IEffect
        {
            public Modifier(Stats attrib, double amount, Operators operation)
            {
                Attribute = attrib;
                Amount = amount;
                Op = operation;
            }
            public StatusEffect Parent { get; private set; }
            public string Name { get => "effect.type." + Attribute.ToString(); }
            public Stats Attribute { get; }
            public double Amount { get; }
            public Operators Op { get; }
        }
        internal class Adder : IEffect
        {
            public Adder(Pools attrib, double amount)
            {
                Attribute = attrib;
                Amount = amount;
            }
            public StatusEffect Parent { get; private set; }
            public string Name { get => "effect.type." + Attribute.ToString(); }
            public Pools Attribute { get; }
            public double Amount { get; set; }
        }
        internal class DamageOverTime : IEffect
        {
            private double _ratio;
            public DamageOverTime(Stats attrib, double ratio)
            {
                Attribute = attrib;
                _ratio = ratio;
            }
            public StatusEffect Parent { get; private set; }
            public string Name { get => "effect.type." + Attribute.ToString(); }
            public Stats Attribute { get; }
            public double Amount { get => _ratio * Parent.Source.Resolve(Attribute); }
        }
        /// <summary>
        /// Every effect that this instance of <see cref="StatusEffect"/> has.
        /// </summary>
        public IEnumerable<IEffect> Effects
        {
            get
            {
                return modifiers.AsEnumerable<IEffect>().Concat(adders.AsEnumerable<IEffect>());
            }
        }
        /// <summary>
        /// Searches <see cref="modifiers"/> for all modifiers.
        /// </summary>
        /// <returns>All modifiers that this StatusEffect encompasses.</returns>
        public IEnumerable<Modifier> GetModifiers()
        {
            return modifiers;
        }
        /// <summary>
        /// Searches <see cref="modifiers"/> for all modifiers that match the given predicate.
        /// </summary>
        /// <param name="predicate">The condition that a <see cref="Modifier"/> must match.</param>
        /// <returns>An enumerable of modifiers that match the given predicate.</returns>
        public IEnumerable<Modifier> GetModifiers(Predicate<Modifier> predicate)
        {
            return modifiers.FindAll(predicate);
        }
        /// <summary>
        /// Searches <see cref="modifiers"/> for all modifiers that modify <paramref name="stat"/>.
        /// </summary>
        /// <param name="stat"></param>
        /// <returns>An enumerable of modifiers that affect <paramref name="stat"/>.</returns>
        public IEnumerable<Modifier> GetModifiers(Stats stat)
        {
            return modifiers.FindAll((item) => item.Attribute == stat);
        }
        public IEnumerable<Adder> GetAdders()
        {
            return adders;
        }
    }
    internal class Shield : StatusEffect
    {
        private double maxValue;
        private Adder shield;
        public Shield(Fighter src, double hp, double maxValue, Properties props, Texture2D icon, params IEffect[] extraModifiers) : base(src, 1, props, icon, extraModifiers.Prepend(new Adder(Pools.Shield, hp)).ToArray())
        {
            this.maxValue = maxValue;
            shield = adders.First();
        }
        public double ReduceShield(double amount)
        {
            return Math.Max(0, -(shield.Amount -= amount));
        }
        public double GetShield()
        {
            return shield.Amount;
        }
    }
    internal class Barrier : StatusEffect
    {
        private uint maxCount;
        private Adder barrier;
        public Barrier(Fighter src, int count, uint maxCount, Properties props, Texture2D icon, params IEffect[] extraModifiers) : base(src, 1, props, icon, extraModifiers.Prepend(new Adder(Pools.Barrier, count)).ToArray())
        {
            this.maxCount = maxCount;
            barrier = adders.First();
        }
        public bool ReduceBarrier(uint amount)
        {
            return (barrier.Amount -= amount) <= 0;
        }
        public double GetBarrier()
        {
            return barrier.Amount;
        }
    }
}
