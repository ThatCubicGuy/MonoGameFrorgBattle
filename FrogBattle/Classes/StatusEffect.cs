using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal abstract class StatusEffect
    {
        private static uint uid_count = 0;
        private readonly uint uid;
        protected Fighter source;
        public uint turns;
        public uint maxStacks;
        public Props properties;
        public List<Effect> effects;
        
        public StatusEffect(uint turns, uint maxStacks, Props properties, params Effect[] effects)
        {
            this.turns = turns;
            this.maxStacks = maxStacks;
            this.properties = properties;
            this.effects = [.. effects];
            uid = uid_count++;
        }
        public StatusEffect(StatusEffect other) : this(other.turns, other.maxStacks, other.properties, other.effects.ToArray())
        {
            source = other.source;
            uid = other.uid;
            uid_count -= 1;
        }

        public string Name { get; set; }
        
        [Flags] public enum Props
        {
            None        = 1 << 0,
            Debuff      = 1 << 1,
            Unremovable = 1 << 2,
            Hidden      = 1 << 3,
            Infinite    = 1 << 4,
            StartTick   = 1 << 5,
            StackTurns  = 1 << 6,
        }

        internal class Effect : IEffect
        {
            public Effect(StatusEffect parent, Stats stat, double amount, Operators op)
            {
                Parent = parent;
                Stat = stat;
                Amount = amount;
                Op = op;
            }

            public StatusEffect Parent { get; }
            public string Name { get => "effect.type." + Stat.ToTranslatable(); }
            public Stats Stat { get; }
            public double Amount { get; }
            public Operators Op { get; }
        }

        public bool Is(Props p)
        {
            return (properties & p) != 0;
        }
        public bool Expire()
        {
            if (Is(Props.Infinite) || (--turns > 0)) return false;
            else return true;
        }
        public string Display()
        {
            char a = Is(Props.Debuff) ? '\u2193' : '\u2191';
            if (Is(Props.Infinite)) return $"{a}[{Name}]";
            else return $"{a}[{Name} ({turns})]";
        }
    }
    internal class Modifier : StatusEffect
    {
        public Modifier(uint turns, uint maxStacks, Props properties) : base(turns, maxStacks, properties)
        {
            // take a break bro holy shit 04:18
        }
        public Modifier AddModifier(Stats stat, double amount, Operators op)
        {
            effects = effects.Append(new(this, stat, amount, op)).ToList();
            return this;
        }
    }
    internal class Shield : StatusEffect
    {
        private double shield;
        private double healPerTurn;
        public Shield(uint turns, uint maxStacks, Props properties) : base(turns, maxStacks, properties)
        {
        }
        public Shield SetShield(double shield, double healPerTurn)
        {
            this.shield = shield;
            this.healPerTurn = healPerTurn;
            return this;
        }
    }
}
