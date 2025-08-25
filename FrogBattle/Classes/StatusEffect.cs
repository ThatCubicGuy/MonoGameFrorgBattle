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
    internal class StatusEffect
    {
        private static uint uid_count = 0;
        private readonly uint uid;
        protected Character source;
        public uint turns;
        public uint maxStacks;
        public Props properties;
        private readonly Dictionary<Stats, Modifier> modifiers = [];
        
        public StatusEffect(uint turns, uint maxStacks, Props properties, params Modifier[] modifiers)
        {
            this.turns = turns;
            this.maxStacks = maxStacks;
            this.properties = properties;
            foreach (var mod in modifiers)
            {
                this.modifiers[mod.Stat] = mod;
            }
            uid = uid_count;
            uid_count += 1;
        }
        public StatusEffect(StatusEffect other) : this(other.turns, other.maxStacks, other.properties)
        {
            modifiers = new(other.modifiers);
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

        public bool Is(Props p)
        {
            return properties.HasFlag(p);
        }
        public bool Expire()
        {
            if (Is(Props.Infinite) || (--turns > 0)) return false;
            else return true;
        }
        public StatusEffect AddModifier(Stats stat, double amount, Operators op)
        {
            modifiers[stat] = new(this, stat, amount, op);
            return this;
        }
        public StatusEffect AddModifier(Modifier modifier)
        {
            modifiers[modifier.Stat] = modifier;
            return this;
        }
        public Dictionary<Stats, Modifier> GetModifiers()
        {
            return modifiers;
        }
        public string Display()
        {
            char a = Is(Props.Debuff) ? '\u2193' : '\u2191';
            if (Is(Props.Infinite)) return $"{a}[{Name}]";
            else return $"{a}[{Name} ({turns})]";
        }

        internal class Modifier : ISubcomponent<StatusEffect>
        {
            public Modifier(StatusEffect parent, Stats stat, double amount, Operators op)
            {
                Parent = parent;
                Stat = stat;
                Amount = amount;
                Op = op;
            }

            public StatusEffect Parent { get; }
            public string Name { get => "effect.type." + Stat.ToString().toCamelCase(); }
            public Stats Stat { get; }
            public double Amount { get; }
            public Operators Op { get; }
        }
    }
    internal class Shield : StatusEffect
    {
        private double shieldValue;
        private double healPerTurn;
        public Shield(uint turns, uint maxStacks, Props properties) : base(turns, maxStacks, properties)
        {
        }
        public Shield SetShield(double shieldValue, double healPerTurn = 0)
        {
            this.shieldValue = shieldValue;
            this.healPerTurn = healPerTurn;
            return this;
        }
        public double GetShield()
        {
            return shieldValue;
        }
        public double GetHealing()
        {
            return healPerTurn;
        }
    }
    internal class Barrier : StatusEffect
    {
        private double barrierCount;
        public Barrier(uint turns, uint maxStacks, Props properties) : base(turns, maxStacks, properties)
        {
        }
        public Barrier SetShield(double barrierCount)
        {
            this.barrierCount = barrierCount;
            return this;
        }
        public double GetBarrier()
        {
            return barrierCount;
        }
    }
    internal class Drain : StatusEffect
    {
        private Damage.Properties damageProperties;
        private readonly Dictionary<Pools, double> drainAmount = [];
        public Drain(uint turns, uint maxStacks, Props properties, Damage.Properties damageProperties) : base(turns, maxStacks, properties)
        {
            this.damageProperties = damageProperties;
        }
        public Drain SetDrain(Pools damageTarget, double damageAmount)
        {
            drainAmount[damageTarget] = damageAmount;
            return this;
        }
        public static Damage.Properties DrainProperties(double defenseIgnore, double typeResPen, DamageTypes type)
        {
            return new(crit: false, critDamage: 0, defenseIgnore: defenseIgnore, typeResPen: typeResPen, type: type, source: DamageSources.DamageOverTime);
        }
        public Drain SetDoT(double amount, Damage.Properties props = null)
        {
            drainAmount[Pools.Hp] = amount;
            if (props != null) damageProperties = props with { crit = false, source = DamageSources.DamageOverTime };
            return this;
        }
        public Damage GetDoT()
        {
            return new(drainAmount[Pools.Hp], damageProperties);
        }
        public double GetDrain(Pools damageTarget)
        {
            return drainAmount[damageTarget];
        }
    }
}
