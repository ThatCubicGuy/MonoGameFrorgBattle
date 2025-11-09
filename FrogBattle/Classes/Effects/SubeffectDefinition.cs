using System;
using System.Collections.Generic;

namespace FrogBattle.Classes.Effects
{
    internal abstract record class SubeffectDefinition : ISubeffect
    {
        public SubeffectDefinition(bool buff)
        {
            IsBuff = buff;
        }

        public bool IsBuff { get; }
        public string TranslationKey { get => "effect.type." + GetType().Name.FirstLower(); }

        public ISubeffectInstance GetInstance(IAttributeModifier ctx) => this is IMutableEffect mt ? new MutableSubeffectInstance(ctx, mt) : new StaticSubeffectInstance(ctx, this);
        public string GetLocalizedText(ISubeffectInstance ctx) => Localization.Translate(TranslationKey, GetFormatArgs(ctx));
        /// <summary>
        /// Gets the subeffect's amount based on the context.<br/>
        /// This method does NOT include stacks.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public abstract double GetAmount(ISubeffectInstance ctx);
        /// <summary>
        /// Gets translation string format arguments for displaying things.
        /// </summary>
        /// <param name="ctx">The context for which to get the format arguments.</param>
        /// <returns>An array of objects to be passed as translation parameters.</returns>
        public abstract object[] GetFormatArgs(ISubeffectInstance ctx);
        public abstract object GetKey();
    }
    internal record class Modifier : SubeffectDefinition
    {
        private readonly double _amount;
        public Modifier(double amount, Stats stat, Operators op) : base(amount > 0 == Registry.IsHigherBetter(stat))
        {
            _amount = amount;
            Stat = stat;
            Op = op;
        }

        public Stats Stat { get; init; }
        public Operators Op { get; init; }

        public override double GetAmount(ISubeffectInstance ctx) => Op.Apply(_amount, ctx.Target.Base[Stat]);
        public override object[] GetFormatArgs(ISubeffectInstance ctx) => [Op == Operators.AddValue ? (GetAmount(ctx) > 0 ? '+' : string.Empty) + GetAmount(ctx).ToString("F0") : (_amount > 0 ? '+' : string.Empty) + (_amount * 100).ToString("P"), Stat.GetLocalizedText()];
        public override object GetKey() => GetKeyStatic(Stat);
        public double GetBaseAmount() => _amount;
        public static object GetKeyStatic(Stats stat) => (typeof(Modifier), stat);
    }
    internal record class Shield : SubeffectDefinition, IMutableEffect
    {
        public Shield(double amount, double? maxAmount = null) : base(true)
        {
            if (amount <= 0 || maxAmount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Shield value must be positive.");
            if (maxAmount.HasValue) MaxAmount = maxAmount.Value;
        }
        public Shield(double amount, Stats scalar, Operators op, double? maxAmount = null) : this(amount, maxAmount)
        {
            Scalar = scalar;
            Op = op;
        }

        public double BaseAmount { get; }
        public double MaxAmount { get; }
        public DamageTypes ShieldType { get; init; }
        public Stats Scalar { get; } = Stats.None;
        public Operators Op { get; }

        public double GetMaxAmount(ISubeffectInstance ctx) => Scalar == Stats.None ? MaxAmount : Op.Apply(MaxAmount, ctx.Source.GetStatVersus(Scalar, ctx.Target));
        public override double GetAmount(ISubeffectInstance ctx) => Scalar == Stats.None ? BaseAmount : Op.Apply(BaseAmount, ctx.Source.GetStatVersus(Scalar, ctx.Target));
        public override object[] GetFormatArgs(ISubeffectInstance ctx)
        {
            // need to go through an intermediary step of deserialization before passing resolved string as argument
            var addons = new List<string>();
            if (ShieldType != DamageTypes.None) addons.Add(Localization.Translate(TranslationKey + ".addon.type", ShieldType));
            addons.Add(Localization.Translate(TranslationKey + ".addon"));
            var addonText = string.Join(' ', addons);
            return [GetAmount(ctx), addonText];
        }
        public override object GetKey() => typeof(Shield);
    }
    internal record class Barrier : SubeffectDefinition, IMutableEffect
    {
        public Barrier(uint count, uint? maxCount = null) : base(true)
        {
            if (count == 0 || maxCount == 0) throw new ArgumentOutOfRangeException(nameof(count), "Barrier count must be positive.");
            if (maxCount != null) MaxCount = maxCount.Value;
            BaseCount = count;
        }
        public uint BaseCount { get; }
        public uint MaxCount { get; }

        double IMutableEffect.BaseAmount => BaseCount;
        double IMutableEffect.MaxAmount => MaxCount;

        public double GetMaxAmount(ISubeffectInstance ctx) => MaxCount;
        public override double GetAmount(ISubeffectInstance ctx) => BaseCount;
        public override object[] GetFormatArgs(ISubeffectInstance ctx) => [BaseCount];
        public override object GetKey() => typeof(Barrier);
    }
    internal record class PerTurnChange : SubeffectDefinition
    {
        private readonly double _amount;
        public PerTurnChange(double amount, Pools pool, Operators op) : base(amount > 0)
        {
            if (pool == Pools.Hp) throw new ArgumentOutOfRangeException(nameof(pool), "Cannot use PerTurnChange for HP changes. Use DamageOverTime or HealingOverTime instead.");
            _amount = amount;
            Pool = pool;
            Op = op;
        }

        public Pools Pool { get; }
        public Operators Op { get; }

        public override double GetAmount(ISubeffectInstance ctx) => Op.Apply(_amount, Pool, ctx.Parent.Target);
        public override object[] GetFormatArgs(ISubeffectInstance ctx) => [Op == Operators.AddValue ? GetAmount(ctx) : $"{(_amount > 0 ? '+' : string.Empty)}{_amount * 100:P}", Pool];
        public override object GetKey() => (typeof(PerTurnChange), Pool);
        public double GetBaseAmount() => _amount;
    }
    internal record class DamageOverTime : SubeffectDefinition
    {
        public static readonly DamageInfo Default = new(CanCrit: false, Source: DamageSources.DamageOverTime);
        private readonly double _amount;
        public DamageOverTime(double amount, Operators op, DamageInfo damageProps = null) : base(false)
        {
            _amount = amount;
            Op = op;
            DamageProps = damageProps ?? Default;
        }
        public DamageOverTime(double amount, Operators op, Stats scalar, Operators scalarOp, DamageInfo damageProps = null) : this(amount, op, damageProps)
        {
            Scalar = scalar;
            ScalarOp = scalarOp;
        }


        /// <summary>
        /// The type of DoT. There are four base types, but it can just be none of them. Some characters use this.
        /// </summary>
        public DoTTypes Type { get; init; } = DoTTypes.None;
        /// <summary>
        /// The stat of the source fighter which the damage will base itself off of.
        /// (For instance, Atk).
        /// </summary>
        public Stats Scalar { get; } = Stats.None;
        public Operators ScalarOp { get; }
        public DamageInfo DamageProps { get; }
        public Operators Op { get; }

        /// <summary>
        /// Get the damage associated with this DoT instance.
        /// </summary>
        /// <returns>An instance of Damage associated with this Damage-over-Time.</returns>
        public Damage GetDamage(ISubeffectInstance ctx) => new(ctx.Source, ctx.Target, (Scalar == Stats.None ? ctx.Target.GetStatVersus(Scalar, ctx.Source) : 1) * GetAmount(ctx) * ((StatusEffectInstance)ctx.Parent).Stacks, DamageProps);

        public override double GetAmount(ISubeffectInstance ctx) => Op.Apply((Scalar == Stats.None) ? _amount : ScalarOp.Apply(_amount, ctx.Source.GetStatVersus(Scalar, ctx.Target)), Pools.Hp, ctx.Target);
        public override object[] GetFormatArgs(ISubeffectInstance ctx) => [GetDamage(ctx).Amount];
        public override object GetKey() => typeof(DamageOverTime);
        public double GetBaseAmount() => _amount;
    }
    internal record class HealingOverTime : SubeffectDefinition
    {
        private readonly double _amount;
        public HealingOverTime(double amount, Operators op) : base(true)
        {
            _amount = amount;
            Op = op;
        }

        /// <summary>
        /// The stat of the source fighter which the healing will base itself off of.
        /// (For instance, MaxHp).
        /// </summary>
        public Stats Scalar { get; } = Stats.None;
        public Operators ScalarOp { get; }
        public Operators Op { get; }

        /// <summary>
        /// Gets the healing associated with this HoT (lmao) instance.
        /// </summary>
        /// <returns>An instance of Healing associated with this Healing-over-Time.</returns>
        public Healing GetHealing(ISubeffectInstance ctx) => new(ctx.Source, ctx.Target, new((Scalar == Stats.None ? ctx.Target.GetStatVersus(Scalar, ctx.Source) : 1) * GetAmount(ctx) * ((StatusEffectInstance)ctx.Parent).Stacks));

        public override double GetAmount(ISubeffectInstance ctx) => Op.Apply((Scalar == Stats.None) ? _amount : ScalarOp.Apply(_amount, ctx.Source.GetStatVersus(Scalar, ctx.Target)), Pools.Hp, ctx.Target);
        public override object[] GetFormatArgs(ISubeffectInstance ctx) => [GetHealing(ctx).Amount];
        public override object GetKey() => typeof(DamageOverTime);
        public double GetBaseAmount() => _amount;
    }

    /// <summary>
    /// Influences a specific type of damage. Percentage only.
    /// </summary>
    internal record class DamageTypeBonus : SubeffectDefinition
    {
        private readonly double _amount;
        public DamageTypeBonus(double amount, DamageTypes type) : base(amount >= 0)
        {
            _amount = amount;
            Type = type;
        }

        public DamageTypes Type { get; }

        public override double GetAmount(ISubeffectInstance ctx) => _amount;
        public override object[] GetFormatArgs(ISubeffectInstance ctx) => [(GetAmount(ctx) > 0 ? '+' : string.Empty) + GetAmount(ctx).ToString("P"), Type];
        public override object GetKey() => GetKeyStatic(Type);
        public static object GetKeyStatic(DamageTypes type) => (typeof(DamageTypeBonus), type);
    }

    /// <summary>
    /// Influences a specific type of damage resistance. Percentage only.
    /// </summary>
    internal record class DamageTypeRES : SubeffectDefinition
    {
        private readonly double _amount;
        public DamageTypeRES(double amount, DamageTypes type) : base(amount >= 0)
        {
            _amount = amount;
            Type = type;
        }

        public DamageTypes Type { get; }

        public override double GetAmount(ISubeffectInstance ctx) => _amount;
        public override object[] GetFormatArgs(ISubeffectInstance ctx) => [(GetAmount(ctx) > 0 ? '+' : string.Empty) + GetAmount(ctx).ToString("P"), Type];
        public override object GetKey() => GetKeyStatic(Type);
        public static object GetKeyStatic(DamageTypes type) => (typeof(DamageTypeRES), type);
    }

    /// <summary>
    /// Influences damage coming from a specific source. Percentage only.
    /// </summary>
    internal record class DamageSourceBonus : SubeffectDefinition
    {
        private readonly double _amount;
        public DamageSourceBonus(double amount, DamageSources source) : base(amount >= 0)
        {
            _amount = amount;
            Source = source;
        }

        public DamageSources Source { get; }

        public override double GetAmount(ISubeffectInstance ctx) => _amount;
        public override object[] GetFormatArgs(ISubeffectInstance ctx) => [(GetAmount(ctx) > 0 ? '+' : string.Empty) + GetAmount(ctx).ToString("P"), Source];
        public override object GetKey() => GetKeyStatic(Source);
        public static object GetKeyStatic(DamageSources source) => (typeof(DamageSourceBonus), source);
    }

    /// <summary>
    /// Influences damage resistance against a specific source. Percentage only.
    /// </summary>
    internal record class DamageSourceRES : SubeffectDefinition
    {
        private readonly double _amount;
        public DamageSourceRES(double amount, DamageSources source) : base(amount >= 0)
        {
            _amount = amount;
            Source = source;
        }

        public DamageSources Source { get; }

        public override double GetAmount(ISubeffectInstance ctx) => _amount;
        public override object[] GetFormatArgs(ISubeffectInstance ctx) => [(GetAmount(ctx) > 0 ? '+' : string.Empty) + GetAmount(ctx).ToString("P"), Source];
        public override object GetKey() => GetKeyStatic(Source);
        public static object GetKeyStatic(DamageSources source) => (typeof(DamageSourceRES), source);
    }

    /// <summary>
    /// Influences damage dealt, positive values are good. Percentage only.
    /// </summary>
    internal record class DamageBonus : SubeffectDefinition
    {
        private readonly double _amount;
        public DamageBonus(double amount) : base(amount >= 0)
        {
            _amount = amount;
        }

        public override double GetAmount(ISubeffectInstance ctx) => _amount;
        public override object[] GetFormatArgs(ISubeffectInstance ctx) => [(GetAmount(ctx) > 0 ? '+' : string.Empty) + GetAmount(ctx).ToString("P")];
        public override object GetKey() => GetKeyStatic();
        public static object GetKeyStatic() => typeof(DamageBonus);
    }

    /// <summary>
    /// Influences damage taken, positive values are good. Percentage only.
    /// </summary>
    internal record class DamageRES : SubeffectDefinition
    {
        private readonly double _amount;
        public DamageRES(double amount) : base(amount >= 0)
        {
            _amount = amount;
        }

        public override double GetAmount(ISubeffectInstance ctx) => _amount;
        public override object[] GetFormatArgs(ISubeffectInstance ctx) => [(GetAmount(ctx) > 0 ? '+' : string.Empty) + GetAmount(ctx).ToString("P")];
        public override object GetKey() => GetKeyStatic();
        public static object GetKeyStatic() => typeof(DamageRES);
    }

    /// <summary>
    /// Temporary HP. Gets consumed before normal HP, and parent is generally removed upon reaching zero.
    /// </summary>
    internal record class Overheal : SubeffectDefinition, IMutableEffect
    {
        public Overheal(double amount, double maxAmount) : base(true)
        {
            BaseAmount = amount;
            MaxAmount = maxAmount;
        }

        public double BaseAmount { get; }
        public double MaxAmount { get; }
        /// <summary>
        /// The stat of the source fighter which the overheal will base itself off of.
        /// (For instance, MaxHp).
        /// </summary>
        public Stats Scalar { get; } = Stats.None;
        public Operators Op { get; }

        public double GetMaxAmount(ISubeffectInstance ctx) => Scalar == Stats.None ? MaxAmount : Op.Apply(MaxAmount, ctx.Source.GetStatVersus(Scalar, ctx.Target));
        public override double GetAmount(ISubeffectInstance ctx) => Scalar == Stats.None ? BaseAmount : Op.Apply(BaseAmount, ctx.Source.GetStatVersus(Scalar, ctx.Target));
        public override object[] GetFormatArgs(ISubeffectInstance ctx) => [GetAmount(ctx)];
        public override object GetKey() => typeof(Overheal);
    }

    /// <summary>
    /// Makes characters unable to act. Lasts as long as the StatusEffect it is attached to.
    /// </summary>
    internal record class Stun : SubeffectDefinition
    {
        public Stun() : base(false)
        {
        }

        public override double GetAmount(ISubeffectInstance ctx) => ((StatusEffectInstance)ctx).Turns;
        public override object[] GetFormatArgs(ISubeffectInstance ctx) => [GetAmount(ctx)];
        public override object GetKey() => typeof(Stun); // might add stun type like freeze, etc
    }
}
