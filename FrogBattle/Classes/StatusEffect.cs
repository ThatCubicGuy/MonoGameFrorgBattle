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
        private readonly Dictionary<object, Effect> effects = [];
        
        public StatusEffect(Character source, Character target, uint turns, uint maxStacks, Flags properties, params Effect[] effects)
        {
            Source = source;
            Target = target;
            Turns = turns;
            MaxStacks = maxStacks;
            Properties = properties;
            foreach (var mod in effects)
            {
                this.effects[mod.GetKey()] = mod;
            }
            uid = uid_count;
            uid_count += 1;
        }
        public StatusEffect(StatusEffect other) : this(other.Source, other.Target, other.Turns, other.MaxStacks, other.Properties)
        {
            effects = new(other.effects);
            uid = other.uid;
            uid_count -= 1;
        }

        public string Name { get; set; }
        public Character Source { get; }
        public Character Target { get; }
        public uint Turns { get; private set; }
        public uint Stacks { get; set; }
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
            if (Is(Flags.Infinite) || (--Turns > 0)) return false;
            else return true;
        }
        public StatusEffect AddEffect(Effect effect)
        {
            effects[effect.GetKey()] = effect;
            return this;
        }
        /// <summary>
        /// Get all effects within this <see cref="StatusEffect"/>.
        /// </summary>
        /// <returns>A dictionary containing every effect.</returns>
        public Dictionary<object, Effect> GetEffects()
        {
            return effects;
        }
        /// <summary>
        /// Get all effects of type <typeparamref name="TResult"/> within this <see cref="StatusEffect"/>.
        /// </summary>
        /// <returns>A dictionary containing every effect of type <typeparamref name="TResult"/>.</returns>
        public Dictionary<object, TResult> GetEffects<TResult>() where TResult : Effect
        {
            return effects.OfType<KeyValuePair<object, TResult>>().ToDictionary();
        }
        /// <summary>
        /// Get the single effect of type <typeparamref name="TResult"/> contained within effects.
        /// If effects has more than one <typeparamref name="TResult"/> effect, throws an exception.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public TResult SingleEffect<TResult>() where TResult : Effect
        {
            return effects.Values.OfType<TResult>().SingleOrDefault();
        }
        public Dictionary<object, Modifier> GetModifiers()
        {
            return (Dictionary<object, Modifier>)effects.OfType<KeyValuePair<object, Modifier>>();
        }
        public string Display()
        {
            // '↑' = '\u2191'; '↓' = '\u2913'
            string arrow = Stacks <= 4 ? new(Is(Flags.Debuff) ? '↓' : '↑', (int)Stacks) : (Is(Flags.Debuff) ? $"↓{Stacks}" : $"↑{Stacks}");
            if (Is(Flags.Infinite)) return $"{arrow}[{Name}]";
            else return $"{arrow}[{Name} ({Turns})]";
        }
        internal abstract class Effect
        {
            public Effect(StatusEffect parent, bool buff)
            {
                Parent = parent;
                IsBuff = buff;
            }
            public StatusEffect Parent { get; }
            public Character SourceFighter => Parent.Source;
            public Character TargetFighter => Parent.Target;
            public bool IsBuff { get; }
            public string TranslationKey { get => "effect.type." + GetType().Name.camelCase(); }
            public virtual string GetLocalizedText() => Localization.Translate(TranslationKey, GetFormatArgs());
            public abstract object[] GetFormatArgs();
            public abstract object GetKey();
        }
        internal class Modifier : Effect
        {
            private readonly double _amount;
            public Modifier(StatusEffect parent, double amount, Stats stat, Operators op) : base(parent, (amount < 0) ^ Registry.IsHigherBetter(stat))
            {
                _amount = amount;
                Stat = stat;
                Op = op;
            }
            /// <summary>
            /// Automatically applies the operator based on the fighter it is attached to.
            /// </summary>
            public double Amount
            {
                get => Op.Apply(_amount, TargetFighter.Base[Stat]);
            }
            public Stats Stat { get; }
            private Operators Op { get; }
            public override object[] GetFormatArgs() => [Amount, Stat];
            public override object GetKey() => (typeof(Modifier), Stat);

        }
        internal class Shield : Effect
        {
            private readonly double maxAmount;
            private double currentAmount;
            public Shield(StatusEffect parent, double amount, DamageTypes? shieldType = null, double? maxAmount = null, double? healPerTurn = null) : base(parent, true)
            {
                if (amount <= 0 || maxAmount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Shield value must be positive.");
                if (maxAmount != null) this.maxAmount = maxAmount.Value;
                Amount = amount;
                ShieldType = shieldType;
                Healing = healPerTurn;
            }
            public double Amount
            {
                get => currentAmount;
                set
                {
                    currentAmount = value;
                    if (currentAmount > maxAmount) currentAmount = maxAmount;
                }
            }
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
            private readonly uint maxCount;
            private uint currentCount;
            public Barrier(StatusEffect parent, uint count, uint? maxCount = null) : base(parent, true)
            {
                if (count == 0 || maxCount == 0) throw new ArgumentOutOfRangeException(nameof(count), "Barrier count must be positive.");
                if (maxCount != null) this.maxCount = maxCount.Value;
                Count = count;
            }
            public uint Count
            {
                get => currentCount;
                set
                {
                    currentCount = value;
                    if (currentCount > maxCount) currentCount = maxCount;
                }
            }
            public override object[] GetFormatArgs() => [Count];
            public override object GetKey() => typeof(Barrier);
        }
        internal class Drain : Effect
        {
            private readonly double _amount;
            public Drain(StatusEffect parent, double amount, Pools pool, Operators op) : base(parent, false)
            {
                if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Drain value must be positive.");
                if (pool == Pools.Hp) throw new ArgumentOutOfRangeException(nameof(pool));
                _amount = amount;
                Pool = pool;
                Op = op;
            }
            /// <summary>
            /// Automatically applies the operator based on the fighter it is attached to.
            /// </summary>
            public double Amount
            {
                get => Op.Apply(_amount, TargetFighter.Resolve(Pool));
            }
            public Pools Pool { get; }
            private Operators Op { get; }
            public override object[] GetFormatArgs() => [Amount, Pool];
            public override object GetKey() => (typeof(Drain), Pool);
        }
        internal class DamageOverTime : Effect
        {
            private readonly double _amount;
            public DamageOverTime(StatusEffect parent, double amount, Operators op, Damage.Properties props) : base(parent, false)
            {
                _amount = amount;
                Op = op;
                Props = props with { CanCrit = false, Source = DamageSources.DamageOverTime };
            }
            /// <summary>
            /// Automatically applies the operator based on the fighter it is attached to.
            /// </summary>
            public double Amount
            {
                get
                {
                    return Op.Apply(_amount, TargetFighter.Hp);
                }
            }
            private Operators Op { get; }
            public Damage.Properties Props { get; }
            public override object[] GetFormatArgs() => [Amount];
            public override object GetKey() => typeof(DamageOverTime);
        }
        /// <summary>
        /// Influences a specific type of damage. Percentage only.
        /// </summary>
        internal class DamageTypeBonus : Effect
        {
            private readonly double _amount;
            public DamageTypeBonus(StatusEffect parent, double amount, DamageTypes type) : base(parent, amount >= 0)
            {
                _amount = amount;
                Type = type;
            }
            public double Amount
            {
                get => _amount;
            }
            public DamageTypes Type { get; }
            public override object[] GetFormatArgs() => [Amount, Type];
            public override object GetKey() => (typeof(DamageTypeBonus), Type);
        }
        /// <summary>
        /// Influences a specific type of damage resistance. Percentage only.
        /// </summary>
        internal class DamageTypeRES : Effect
        {
            private readonly double _amount;
            public DamageTypeRES(StatusEffect parent, double amount, DamageTypes type) : base(parent, amount >= 0)
            {
                _amount = amount;
                Type = type;
            }
            public double Amount
            {
                get => _amount;
            }
            public DamageTypes Type { get; }
            public override object[] GetFormatArgs() => [Amount, Type];
            public override object GetKey() => (typeof(DamageTypeRES), Type);
        }
        /// <summary>
        /// Influences damage coming from a specific source. Percentage only.
        /// </summary>
        internal class DamageSourceBonus : Effect
        {
            private readonly double _amount;
            public DamageSourceBonus(StatusEffect parent, double amount, DamageSources source) : base(parent, amount >= 0)
            {
                _amount = amount;
                Source = source;
            }
            public double Amount
            {
                get => _amount;
            }
            public DamageSources Source { get; }
            public override object[] GetFormatArgs() => [Amount, Source];
            public override object GetKey() => (typeof(DamageTypeBonus), Source);
        }
        /// <summary>
        /// Influences damage resistance against a specific source. Percentage only.
        /// </summary>
        internal class DamageSourceRES : Effect
        {
            private readonly double _amount;
            public DamageSourceRES(StatusEffect parent, double amount, DamageSources source) : base(parent, amount >= 0)
            {
                _amount = amount;
                Source = source;
            }
            public double Amount
            {
                get => _amount;
            }
            public DamageSources Source { get; }
            public override object[] GetFormatArgs() => [Amount, Source];
            public override object GetKey() => (typeof(DamageTypeRES), Source);
        }
        /// <summary>
        /// Influences damage dealt, positive values are good. Percentage only.
        /// </summary>
        internal class DamageBonus : Effect
        {
            private readonly double _amount;
            public DamageBonus(StatusEffect parent, double amount) : base(parent, amount >= 0)
            {
                _amount = amount;
            }
            public double Amount
            {
                get => _amount;
            }
            public override object[] GetFormatArgs() => [Amount];
            public override object GetKey() => typeof(DamageBonus);
        }
        /// <summary>
        /// Influences damage taken, positive values are good. Percentage only.
        /// </summary>
        internal class DamageRES : Effect
        {
            private readonly double _amount;
            public DamageRES(StatusEffect parent, double amount) : base(parent, amount >= 0)
            {
                _amount = amount;
            }
            public double Amount
            {
                get => _amount;
            }
            public override object[] GetFormatArgs() => [Amount];
            public override object GetKey() => typeof(DamageRES);
        }
        internal class Overheal : Effect
        {
            private double currentAmount;
            public Overheal(StatusEffect parent, double amount) : base(parent, true)
            {
                currentAmount = amount;
            }
            public double Amount
            {
                get => currentAmount;
                set
                {
                    currentAmount -= value;
                }
            }
            public override object[] GetFormatArgs() => [Amount];
            public override object GetKey() => typeof(Overheal);
        }
    }
}
