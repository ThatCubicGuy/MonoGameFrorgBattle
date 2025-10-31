using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FrogBattle.Classes.Ability;

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
    /// Information related to the current ability.
    /// </summary>
    /// <param name="Costs">Pool changes that execute after the ability is checked, but before it is launched.</param>
    /// <param name="Rewards">Pool changes only execute if the ability is launched successfully (e.g. not a miss).</param>
    /// <param name="Conditions">Requirements for launching the ability. These are not taxed.</param>
    /// <param name="AdditionalEffects">Additional effects at the end of the ability.</param>
    internal record AbilityInfo
    (
        // lmao literally nothing rn
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
    internal record EffectInfo<AppliedEffect>(double Chance = 1, ChanceTypes ChanceType = ChanceTypes.Fixed) : EffectInfo(Chance, ChanceType)
        where AppliedEffect : StatusEffect, new()
    {
        public override StatusEffect Apply(Character source, Character target)
        {
            if (target == null) return null;
            if (BattleManager.RNG < (ChanceType switch
            {
                ChanceTypes.Fixed => Chance,
                // Base chance takes into account your EHR and the enemy's EffectRES
                ChanceTypes.Base => Chance + source.GetStatVersus(Stats.EffectHitRate, target) - target.GetStatVersus(Stats.EffectRES, source),
                _ => throw new InvalidDataException($"Unknown chance type: {ChanceType}")
            })) return new AppliedEffect() { Source = source, Target = target }.Init();
            return null;
        }
    }
    internal abstract record class EffectInfo(double Chance = 1, ChanceTypes ChanceType = ChanceTypes.Fixed)
    {
        public abstract StatusEffect Apply(Character source, Character target);
    }
}
