using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrogBattle.Classes;

namespace FrogBattle.Characters
{
    internal class Snake : Character
    {
        public Snake(string name, BattleManager battle, bool team) : base(name, battle, team)
        {
            Pronouns = new("he", "him", "his", "his", "himself", true);
        }

        public override Ability SelectAbility(Character target, int selector)
        {
            return selector switch
            {
                0 => new SkipTurn(this),
                1 => new ThrowGrenade(this, target),
                _ => throw new ArgumentOutOfRangeException($"Invalid ability number: {selector}")
            };
        }
        #region Abilities
        public class ThrowGrenade : BlastAttack
        {
            public static AbilityInfo AbilityProps => new AbilityInfo() with
            {
                
            };
            public static AttackInfo AttackProps => new AttackInfo() with
            {
                Ratio = 1.15,
                Scalar = Stats.Atk,
                HitRate = 0.80,
                IndependentHitRate = false,
                DamageInfo = DamageProps
            };
            public static DamageInfo DamageProps => new DamageInfo() with
            {
                Type = DamageTypes.Blast,
                Source = DamageSources.Attack,
                DefenseIgnore = 0,
                TypeResPen = 0,
                CanCrit = true
            };
            public ThrowGrenade(Character source, Character target) : base(source, target, AbilityProps, AttackProps, null,
                falloff: 0.5)
            {
                WithGenericManaCost(13);
            }
        }
        #endregion
    }
}
