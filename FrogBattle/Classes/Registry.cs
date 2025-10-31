using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace FrogBattle.Classes
{
    internal static class Registry
    {
        public readonly struct CommonPronouns
        {
            public static readonly Pronouns IT_ITS = new("it", "it", "its", "its", "itself");
            public static readonly Pronouns HE_HIM = new("he", "him", "his", "his", "himself");
            public static readonly Pronouns SHE_HER = new("she", "her", "her", "hers", "herself");
            public static readonly Pronouns THEY_THEM = new("they", "them", "their", "theirs", "themself");
            public static readonly Pronouns MULTIPLE = new("they", "them", "their", "theirs", "themselves", false);
        }
        public static readonly FrozenDictionary<Stats, double> DefaultStats =
            new Dictionary<Stats, double>()
            {
                { Stats.MaxHp, 400000 },
                { Stats.MaxMana, 100 },
                { Stats.MaxEnergy, 120 },
                { Stats.Atk, 4000 },
                { Stats.Def, 800 },
                { Stats.Spd, 100 },
                { Stats.Dex, 0 },
                { Stats.CritRate, 0.10 },
                { Stats.CritDamage, 0.50 },
                { Stats.HitRateBonus, 0 },
                { Stats.EffectHitRate, 1 },
                { Stats.EffectRES, 0 },
                { Stats.ManaCost, 1 },
                { Stats.ManaRegen, 1 },
                { Stats.EnergyRecharge, 1 },
                { Stats.IncomingHealing, 1 },
                { Stats.OutgoingHealing, 1 },
                { Stats.ShieldToughness, 1 }
            }
            .ToFrozenDictionary();
        private static readonly FrozenSet<Stats> HigherIsWorse = new HashSet<Stats>()
        {
            Stats.MaxEnergy,
            Stats.ManaCost
        }
        .ToFrozenSet();
        public static bool IsHigherBetter(Stats stat) => !HigherIsWorse.Contains(stat);
        /// <summary>
        /// These are keys for effects whose value is to be changed by external factors during gameplay,
        /// and as such are banned from things like passive effects.
        /// </summary>
        public static readonly FrozenSet<object> VolatileEffectsKeys = new HashSet<object>()
        {
            typeof(Shield),
            typeof(Barrier),
            typeof(Overheal)
        }
        .ToFrozenSet();
        /// <summary>
        /// This is merely a funny little joke that i probably won't use. But who knows !!!
        /// </summary>
        public static readonly FrozenDictionary<Scalars, double> GenericScalars =
            new Dictionary<Scalars, double>()
            {
                { Scalars.Light, 0.70 },
                { Scalars.Medium, 1.00 },
                { Scalars.Heavy, 1.30 },
                { Scalars.Huge, 2.00 },
                { Scalars.Massive, 3.00 },
                { Scalars.Mental, 5.00 },
                { Scalars.Deranged, 7.50 },
                { Scalars.Irrational, 10.00 },
                { Scalars.Outrageous, 15.00 },
                { Scalars.Bonkers, 20.00 },
                { Scalars.Baffling, 30.00 },
                { Scalars.Obscene, 50.00 },
                { Scalars.Crikey, 69.00 },
                { Scalars.Cringe, 100.00 }
            }
            .ToFrozenDictionary();
        public static StatusEffect CommonDoT(DoTTypes type, Character source, Character target, int? amount = null)
        {
            return type switch
            {
                DoTTypes.Bleed => new Bleed() { Source = source, Target = target, BleedAmount = amount }.Init(),
                DoTTypes.Burn => new Burn() { Source = source, Target = target, BurnAmount = amount }.Init(),
                DoTTypes.Shock => new Shock() { Source = source, Target = target, ShockAmount = amount }.Init(),
                DoTTypes.WindShear => new WindShear() { Source = source, Target = target, ShearAmount = amount }.Init(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown DoT type: {type}")
            };
        }
        public static readonly StatusEffect.Flags DoTFlags = StatusEffect.Flags.StartTick | StatusEffect.Flags.Debuff;
        public class Bleed : StatusEffect
        {
            public Bleed() : base()
            {
                Name = "Bleed";
                Turns = 3;
                MaxStacks = 10;
                Properties = DoTFlags;
            }
            public double? BleedAmount { get; init; }
            public override StatusEffect Init() => AddEffect(new DamageOverTime(BleedAmount ?? 0.004 * Target.GetStatVersus(Stats.MaxHp, Source), Operators.AddValue) { Type = DoTTypes.Bleed });
        }
        public class Burn : StatusEffect
        {
            public Burn() : base()
            {
                Name = "Burn";
                Turns = 3;
                MaxStacks = 1;
                Properties = DoTFlags;
            }
            public double? BurnAmount { get; init; }
            public override StatusEffect Init() => AddEffect(new DamageOverTime(BurnAmount ?? 25000 * (BattleManager.RNG / 5 + 0.9), Operators.AddValue) { Type = DoTTypes.Burn });
        }
        public class Shock : StatusEffect
        {
            public Shock() : base()
            {
                Name = "Shock";
                Turns = 3;
                MaxStacks = 1;
                Properties = DoTFlags;
            }
            public double? ShockAmount { get; init; }
            public override StatusEffect Init() => AddEffect(new DamageOverTime(ShockAmount ?? 2.10 * Source.GetStatVersus(Stats.Atk, Target), Operators.AddValue) { Type = DoTTypes.Shock });
        }
        public class WindShear : StatusEffect
        {
            public WindShear() : base()
            {
                Name = "Wind Shear";
                Turns = 3;
                MaxStacks = 5;
                Properties = DoTFlags;
            }
            public double? ShearAmount { get; init; }
            public override StatusEffect Init() => AddEffect(new DamageOverTime(ShearAmount ?? 0.65 * Source.GetStatVersus(Stats.Atk, Target), Operators.AddValue) { Type = DoTTypes.WindShear });
        }
    }
}
