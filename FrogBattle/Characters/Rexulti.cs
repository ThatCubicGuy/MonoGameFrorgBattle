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
        // Passives
        private PassiveEffect CritRateAndDamageBoost(Character parent) => new(parent)
        {
            ConditionFulfill = delegate (Character[] targets) { return targets.SelectMany(x => x.GetActives<DamageOverTime>()).Count(); }
            
        };

        public Rexulti(string name, BattleManager battle, bool team) : base(name, battle, team)
        {
            Pronouns = new("he", "him", "his", "his", "himself", true);
            PassiveEffects.Add(CritRateAndDamageBoost(this));
        }

        public override Ability SelectAbility(Character target, int selector)
        {
            return selector switch
            {
                0 => new SkipTurn(this),
                1 => new Pathetic(this, target),
                2 => new ShadowFlare(this, target),
                _ => throw new ArgumentOutOfRangeException($"Invalid ability number: {selector}")
            };
        }

        public class Pathetic : SingleTargetAttack
        {
            public class Bleed : StatusEffect
            {
                public Bleed(Character source, Character target) : base(source, target, 3, 2, Flags.Debuff)
                {
                    AddEffect(new DamageOverTime(this, 0.96 * source.GetStatVersus(Stats.Atk, target), Operators.Additive, new()));
                    Name = "Bleed";
                }
            }
            public static AttackInfo AttackProps => new AttackInfo() with
            {
                Ratio = 1.1,
                Scalar = Stats.Atk,
                DamageInfo = DamageProps,
                HitRate = 1
            };
            public static DamageInfo DamageProps => new DamageInfo() with
            {
                Type = DamageTypes.Blunt,
                Source = DamageSources.Attack
            };
            public static EffectInfo EffectProps(Character parent, Character target) => new EffectInfo
            (
                AppliedEffect: new Bleed(parent, target)
            );
            public Pathetic(Character source, Character target) : base(source, target, new(), AttackProps, new(AppliedEffect: new Bleed(source, target)))
            {
                WithGenericManaCost(15);
            }
        }
        public class ShadowFlare : BounceAttack
        {
            public static AttackInfo AttackProps => new AttackInfo() with
            {
                Ratio = 2.00,
                Scalar = Stats.Atk,
                DamageInfo = DamageProps,
                HitRate = 1,
            };
            public static DamageInfo DamageProps => new DamageInfo() with
            {
                Type = DamageTypes.Magic,
                Source = DamageSources.Attack
            };
            public ShadowFlare(Character source, Character target) : base(source, target, new(), AttackProps, null,
                count: (uint)Math.Floor(BattleManager.RNG * 5 + 1))
            {
                WithGenericManaCost(20);
            }
        }
    }
}
