using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal abstract class Subeffect : ISubeffect
    {
        private IAttributeModifier parent;
        public Subeffect(bool buff)
        {
            IsBuff = buff;
        }
        public IAttributeModifier Parent { get => parent; init => parent = value; }
        /// <summary>
        /// The character that applied this effect's parent.
        /// </summary>
        public Character SourceFighter => Parent.Source;
        /// <summary>
        /// The character to which this effect's parent is applied.
        /// </summary>
        public Character TargetFighter => Parent.Target;
        public bool IsBuff { get; }
        //public virtual double Amount { get; set; }
        public string TranslationKey { get => "effect.type." + GetType().Name.FirstLower(); }
        public virtual string GetLocalizedText() => Localization.Translate(TranslationKey, GetFormatArgs());
        public abstract object[] GetFormatArgs();
        public abstract object GetKey();
        public Subeffect SetParent(IAttributeModifier parent)
        {
            this.parent = parent;
            return this;
        }
    }
    internal class Modifier : Subeffect
    {
        private readonly double _amount;
        public Modifier(double amount, Stats stat, Operators op) : base((amount > 0) == Registry.IsHigherBetter(stat))
        {
            _amount = amount;
            Stat = stat;
            Op = op;
        }
        private Modifier(Modifier clone) : this(clone._amount, clone.Stat, clone.Op) { }
        /// <summary>
        /// Automatically applies the operator based on the fighter it is attached to.
        /// </summary>
        public double Amount
        {
            get => Op.Apply(_amount, TargetFighter.Base[Stat]);
            init => _amount = value;
        }
        public Stats Stat { get; init; }
        public Operators Op { get; init; }
        public override object[] GetFormatArgs() => [Op == Operators.AddValue ? (Amount > 0 ? '+' : string.Empty) + Amount.ToString("F0") : ((_amount > 0 ? '+' : string.Empty) + (_amount * 100).ToString("P")), Stat.GetLocalizedText()];
        public override object GetKey() => (typeof(Modifier), Stat);
        public double GetBaseAmount() => _amount;
    }
    internal class Shield : Subeffect, IMutableEffect
    {
        private readonly double maxAmount;
        private double currentAmount;
        public Shield(double amount, DamageTypes? shieldType = null, double? maxAmount = null) : base(true)
        {
            if (amount <= 0 || maxAmount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Shield value must be positive.");
            if (maxAmount.HasValue) this.maxAmount = maxAmount.Value;
            Amount = amount;
            ShieldType = shieldType;
        }
        private Shield(Shield clone) : this(clone.currentAmount, clone.ShieldType, clone.maxAmount) { }
        public double Amount
        {
            get => currentAmount;
            set
            {
                currentAmount = Math.Min(value, maxAmount);
            }
        }
        public DamageTypes? ShieldType { get; }
        public override object[] GetFormatArgs()
        {
            // need to go through an intermediary step of deserialization before passing resolved string as argument
            var addons = new List<string>();
            if (ShieldType != null) addons.Add(Localization.Translate(TranslationKey + ".addon.type", ShieldType));
            addons.Add(Localization.Translate(TranslationKey + ".addon"));
            var addonText = string.Join(' ', addons);
            return [Amount, addonText];
        }
        public override object GetKey() => typeof(Shield);
        public double GetMaxAmount() => maxAmount;
    }
    internal class Barrier : Subeffect, IMutableEffect
    {
        private readonly uint maxCount;
        private uint currentCount;
        public Barrier(uint count, uint? maxCount = null) : base(true)
        {
            if (count == 0 || maxCount == 0) throw new ArgumentOutOfRangeException(nameof(count), "Barrier count must be positive.");
            if (maxCount != null) this.maxCount = maxCount.Value;
            Count = count;
        }
        private Barrier(Barrier clone) : this(clone.currentCount, clone.maxCount) { }
        public uint Count
        {
            get => currentCount;
            set
            {
                currentCount = Math.Min(value, maxCount);
            }
        }
        public double Amount
        {
            get => Count;
            set => Count = (uint)value;
        }
        public override object[] GetFormatArgs() => [Count];
        public override object GetKey() => typeof(Barrier);
        public uint GetMaxCount() => maxCount;
    }
    internal class PerTurnChange : Subeffect
    {
        private readonly double _amount;
        public PerTurnChange(double amount, Pools pool, Operators op) : base(amount > 0)
        {
            if (pool == Pools.Hp) throw new ArgumentOutOfRangeException(nameof(pool), "Cannot use PerTurnChange for HP changes. Use DamageOverTime or HealingOverTime instead.");
            _amount = amount;
            Pool = pool;
            Op = op;
        }
        private PerTurnChange(PerTurnChange clone) : this(clone._amount, clone.Pool, clone.Op) { }
        /// <summary>
        /// Automatically applies the operator based on the fighter it is attached to.
        /// </summary>
        public double Amount
        {
            get => Op.Apply(_amount, TargetFighter.Resolve(Pool));
        }
        public Pools Pool { get; }
        public Operators Op { get; }
        public override object[] GetFormatArgs() => [Op == Operators.AddValue ? Amount : $"{(_amount > 0 ? '+' : string.Empty)}{_amount * 100:P}", Pool];
        public override object GetKey() => (typeof(PerTurnChange), Pool);
        public double GetBaseAmount() => _amount;
    }
    internal class DamageOverTime : Subeffect
    {
        public static readonly DamageInfo Default = new(CanCrit: false, Source: DamageSources.DamageOverTime);
        private readonly double _amount;
        public DamageOverTime(double amount, Operators op, DamageInfo props = null) : base(false)
        {
            _amount = amount;
            Op = op;
            DamageProps = props ?? Default;
        }
        private DamageOverTime(DamageOverTime clone) : this(clone._amount, clone.Op, clone.DamageProps)
        {
            DamageProps = clone.DamageProps;
        }
        /// <summary>
        /// Automatically applies the operator based on the fighter it is attached to.
        /// <see cref="DamageOverTime.Amount"/> automatically applies stacks.
        /// </summary>
        public double Amount
        {
            get => Op.Apply(_amount, TargetFighter.Hp) * (Parent as StatusEffect).Stacks;
        }
        public Operators Op { get; }
        public DamageInfo DamageProps { get; }
        /// <summary>
        /// The type of DoT. There are four base types, but it can just be none of them. Some characters use this.
        /// </summary>
        public DoTTypes Type { get; init; } = DoTTypes.None;
        /// <summary>
        /// Get the damage associated with this DoT instance.
        /// </summary>
        /// <returns>An instance of Damage associated with this Damage-over-Time.</returns>
        public Damage GetDamage() => new(SourceFighter, TargetFighter, Amount, DamageProps);
        public override object[] GetFormatArgs() => [GetDamage().Amount];
        public override object GetKey() => typeof(DamageOverTime);
        public double GetBaseAmount() => _amount;
    }
    internal class HealingOverTime : Subeffect
    {
        private readonly double _amount;
        public HealingOverTime(double amount, Operators op) : base(true)
        {
            _amount = amount;
            Op = op;
        }
        private HealingOverTime(HealingOverTime clone) : this(clone._amount, clone.Op) { }
        /// <summary>
        /// Automatically applies the operator based on the fighter it is attached to.
        /// </summary>
        public double Amount
        {
            get => Op.Apply(_amount, TargetFighter.Hp) * (Parent as StatusEffect).Stacks;
        }
        public Operators Op { get; }
        /// <summary>
        /// Gets the healing associated with this HoT (lmao) instance.
        /// </summary>
        /// <returns>An instance of Healing associated with this Healing-over-Time.</returns>
        public Healing GetHealing() => new(SourceFighter, TargetFighter, new(Amount));
        public override object[] GetFormatArgs() => [GetHealing().Amount];
        public override object GetKey() => typeof(DamageOverTime);
        public double GetBaseAmount() => _amount;
    }

    /// <summary>
    /// Influences a specific type of damage. Percentage only.
    /// </summary>
    internal class DamageTypeBonus : Subeffect
    {
        private readonly double _amount;
        public DamageTypeBonus(double amount, DamageTypes type) : base(amount >= 0)
        {
            _amount = amount;
            Type = type;
        }
        private DamageTypeBonus(DamageTypeBonus clone) : this(clone._amount, clone.Type) { }
        public double Amount
        {
            get => _amount;
        }
        public DamageTypes Type { get; }
        public override object[] GetFormatArgs() => [(Amount > 0 ? '+' : string.Empty) + Amount.ToString("P"), Type];
        public override object GetKey() => (typeof(DamageTypeBonus), Type);
    }

    /// <summary>
    /// Influences a specific type of damage resistance. Percentage only.
    /// </summary>
    internal class DamageTypeRES : Subeffect
    {
        private readonly double _amount;
        public DamageTypeRES(double amount, DamageTypes type) : base(amount >= 0)
        {
            _amount = amount;
            Type = type;
        }
        private DamageTypeRES(DamageTypeRES clone) : this(clone._amount, clone.Type) { }
        public double Amount
        {
            get => _amount;
        }
        public DamageTypes Type { get; }
        public override object[] GetFormatArgs() => [(Amount > 0 ? '+' : string.Empty) + Amount.ToString("P"), Type];
        public override object GetKey() => (typeof(DamageTypeRES), Type);
    }

    /// <summary>
    /// Influences damage coming from a specific source. Percentage only.
    /// </summary>
    internal class DamageSourceBonus : Subeffect
    {
        private readonly double _amount;
        public DamageSourceBonus(double amount, DamageSources source) : base(amount >= 0)
        {
            _amount = amount;
            Source = source;
        }
        private DamageSourceBonus(DamageSourceBonus clone) : this(clone._amount, clone.Source) { }
        public double Amount
        {
            get => _amount;
        }
        public DamageSources Source { get; }
        public override object[] GetFormatArgs() => [(Amount > 0 ? '+' : string.Empty) + Amount.ToString("P"), Source];
        public override object GetKey() => (typeof(DamageTypeBonus), Source);
    }

    /// <summary>
    /// Influences damage resistance against a specific source. Percentage only.
    /// </summary>
    internal class DamageSourceRES : Subeffect
    {
        private readonly double _amount;
        public DamageSourceRES(double amount, DamageSources source) : base(amount >= 0)
        {
            _amount = amount;
            Source = source;
        }
        private DamageSourceRES(DamageSourceRES clone) : this(clone._amount, clone.Source) { }
        public double Amount
        {
            get => _amount;
        }
        public DamageSources Source { get; }
        public override object[] GetFormatArgs() => [(Amount > 0 ? '+' : string.Empty) + Amount.ToString("P"), Source];
        public override object GetKey() => (typeof(DamageTypeRES), Source);
    }

    /// <summary>
    /// Influences damage dealt, positive values are good. Percentage only.
    /// </summary>
    internal class DamageBonus : Subeffect
    {
        private readonly double _amount;
        public DamageBonus(double amount) : base(amount >= 0)
        {
            _amount = amount;
        }
        private DamageBonus(DamageBonus clone) : this(clone._amount) { }
        public double Amount
        {
            get => _amount;
        }
        public override object[] GetFormatArgs() => [(Amount > 0 ? '+' : string.Empty) + Amount.ToString("P")];
        public override object GetKey() => typeof(DamageBonus);
    }

    /// <summary>
    /// Influences damage taken, positive values are good. Percentage only.
    /// </summary>
    internal class DamageRES : Subeffect
    {
        private readonly double _amount;
        public DamageRES(double amount) : base(amount >= 0)
        {
            _amount = amount;
        }
        private DamageRES(DamageRES clone) : this(clone._amount) { }
        public double Amount
        {
            get => _amount;
        }
        public override object[] GetFormatArgs() => [(Amount > 0 ? '+' : string.Empty) + Amount.ToString("P")];
        public override object GetKey() => typeof(DamageRES);
    }

    /// <summary>
    /// Temporary HP. Gets consumed before normal HP, and parent is generally removed upon reaching zero.
    /// </summary>
    internal class Overheal : Subeffect, IMutableEffect
    {
        private double currentAmount;
        public Overheal(double amount) : base(true)
        {
            currentAmount = amount;
        }
        private Overheal(Overheal clone) : this(clone.currentAmount) { }
        public double Amount
        {
            get => currentAmount;
            set
            {
                currentAmount = value;
            }
        }
        public override object[] GetFormatArgs() => [Amount];
        public override object GetKey() => typeof(Overheal);
    }

    /// <summary>
    /// Makes characters unable to act. Lasts as long as the StatusEffect it is attached to.
    /// </summary>
    internal class Stun : Subeffect
    {
        public Stun() : base(false)
        {
        }
        private Stun(Stun clone) : this() { }
        public uint Count
        {
            get => (Parent as StatusEffect).Turns;
        }
        //public override double Amount => Count;
        public override object[] GetFormatArgs() => [Count];
        public override object GetKey() => typeof(Stun); // might add stun type like freeze, etc
    }
}
