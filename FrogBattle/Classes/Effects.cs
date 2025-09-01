using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal abstract class Subeffect
    {
        public Subeffect(IAttributeModifier parent, bool buff)
        {
            ArgumentNullException.ThrowIfNull(parent, $"{parent}, {buff}, {GetType()}");
            Parent = parent;
            IsBuff = buff;
        }
        public IAttributeModifier Parent { get; }
        /// <summary>
        /// The character that applied this effect's parent.
        /// </summary>
        public Character SourceFighter => Parent.Source;
        /// <summary>
        /// The character to which this effect's parent is applied.
        /// </summary>
        public Character TargetFighter => Parent.Target;
        public bool IsBuff { get; }
        public string TranslationKey { get => "effect.type." + GetType().Name.camelCase(); }
        public virtual string GetLocalizedText() => Localization.Translate(TranslationKey, GetFormatArgs());
        public abstract object[] GetFormatArgs();
        public abstract object GetKey();
    }
    internal class Modifier : Subeffect
    {
        private readonly double _amount;
        public Modifier(IAttributeModifier parent, double amount, Stats stat, Operators op) : base(parent, (amount > 0) == Registry.IsHigherBetter(stat))
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
        public override object[] GetFormatArgs() => [Op == Operators.Additive ? Amount : ((_amount > 0 ? '+' : string.Empty) + (_amount * 100).ToString("P")), Stat];
        public override object GetKey() => (typeof(Modifier), Stat);

    }
    internal class Shield : Subeffect
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
                currentAmount = Math.Min(value, maxAmount);
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
    internal class Barrier : Subeffect
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
                currentCount = Math.Min(value, maxCount);
            }
        }
        public override object[] GetFormatArgs() => [Count];
        public override object GetKey() => typeof(Barrier);
    }
    internal class PerTurnChange : Subeffect
    {
        private readonly double _amount;
        public PerTurnChange(StatusEffect parent, double amount, Pools pool, Operators op) : base(parent, amount > 0)
        {
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
        public override object[] GetFormatArgs() => [Op == Operators.Additive ? Amount : ($"{(_amount > 0 ? '+' : string.Empty)}{_amount * 100:P}"), Pool];
        public override object GetKey() => (typeof(PerTurnChange), Pool);
    }
    internal class DamageOverTime : Subeffect
    {
        private readonly double _amount;
        public DamageOverTime(StatusEffect parent, double amount, Operators op, DamageInfo props) : base(parent, false)
        {
            _amount = amount;
            Op = op;
            DamageProps = props with { CanCrit = false, Source = DamageSources.DamageOverTime };
        }
        /// <summary>
        /// Automatically applies the operator based on the fighter it is attached to.
        /// </summary>
        public double Amount
        {
            get => Op.Apply(_amount, TargetFighter.Hp);
        }
        private Operators Op { get; }
        public DamageInfo DamageProps { get; }
        /// <summary>
        /// Get the damage associated with this DoT instance, optionally at a different ratio.
        /// </summary>
        /// <param name="ratio">The ratio of DoT amount to actual Damage.</param>
        /// <returns></returns>
        public Damage GetDamage() => new(SourceFighter, TargetFighter, Amount, DamageProps);
        public override object[] GetFormatArgs() => [GetDamage().Amount];
        public override object GetKey() => typeof(DamageOverTime);
    }
    internal class HealingOverTime : Subeffect
    {
        private readonly double _amount;
        public HealingOverTime(StatusEffect parent, double amount, Operators op, DamageInfo props) : base(parent, true)
        {
            _amount = amount;
            Op = op;
        }
        /// <summary>
        /// Automatically applies the operator based on the fighter it is attached to.
        /// </summary>
        public double Amount
        {
            get => Op.Apply(_amount, TargetFighter.Hp);
        }
        private Operators Op { get; }
        /// <summary>
        /// Get the damage associated with this DoT instance, optionally at a different ratio.
        /// </summary>
        /// <param name="ratio">The ratio of DoT amount to actual Damage.</param>
        /// <returns></returns>
        public Healing GetHealing() => new(SourceFighter, TargetFighter, Amount);
        public override object[] GetFormatArgs() => [GetHealing().Amount];
        public override object GetKey() => typeof(DamageOverTime);
    }
    /// <summary>
    /// Influences a specific type of damage. Percentage only.
    /// </summary>
    internal class DamageTypeBonus : Subeffect
    {
        private readonly double _amount;
        public DamageTypeBonus(IAttributeModifier parent, double amount, DamageTypes type) : base(parent, amount >= 0)
        {
            _amount = amount;
            Type = type;
        }
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
        public DamageTypeRES(IAttributeModifier parent, double amount, DamageTypes type) : base(parent, amount >= 0)
        {
            _amount = amount;
            Type = type;
        }
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
        public DamageSourceBonus(IAttributeModifier parent, double amount, DamageSources source) : base(parent, amount >= 0)
        {
            _amount = amount;
            Source = source;
        }
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
        public DamageSourceRES(IAttributeModifier parent, double amount, DamageSources source) : base(parent, amount >= 0)
        {
            _amount = amount;
            Source = source;
        }
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
        public DamageBonus(IAttributeModifier parent, double amount) : base(parent, amount >= 0)
        {
            _amount = amount;
        }
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
        public DamageRES(IAttributeModifier parent, double amount) : base(parent, amount >= 0)
        {
            _amount = amount;
        }
        public double Amount
        {
            get => _amount;
        }
        public override object[] GetFormatArgs() => [(Amount > 0 ? '+' : string.Empty) + Amount.ToString("P")];
        public override object GetKey() => typeof(DamageRES);
    }
    internal class Overheal : Subeffect
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
    /// <summary>
    /// Makes characters unable to act. Lasts as long as the StatusEffect it is attached to.
    /// </summary>
    internal class Stun : Subeffect
    {
        public Stun(StatusEffect parent) : base(parent, false)
        {
        }
        public uint Count
        {
            get => (Parent as StatusEffect).Turns;
        }
        public override object[] GetFormatArgs() => [Count];
        public override object GetKey() => typeof(Stun); // might add stun type like freeze, etc
    }
}
