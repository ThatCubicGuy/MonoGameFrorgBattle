using FrogBattle.Classes;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Characters
{
    internal class Snake : Character
    {
        public Snake(string name, BattleManager battle) : base(name, battle, new()
        {
            { Stats.CritRate, 0.30 },
            { Stats.EffectRES, 0.30 }
        })
        {
            Pronouns = Registry.CommonPronouns.HE_HIM;
        }
        private class OverhealOnDamage : StatusEffect, IEvent
        {
            public double Amount { get; }
            public OverhealOnDamage(Damage.Snapshot damage) : base()
            {
                Turns = 3;
                Name = "Overheal";
                Amount = damage.Amount * 0.2;
            }
            public override StatusEffect Init() => AddEffect(new Overheal(Amount));
            public static void Event(object sender, Damage.Snapshot e)
            {
                if (e.Source is not Character sc) return;
                var overheal = new OverhealOnDamage(e) { Source = sc, Target = e.Target as Character }.Init();
                sc.AddEffect(overheal);
            }
        }
        private class Box : StatusEffect
        {
            public Box() : base()
            {
                Turns = 1;
                MaxStacks = 1;
                Properties = Flags.Hidden | Flags.StartTick;
                Name = "Box";
            }
            public override StatusEffect Init()
            {

                AddEffect(new Shield(8000, DamageTypes.Blunt));
                AddEffect(new Stun());
                return this;
            }
        }

        public override Ability SelectAbility(Character target, int selector)
        {
            return selector switch
            {
                0 => new SkipTurn(this),
                1 => new ThrowGrenade(this, target),
                2 => new UpKick(this, target),
                3 => new ClusterLauncher(this, target),
                4 => new MortarVolley(this, target),
                _ => throw InvalidAbility(selector)
            };
        }

        public override void LoadAbilities(Character target)
        {
            abilityList.Clear();
            abilityList.Add(new SkipTurn(this));
            abilityList.Add(new ThrowGrenade(this, target));
            abilityList.Add(new UpKick(this, target));
            abilityList.Add(new ClusterLauncher(this, target));
            abilityList.Add(new MortarVolley(this, target));
            abilityList.TrimExcess();
        }
        #region Abilities
        public class ThrowGrenade : BlastAttack, ClusterTrigger
        {
            private static readonly AbilityInfo AbilityProps = new();
            private static readonly DamageInfo DamageProps = new DamageInfo() with
            {
                Type = DamageTypes.Blast,
                Source = DamageSources.Attack,
                DefenseIgnore = 0,
                TypeResPen = 0,
                CanCrit = true
            };
            private static readonly AttackInfo AttackProps = new AttackInfo() with
            {
                DamageInfo = DamageProps,
                Ratio = 1.15,
                Scalar = Stats.Atk,
                HitRate = 0.80,
                IndependentHitRate = false
            };
            public ThrowGrenade(Character source, Character target) : base(source, target, AbilityProps, AttackProps, null, 0.5)
            {
                Falloff = 0.5;
                WithGenericManaCost(13);
            }
        }
        public class UpKick : SingleTargetAttack, MortarTrigger
        {
            private static readonly DamageInfo DamageProps = new()
            {
                Type = DamageTypes.Blunt,
                Source = DamageSources.Attack,
                DefenseIgnore = 0,
                TypeResPen = 0.6,
            };
            private static readonly AttackInfo AttackProps = new()
            {
                DamageInfo = DamageProps,
                Ratio = 5.45,
                Scalar = Stats.Def,
                HitRate = 1
            };
            public UpKick(Character source, Character target) : base(source, target, new(), AttackProps, null)
            {
                WithGenericManaCost(14);
                WithReward(new(target, source, 0.1 * source.GetStat(Stats.MaxEnergy), Pools.Energy, Operators.AddValue));
            }
        }
        public class ClusterLauncher : FollowUpSetup
        {
            public class ClusterGrenade(Character source, Character target) : SingleTargetAttack(source, target, new(), AttackProps, null), MortarTrigger
            {
                public static AttackInfo AttackProps => new()
                {
                    Ratio = 7.25,
                    Scalar = Stats.Atk,
                    DamageInfo = DamageProps,
                    HitRate = 1,
                    Split = [5, 1, 1, 1, 1, 1]
                };
                public static DamageInfo DamageProps => new()
                {
                    Type = DamageTypes.Blast,
                    Source = DamageSources.FollowUp,
                    DefenseIgnore = 0,
                    TypeResPen = 0.25,
                    CanCrit = true
                };
            }
            public ClusterLauncher(Character source, Character target) : base(source, target, new(), LimitedFollowUp<ClusterTrigger>(new ClusterGrenade(source, target), 1))
            {
                WithRequirement(new StatThresholdRequirement(this, source.Base[Stats.Atk], null, Stats.Atk, Operators.AddValue));
                WithGenericManaCost(28);
            }
        }
        public class MortarVolley : FollowUpSetup
        {
            public class MortarShot(Character source, Character target) : BounceAttack(source, target, new(), AttackProps, null, 5)
            {
                private static readonly DamageInfo DamageProps = new()
                {
                    Type = DamageTypes.Blast,
                    Source = DamageSources.FollowUp,
                    DefenseIgnore = 0.2,
                    TypeResPen = 0,
                    CanCrit = true
                };
                private static readonly AttackInfo AttackProps = new()
                {
                    DamageInfo = DamageProps,
                    Ratio = 2.12,
                    Scalar = Stats.Atk,
                    HitRate = 0.85
                };
            }
            public MortarVolley(Character source, Character target) : base(source, target, new(), LimitedFollowUp<MortarTrigger>(new MortarShot(source, target), 3))
            {
                WithRequirement(new StatThresholdRequirement(this, source.Base[Stats.Def], null, Stats.Def, Operators.AddValue));
                WithGenericManaCost(24);
            }
        }
        #endregion
        #region FollowUp Triggers
#pragma warning disable IDE1006 // Naming Styles
        private interface ClusterTrigger : ITrigger;
        private interface MortarTrigger : ITrigger;
#pragma warning restore IDE1006 // Naming Styles
        #endregion
    }
}
