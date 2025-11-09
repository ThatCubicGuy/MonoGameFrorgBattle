using FrogBattle.Classes;
using FrogBattle.Classes.BattleManagers;
using FrogBattle.Classes.Effects;

namespace FrogBattle.Characters
{
    internal class Snake : Character
    {
        public Snake(string name, ConsoleBattleManager battle) : base(name, battle, new()
        {
            { Stats.CritRate, 0.30 },
            { Stats.EffectRES, 0.30 }
        })
        {
            Pronouns = Registry.CommonPronouns.HE_HIM;
        }
        private record class OverhealOnDamage : StatusEffectDefinition, IEvent
        {
            public OverhealOnDamage(Damage.Snapshot damage) : base(new Overheal(damage.Amount * 0.2, 20000))
            {
                BaseTurns = 3;
                Name = "Overheal";
            }
            public static void Event(object sender, Damage.Snapshot e)
            {
                if (e.Source is not Character sc) return;
                var overheal = new OverhealOnDamage(e).GetInstance(e);
                sc.AddEffect(overheal);
            }
        }
        private record class Box : StatusEffectDefinition
        {
            public Box() : base(new Shield(8000) { ShieldType = DamageTypes.Blunt }, new Stun())
            {
                BaseTurns = 1;
                MaxStacks = 1;
                Properties = EffectFlags.Hidden | EffectFlags.StartTick;
                Name = "Box";
            }
        }

        public override void LoadAbilities(Character target)
        {
            abilityList.Clear();
            abilityList.Add(new SkipTurn());
            abilityList.Add(new ThrowGrenade());
            abilityList.Add(new UpKick());
            abilityList.Add(new ClusterLauncher());
            abilityList.Add(new MortarVolley());
            abilityList.TrimExcess();
        }
        #region Abilities
        private static readonly AbilityInfo AbilityProps = new(typeof(Snake));
        public class ThrowGrenade : BlastAttack, ClusterTrigger
        {
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
            public ThrowGrenade() : base(AbilityProps, AttackProps, null, 0.5)
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
            public UpKick() : base(AbilityProps, AttackProps, null)
            {
                WithGenericManaCost(14);
                WithReward(new(0.1, Pools.Energy, Operators.MultiplyBase));
            }
        }
        public class ClusterLauncher : FollowUpSetup
        {
            public class ClusterGrenade() : SingleTargetAttack(AbilityProps, AttackProps, null), MortarTrigger
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
            public ClusterLauncher() : base(AbilityProps, LimitedFollowUp<ClusterTrigger>(new ClusterGrenade(), 1))
            {
                WithRequirement(new StatRequirement(1, Stats.Atk, Operators.MultiplyBase));
                WithGenericManaCost(28);
            }
        }
        public class MortarVolley : FollowUpSetup
        {
            public class MortarShot() : BounceAttack(AbilityProps, AttackProps, null, 5)
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
            public MortarVolley() : base(AbilityProps, LimitedFollowUp<MortarTrigger>(new MortarShot(), 3))
            {
                WithRequirement(new StatRequirement(0.9, Stats.Def, Operators.MultiplyBase));
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
