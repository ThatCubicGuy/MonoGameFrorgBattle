using FrogBattle.Classes;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static FrogBattle.Classes.StatusEffect;

namespace FrogBattle.Characters
{
    internal class Rexulti : Character
    {
        public Rexulti(string name, BattleManager battle, bool team) : base(name, battle, team)
        {
        }

        public override Ability SelectAbility(Character target, int selector)
        {
            return selector switch
            {
                0 => new SkipTurn(this),
                1 => new Pathetic(this, target),
                _ => throw new ArgumentOutOfRangeException($"Invalid ability number: {selector}")
            };
        }

        public class Pathetic : SingleTargetAttack
        {
            public class Bleed : StatusEffect
            {
                public Bleed(Character source, Character target) : base(source, target, 3, 2, Flags.Debuff)
                {
                    AddEffect(new DamageOverTime(this, 0.96 * source.GetStat(Stats.Atk), Operators.Additive, new()));
                    Name = "Bleed";
                }
            }
            public static DamageInfo DamageProps => new DamageInfo() with
            {
                Type = DamageTypes.Blunt,
                Source = DamageSources.Attack
            };
            public Pathetic(Character source, Character target) : base(source, target, new(typeof(Pathetic).Name, false),
                new(Ratio: 1.1, Scalar: Stats.Atk, DamageInfo: DamageProps, HitRate: 1, IndependentHitRate: false, Split: []), new(new Bleed(source, target)))
            {
                WithGenericManaCost(15);
            }
        }
        public class ShadowFlare : BounceAttack
        {
            public static DamageInfo DamageProps => new DamageInfo() with
            {
                Type = DamageTypes.Magic,
                Source = DamageSources.Attack
            };
            public ShadowFlare(Character source, Character target) : base(source, target, new(typeof(ShadowFlare).Name, false),
                count: (uint)Math.Floor(BattleManager.RNG * 5 + 1),
                new(Ratio: 2.00, Scalar: Stats.Atk, DamageInfo: DamageProps, HitRate: 1))
            {
                WithGenericManaCost(20);
            }
        }
    }
}
