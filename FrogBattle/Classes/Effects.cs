using System;
using System.Collections.Generic;

namespace FrogBattle.Classes
{
    internal sealed class SubeffectInstance : ISubeffectInstance
    {
        private readonly SubeffectDefinition _definition;
        public SubeffectInstance(IAttributeModifier ctx, SubeffectDefinition definition)
        {
            Parent = ctx;
            _definition = definition;
        }

        public IAttributeModifier Parent { get; }
        public SubeffectDefinition Definition => _definition;

        public double Amount
        {
            get => _definition.GetAmount(Parent);
            set
            {
                // Do nothing if we can't set the amount.
                if (_definition is IMutableEffect mt) mt.SetAmount(value);
            }
        }
        public string GetLocalizedText() => _definition.GetLocalizedText(Parent);
        public object GetKey() => _definition.GetKey();

        // Use the following methods only after making sure that is a valid operation. They throw casting errors otherwise.
        public Damage GetDamage() => ((DamageOverTime)_definition).GetDamage((StatusEffectInstance)Parent);
        public Healing GetHealing() => ((HealingOverTime)_definition).GetHealing((StatusEffectInstance)Parent);
    }
    internal abstract class SubeffectDefinition : ISubeffect
    {
        private IAttributeModifier parent;
        public SubeffectDefinition(bool buff)
        {
            IsBuff = buff;
        }

        public IAttributeModifier Parent { get => parent; init => parent = value; }
        public bool IsBuff { get; }
        public string TranslationKey { get => "effect.type." + GetType().Name.FirstLower(); }

        public SubeffectInstance GetInstance(IAttributeModifier ctx) => new(ctx, this);
        public string GetLocalizedText(IAttributeModifier ctx) => Localization.Translate(TranslationKey, GetFormatArgs(ctx));
        /// <summary>
        /// Gets the subeffect's amount based on the context.<br/>
        /// This method does NOT include stacks.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public abstract double GetAmount(IAttributeModifier ctx);
        /// <summary>
        /// Gets translation string format arguments for displaying things.
        /// </summary>
        /// <param name="ctx">The context for which to get the format arguments.</param>
        /// <returns>An array of objects to be passed as translation parameters.</returns>
        public abstract object[] GetFormatArgs(IAttributeModifier ctx);
        public abstract object GetKey();
    }
    internal class Modifier : SubeffectDefinition
    {
        private readonly double _amount;
        public Modifier(double amount, Stats stat, Operators op) : base((amount > 0) == Registry.IsHigherBetter(stat))
        {
            _amount = amount;
            Stat = stat;
            Op = op;
        }
        private Modifier(Modifier clone) : this(clone._amount, clone.Stat, clone.Op) { }

        public Stats Stat { get; init; }
        public Operators Op { get; init; }

        public override double GetAmount(IAttributeModifier ctx) => Op.Apply(_amount, ctx.Target.Base[Stat]);
        public override object[] GetFormatArgs(IAttributeModifier ctx) => [Op == Operators.AddValue ? (GetAmount(ctx) > 0 ? '+' : string.Empty) + GetAmount(ctx).ToString("F0") : ((_amount > 0 ? '+' : string.Empty) + (_amount * 100).ToString("P")), Stat.GetLocalizedText()];
        public override object GetKey() => GetKeyStatic(Stat);
        public double GetBaseAmount() => _amount;
        public static object GetKeyStatic(Stats stat) => (typeof(Modifier), stat);
    }
    internal class Shield : SubeffectDefinition, IMutableEffect
    {
        private double currentAmount;
        public Shield(double amount, DamageTypes? shieldType = null, double? maxAmount = null) : base(true)
        {
            if (amount <= 0 || maxAmount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Shield value must be positive.");
            if (maxAmount.HasValue) MaxAmount = maxAmount.Value;
            ShieldType = shieldType;
        }
        private Shield(Shield clone) : this(clone.currentAmount, clone.ShieldType, clone.MaxAmount) { }

        public double MaxAmount { get; }
        public DamageTypes? ShieldType { get; }

        public void SetAmount(double value) => currentAmount = Math.Min(value, MaxAmount);
        public override double GetAmount(IAttributeModifier ctx) => currentAmount;
        public override object[] GetFormatArgs(IAttributeModifier ctx)
        {
            // need to go through an intermediary step of deserialization before passing resolved string as argument
            var addons = new List<string>();
            if (ShieldType != null) addons.Add(Localization.Translate(TranslationKey + ".addon.type", ShieldType));
            addons.Add(Localization.Translate(TranslationKey + ".addon"));
            var addonText = string.Join(' ', addons);
            return [GetAmount(ctx), addonText];
        }
        public override object GetKey() => typeof(Shield);
        public double GetMaxAmount() => MaxAmount;
    }
    internal class Barrier : SubeffectDefinition, IMutableEffect
    {
        private uint currentCount;
        public Barrier(uint count, uint? maxCount = null) : base(true)
        {
            if (count == 0 || maxCount == 0) throw new ArgumentOutOfRangeException(nameof(count), "Barrier count must be positive.");
            if (maxCount != null) this.MaxCount = maxCount.Value;
            currentCount = count;
        }
        private Barrier(Barrier clone) : this(clone.currentCount, clone.MaxCount) { }

        public uint MaxCount { get; }

        public void SetAmount(double value) => currentCount = (uint)Math.Min(value, MaxCount);
        public override double GetAmount(IAttributeModifier ctx) => currentCount;
        public override object[] GetFormatArgs(IAttributeModifier ctx) => [currentCount];
        public override object GetKey() => typeof(Barrier);
        public uint GetMaxCount() => MaxCount;
    }
    internal class PerTurnChange : SubeffectDefinition
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

        public Pools Pool { get; }
        public Operators Op { get; }

        public override double GetAmount(IAttributeModifier ctx) => Op.Apply(_amount, Pool, ctx.Target);
        public override object[] GetFormatArgs(IAttributeModifier ctx) => [Op == Operators.AddValue ? GetAmount(ctx) : $"{(_amount > 0 ? '+' : string.Empty)}{_amount * 100:P}", Pool];
        public override object GetKey() => (typeof(PerTurnChange), Pool);
        public double GetBaseAmount() => _amount;
    }
    internal class DamageOverTime : SubeffectDefinition
    {
        public static readonly DamageInfo Default = new(CanCrit: false, Source: DamageSources.DamageOverTime);
        private readonly double _amount;
        public DamageOverTime(double amount, Operators op, DamageInfo props = null) : base(false)
        {
            _amount = amount;
            Op = op;
            DamageProps = props ?? Default;
        }
        private DamageOverTime(DamageOverTime clone) : this(clone._amount, clone.Op, clone.DamageProps) { Type = clone.Type; Scalar = clone.Scalar; }

        /// <summary>
        /// The type of DoT. There are four base types, but it can just be none of them. Some characters use this.
        /// </summary>
        public DoTTypes Type { get; init; } = DoTTypes.None;
        /// <summary>
        /// The stat of the source fighter which the damage will base itself off of.
        /// (For instance, Atk).
        /// </summary>
        public Stats Scalar { get; init; } = Stats.None;
        public DamageInfo DamageProps { get; }
        public Operators Op { get; }


        /// <summary>
        /// Get the damage associated with this DoT instance.
        /// </summary>
        /// <returns>An instance of Damage associated with this Damage-over-Time.</returns>
        public Damage GetDamage(StatusEffectInstance ctx) => new(ctx.Source, ctx.Target, (Scalar == Stats.None ? ctx.Target.GetStatVersus(Scalar, ctx.Source) : 1) * GetAmount(ctx) * ctx.Stacks, DamageProps);

        public override double GetAmount(IAttributeModifier ctx) => Op.Apply(_amount, Pools.Hp, ctx.Target);
        public override object[] GetFormatArgs(IAttributeModifier ctx) => [GetDamage((StatusEffectInstance)ctx).Amount];
        public override object GetKey() => typeof(DamageOverTime);
        public double GetBaseAmount() => _amount;
    }
    internal class HealingOverTime : SubeffectDefinition
    {
        private readonly double _amount;
        public HealingOverTime(double amount, Operators op) : base(true)
        {
            _amount = amount;
            Op = op;
        }
        private HealingOverTime(HealingOverTime clone) : this(clone._amount, clone.Op) { }

        /// <summary>
        /// The stat of the source fighter which the healing will base itself off of.
        /// (For instance, MaxHp).
        /// </summary>
        public Stats Scalar { get; init; } = Stats.None;
        public Operators Op { get; }

        /// <summary>
        /// Gets the healing associated with this HoT (lmao) instance.
        /// </summary>
        /// <returns>An instance of Healing associated with this Healing-over-Time.</returns>
        public Healing GetHealing(StatusEffectInstance ctx) => new(ctx.Source, ctx.Target, new((Scalar == Stats.None ? ctx.Target.GetStatVersus(Scalar, ctx.Source) : 1) * GetAmount(ctx) * ctx.Stacks));

        public override double GetAmount(IAttributeModifier ctx) => Op.Apply(_amount, Pools.Hp, ctx.Target) * (Parent as StatusEffectInstance).Stacks;
        public override object[] GetFormatArgs(IAttributeModifier ctx) => [GetHealing((StatusEffectInstance)ctx).Amount];
        public override object GetKey() => typeof(DamageOverTime);
        public double GetBaseAmount() => _amount;
    }

    /// <summary>
    /// Influences a specific type of damage. Percentage only.
    /// </summary>
    internal class DamageTypeBonus : SubeffectDefinition
    {
        private readonly double _amount;
        public DamageTypeBonus(double amount, DamageTypes type) : base(amount >= 0)
        {
            _amount = amount;
            Type = type;
        }
        private DamageTypeBonus(DamageTypeBonus clone) : this(clone._amount, clone.Type) { }

        public DamageTypes Type { get; }

        public override double GetAmount(IAttributeModifier ctx) => _amount;
        public override object[] GetFormatArgs(IAttributeModifier ctx) => [(GetAmount(ctx) > 0 ? '+' : string.Empty) + GetAmount(ctx).ToString("P"), Type];
        public override object GetKey() => GetKeyStatic(Type);
        public static object GetKeyStatic(DamageTypes type) => (typeof(DamageTypeBonus), type);
    }

    /// <summary>
    /// Influences a specific type of damage resistance. Percentage only.
    /// </summary>
    internal class DamageTypeRES : SubeffectDefinition
    {
        private readonly double _amount;
        public DamageTypeRES(double amount, DamageTypes type) : base(amount >= 0)
        {
            _amount = amount;
            Type = type;
        }
        private DamageTypeRES(DamageTypeRES clone) : this(clone._amount, clone.Type) { }

        public DamageTypes Type { get; }

        public override double GetAmount(IAttributeModifier ctx) => _amount;
        public override object[] GetFormatArgs(IAttributeModifier ctx) => [(GetAmount(ctx) > 0 ? '+' : string.Empty) + GetAmount(ctx).ToString("P"), Type];
        public override object GetKey() => GetKeyStatic(Type);
        public static object GetKeyStatic(DamageTypes type) => (typeof(DamageTypeRES), type);
    }

    /// <summary>
    /// Influences damage coming from a specific source. Percentage only.
    /// </summary>
    internal class DamageSourceBonus : SubeffectDefinition
    {
        private readonly double _amount;
        public DamageSourceBonus(double amount, DamageSources source) : base(amount >= 0)
        {
            _amount = amount;
            Source = source;
        }
        private DamageSourceBonus(DamageSourceBonus clone) : this(clone._amount, clone.Source) { }

        public DamageSources Source { get; }

        public override double GetAmount(IAttributeModifier ctx) => _amount;
        public override object[] GetFormatArgs(IAttributeModifier ctx) => [(GetAmount(ctx) > 0 ? '+' : string.Empty) + GetAmount(ctx).ToString("P"), Source];
        public override object GetKey() => GetKeyStatic(Source);
        public static object GetKeyStatic(DamageSources source) => (typeof(DamageSourceBonus), source);
    }

    /// <summary>
    /// Influences damage resistance against a specific source. Percentage only.
    /// </summary>
    internal class DamageSourceRES : SubeffectDefinition
    {
        private readonly double _amount;
        public DamageSourceRES(double amount, DamageSources source) : base(amount >= 0)
        {
            _amount = amount;
            Source = source;
        }
        private DamageSourceRES(DamageSourceRES clone) : this(clone._amount, clone.Source) { }

        public DamageSources Source { get; }

        public override double GetAmount(IAttributeModifier ctx) => _amount;
        public override object[] GetFormatArgs(IAttributeModifier ctx) => [(GetAmount(ctx) > 0 ? '+' : string.Empty) + GetAmount(ctx).ToString("P"), Source];
        public override object GetKey() => GetKeyStatic(Source);
        public static object GetKeyStatic(DamageSources source) => (typeof(DamageSourceRES), source);
    }

    /// <summary>
    /// Influences damage dealt, positive values are good. Percentage only.
    /// </summary>
    internal class DamageBonus : SubeffectDefinition
    {
        private readonly double _amount;
        public DamageBonus(double amount) : base(amount >= 0)
        {
            _amount = amount;
        }
        private DamageBonus(DamageBonus clone) : this(clone._amount) { }

        public override double GetAmount(IAttributeModifier ctx) => _amount;
        public override object[] GetFormatArgs(IAttributeModifier ctx) => [(GetAmount(ctx) > 0 ? '+' : string.Empty) + GetAmount(ctx).ToString("P")];
        public override object GetKey() => GetKeyStatic();
        public static object GetKeyStatic() => typeof(DamageBonus);
    }

    /// <summary>
    /// Influences damage taken, positive values are good. Percentage only.
    /// </summary>
    internal class DamageRES : SubeffectDefinition
    {
        private readonly double _amount;
        public DamageRES(double amount) : base(amount >= 0)
        {
            _amount = amount;
        }
        private DamageRES(DamageRES clone) : this(clone._amount) { }

        public override double GetAmount(IAttributeModifier ctx) => _amount;
        public override object[] GetFormatArgs(IAttributeModifier ctx) => [(GetAmount(ctx) > 0 ? '+' : string.Empty) + GetAmount(ctx).ToString("P")];
        public override object GetKey() => GetKeyStatic();
        public static object GetKeyStatic() => typeof(DamageRES);
    }

    /// <summary>
    /// Temporary HP. Gets consumed before normal HP, and parent is generally removed upon reaching zero.
    /// </summary>
    internal class Overheal : SubeffectDefinition, IMutableEffect
    {
        private double currentAmount;
        public Overheal(double amount) : base(true)
        {
            currentAmount = amount;
        }
        private Overheal(Overheal clone) : this(clone.currentAmount) { }

        public void SetAmount(double value)
        {
            currentAmount = value;
        }
        public override double GetAmount(IAttributeModifier ctx) => currentAmount;
        public override object[] GetFormatArgs(IAttributeModifier ctx) => [GetAmount(ctx)];
        public override object GetKey() => typeof(Overheal);
    }

    /// <summary>
    /// Makes characters unable to act. Lasts as long as the StatusEffect it is attached to.
    /// </summary>
    internal class Stun : SubeffectDefinition
    {
        public Stun() : base(false)
        {
        }
        private Stun(Stun clone) : this() { }

        public uint Count
        {
            get => (Parent as StatusEffectInstance).Turns;
        }

        public override double GetAmount(IAttributeModifier ctx) => Count;
        public override object[] GetFormatArgs(IAttributeModifier ctx) => [Count];
        public override object GetKey() => typeof(Stun); // might add stun type like freeze, etc
    }
}
