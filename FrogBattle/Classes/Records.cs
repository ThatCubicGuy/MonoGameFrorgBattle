using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    public record DamageInfo
    (
        DamageTypes Type = DamageTypes.None,
        DamageSources Source = DamageSources.None,
        double DefenseIgnore = 0,
        double TypeResPen = 0,
        bool CanCrit = true
    );
    public record AbilityInfo
    (
        string Name,
        bool RepeatsTurn
    );
    internal record AttackInfo
    (
        double Ratio,
        Stats Scalar,
        DamageInfo DamageInfo,
        double? HitRate = null,
        bool IndependentHitRate = false,
        uint[] Split = null
    );
    internal record EffectInfo
    (
        StatusEffect AppliedEffect,
        double Chance = 1,
        ChanceTypes ChanceType = ChanceTypes.Fixed
    );
}
