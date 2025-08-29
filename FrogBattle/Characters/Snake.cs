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

        public class ThrowGrenade : BlastAttack
        {
            public static DamageInfo DamageProps => new DamageInfo() with
            {
                Type = DamageTypes.Blast,
                Source = DamageSources.Attack,
                DefenseIgnore = 0,
                TypeResPen = 0,
                CanCrit = true
            };
            public ThrowGrenade(Character source, Character target) : base(source, target, new(typeof(ThrowGrenade).Name, false),
                falloff: 0.5,
                new(Ratio: 1.15, Scalar: Stats.Atk, HitRate: 0.80, IndependentHitRate: false, DamageInfo: DamageProps, Split: [])) {
                WithGenericManaCost(13);
            }
        }
    }
}
