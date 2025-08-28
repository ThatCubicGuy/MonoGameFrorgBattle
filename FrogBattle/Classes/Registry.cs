using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    public static class Registry
    {
        public static readonly FrozenDictionary<Stats, double> DefaultStats =
            new Dictionary<Stats, double>()
            {
                { Stats.MaxHp, 40000 },
                { Stats.MaxMana, 100 },
                { Stats.MaxEnergy, 120 },
                { Stats.Atk, 1000 },
                { Stats.Def, 300 },
                { Stats.Spd, 100 },
                { Stats.Dex, 0 },
                { Stats.CritRate, 0.1 },
                { Stats.CritDamageBonus, 0.5 },
                { Stats.HitRateBonus, 0 },
                { Stats.EffectHitRate, 1 },
                { Stats.EffectRES, 0 },
                { Stats.ManaCost, 1 },
                { Stats.ManaRegen, 1 },
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
                { Scalars.Obscene, 40.00 },
                { Scalars.Crikey, 50.00 },
                { Scalars.Cringe, 100.00 }
            }
            .ToFrozenDictionary();
    }
}
