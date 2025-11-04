using FrogBattle.Classes;
using System;

namespace FrogBattle.Characters
{
    internal class Charlotte : Character
    {
        public Charlotte(string name, BattleManager battle) : base(name, battle)
        {
            Pronouns = Registry.CommonPronouns.SHE_HER;
            DamageDealt += GuiltProtocolCritDamageBuff;
        }

        private void GuiltProtocolCritDamageBuff(object sender, Damage.Snapshot e)
        {
            if (e.IsCrit && this.EffectIsActive<GuiltProtocol.Buff>())
            {
                AddEffect(new GuiltProtocol.Buff());
            }
        }

        public override Ability SelectAbility(Character target, int selector)
        {
            return selector switch
            {
                0 => new SkipTurn(this),
                1 => new AlphaProtocol(this),
                2 => new GuiltProtocol(this),
                3 => new BlossomProtocol(this, target),
                //4 => new NightmareProtocol(this),
                //5 => new DesireProtocol(this, target),
                //6 => new DarkwaterProtocol(this, target),
                _ => throw InvalidAbility(selector)
            };
        }

        public override void LoadAbilities(Character target)
        {
            abilityList.Clear();
            abilityList.Add(new SkipTurn(this));
            abilityList.Add(new AlphaProtocol(this));
            abilityList.Add(new GuiltProtocol(this));
            abilityList.Add(new BlossomProtocol(this, target));
            //abilityList.Add(new NightmareProtocol(this));
            //abilityList.Add(new DesireProtocol(this));
            //abilityList.Add(new DarkwaterProtocol(this));
        }

        #region Abilities
        private class AlphaProtocol : ApplyEffectOn
        {
            public class Buff : StatusEffect
            {
                public Buff() : base()
                {
                    Name = "α Protocol";
                    Turns = 3;
                    MaxStacks = 1;
                }
                public override StatusEffect Init()
                {
                    AddEffect(new Modifier(0.20, Stats.Spd, Operators.MultiplyBase));
                    return this;
                }
            }
            private static readonly EffectInfo[] effectInfos = [new EffectInfo<Buff>()];
            public AlphaProtocol(Character self) : base(self, self, new(), effectInfos)
            {
                WithGenericManaCost(16);
            }
        }
        private class GuiltProtocol : ApplyEffectOn
        {
            public class Buff : StatusEffect
            {
                public Buff() : base()
                {
                    Name = "Guilt Protocol";
                }
                public override StatusEffect Init()
                {
                    AddEffect(new Modifier(0.20, Stats.CritDamage, Operators.AddValue));
                    return this;
                }
            }
            public class Bloodlust : StatusEffect
            {
                public Bloodlust() : base()
                {
                    Name = "Bloodlust";
                    Turns = 3;
                    MaxStacks = 10;
                }
                public override StatusEffect Init()
                {
                    AddEffect(new Modifier(0.10, Stats.CritDamage, Operators.MultiplyBase));
                    return this;
                }
            }
            public class Passive : PassiveEffect
            {
                public Passive() : base()
                {
                    Condition = new EffectsTypeCount<DamageOverTime>(new(Min: 2, Max: 2));
                    AddEffect(new Modifier(-100, Stats.MaxEnergy, Operators.AddValue));
                }
            }
            private static readonly EffectInfo[] effectInfos = [new EffectInfo<Buff>()];
            public GuiltProtocol(Character self) : base(self, self, new(), effectInfos)
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
            public BlossomProtocol(Character source, Character mainTarget) : base(source, mainTarget, new(), AttackProps, null, AttackProps.Ratio * 0.65)
            {
                WithGenericManaCost(28, 0.80);
            }
            protected override void DealtDamage(Damage dmg) => HealOnDamage(dmg, 0.45);
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
