using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    public record Pronouns
    (
        string Subjective,
        string Objective,
        string Attributive,
        string Absolute,
        string Reflexive,
        bool Extra_S
    )
    {
        public string[] PronArray => [Subjective, Objective, Attributive, Absolute, Reflexive];
    }
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
        bool RepeatsTurn = false
    );
    internal record AttackInfo
    (
        double Ratio = 0,
        Stats Scalar = Stats.None,
        DamageInfo DamageInfo = null,
        double? HitRate = null,
        bool IndependentHitRate = false,
        uint[] Split = null
    );
    internal record EffectInfo
    (
        StatusEffect AppliedEffect = null,
        double Chance = 1,
        ChanceTypes ChanceType = ChanceTypes.Fixed
    );
    internal record InstaAction(Ability Action) : ITakesAction
    {
        public void TakeAction()
        {
            Action.TryUse();
        }
    }
}
