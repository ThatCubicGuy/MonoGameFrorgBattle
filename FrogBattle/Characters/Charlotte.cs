using FrogBattle.Classes;
using FrogBattle.Classes.BattleManagers;
using FrogBattle.Classes.Effects;
using System;

namespace FrogBattle.Characters
{
    internal class Charlotte : Character
    {
        public Charlotte(string name, ConsoleBattleManager battle) : base(name, battle)
        {
            Pronouns = Registry.CommonPronouns.SHE_HER;
            DamageDealt += GuiltProtocolCritDamageBuff;
        }

        private void GuiltProtocolCritDamageBuff(object sender, Damage.Snapshot e)
        {
            if (e.IsCrit && this.EffectIsActive<GuiltProtocol.Buff>())
            {
                AddEffect(new GuiltProtocol.Buff().GetInstance(e));
            }
        }

        public override void LoadAbilities(Character target)
        {
            abilityList.Clear();
            abilityList.Add(new SkipTurn());
            abilityList.Add(new AlphaProtocol());
            abilityList.Add(new GuiltProtocol());
            abilityList.Add(new BlossomProtocol());
            //abilityList.Add(new NightmareProtocol());
            //abilityList.Add(new DesireProtocol());
            //abilityList.Add(new DarkwaterProtocol());
            abilityList.TrimExcess();
        }

        #region Abilities
        private static readonly AbilityInfo AbilityProps = new(typeof(Charlotte));
        private class AlphaProtocol : ApplyEffectOn
        {
            public record class Buff : StatusEffectDefinition
            {
                public Buff() : base(new Modifier(0.20, Stats.Spd, Operators.MultiplyBase))
                {
                    Name = "α Protocol";
                    BaseTurns = 3;
                    MaxStacks = 1;
                }
            }
            private static readonly EffectInfo[] effectInfos = [new EffectInfo(new Buff())];
            public AlphaProtocol() : base(AbilityProps, effectInfos)
            {
                WithGenericManaCost(16);
            }
        }
        private class GuiltProtocol : ApplyEffectOn
        {
            public record class Buff : StatusEffectDefinition
            {
                public Buff() : base(new Modifier(0.20, Stats.CritDamage, Operators.AddValue))
                {
                    Name = "Guilt Protocol";
                    BaseTurns = 3;
                    MaxStacks = 1;
                }
            }
            public record class Bloodlust : StatusEffectDefinition
            {
                public Bloodlust() : base(new Modifier(0.10, Stats.CritDamage, Operators.MultiplyBase))
                {
                    Name = "Bloodlust";
                    BaseTurns = 3;
                    MaxStacks = 10;
                }
            }
            public record class Passive : PassiveEffect
            {
                public Passive() : base(new Modifier(-100, Stats.MaxEnergy, Operators.AddValue))
                {
                    Condition = new EffectsTypeCount<DamageOverTime>(new(Min: 2, Max: 2));
                }
            }
            private static readonly EffectInfo[] effectInfos = [new EffectInfo(new Buff())];
            public GuiltProtocol() : base(AbilityProps, effectInfos)
            {
                WithGenericManaCost(26);
            }
        }
        private class BlossomProtocol : BlastAttack
        {
            private static readonly DamageInfo DamageProps = new()
            {
                Type = DamageTypes.Slash,
                Source = DamageSources.Attack,
                CanCrit = true
            };
            private static readonly AttackInfo AttackProps = new()
            {
                DamageInfo = DamageProps,
                Ratio = 0.0075,
                Scalar = Stats.MaxHp,
                HitRate = 1
            };
            public BlossomProtocol() : base(AbilityProps, AttackProps, null, AttackProps.Ratio * 0.65)
            {
                WithGenericManaCost(28, 0.80);
            }
            //protected override void DealtDamage(AbilityInstance ctx, Damage dmg) => HealOnDamage(dmg, 0.45);
        }
        private class NightmareProtocol
        {

        }
        private class DesireProtocol
        {

        }
        private class DarkwaterProtocol
        {

        }
        private class BrightwaterProtocol
        {

        }
        private class Bloodsummer //: ApplyEffectOn
        {

        }
        #endregion
    }
}
