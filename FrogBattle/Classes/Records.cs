using FrogBattle.Classes.BattleManagers;
using FrogBattle.Classes.Effects;
using System;
using System.IO;

namespace FrogBattle.Classes
{
    public record Pronouns
    (
        string Subjective,
        string Objective,
        string Attributive,
        string Absolute,
        string Reflexive,
        bool Singular = true
    )
    {
        public string[] PronArray => [Subjective, Objective, Attributive, Absolute, Reflexive];
    }
    internal record DamageInfo
    (
        //Stats Scalar = Stats.None,
        DamageTypes Type = DamageTypes.None,
        DamageSources Source = DamageSources.None,
        double DefenseIgnore = 0,
        double TypeResPen = 0,
        bool CanCrit = true
    );
    internal record HealingInfo
    (
        double Amount,
        uint DebuffCleanse = 0
    );
    /// <summary>
    /// Information about an ability.
    /// </summary>
    /// <param name="Owner">The character who declares this ability.</param>
    internal record AbilityInfo
    (
        Type Owner
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
    internal record EffectInfo(StatusEffectDefinition AppliedEffect, double Chance = 1, ChanceTypes ChanceType = ChanceTypes.Fixed)
    {
        public StatusEffectInstance Apply(Character source, Character target)
        {
            if (target == null) return null;
            if (BattleManager.RNG < (ChanceType switch
            {
                ChanceTypes.Fixed => Chance,
                // Base chance takes into account your EHR and the enemy's EffectRES
                ChanceTypes.Base => Chance + source.GetStatVersus(Stats.EffectHitRate, target) - target.GetStatVersus(Stats.EffectRES, source),
                _ => throw new InvalidDataException($"Unknown chance type: {ChanceType}")
            })) return AppliedEffect.GetInstance(source, target);
            return null;
        }
    }

    internal record class UserTargetWrapper(Character Source, Character Target) : ISourceTargetContext;
}
