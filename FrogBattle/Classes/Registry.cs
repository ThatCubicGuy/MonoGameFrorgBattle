using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static FrogBattle.Classes.Registry;

namespace FrogBattle.Classes
{
    internal static class Registry
    {
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
        public static StatusEffect CommonDoT(DoTTypes type, Character source, Character target)
        {
            return type switch
            {
                DoTTypes.Bleed => new Bleed(source, target),
                DoTTypes.Burn => new Burn(source, target),
                DoTTypes.Shock => new Shock(source, target),
                DoTTypes.WindShear => new WindShear(source, target),
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown DoT type: {type}")
            };
        }
        public static readonly StatusEffect.Flags DoTFlags = StatusEffect.Flags.StartTick | StatusEffect.Flags.Debuff;
        public static readonly DamageInfo DoTProps = new(Source: DamageSources.DamageOverTime, CanCrit: false);
        public class Bleed : StatusEffect
        {
            public double Amount
            {
                // Default
                get => 0.004 * Target.GetStatVersus(Stats.MaxHp, Source);
                // Only way I could find to override lmao
                init => AddEffect(new DamageOverTime(this, value, Operators.Additive, DoTProps) { Type = DoTTypes.Bleed });
            }
            public Bleed(Character source, Character target) : base(source, target, 3, 10, DoTFlags)
            {
                AddEffect(new DamageOverTime(this, Amount, Operators.Additive, DoTProps) { Type = DoTTypes.Bleed });
                Name = "Bleed";
            }
        }
        public class Burn : StatusEffect
        {
            public double Amount
            {
                // Default
                get => 25000 * (BattleManager.RNG / 5 + 0.9);
                // Only way I could find to override lmao
                init => AddEffect(new DamageOverTime(this, value, Operators.Additive, DoTProps) { Type = DoTTypes.Bleed });
            }
            public Burn(Character source, Character target) : base(source, target, 3, 1, DoTFlags)
            {
                AddEffect(new DamageOverTime(this, Amount, Operators.Additive, DoTProps) { Type = DoTTypes.Burn });
                Name = "Burn";
            }
        }
        public class Shock : StatusEffect
        {
            public double Amount
            {
                // Default
                get => 2.10 * Source.GetStatVersus(Stats.Atk, Target);
                // Only way I could find to override lmao
                init => AddEffect(new DamageOverTime(this, value, Operators.Additive, DoTProps) { Type = DoTTypes.Bleed });
            }
            public Shock(Character source, Character target) : base(source, target, 3, 1, DoTFlags)
            {
                AddEffect(new DamageOverTime(this, Amount, Operators.Additive, DoTProps) { Type = DoTTypes.Shock });
                Name = "Shock";
            }
        }
        public class WindShear : StatusEffect
        {
            public double Amount
            {
                // Default
                get => 0.65 * Source.GetStatVersus(Stats.Atk, Target);
                // Only way I could find to override lmao
                init => AddEffect(new DamageOverTime(this, value, Operators.Additive, DoTProps) { Type = DoTTypes.Bleed });
            }
            public WindShear(Character source, Character target) : base(source, target, 3, 5, DoTFlags)
            {
                AddEffect(new DamageOverTime(this, Amount, Operators.Additive, DoTProps) { Type = DoTTypes.WindShear });
                Name = "Wind Shear";
            }
        }
    }
}
