using FrogBattle.Classes.BattleManagers;
using FrogBattle.Classes.Effects;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

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
        public static StatusEffectDefinition CommonDoT(DoTTypes type, double? amount = null)
        {
            return amount is null ? type switch
            {
                DoTTypes.Bleed => new Bleed(),
                DoTTypes.Burn => new Burn(),
                DoTTypes.Shock => new Shock(),
                DoTTypes.WindShear => new WindShear(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown DoT type: {type}")
            } : type switch
            {
                DoTTypes.Bleed => new Bleed(amount.Value),
                DoTTypes.Burn => new Burn(amount.Value),
                DoTTypes.Shock => new Shock(amount.Value),
                DoTTypes.WindShear => new WindShear(amount.Value),
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown DoT type: {type}")
            };
        }
        public static readonly EffectFlags DoTFlags = EffectFlags.StartTick | EffectFlags.Debuff;
        public record class Bleed : StatusEffectDefinition
        {
            public Bleed(double bleedAmount = 0.004) : base(new DamageOverTime(bleedAmount, Operators.MultiplyBase) { Type = DoTTypes.Bleed })
            {
                Name = "Bleed";
                BaseTurns = 3;
                MaxStacks = 10;
                Properties = DoTFlags;
                BleedAmount = bleedAmount;
            }
            public double BleedAmount { get; }
        }
        public record class Burn : StatusEffectDefinition
        {
            public Burn(double burnAmount = 25000) : base(new DamageOverTime(burnAmount * (BattleManager.RNG / 5 + 0.9), Operators.AddValue) { Type = DoTTypes.Burn })
            {
                Name = "Burn";
                BaseTurns = 3;
                MaxStacks = 1;
                Properties = DoTFlags;
                BurnAmount = burnAmount;
            }
            public double BurnAmount { get; }
        }
        public record class Shock : StatusEffectDefinition
        {
            public Shock(double shockAmount = 2.10) : base(new DamageOverTime(shockAmount, Operators.AddValue, Stats.Atk, Operators.MultiplyBase) { Type = DoTTypes.Shock })
            {
                Name = "Shock";
                BaseTurns = 3;
                MaxStacks = 1;
                Properties = DoTFlags;
            }
            public double ShockAmount { get; }
        }
        public record class WindShear : StatusEffectDefinition
        {
            public WindShear(double shearAmount = 0.65) : base(new DamageOverTime(shearAmount, Operators.AddValue, Stats.Atk, Operators.MultiplyBase) { Type = DoTTypes.WindShear })
            {
                Name = "Wind Shear";
                BaseTurns = 3;
                MaxStacks = 5;
                Properties = DoTFlags;
            }
            public double ShearAmount { get; }
        }
    }
}
