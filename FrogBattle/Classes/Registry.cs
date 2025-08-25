using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    public static class Registry
    {
        public static readonly Dictionary<Stats, double> DefaultStats =
            new Dictionary<Stats, double>()
            {
                { Stats.MaxHp, 40000 },
                { Stats.MaxMana, 100 },
                { Stats.MaxEnergy, 120 },
                { Stats.Atk, 1000 },
                { Stats.Def, 300 },
                { Stats.Spd, 100 },
                { Stats.Dex, 0 },
                { Stats.HitRate, 0 },
                { Stats.EffectHitRate, 1 },
                { Stats.EffectRES,0 },
                { Stats.AllTypeRES, 0 },
                { Stats.ManaCost, 1 },
                { Stats.ManaRegen, 1 },
                { Stats.IncomingHealing, 1 },
                { Stats.OutgoingHealing, 1 },
                { Stats.ShieldToughness, 1 }
            };
        private static readonly HashSet<Stats> HigherIsWorse =
        [
            Stats.MaxEnergy,
            Stats.ManaCost
        ];
        public static bool IsHigherBetter(Stats stat) => !HigherIsWorse.Contains(stat);
    }
}
