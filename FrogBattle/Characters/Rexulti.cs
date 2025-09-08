using FrogBattle.Classes;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Characters
{
    internal class Rexulti : Character
    {
        // Passives
        private PassiveEffect CritRateAndDamageBoost()
        {
            var result = new PassiveEffect(this)
            {
                Condition = new EffectsTypeCount<DamageOverTime>(new(Max: 10))
            };
            var effect1 = new Modifier(result, 0.02, Stats.CritRate, Operators.Additive);
            var effect2 = new Modifier(result, 0.05, Stats.CritDamageBonus, Operators.Additive);
            result.Subeffects[effect1.GetKey()] = effect1;
            result.Subeffects[effect2.GetKey()] = effect2;
            return result;
        }
        public Rexulti(string name, BattleManager battle, bool team) : base(name, battle, team, new()
        {
            { Stats.MaxHp, 250000 },
            { Stats.MaxEnergy, 5 },
            { Stats.Atk, 800 },
            { Stats.Dex, 10 },
            { Stats.CritRate, 0.05 },
        })
        {
            Pronouns = new("he", "him", "his", "his", "himself", true);
            PassiveEffects.Add(CritRateAndDamageBoost());
            DamageDealt += DoTEnergyRecharge;
            DamageDealt += BlessedDoTBoost;
            EffectApplied += BlessedDoTApplication;
            DamageDealt += SinfulCreatureAdditionalDamage;
            AbilityLaunched += DoTTrigger<Memory>(2);
            AbilityLaunched += DoTTrigger<Devastate>(1);
            AddEffect(new DoTDamageRES(this, this));
        }

        private void BlessedDoTApplication(object sender, StatusEffect e)
        {
            if (e is Blessed)
            {
                const int count = 3;
                var dotList = new List<StatusEffect>()
            {
                new Registry.Bleed(e.Source, e.Target) { Turns = 5 },
                new Registry.Burn(e.Source, e.Target) { Turns = 5 },
                new Registry.Shock(e.Source, e.Target) { Turns = 5 },
                new Registry.WindShear(e.Source, e.Target) { Turns = 5 }
            };
                dotList = [.. dotList.OrderBy(x => BattleManager.RNG)];
                foreach (var item in dotList.Take(count))
                {
                    e.Target.AddEffect(item);
                }
            }
        }

        // 25% chance to do damage again
        private void BlessedDoTBoost(object sender, Damage.DamageSnapshot e)
        {
            if (e.Info.Source == DamageSources.DamageOverTime)
            {
                if (BattleManager.RNG < 0.25)
                {
                    e.Target.TakeDamage(e with { Info = e.Info with { Source = DamageSources.Additional } });
                }
            }
        }

        private class Bleed : StatusEffect
        {
            public Bleed(Character source, Character target) : base(source, target, 3, 10, Flags.Debuff | Flags.StartTick)
            {
                AddEffect(new DamageOverTime(this, 0.96 * source.GetStatVersus(Stats.Atk, target), Operators.Additive, new()));
                Name = "Bleed";
            }
        }
        private class Burn : StatusEffect
        {
            public Burn(Character source, Character target) : base(source, target, 1, 5, Flags.Debuff | Flags.StartTick)
            {
                AddEffect(new DamageOverTime(this, 0.50 * source.GetStatVersus(Stats.Atk, target), Operators.Additive, new()));
                Name = "Burn";
            }
        }
        private class Blessed : StatusEffect
        {
            public Blessed(Character source, Character target) : base(source, target, 5, 1, Flags.StartTick | Flags.Debuff)
            {
                Name = "Blessed";
            }
        }
        private class SinfulCreature : StatusEffect
        {
            public SinfulCreature(Character source, Character target) : base(source, target, 3, 5, Flags.Debuff | Flags.Unremovable)
            {
                AddEffect(new DamageRES(this, -0.02));
                Name = "Sinful Creature";
            }
        }
        private class DoTDamageRES : StatusEffect
        {
            public DoTDamageRES(Character source, Character target) : base(source, target, 1, 1, Flags.Unremovable | Flags.Hidden | Flags.Infinite)
            {
                AddEffect(new DamageSourceRES(this, 0.15, DamageSources.DamageOverTime));
                Name = "DoT Damage RES";
            }
        }
        private void SinfulCreatureAdditionalDamage(object sender, Damage.DamageSnapshot e)
        {
            if (e.Target.StatusEffects.Any(x => x is SinfulCreature) && e.Target.GetActives<DamageOverTime>().Count != 0)
            {
                e.Target.TakeDamage(new Damage(this, e.Target, e.Amount * 0.5, e.Info));
            }
        }

        private void DoTEnergyRecharge(object sender, Damage.DamageSnapshot e)
        {
            if (e.Info.Source == DamageSources.DamageOverTime)
            {
                e.Source.ApplyChange(new Reward(sender as Character, this, 1, Pools.Energy, Operators.Additive));
            }
        }

        public override Ability SelectAbility(Character target, int selector)
        {
            return selector switch
            {
                0 => new SkipTurn(this),
                1 => new Pathetic(this, target),
                2 => new ShadowFlare(this, target),
                3 => new Sacrifice(this),
                4 => new Memory(this, target),
                5 => new ThisEndsNow(this, target),
                6 => new Devastate(this, target),
                _ => throw new ArgumentOutOfRangeException(nameof(selector), $"Invalid ability number: {selector}")
            };
        }

        public class Pathetic : SingleTargetAttack
        {
            public static AttackInfo AttackProps => new()
            {
                Ratio = 1.1,
                Scalar = Stats.Atk,
                DamageInfo = DamageProps,
                HitRate = 1
            };
            public static DamageInfo DamageProps => new()
            {
                Type = DamageTypes.Blunt,
                Source = DamageSources.Attack
            };
            public static EffectInfo[] EffectProps(Character parent, Character target) => [new()
            {
                AppliedEffect = new Bleed(parent, target),
                Chance = 1,
                ChanceType = ChanceTypes.Base
            }];
            public Pathetic(Character source, Character target) : base(source, target, new(), AttackProps, EffectProps(source, target))
            {
                WithGenericManaCost(15);
            }
        }
        public class ShadowFlare : BounceAttack
        {
            public static AttackInfo AttackProps => new()
            {
                Ratio = 2.00,
                Scalar = Stats.Atk,
                DamageInfo = DamageProps,
                HitRate = 1,
            };
            public static DamageInfo DamageProps => new()
            {
                Type = DamageTypes.Magic,
                Source = DamageSources.Attack
            };
            public static EffectInfo[] EffectProps(Character parent, Character target) => [new()
            {
                AppliedEffect = new Burn(parent, target),
                Chance = 0.65,
                ChanceType = ChanceTypes.Base
            }];
            public ShadowFlare(Character source, Character target) : base(source, target, new(), AttackProps, EffectProps(source, target),
                count: (uint)Math.Floor(BattleManager.RNG * 5 + 1))
            {
                WithGenericManaCost(20);
            }
        }
        public class Sacrifice : Buff
        {
            private class Buff : StatusEffect
            {
                public Buff(Character source) : base(source, source, 5, 1, Flags.Unremovable)
                {
                    AddEffect(new Modifier(this, 350, Stats.Atk, Operators.Additive));
                    Name = "Sacrifice";
                }
            }
            private static EffectInfo[] EffectProps(Character parent) => [new()
            {
                AppliedEffect = new Buff(parent)
            }];
            public Sacrifice(Character source) : base(source, source, new(), EffectProps(source))
            {
                WithGenericManaCost(20, 0.5);
                WithGenericCost(new(this, 0.01, Pools.Hp, Operators.Multiplicative));
            }
        }
        public class Memory : SingleTargetAttack
        {
            private static AttackInfo AttackProps => new()
            {
                Ratio = 5.66,
                Scalar = Stats.Atk,
                DamageInfo = DamageProps
            };
            private static DamageInfo DamageProps => new()
            {
                Type = DamageTypes.Magic,
                Source = DamageSources.Attack,
                DefenseIgnore = 1,
                CanCrit = false
            };
            public Memory(Character source, Character target) : base(source, target, new(), AttackProps, null)
            {
                WithGenericManaCost(40);
                // next up: figure out how to kafka my bluh's attack
            }
        }
        public class ThisEndsNow : SingleTargetAttack
        {
            private static AttackInfo AttackProps => new()
            {
                Ratio = 7.00,
                Scalar = Stats.Atk,
                DamageInfo = DamageProps
            };
            private static DamageInfo DamageProps => new()
            {
                Type = DamageTypes.Bullet,
                Source = DamageSources.Attack
            };
            private static EffectInfo[] EffectProps(Character source, Character target) => [new()
            {
                AppliedEffect = new Bleed(source, target),
                Chance = 1,
                ChanceType = ChanceTypes.Base
            }];

            public ThisEndsNow(Character source, Character target) : base(source, target, new(), AttackProps, EffectProps(source, target))
            {
                WithGenericManaCost(34);
            }
        }
        public class Devastate : Ability
        {
            private class DevastateExplosion(Character source, Character target) : AoEAttack(source, target, new(), AttackProps, null)
            {
                private static AttackInfo AttackProps => new()
                {
                    Ratio = 2.35,
                    Scalar = Stats.Atk,
                    DamageInfo = DamageProps
                };
                private static DamageInfo DamageProps => new()
                {
                    Type = DamageTypes.Blast,
                    Source = DamageSources.Attack,
                    CanCrit = true,
                };
            }
            private class DevastateSlash1(Character source, Character target) : SingleTargetAttack(source, target, new(), AttackProps, EffectProps(source, target))
            {
                private static AttackInfo AttackProps => new()
                {
                    Ratio = 1.10,
                    Scalar = Stats.Atk,
                    DamageInfo = DamageProps,
                    HitRate = 0.85,
                };
                private static DamageInfo DamageProps => new()
                {
                    Type = DamageTypes.Slash,
                    Source = DamageSources.Attack,
                    CanCrit = true,
                };
                private static EffectInfo[] EffectProps(Character source, Character target) => [new()
                {
                    AppliedEffect = new Registry.Bleed(source, target),
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }];
            }
            private class DevastateSlash2(Character source, Character target) : SingleTargetAttack(source, target, new(), AttackProps, EffectProps(source, target))
            {
                private static AttackInfo AttackProps => new()
                {
                    Ratio = 2.35,
                    Scalar = Stats.Atk,
                    DamageInfo = DamageProps,
                    HitRate = 0.75,
                };
                private static DamageInfo DamageProps => new()
                {
                    Type = DamageTypes.Slash,
                    Source = DamageSources.Attack,
                    CanCrit = true,
                };
                private static EffectInfo[] EffectProps(Character source, Character target) => [new()
                {
                    AppliedEffect = new Blessed(source, target),
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }];
            }
            private class DevastateSlash3(Character source, Character target) : SingleTargetAttack(source, target, new(), AttackProps, EffectProps(source, target))
            {
                private static AttackInfo AttackProps => new()
                {
                    Ratio = 15.00,
                    Scalar = Stats.Atk,
                    DamageInfo = DamageProps,
                    HitRate = 1
                };
                private static DamageInfo DamageProps => new()
                {
                    Type = DamageTypes.Slash,
                    Source = DamageSources.Attack,
                    CanCrit = true,
                };
                private static EffectInfo[] EffectProps(Character source, Character target) => [new()
                {
                    AppliedEffect = new SinfulCreature(source, target),
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }, new()
                {
                    AppliedEffect = new Registry.Bleed(source, target),
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }, new()
                {
                    AppliedEffect = new Registry.Burn(source, target),
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }, new()
                {
                    AppliedEffect = new Registry.Shock(source, target),
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }, new()
                {
                    AppliedEffect = new Registry.WindShear(source, target),
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }];
            }
            public Devastate(Character source, Character target) : base(source, target, new())
            {
                WithGenericCost(new(this, source.GetStat(Stats.MaxEnergy), Pools.Energy, Operators.Additive));
            }
            private protected override bool Use()
            {
                var text = FlavourText();
                double finalDamage = 0;
                void checkTotalDamage(object s, Damage.DamageSnapshot e)
                {
                    finalDamage += e.Amount;
                }
                Target.DamageTaken += checkTotalDamage;
                // Ability start text
                AddText(text[TextTypes.Start], Parent, Target);
                // Explosion
                var explosion = new DevastateExplosion(Parent, Target);
                if (!explosion.TryUse()) return false;
                // Ability continuation text
                AddText(text[TextTypes.Damage1], Parent, Target);
                // 3 Slashes
                var slashset1 = new DevastateSlash1(Parent, Target);
                if (!slashset1.TryUse() || !slashset1.TryUse() || !slashset1.TryUse()) return false;
                // 2 Slashes
                var slashset2 = new DevastateSlash2(Parent, Target);
                if (!slashset2.TryUse() || !slashset2.TryUse()) return false;
                // 1 Final slash
                var slash3 = new DevastateSlash3(Parent, Target);
                if (!slash3.TryUse()) return false;
                // Ability end text
                AddText(text[TextTypes.End], Parent, Target, finalDamage);
                Target.DamageTaken -= checkTotalDamage;
                return true;
            }
        }
    }
}
