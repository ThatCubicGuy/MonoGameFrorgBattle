using FrogBattle.Classes;
using FrogBattle.Classes.BattleManagers;
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
        public Rexulti(string name, ConsoleBattleManager battle) : base(name, battle, new()
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
            AddEffect(new DoTDamageRES() { Target = this });
        }

        // Passives
        private class CritRateAndDamageBoost : PassiveEffect
        {
            public CritRateAndDamageBoost(Character src) : base()
            {
                Condition = new EffectsTypeCount<DamageOverTime>(new(Min: 0, Max: 10));
                AddEffect(new Modifier(0.02, Stats.CritRate, Operators.AddValue));
                AddEffect(new Modifier(0.05, Stats.CritDamage, Operators.AddValue));
                Source = src;
            }
        }

        private void BlessedDoTApplication(object sender, StatusEffectDefinition e)
        {
            if (e is Blessed)
            {
                const int count = 3;
                var dotList = new List<StatusEffectDefinition>()
                {
                    new Registry.Bleed() { Source = e.Source, Target = e.Target, Turns = 5 },
                    new Registry.Burn() { Source = e.Source, Target = e.Target, Turns = 5 },
                    new Registry.Shock() { Source = e.Source, Target = e.Target, Turns = 5 },
                    new Registry.WindShear() { Source = e.Source, Target = e.Target, Turns = 5 }
                };
                dotList = [.. dotList.OrderBy(x => ConsoleBattleManager.RNG)];
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

        private class Bleed : StatusEffectDefinition
        {
            public Bleed() : base()
            {
                Turns = 3;
                MaxStacks = 10;
                Properties = Flags.Debuff | Flags.StartTick;
                Name = "Bleed";
            }
            public override StatusEffectDefinition Init() => AddEffect(new DamageOverTime(0.96 * Source.GetStatVersus(Stats.Atk, Target), Operators.AddValue));
        }
        private class Burn : StatusEffectDefinition
        {
            public Burn() : base()
            {
                Turns = 1;
                MaxStacks = 99;
                Properties = Flags.Debuff | Flags.StartTick;
                Name = "Burn";
            }
            public override StatusEffectDefinition Init() => AddEffect(new DamageOverTime(0.50 * Source.GetStatVersus(Stats.Atk, Target), Operators.AddValue));
        }
        private class Blessed : StatusEffectDefinition
        {
            public Blessed() : base()
            {
                Turns = 5;
                MaxStacks = 1;
                Properties = Flags.StartTick | Flags.Debuff;
                Name = "Blessed";
            }
            public override StatusEffectDefinition Init() => this;
        }
        private class SinfulCreature : StatusEffectDefinition
        {
            public SinfulCreature() : base()
            {
                Turns = 3;
                MaxStacks = 5;
                Properties = Flags.Debuff | Flags.Unremovable;
                Name = "Sinful Creature";
            }
            public override StatusEffectDefinition Init() => AddEffect(new DamageRES(-0.02));
        }
        private class DoTDamageRES : StatusEffectDefinition
        {
            public DoTDamageRES() : base()
            {
                Turns = 1;
                MaxStacks = 1;
                Properties = Flags.Unremovable | Flags.Hidden | Flags.Infinite;
                Name = "DoT Damage RES";
            }
            public override StatusEffectDefinition Init() => AddEffect(new DamageSourceRES(0.15, DamageSources.DamageOverTime));
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
            private static readonly EffectInfo[] EffectProps = [new EffectInfo<Bleed>()
            {
                Chance = 1,
                ChanceType = ChanceTypes.Base
            }];
            public Pathetic() : base(new(), AttackProps, EffectProps)
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
            private static readonly EffectInfo[] EffectProps = [new EffectInfo<Burn>()
            {
                Chance = 0.65,
                ChanceType = ChanceTypes.Base
            }];
            public ShadowFlare() : base(new(), AttackProps, EffectProps,
                count: (uint)Math.Floor(ConsoleBattleManager.RNG * 5 + 1))
            {
                WithGenericManaCost(20);
            }
        }
        public class Sacrifice : ApplyEffectOn
        {
            private class AtkBuff : StatusEffectDefinition
            {
                public AtkBuff() : base()
                {
                    Turns = 5;
                    MaxStacks = 1;
                    Properties = Flags.Unremovable;
                    Name = "Sacrifice";
                }
                public override StatusEffectDefinition Init() => AddEffect(new Modifier(350, Stats.Atk, Operators.AddValue));
            }
            private static readonly EffectInfo[] EffectProps = [new EffectInfo<AtkBuff>()];
            public Sacrifice() : base(new(), EffectProps)
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
            public Memory() : base(new(), AttackProps, null)
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
            private static readonly EffectInfo[] EffectProps = [new EffectInfo<Bleed>()
            {
                Chance = 1,
                ChanceType = ChanceTypes.Base
            }];

            public ThisEndsNow() : base(new(), AttackProps, EffectProps)
            {
                WithGenericManaCost(34);
            }
        }
        public class Devastate : AbilityDefinition
        {
            private class DevastateExplosion() : AoEAttack(new(), AttackProps, null)
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
            private class DevastateSlash1() : SingleTargetAttack(new(), AttackProps, EffectProps)
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
                private static readonly EffectInfo[] EffectProps = [new EffectInfo<Registry.Bleed>()
                {
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }];
            }
            private class DevastateSlash2() : SingleTargetAttack(new(), AttackProps, EffectProps)
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
                private static readonly EffectInfo[] EffectProps = [new EffectInfo<Blessed>()
                {
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }];
            }
            private class DevastateSlash3() : SingleTargetAttack(new(), AttackProps, EffectProps)
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
                private static readonly EffectInfo[] EffectProps = [new EffectInfo<SinfulCreature>()
                {
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }, new EffectInfo<Registry.Bleed>()
                {
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }, new EffectInfo<Registry.Burn>()
                {
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }, new EffectInfo<Registry.Shock>()
                {
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }, new EffectInfo<Registry.WindShear>()
                {
                    Chance = 1.00,
                    ChanceType = ChanceTypes.Base
                }];
            }
            public Devastate() : base(new())
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
        public RexultiPhase2(string name, ConsoleBattleManager battle) : base(name, battle, new()
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
                        new EffectInfo<Registry.Bleed>() { Chance = 1.00, ChanceType = ChanceTypes.Base },
                        new EffectInfo<Registry.Burn>() { Chance = 1.00, ChanceType = ChanceTypes.Base },
                        new EffectInfo<Registry.Shock>() { Chance = 1.00, ChanceType = ChanceTypes.Base },
                        new EffectInfo<Registry.WindShear>() { Chance = 1.00, ChanceType = ChanceTypes.Base }
                    ];
                    result.RemoveAt((int)Math.Floor(ConsoleBattleManager.RNG * 4));
                    return [.. result];
                }
            }
            public Kneel() : base(new(), AttackProps, EffectProps)
            {
                WithGenericManaCost(21);
            }
        }
        public class Court : ApplyEffectOn
        {
            private class DefenseDown : StatusEffectDefinition
            {
                public DefenseDown() : base()
                {
                    Turns = 4;
                    MaxStacks = 1;
                    Properties = Flags.Debuff;
                    Name = "Defense Down";
                }
                public override StatusEffectDefinition Init() => AddEffect(new Modifier(-0.5, Stats.Def, Operators.MultiplyBase));
            }
            private static readonly EffectInfo[] EffectProps =
                [
                    new EffectInfo<DefenseDown>(Chance: 1.00, ChanceType: ChanceTypes.Base)
                ];
            public Court() : base(new(), EffectProps)
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
            public Dance() : base(new(), AttackProps, null, 3)
            {
                WithGenericManaCost(29);
            }
        }
        #endregion
    }
}
