using FrogBattle.Classes;
using FrogBattle.Classes.BattleManagers;
using FrogBattle.Classes.Effects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrogBattle.Characters
{
    internal class Rexulti : Character
    {
        public Rexulti(string name, BattleManager battle) : base(name, battle, new()
        {
            { Stats.MaxHp, Registry.DefaultStats[Stats.MaxHp] * 0.375 },
            { Stats.Atk, Registry.DefaultStats[Stats.Atk] * 0.8 },
            { Stats.Dex, 5 },
            { Stats.CritRate, 0.05 },
        })
        {
            Pronouns = Registry.CommonPronouns.HE_HIM;
            PassiveEffects.Add(new CritRateAndDamageBoost(this));
            DamageDealt += DoTEnergyRecharge;
            DamageDealt += BlessedDoTBoost;
            EffectApplied += BlessedDoTApplication;
            DamageDealt += SinfulCreatureAdditionalDamage;
            AbilityLaunched += DoTTrigger<Memory>(2);
            AbilityLaunched += DoTTrigger<Devastate>(1);
            AddEffect(new DoTDamageRES().GetInstance(null, this));
        }

        // Passives
        private record class CritRateAndDamageBoost : PassiveEffect
        {
            public CritRateAndDamageBoost(Character src) : base(new Modifier(0.02, Stats.CritRate, Operators.AddValue), new Modifier(0.05, Stats.CritDamage, Operators.AddValue))
            {
                Condition = new EffectsTypeCount<DamageOverTime>(new(Min: 0, Max: 10));
                Source = src;
            }
        }

        private void BlessedDoTApplication(object sender, StatusEffectInstance e)
        {
            if (e.Definition is Blessed)
            {
                const int count = 3;
                var dotList = new List<StatusEffectInstance>()
                {
                    new Registry.Bleed() { BaseTurns = 5 }.GetInstance(e.Source, e.Target),
                    new Registry.Burn() { BaseTurns = 5 }.GetInstance(e.Source, e.Target),
                    new Registry.Shock() { BaseTurns = 5 }.GetInstance(e.Source, e.Target),
                    new Registry.WindShear() { BaseTurns = 5 }.GetInstance(e.Source, e.Target)
                };
                dotList = [.. dotList.OrderBy(x => BattleManager.RNG)];
                foreach (var item in dotList.Take(count))
                {
                    e.Target.AddEffect(item);
                }
            }
        }

        // 25% chance to do damage again
        private void BlessedDoTBoost(object sender, Damage.Snapshot e)
        {
            if (e.Info.Source == DamageSources.DamageOverTime)
            {
                if (ConsoleBattleManager.RNG < 0.25)
                {
                    var additionalDamage = e with { Info = e.Info with { Source = DamageSources.Additional } };
                    additionalDamage.Take();
                    AddBattleText(Generic.Damage, additionalDamage.Source, additionalDamage.Target, additionalDamage);
                }
            }
        }

        private record class Bleed : StatusEffectDefinition
        {
            public Bleed() : base(new DamageOverTime(0.96, Operators.AddValue, Stats.Atk, Operators.MultiplyBase))
            {
                BaseTurns = 3;
                MaxStacks = 10;
                Properties = EffectFlags.Debuff | EffectFlags.StartTick;
                Name = "Bleed";
            }
        }
        private record class Burn : StatusEffectDefinition
        {
            public Burn() : base(new DamageOverTime(0.50, Operators.AddValue, Stats.Atk, Operators.MultiplyBase))
            {
                BaseTurns = 1;
                MaxStacks = 99;
                Properties = EffectFlags.Debuff | EffectFlags.StartTick;
                Name = "Burn";
            }
        }
        private record class Blessed : StatusEffectDefinition
        {
            public Blessed() : base()
            {
                BaseTurns = 5;
                MaxStacks = 1;
                Properties = EffectFlags.StartTick | EffectFlags.Debuff;
                Name = "Blessed";
            }
        }
        private record class SinfulCreature : StatusEffectDefinition
        {
            public SinfulCreature() : base(new DamageRES(-0.02))
            {
                BaseTurns = 3;
                MaxStacks = 5;
                Properties = EffectFlags.Debuff | EffectFlags.Unremovable;
                Name = "Sinful Creature";
            }
        }
        private record class DoTDamageRES : StatusEffectDefinition
        {
            public DoTDamageRES() : base(new DamageSourceRES(0.15, DamageSources.DamageOverTime))
            {
                BaseTurns = 1;
                MaxStacks = 1;
                Properties = EffectFlags.Unremovable | EffectFlags.Hidden | EffectFlags.Infinite;
                Name = "DoT Damage RES";
            }
        }
        private void SinfulCreatureAdditionalDamage(object sender, Damage.Snapshot e)
        {
            if (e.Target is Character tg && tg.ActiveEffects.Any(x => x.Definition is SinfulCreature) && tg.GetActives<DamageOverTime>().Count != 0)
            {
                new Damage(this, tg, e.Amount * 0.5, e.Info).Take();
            }
        }

        private void DoTEnergyRecharge(object sender, Damage.Snapshot e)
        {
            if (e.Info.Source == DamageSources.DamageOverTime)
            {
                ApplyChange(new Reward(1, Pools.Energy, Operators.AddValue), e.Target as Character);
            }
        }

        public override void LoadAbilities(Character target)
        {
            abilityList.Clear();
            abilityList.Add(new SkipTurn());
            abilityList.Add(new Pathetic());
            abilityList.Add(new ShadowFlare());
            abilityList.Add(new Sacrifice());
            abilityList.Add(new Memory());
            abilityList.Add(new ThisEndsNow());
            abilityList.Add(new Devastate());
            abilityList.TrimExcess();
        }

        #region Abilities
        private static readonly AbilityInfo AbilityProps = new(typeof(Rexulti));
        public class Pathetic : SingleTargetAttack
        {
            private static readonly DamageInfo DamageProps = new()
            {
                Type = DamageTypes.Blunt,
                Source = DamageSources.Attack,
                CanCrit = true
            };
            private static readonly AttackInfo AttackProps = new()
            {
                DamageInfo = DamageProps,
                Ratio = 1.1,
                Scalar = Stats.Atk,
                HitRate = 1
            };
            private static readonly EffectInfo[] EffectProps = [new EffectInfo(new Bleed())
            {
                Chance = 1,
                ChanceType = ChanceTypes.Base
            }];
            public Pathetic() : base(AbilityProps, AttackProps, EffectProps)
            {
                WithGenericManaCost(15);
            }
        }
        public class ShadowFlare : BounceAttack
        {
            private static readonly DamageInfo DamageProps = new()
            {
                Type = DamageTypes.Magic,
                Source = DamageSources.Attack
            };
            private static readonly AttackInfo AttackProps = new()
            {
                DamageInfo = DamageProps,
                Ratio = 2.00,
                Scalar = Stats.Atk,
                HitRate = 1,
            };
            private static readonly EffectInfo[] EffectProps = [new EffectInfo(new Burn())
            {
                Chance = 0.65,
                ChanceType = ChanceTypes.Base
            }];
            public ShadowFlare() : base(AbilityProps, AttackProps, EffectProps,
                count: (uint)Math.Floor(BattleManager.RNG * 5 + 1))
            {
                WithGenericManaCost(20);
            }
        }
        public class Sacrifice : ApplyEffectOn
        {
            private record class AtkBuff : StatusEffectDefinition
            {
                public AtkBuff() : base(new Modifier(350, Stats.Atk, Operators.AddValue))
                {
                    BaseTurns = 5;
                    MaxStacks = 1;
                    Properties = EffectFlags.Unremovable;
                    Name = "Sacrifice";
                }
            }
            private static readonly EffectInfo[] EffectProps = [new EffectInfo(new AtkBuff())];
            public Sacrifice() : base(AbilityProps, EffectProps)
            {
                WithGenericManaCost(20, 0.5);
                WithGenericCost(new(0.01, Pools.Hp, Operators.MultiplyBase));
            }
        }
        public class Memory : SingleTargetAttack
        {
            private static readonly DamageInfo DamageProps = new()
            {
                Type = DamageTypes.Magic,
                Source = DamageSources.Attack,
                DefenseIgnore = 1,
                CanCrit = false
            };
            private static readonly AttackInfo AttackProps = new()
            {
                DamageInfo = DamageProps,
                Ratio = 5.66,
                Scalar = Stats.Atk
            };
            public Memory() : base(AbilityProps, AttackProps, null)
            {
                WithGenericManaCost(40);
            }
        }
        public class ThisEndsNow : SingleTargetAttack
        {
            private static readonly DamageInfo DamageProps = new()
            {
                Type = DamageTypes.Bullet,
                Source = DamageSources.Attack
            };
            private static readonly AttackInfo AttackProps = new()
            {
                DamageInfo = DamageProps,
                Ratio = 7.00,
                Scalar = Stats.Atk
            };
            private static readonly EffectInfo[] EffectProps = [new EffectInfo(new Bleed())
            {
                Chance = 1,
                ChanceType = ChanceTypes.Base
            }];

            public ThisEndsNow() : base(AbilityProps, AttackProps, EffectProps)
            {
                WithGenericManaCost(34);
            }
        }
        public class Devastate : AbilityDefinition
        {
            private class DevastateExplosion() : AoEAttack(AbilityProps, AttackProps, null)
            {
                private static readonly DamageInfo DamageProps = new()
                {
                    Type = DamageTypes.Blast,
                    Source = DamageSources.Attack,
                    CanCrit = true,
                };
                private static readonly AttackInfo AttackProps = new()
                {
                    DamageInfo = DamageProps,
                    Ratio = 2.35,
                    Scalar = Stats.Atk
                };
            }
            private class DevastateSlash1() : SingleTargetAttack(AbilityProps, AttackProps, EffectProps)
            {
                private static readonly DamageInfo DamageProps = new()
                {
                    Type = DamageTypes.Slash,
                    Source = DamageSources.Attack,
                    CanCrit = true,
                };
                private static readonly AttackInfo AttackProps = new()
                {
                    DamageInfo = DamageProps,
                    Ratio = 1.10,
                    Scalar = Stats.Atk,
                    HitRate = 0.85
                };
                private static readonly EffectInfo[] EffectProps = [new EffectInfo(new Registry.Bleed())
                {
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }];
            }
            private class DevastateSlash2() : SingleTargetAttack(AbilityProps, AttackProps, EffectProps)
            {
                private static readonly DamageInfo DamageProps = new()
                {
                    Type = DamageTypes.Slash,
                    Source = DamageSources.Attack,
                    CanCrit = true,
                };
                private static readonly AttackInfo AttackProps = new()
                {
                    DamageInfo = DamageProps,
                    Ratio = 2.35,
                    Scalar = Stats.Atk,
                    HitRate = 0.75
                };
                private static readonly EffectInfo[] EffectProps = [new EffectInfo(new Blessed())
                {
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }];
            }
            private class DevastateSlash3() : SingleTargetAttack(AbilityProps, AttackProps, EffectProps)
            {
                private static readonly DamageInfo DamageProps = new()
                {
                    Type = DamageTypes.Slash,
                    Source = DamageSources.Attack,
                    CanCrit = true,
                };
                private static readonly AttackInfo AttackProps = new()
                {
                    DamageInfo = DamageProps,
                    Ratio = 15.00,
                    Scalar = Stats.Atk,
                    HitRate = 1
                };
                private static readonly EffectInfo[] EffectProps = [new EffectInfo (new SinfulCreature())
                {
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }, new EffectInfo (new Registry.Bleed())
                {
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }, new EffectInfo (new Registry.Burn())
                {
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }, new EffectInfo (new Registry.Shock())
                {
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }, new EffectInfo (new Registry.WindShear())
                {
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }];
            }
            public Devastate() : base(AbilityProps)
            {
                WithBurstCost();
            }
            public override bool Use(AbilityInstance ctx)
            {
                var text = ctx.GetFlavourText();
                double finalDamage = 0;
                void checkTotalDamage(object sender, Damage.Snapshot e)
                {
                    finalDamage += e.Amount;
                }
                ctx.Target.DamageTaken += checkTotalDamage;
                // Ability start text
                ctx.AddText(text[TextTypes.Start], ctx.User, ctx.Target);
                // Explosion
                var explosion = new DevastateExplosion();
                if (!explosion.Use(ctx)) return false;
                // Ability continuation text
                ctx.AddText(text[TextTypes.Damage1], ctx.User, ctx.Target);
                // 3 Slashes
                var slashset1 = new AbilityInstance(new DevastateSlash1(), ctx.User, ctx.Target);
                bool success1 = slashset1.TryUse() && slashset1.TryUse() && slashset1.TryUse();
                // 2 Slashes
                var slashset2 = new AbilityInstance(new DevastateSlash2(), ctx.User, ctx.Target);
                bool success2 = slashset2.TryUse() && slashset2.TryUse();
                // 1 Final slash
                var slash3 = new AbilityInstance(new DevastateSlash3(), ctx.User, ctx.Target);
                bool success3 = slash3.TryUse();
                // Ability end text
                ctx.AddText(text[TextTypes.End], ctx.User, ctx.Target, finalDamage);
                ctx.Target.DamageTaken -= checkTotalDamage;
                return success1 || success2 || success3;
            }
        }
        #endregion

        public override void Die()
        {
            var phase2 = new RexultiPhase2(Name, ParentBattle) { EnemyTeam = EnemyTeam, Team = Team };
            Team.Remove(this);
            Team.Add(phase2);
            AddBattleText(_internalName + ".phase2", this, phase2);
        }
    }
    internal class RexultiPhase2 : Character
    {
        public RexultiPhase2(string name, BattleManager battle) : base(name, battle, new()
            {
                { Stats.MaxHp, Registry.DefaultStats[Stats.MaxHp] * 0.625 },
                { Stats.Atk, Registry.DefaultStats[Stats.Atk] * 1.1 },
                { Stats.Dex, 10 },
                { Stats.CritRate, 0.20 },
                { Stats.CritDamage, 0.65 },
            })
        {

        }

        public override void LoadAbilities(Character target)
        {
            abilityList.Clear();
            abilityList.Add(new SkipTurn());
            abilityList.Add(new Kneel());
            abilityList.Add(new Court());
            abilityList.Add(new Dance());
            abilityList.TrimExcess();
        }

        #region Abilities
        private static readonly AbilityInfo AbilityProps = new(typeof(RexultiPhase2));
        public class Kneel : SingleTargetAttack
        {
            private static readonly DamageInfo DamageProps = new()
            {
                Source = DamageSources.Attack,
                Type = DamageTypes.Magic,
                CanCrit = true
            };
            private static readonly AttackInfo AttackProps = new()
            {
                DamageInfo = DamageProps,
                Ratio = 7.00,
                Scalar = Stats.Atk,
                HitRate = 1
            };
            private static EffectInfo[] EffectProps
            {
                get
                {
                    List<EffectInfo> result =
                    [
                        new EffectInfo (new Registry.Bleed()) { Chance = 1.00, ChanceType = ChanceTypes.Base },
                        new EffectInfo (new Registry.Burn()) { Chance = 1.00, ChanceType = ChanceTypes.Base },
                        new EffectInfo (new Registry.Shock()) { Chance = 1.00, ChanceType = ChanceTypes.Base },
                        new EffectInfo (new Registry.WindShear()) { Chance = 1.00, ChanceType = ChanceTypes.Base }
                    ];
                    result.RemoveAt((int)Math.Floor(BattleManager.RNG * 4));
                    return [.. result];
                }
            }
            public Kneel() : base(AbilityProps, AttackProps, EffectProps)
            {
                WithGenericManaCost(21);
            }
        }
        public class Court : ApplyEffectOn
        {
            private record class DefenseDown : StatusEffectDefinition
            {
                public DefenseDown() : base(new Modifier(-0.5, Stats.Def, Operators.MultiplyBase))
                {
                    BaseTurns = 4;
                    MaxStacks = 1;
                    Properties = EffectFlags.Debuff;
                    Name = "Defense Down";
                }
            }
            private static readonly EffectInfo[] EffectProps =
                [
                    new EffectInfo(new DefenseDown(), Chance: 1.00, ChanceType: ChanceTypes.Base)
                ];
            public Court() : base(AbilityProps, EffectProps)
            {
                WithGenericManaCost(19);
            }
        }
        public class Dance : BounceAttack
        {
            private static readonly AttackInfo AttackProps = new()
            {
                Ratio = 1.90,
                Scalar = Stats.Atk,
                DamageInfo = DamageProps,
            };
            private static readonly DamageInfo DamageProps = new()
            {
                Type = DamageTypes.Magic,
                Source = DamageSources.Attack,
                CanCrit = true,
            };
            public Dance() : base(AbilityProps, AttackProps, null, 3)
            {
                WithGenericManaCost(29);
            }
        }
        #endregion
    }
}
