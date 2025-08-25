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
        private readonly Dictionary<object, Effect> effects = [];
        
        public StatusEffect(uint turns, uint maxStacks, Props properties, params Effect[] effects)
        {
            this.turns = turns;
            this.maxStacks = maxStacks;
            this.properties = properties;
            foreach (var mod in effects)
            {
                this.effects[mod.GetKey()] = mod;
            }
            uid = uid_count;
            uid_count += 1;
        }
        public StatusEffect(StatusEffect other) : this(other.turns, other.maxStacks, other.properties)
        {
            effects = new(other.effects);
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
        public StatusEffect AddEffect(Effect effect)
        {
            effects[effect.GetKey()] = effect;
            return this;
        }
        public Dictionary<object, Effect> GetEffects()
        {
            return effects;
        }
        public Dictionary<object, T> GetEffects<T>() where T : Effect
        {
            return effects.OfType<KeyValuePair<object, T>>().ToDictionary();
        }
        public Dictionary<object, Modifier> GetModifiers()
        {
            return (Dictionary<object, Modifier>)effects.OfType<KeyValuePair<object, Modifier>>();
        }
        public string Display()
        {
            char a = Is(Props.Debuff) ? '\u2193' : '\u2191';
            if (Is(Props.Infinite)) return $"{a}[{Name}]";
            else return $"{a}[{Name} ({turns})]";
        }
        internal abstract class Effect
        {
            public Effect(StatusEffect parent, bool buff)
            {
                Parent = parent;
                IsBuff = buff;
            }
            public StatusEffect Parent { get; }
            public bool IsBuff { get; }
            public string TranslationKey { get => "effect.type." + GetType().Name.camelCase(); }
            public virtual string GetLocalizedText() => Localization.Translate(TranslationKey, GetFormatArgs());
            public abstract object[] GetFormatArgs();
            public abstract object GetKey();
        }
        internal class Modifier : Effect
        {
            public Modifier(StatusEffect parent, double amount, Stats stat, Operators op) : base(parent, (amount > 0) ^ Registry.IsHigherBetter(stat))
            {
                Amount = amount;
                Stat = stat;
                Op = op;
            }
            public double Amount { get; }
            public Operators Op { get; }
            public Stats Stat { get; }
            public override object[] GetFormatArgs() => [Amount, Stat];
            public override object GetKey() => Stat;

        }
        internal class Shield : Effect
        {
            public Shield(StatusEffect parent, double amount, DamageTypes? shieldType = null, double? healPerTurn = null) : base(parent, true)
            {
                if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Shield value must be positive.");
                Amount = amount;
                ShieldType = shieldType;
                Healing = healPerTurn;
            }
            public double Amount { get; }
            public DamageTypes? ShieldType { get; }
            public double? Healing { get; }
            public override object[] GetFormatArgs()
            {
                // need to go through an intermediary step of deserialization before passing resolved string as argument
                var addons = new List<string>();
                if (ShieldType != null) addons.Add(Localization.Translate(TranslationKey + ".addon.type", ShieldType));
                addons.Add(Localization.Translate(TranslationKey + ".addon"));
                if (Healing != null) addons.Add(Localization.Translate(TranslationKey + ".addon.healing", Healing));
                var addonText = string.Join(' ', addons);
                return [Amount, addonText];
            }
            public override object GetKey() => typeof(Shield);
        }
        internal class Barrier : Effect
        {
            public Barrier(StatusEffect parent, uint count) : base(parent, true)
            {
                if (count == 0) throw new ArgumentOutOfRangeException(nameof(count), "Barrier count must be positive.");
                Count = count;
            }
            public uint Count { get; }
            public override object[] GetFormatArgs() => [Count];
            public override object GetKey() => typeof(Barrier);
        }
        internal class Drain : Effect
        {
            public Drain(StatusEffect parent, double amount, Pools pool, Operators op) : base(parent, false)
            {
                if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Drain value must be positive.");
                if (pool == Pools.Hp) throw new ArgumentOutOfRangeException(nameof(pool));
                Amount = amount;
                Pool = pool;
                Op = op;
            }
            public double Amount { get; }
            public Operators Op { get; }
            public Pools Pool { get; }
            public override object[] GetFormatArgs() => [Amount, Pool];
            public override object GetKey() => Pool;
        }
        internal class DamageOverTime : Effect
        {
            public DamageOverTime(StatusEffect parent, double amount, Operators op, Damage.Properties props) : base(parent, false)
            {
                Amount = amount;
                Op = op;
                Props = props with { crit = false, source = DamageSources.DamageOverTime };
            }
            public double Amount { get; }
            public Operators Op { get; }
            public Damage.Properties Props { get; }
            public override object[] GetFormatArgs() => [Amount];
            public override object GetKey() => typeof(DamageOverTime);
        }
    }
}
