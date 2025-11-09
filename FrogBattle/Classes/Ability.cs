using FrogBattle.Classes.BattleManagers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FrogBattle.Classes
{
    // rewrite #7 gazillion lmfao
    internal abstract class AbilityDefinition : ITrigger
    {
        public AbilityDefinition(AbilityInfo props)
        {
            Properties = props;
            Name = Localization.Translate($"character.{props.Owner.Name}.ability.{GetType().Name}".camelCase('.'));
        }
        public string Name { get; protected init; }
        public string Description { get; protected init; }
        public AbilityInfo Properties { get; init; }
        public Dictionary<Pools, Cost> Costs { get; } = [];
        public Dictionary<Pools, Reward> Rewards { get; } = [];
        public Dictionary<object, Requirement> Requirements { get; } = [];
        
        public AbilityInstance GetInstance(Character user, Character target) => new(this, user, target);

        /// <summary>
        /// Use the ability.
        /// </summary>
        /// <returns>True if used successfully, false if missed.</returns>
        public abstract bool Use(AbilityInstance ctx);

        #region Cost Methods

        public AbilityDefinition WithCost(Cost cost)
        {
            Costs[cost.Pool] = cost;
            return this;
        }
        public AbilityDefinition WithReward(Reward reward)
        {
            Rewards[reward.Pool] = reward;
            return this;
        }
        public AbilityDefinition WithRequirement(Requirement requirement)
        {
            Requirements[requirement.GetKey()] = requirement;
            return this;
        }
        /// <summary>
        /// Attaches a PoolAmount condition to the ability and its corresponding cost.
        /// </summary>
        /// <returns>This ability.</returns>
        protected AbilityDefinition WithGenericCost(PoolRequirement requirement)
        {
            return WithRequirement(requirement).WithCost(requirement.GetCost());
        }
        /// <summary>
        /// Attaches a <see cref="Pools.Mana"/> amount condition, its corresponding cost, and energy generation. Percentage configurable.
        /// The amount of energy restored has a flat 10 added to it.
        /// </summary>
        /// <param name="amount">Base amount of Mana that this ability requires.</param>
        /// <param name="energyGenPercent">The percentage of base mana to gain in energy.</param>
        /// <returns>This.</returns>
        protected AbilityDefinition WithGenericManaCost(double amount, double energyGenPercent = 0.4)
        {
            return WithGenericCost(new PoolRequirement(amount, Pools.Mana, Operators.AddValue)).WithReward(new Reward(amount * energyGenPercent + Math.Min(amount, 10), Pools.Energy, Operators.AddValue));
        }
        /// <summary>
        /// Attaches an <see cref="Pools.Energy"/> amount condition and cost corresponding to MaxEnergy.
        /// </summary>
        /// <returns>This.</returns>
        protected AbilityDefinition WithBurstCost()
        {
            var requirement = new PoolRequirement(1, Pools.Energy, Operators.MultiplyBase);
            return WithGenericCost(requirement);
        }

        #endregion
    }
    // rewrite #8 gazillion babyyyyy
    internal sealed class AbilityInstance : IHasTarget
    {
        public Character User { get; }
        public Character Target { get; }
        public AbilityDefinition Definition { get; }

        public AbilityInstance(AbilityDefinition ability, Character user, Character target)
        {
            User = user;
            Target = target;
            Definition = ability;
        }
        /// <summary>
        /// Tries using the ability. If conditions are not met returns false.
        /// Whether the ability was used successfully or missed does not influence the return value.
        /// </summary>
        /// <returns>True if the turn can continue, false otherwise.</returns>
        public bool TryUse()
        {
            foreach (var item in Definition.Requirements)
            {
                if (!item.Value.Check(this))
                {
                    AddText("conditions.missing.generic", Localization.Translate(item.Value switch
                    {
                        StatRequirement st => "stats." + st.Stat.ToString().FirstLower(),
                        PoolRequirement pl => "pools." + pl.Pool.ToString().FirstLower(),
                        Requirement rq => "generic"
                    }));
                    return false;
                }
            }
            foreach (var item in Definition.Costs)
            {
                User.ApplyChange(item.Value, Target);
            }
            if (!Definition.Use(this)) return true;
            foreach (var item in Definition.Rewards)
            {
                User.ApplyChange(item.Value, Target);
            }
            return true;
        }
        public void AddText(string format, params object[] args) => User.ParentBattle.BattleText.AppendLine(Localization.Translate(format, args));
        public Dictionary<TextTypes, string> GetFlavourText() => Enum.GetValues(typeof(TextTypes)).Cast<TextTypes>().Select(x => new KeyValuePair<TextTypes, string>(x, string.Join('.', User._internalName, "ability", GetType().Name.FirstLower(), "text", x.ToString().FirstLower()))).Where(x => Localization.strings.ContainsKey(x.Value)).ToDictionary();
    }
    // oh god
    internal abstract record class AbilityHelper(Character User, Character Target)
    {
        protected void AddText(string key, params object[] args)
        {
            User.ParentBattle.BattleText.AppendLine(Localization.Translate(key, args));
        }
    }
    internal record class AttackHelper(Character Parent, Character Target, AttackInfo AttackInfo, EffectInfo[] EffectInfos) : AbilityHelper(Parent, Target), IAttack, IAppliesEffect
    {
        public AttackHelper(Character user, Character target, IAttack src, double falloff = 0) : this(user, target, src.AttackInfo with { Ratio = Math.Max(0, src.AttackInfo.Ratio - falloff) }, src is IAppliesEffect ef ? ef.EffectInfos : null) { }
        private IEnumerator<StatusEffectInstance> ApplyEffects()
        {
            if (Target == null) yield break;
            if (EffectInfos == null) yield break;
            foreach (var effect in EffectInfos)
            {
                yield return effect.Apply(User, Target);
            }
            yield break;
        }
        private bool IsHit() => ConsoleBattleManager.RNG < AttackInfo.HitRate + User.GetStatVersus(Stats.HitRateBonus, Target) - Target.GetStatVersus(Stats.Dex, User) / 100;
        /// <summary>
        /// Creates a series of damages for the given target.
        /// </summary>
        /// <returns>An enumerator. Advance </returns>
        private IEnumerator<Damage> Init()
        {
            if (Target == null) yield break;
            if (!AttackInfo.IndependentHitRate && !IsHit()) yield break;
            if (AttackInfo.Split == null || AttackInfo.Split.Length == 0)
            {
                yield return new Damage(User, Target, AttackInfo.Ratio * User.GetStatVersus(AttackInfo.Scalar, Target), AttackInfo.DamageInfo);
                yield break;
            }
            long sum = AttackInfo.Split.Sum(x => x);
            foreach (var i in AttackInfo.Split)
            {
                yield return (!AttackInfo.IndependentHitRate || IsHit()) ? new Damage(User, Target, AttackInfo.Ratio * User.GetStatVersus(AttackInfo.Scalar, Target) * i / sum, AttackInfo.DamageInfo) : Damage.Missed;
            }
            yield break;
        }
        /// <summary>
        /// Uses private Init() to create and then apply a series of damages for the given target,
        /// as well as add corresponding battle text and apply effects.
        /// </summary>
        /// <param name="text">Flavour text dictionary.</param>
        /// <returns>Each instance of damage dealt.</returns>
        public IEnumerator<Damage> FancyInit(Dictionary<TextTypes, string> text)
        {
            // Make sure there's anyone to attack
            if (Target == null) yield break;
            // Create attack helper instance
            var attack = Init();
            // If there is no first element to the collection, means we missed without IHR, so it's over
            if (!attack.MoveNext())
            {
                // Ability miss text
                if (text.TryGetValue(TextTypes.Miss, out var missKey)) AddText(missKey, User, Target);
                else AddText(Generic.Miss, User, Target);
                yield break;
            }
            // Index for displaying damage text
            int index = 0;
            // Check to make sure we hit anything
            var hit = false;
            // Use do-while so we can freely check the first element for a miss. Perfect!
            do
            {
                if (attack.Current != Damage.Missed)
                {
                    hit = true;
                    // Ability damage text
                    if (text.TryGetValue((TextTypes)(++index), out var damageKey)) AddText(damageKey, User, Target, attack.Current);
                    else AddText(Generic.Damage, User, Target, attack.Current);
                    // Pause only when we deal damage
                    yield return attack.Current;
                    attack.Current.Take();
                }
                else
                {
                    // Ability miss text...?
                    if (text.TryGetValue(TextTypes.Miss, out var missKey)) AddText(missKey, User, Target);
                    else AddText(Generic.Miss, User, Target);
                }
            } while (attack.MoveNext());
            // If we missed everything, tough luck.
            if (!hit) yield break;
            // Try to apply the effects to the target after dealing the damage, knowing we hit at least once
            var effects = ApplyEffects();
            while (effects.MoveNext())
            {
                if (effects.Current is not null)
                {
                    Target.AddEffect(effects.Current);
                    // Ability buff/debuff application text
                    var displayEffect = Target.ActiveEffects.FirstOrDefault(x => x == effects.Current);
                    if (text.TryGetValue(TextTypes.ApplyEffect, out var effectKey)) AddText(effectKey, User, Target, displayEffect);
                    else AddText(Generic.ApplyEffect, User, Target, displayEffect);
                }
            }
            yield break;
        }
    }
    internal record class EffectHelper(Character Parent, Character Target, EffectInfo[] EffectInfos) : AbilityHelper(Parent, Target), IAppliesEffect
    {
        public EffectHelper(Character user, Character target, IAppliesEffect src) : this(user, target, src.EffectInfos) { }
        private IEnumerator<StatusEffectInstance> ApplyEffects()
        {
            if (Target == null) yield break;
            if (EffectInfos == null) yield break;
            foreach (var effect in EffectInfos)
            {
                yield return effect.Apply(User, Target);
            }
            yield break;
        }
        public IEnumerator<StatusEffectInstance> Init(Dictionary<TextTypes, string> text)
        {
            // Try to apply the effects
            var effects = ApplyEffects();
            while (effects.MoveNext())
            {
                if (effects.Current is not null)
                {
                    Target.AddEffect(effects.Current);
                    // Ability buff/debuff application text
                    var displayEffect = Target.ActiveEffects.FirstOrDefault(x => x == effects.Current);
                    if (text.TryGetValue(TextTypes.ApplyEffect, out var effectKey)) AddText(effectKey, User, Target, displayEffect);
                    else AddText(Generic.ApplyEffect, User, Target, displayEffect);
                    yield return effects.Current;
                }
            }
            yield break;
        }
    }

    internal abstract class SingleTargetAttack(AbilityInfo properties, AttackInfo attackInfo, EffectInfo[] effectInfos) : AbilityDefinition(properties), IAttack, IAppliesEffect
    {
        public AttackInfo AttackInfo { get; } = attackInfo;
        public EffectInfo[] EffectInfos { get; } = effectInfos ?? [];

        protected virtual void DealtDamage(AbilityInstance ctx, Damage damage) { }
        public override bool Use(AbilityInstance ctx)
        {
            // Get the list for flavour text
            var text = ctx.GetFlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) ctx.AddText(startKey, ctx.User, ctx.Target);
            // Create attack helper instance
            var helper = new AttackHelper(ctx.User, ctx.Target, this);
            var attack = helper.FancyInit(text);
            // Abort if we miss the target
            if (!attack.MoveNext()) return false;
            // Calculate damage
            double finalDamage = 0;
            do { DealtDamage(ctx, attack.Current); finalDamage += attack.Current.Amount; } while (attack.MoveNext());
            // Ability end text
            if (text.TryGetValue(TextTypes.End, out var endKey)) ctx.AddText(endKey, ctx.User, ctx.Target, finalDamage);
            return true;
        }
    }

    internal abstract class BlastAttack(AbilityInfo properties, AttackInfo attackInfo, EffectInfo[] effectInfos, double falloff) : AbilityDefinition(properties), IAttack, IAppliesEffect
    {
        public double Falloff { get; init; } = falloff;
        public AttackInfo AttackInfo { get; } = attackInfo;
        public EffectInfo[] EffectInfos { get; } = effectInfos ?? [];

        protected virtual void DealtDamage(AbilityInstance ctx, Damage damage) { }
        public override bool Use(AbilityInstance ctx)
        {
            // Get the list for flavour text
            var text = ctx.GetFlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) ctx.AddText(startKey, ctx.User, ctx.Target);
            double finalDamage = 0;
            // Create new attacks for the side targets with falloff
            var helperM = new AttackHelper(ctx.User, ctx.Target, this);
            var helperR = new AttackHelper(ctx.User, ctx.Target, this, Falloff) { Target = ctx.Target.RightTeammate };
            var helperL = new AttackHelper(ctx.User, ctx.Target, this, Falloff) { Target = ctx.Target.LeftTeammate };
            var middleAttack = helperM.FancyInit(text);
            var rightAttack = helperR.FancyInit(text);
            var leftAttack = helperL.FancyInit(text);
            // We care mainly about the middle target - miss that and we're cooked. Otherwise continue.
            if (!middleAttack.MoveNext()) return false;
            // Manually move the other two
            rightAttack.MoveNext();
            leftAttack.MoveNext();
            // Use | instead of || to avoid shortcircuiting and exhaust each branch at the same time while adding up their damages
            do { DealtDamage(ctx, leftAttack.Current); DealtDamage(ctx, middleAttack.Current); DealtDamage(ctx, rightAttack.Current); finalDamage += (middleAttack.Current?.Amount ?? 0) + (rightAttack.Current?.Amount ?? 0) + (leftAttack.Current?.Amount ?? 0); } while (middleAttack.MoveNext() | leftAttack.MoveNext() | rightAttack.MoveNext());
            // Ability end text
            if (text.TryGetValue(TextTypes.End, out var endKey)) ctx.AddText(endKey, ctx.User, ctx.Target, finalDamage);
            return true;
        }
    }

    internal abstract class BounceAttack(AbilityInfo properties, AttackInfo attackInfo, EffectInfo[] effectInfos, uint count) : AbilityDefinition(properties), IAttack, IAppliesEffect
    {
        public uint Count { get; } = count;
        public AttackInfo AttackInfo { get; } = attackInfo;
        public EffectInfo[] EffectInfos { get; } = effectInfos ?? [];

        protected virtual void DealtDamage(AbilityInstance ctx, Damage damage) { }
        public override bool Use(AbilityInstance ctx)
        {
            // Get the lists for flavour text
            var text = ctx.GetFlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) ctx.AddText(startKey, ctx.User, ctx.Target, Count);
            // Create helper instance
            var helper = new AttackHelper(ctx.User, ctx.Target, this);
            // Initialise damage for testing whether we hit anything
            double finalDamage = 0;
            for (int i = 0; i < Count; i++)
            {
                var attack = helper.FancyInit(text);
                // Skip if we miss (use break; to end bounces on miss)
                if (!attack.MoveNext()) continue;
                // Add up damage
                do { DealtDamage(ctx, attack.Current); finalDamage += attack.Current.Amount; } while (attack.MoveNext());
                // Move to random target
                helper = helper with { Target = ctx.Target.Team.OrderBy(x => ConsoleBattleManager.RNG).First() };
            }
            if (finalDamage == 0) return false;
            // Ability end text
            if (text.TryGetValue(TextTypes.End, out var endKey)) ctx.AddText(endKey, ctx.User, ctx.Target, finalDamage);
            return true;
        }
    }

    internal abstract class AoEAttack(AbilityInfo properties, AttackInfo attackInfo, EffectInfo[] effectInfos) : AbilityDefinition(properties), IAttack, IAppliesEffect
    {
        public AttackInfo AttackInfo { get; } = attackInfo;
        public EffectInfo[] EffectInfos { get; } = effectInfos ?? [];

        protected virtual void DealtDamage(AbilityInstance ctx, Damage damage) { }
        public override bool Use(AbilityInstance ctx)
        {
            // Get the lists for flavour text
            var text = ctx.GetFlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) ctx.AddText(startKey, ctx.User, ctx.Target);
            // Create attack helper instances for every target
            var attacks = new List<IEnumerator<Damage>>();
            var helpers = new List<AttackHelper>();
            foreach (var item in ctx.Target.Team)
            {
                helpers.Add(new(ctx.User, item, this));
                attacks.Add(helpers.Last().FancyInit(text));
            }
            double finalDamage = 0;
            do { attacks.ForEach(x => DealtDamage(ctx, x.Current)); finalDamage += attacks.Where(x => x.Current != null).Sum(x => x.Current.Amount); } while (attacks.Select(x => x.MoveNext()).Any(x => x));
            if (finalDamage == 0) return false;
            // Ability end text
            if (text.TryGetValue(TextTypes.End, out var endKey)) ctx.AddText(endKey, ctx.User, ctx.Target, finalDamage);
            return true;
        }
    }

    internal abstract class ApplyEffectOn(AbilityInfo properties, EffectInfo[] effectInfos) : AbilityDefinition(properties), IAppliesEffect
    {
        public EffectInfo[] EffectInfos { get; } = effectInfos;
        public override bool Use(AbilityInstance ctx)
        {
            // Get the list for flavour text
            var text = ctx.GetFlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) ctx.AddText(startKey, ctx.User, ctx.Target);
            // Create effect helper instance
            var helper = new EffectHelper(ctx.User, ctx.Target, this);
            var effect = helper.Init(text);
            // Effect application and flavour text are handled by the helper
            do { } while (effect.MoveNext());
            // Ability end text
            if (text.TryGetValue(TextTypes.End, out var endKey)) ctx.AddText(endKey, ctx.User, ctx.Target);
            return true;
        }
    }

    internal abstract class BuffTeam(AbilityInfo properties, EffectInfo[] effectInfos) : AbilityDefinition(properties), IAppliesEffect
    {
        public EffectInfo[] EffectInfos { get; } = effectInfos;
        public override bool Use(AbilityInstance ctx)
        {
            // Get the list for flavour text
            var text = ctx.GetFlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) ctx.AddText(startKey, ctx.User, ctx.Target);
            foreach (var target in ctx.Target.Team)
            {
                // Create effect helper instance
                var helper = new EffectHelper(ctx.User, target, this);
                var effect = helper.Init(text);
                // Effect application and flavour text are handled by the helper
                do { } while (effect.MoveNext());
            }
            // Ability end text
            if (text.TryGetValue(TextTypes.End, out var endKey)) ctx.AddText(endKey, ctx.User, ctx.Target);
            return true;
        }
    }

    internal abstract class HealTarget(AbilityInfo properties, HealingInfo healingInfo, EffectInfo[] effectInfos) : AbilityDefinition(properties), IHealing, IAppliesEffect
    {
        public HealingInfo HealingInfo { get; } = healingInfo;
        public EffectInfo[] EffectInfos { get; } = effectInfos ?? [];
        public override bool Use(AbilityInstance ctx)
        {
            var text = ctx.GetFlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) ctx.AddText(startKey, ctx.User, ctx.Target);
            // Ability heal text
            var healing = new Healing(ctx.User, ctx.Target, HealingInfo);
            if (text.TryGetValue(TextTypes.Healing, out var healingKey)) ctx.AddText(healingKey, ctx.User, ctx.Target, healing);
            else ctx.AddText(Generic.Healing, ctx.User, ctx.Target, healing);
            double finalHealing = healing.Take();
            // Create effect helper instance
            var helper = new EffectHelper(ctx.User, ctx.Target, this);
            var effect = helper.Init(text);
            // Effect application and flavour text are handled by the helper
            do { } while (effect.MoveNext());
            // Ability end text
            if (text.TryGetValue(TextTypes.Start, out var endKey)) ctx.AddText(endKey, ctx.User, ctx.Target, finalHealing);
            return true;
        }
    }

    internal abstract class HealTeam(AbilityInfo properties, HealingInfo healingInfo, EffectInfo[] effectInfos) : AbilityDefinition(properties), IHealing, IAppliesEffect
    {
        public HealingInfo HealingInfo { get; } = healingInfo;
        public EffectInfo[] EffectInfos { get; } = effectInfos ?? [];
        public override bool Use(AbilityInstance ctx)
        {
            var text = ctx.GetFlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) ctx.AddText(startKey, ctx.User, ctx.Target);
            double finalHealing = 0;
            foreach (var target in ctx.Target.Team)
            {
                var healing = new Healing(ctx.User, target, HealingInfo);
                if (text.TryGetValue(TextTypes.Healing, out var healingKey)) ctx.AddText(healingKey, ctx.User, ctx.Target, healing);
                else ctx.AddText(Generic.Healing, ctx.User, ctx.Target, healing);
                finalHealing = healing.Take();
                // Create effect helper instance
                var helper = new EffectHelper(ctx.User, ctx.Target, this) { Target = target };
                var effect = helper.Init(text);
                // Effect application and flavour text are handled by the helper
                do { } while (effect.MoveNext());
            }
            // Ability end text
            if (text.TryGetValue(TextTypes.Start, out var endKey)) ctx.AddText(endKey, ctx.User, ctx.Target, finalHealing);
            return true;
        }
    }

    internal abstract class FollowUpSetup(AbilityInfo properties, params EventHandler<AbilityInstance>[] bonusEffects) : AbilityDefinition(properties)
    {
        public EventHandler<AbilityInstance>[] BonusEffects { get; } = bonusEffects;
        public override bool Use(AbilityInstance ctx)
        {
            // Create text
            var text = ctx.GetFlavourText();
            // Ability launch text
            if (text.TryGetValue(TextTypes.Start, out var startKey)) ctx.AddText(startKey, ctx.User, ctx.Target);
            foreach (var item in BonusEffects) ctx.User.AbilityLaunched += item;
            // Ability end text
            if (text.TryGetValue(TextTypes.End, out var endKey)) ctx.AddText(endKey, ctx.User, ctx.Target);
            return true;
        }
    }

    internal sealed class SkipTurn : AbilityDefinition
    {
        public SkipTurn() : base(new())
        {
            WithReward(new Reward(5, Pools.Mana, Operators.AddValue));
        }
        public override bool Use(AbilityInstance ctx)
        {
            ctx.AddText("character.generic.skip", ctx.User, Rewards.Single().Value.GetAmount(ctx));
            return true;
        }
    }
}
