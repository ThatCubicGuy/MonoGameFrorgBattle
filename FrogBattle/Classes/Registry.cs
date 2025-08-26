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
                { Stats.HitRate, 0 },
                { Stats.EffectHitRate, 1 },
                { Stats.EffectRES, 0 },
                { Stats.AllTypeRES, 0 },
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
    }
}
