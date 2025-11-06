using System;
using System.Collections.Generic;
using System.Linq;

namespace FrogBattle.Classes
{
    internal abstract class Character : IHasTurn, IDamageable, IDamageSource, ISupportsEffects
    {
        public readonly Dictionary<Stats, double> Base;
        private readonly List<StatusEffect> MarkedForDeath = [];
        public readonly string _internalName;
        protected double CurrentHp;
        protected double CurrentMana;
        protected double CurrentEnergy = 0;
        public bool downed = false;
        protected List<Ability> abilityList;

        protected static ArgumentOutOfRangeException InvalidAbility(int selector) => new(nameof(selector), $"Invalid ability number: {selector}");

        #region Events
        public event EventHandler<Damage.Snapshot> DamageTaken;  // FINALLY I UNDERSTAND HOW THIS PMO SHIT WORKS
        public event EventHandler<Damage.Snapshot> DamageDealt;
        public event EventHandler<Healing> HealingReceived;
        public event EventHandler<ITakesAction> TurnStarted;
        public event EventHandler<StatusEffect> EffectGained;
        public event EventHandler<StatusEffect> EffectRemoved;
        public event EventHandler<StatusEffect> EffectApplied;
        public event EventHandler<IPoolChange> PoolChanged;
        public event EventHandler<double> HpChanged;
        public event EventHandler<Ability> AbilityLaunched;
        // Generic Event Builders
        public static class EventHelper<TEvent>
        {
            public record class EventSettings
            (
                Predicate<TEvent> Condition = null,
                uint Count = 0
            );
            /// <summary>
            /// Creates an event that triggers only when the given condition is met by its argument.
            /// </summary>
            /// <param name="predicate">Condition that needs to be fulfilled for the event to trigger.</param>
            /// <param name="action">The event to encompass in the conditional.</param>
            /// <returns>A new event.</returns>
            public static EventHandler<TEvent> ConditionalEvent(Predicate<TEvent> predicate, EventHandler<TEvent> action)
            {
                void handler(object sender, TEvent e)
                {
                    if (predicate.Invoke(e))
                    {
                        action.Invoke(sender, e);
                    }
                }
                return handler;
            }
        }
        protected static EventHandler<Ability> DoTTrigger<TTrigger>(double ratio) where TTrigger : ITrigger
        {
            return EventHelper<Ability>.ConditionalEvent(x => x is TTrigger, (sender, e) => e.Target.TakeDoTDamage(ratio));
        }
        /// <summary>
        /// Create an additional damage effect for the ability <typeparamref name="TTrigger"/>. Add this to <see cref="AbilityLaunched"/>.
        /// </summary>
        /// <param name="damage">The additional damage to take.</param>
        /// <typeparam name="TTrigger">The ability (or type of ability, like <see cref="AoEAttack"/>) which triggers additional damage.</typeparam>
        /// <returns>An <see cref="EventHandler"/> with an <see cref="Ability"/> argument.</returns>
        protected static EventHandler<Ability> AdditionalDamage<TTrigger>(Damage damage) where TTrigger : ITrigger
        {
            return EventHelper<Ability>.ConditionalEvent(x => x is TTrigger, (sender, e) => damage.Take());
        }
        /// <summary>
        /// Create a follow up effect for the ability <typeparamref name="TTrigger"/>. Add this to <see cref="AbilityLaunched"/>.
        /// </summary>
        /// <typeparam name="TTrigger">The ability (or type of ability, like <see cref="BounceAttack"/>) which triggers the follow up ability.</typeparam>
        /// <param name="followUp">The ability to insta-queue as a follow up.</param>
        /// <returns>An <see cref="EventHandler"/> with an <see cref="Ability"/> argument.</returns>
        protected static EventHandler<Ability> FollowUp<TTrigger>(Ability followUp) where TTrigger : ITrigger
        {
            return EventHelper<Ability>.ConditionalEvent(x => x is TTrigger, (sender, e) => QueueInstaAction(followUp));
        }
        protected static EventHandler<Ability> LimitedFollowUp<TTrigger>(Ability followUp, uint count) where TTrigger : ITrigger
        {
            if (count == 0) return null;
            void handler(object sender, Ability e)
            {
                if (e is TTrigger)
                {
                    QueueInstaAction(followUp);
                    e.Parent.AbilityLaunched -= handler;
                    e.Parent.AbilityLaunched += LimitedFollowUp<TTrigger>(followUp, count - 1);
                }
            }
            return handler;
        }
        #endregion
        public Character(string name, BattleManager battle, Dictionary<Stats, double> overrides = null)
        {
            _internalName = string.Join('.', typeof(Character).Name.FirstLower(), GetType().Name.FirstLower());
            Name = name;
            ParentBattle = battle;
            Base = new Dictionary<Stats, double>(Registry.DefaultStats);
            if (overrides != null)
            {
                foreach (var kvp in overrides)
                {
                    Base[kvp.Key] = kvp.Value;
                }
            }
            CurrentHp = Base[Stats.MaxHp];
            CurrentMana = Base[Stats.MaxMana] / 2;
        }
        public string Name { get; init; }
        public Pronouns Pronouns { get; init; }
        public BattleManager ParentBattle { get; init; }
        // crack cocaine team stuff that isn't even implemented yet lmfao
        public required List<Character> Team { get; init; }
        public int PositionInTeam => Team.IndexOf(this);
        public Character LeftTeammate => PositionInTeam != 0 ? Team.ElementAt(PositionInTeam - 1) : null;
        public Character RightTeammate => PositionInTeam != Team.Count - 1 ? Team.ElementAt(PositionInTeam + 1) : null;
        public required List<Character> EnemyTeam { get; init; }
        public int AbilityCount => abilityList.Count;

        public List<StatusEffect> ActiveEffects { get; } = [];
        public List<PassiveEffect> PassiveEffects { get; } = [];

        #region Pools
        public double Hp
        {
            get
            {
                return CurrentHp + this.GetActives<Overheal>().Sum(x => x.Amount);
            }
            private set
            {
                var diff = Hp - value;
                if (diff > 0)
                {
                    // If we're taking damage (difference between current Hp and target Hp is positive)
                    var overheals = this.GetActives<Overheal>();
                    foreach (var item in overheals)
                    {
                        // If there's more overheal than damage dealt, reduce overheal value and skip everything else
                        if (item.Amount > diff)
                        {
                            item.Amount -= diff;
                            return;
                        }
                        // Otherwise, deduct the overheal amount from the damage dealt,
                        // remove the overheal entirely, and continue through the list
                        diff -= item.Amount;
                        RemoveEffect(item.Parent);
                    }
                    // Implementing check for straight 0 here because if diff is equal to overheal,
                    // I still want to remove the effect instead of leaving it active on 0.
                    if (diff != 0)
                    {
                        // If our damage still wasn't beaten, CurrentHp takes the hit
                        CurrentHp -= diff;
                        if (CurrentHp < 0) Die();
                        // Notify any listeners that our HP was changed 
                        HpChanged?.Invoke(this, diff);
                    }
                }
                else
                {
                    // If we're healing, simply add the HP and check MaxHp
                    CurrentHp = Math.Min(value, GetStat(Stats.MaxHp));
                    // Notify any listeners that our HP was changed 
                    HpChanged?.Invoke(this, diff);
                }
            }
        }
        public double Mana
        {
            get
            {
                return CurrentMana;
            }
            private set
            {
                CurrentMana = Math.Min(value, GetStat(Stats.MaxMana));
                if (CurrentMana < 0) CurrentMana = 0;
            }
        }
        public double Energy
        {
            get
            {
                return CurrentEnergy;
            }
            private set
            {
                CurrentEnergy = Math.Min(value, GetStat(Stats.MaxEnergy));
                if (CurrentEnergy < 0) CurrentEnergy = 0;
            }
        }
        public double Special { get; set; }
        public double Shield
        {
            get
            {
                return ActiveEffects.Sum((x) => x.SingleEffect<Shield>()?.Amount ?? 0);
            }
            private set
            {
                if (value <= 0)
                {
                    foreach (var item in this.GetActives<Shield>())
                        RemoveEffect(item.Parent);
                    return;
                }
                double diff = Shield - value;
                if (diff > 0)
                {
                    var shields = ActiveEffects.SelectMany((x) => x.GetSubeffectsOfType<Shield>().Values);
                    foreach (var item in shields)
                    {
                        if (diff >= item.Amount)
                        {
                            diff -= item.Amount;
                            RemoveEffect(item.Parent);
                        }
                        else
                        {
                            item.Amount -= diff;
                            break;
                        }
                    }
                }
                else
                {
                    if (diff == 0) return;
                    throw new NotImplementedException();
                }
            }
        }
        public uint Barrier
        {
            get
            {
                return (uint)ActiveEffects.Sum((x) => x.SingleEffect<Barrier>()?.Count ?? 0);
            }
            private set
            {
                if ((int)value - Barrier <= 0)
                {
                    uint diff = Barrier - value;
                    foreach (var item in ActiveEffects.SelectMany((x) => x.GetSubeffectsOfType<Barrier>().Values))
                    {
                        if (diff >= item.Count)
                        {
                            diff -= item.Count;
                            RemoveEffect(item.Parent);
                        }
                        else
                        {
                            item.Count -= diff;
                            break;
                        }
                    }
                }
                else
                {
                    uint diff = value - Barrier;
                    throw new NotImplementedException();
                }
            }
        }
        #endregion
        public bool IsCrit(IDamageable target) => BattleManager.RNG < (target is Character ch ? GetStatVersus(Stats.CritRate, ch) : GetStat(Stats.CritRate));
        public uint Stun => (uint)this.GetActives<Stun>().Sum(x => x.Count);
        public double BaseActionValue => 10000 / GetStat(Stats.Spd);
        #region Predicates
        public bool InTurn { get; private set; }

        #endregion
        public void TakeInstaAction(Ability action)
        {
            if (Stun > 0) return;
            if (action.TryUse()) AbilityLaunched?.Invoke(this, action);
        }
        public void TakeAction()
        {
            InTurn = true;
            TurnStarted?.Invoke(ParentBattle, this);
            StartOfTurnChecks();
            if (downed) return;
            if (Stun > 0)
            {
                AddBattleText(Generic.Stun, Name, Stun);
            }
            else
            {
                ApplyChange(new Reward(null, this, 5, Pools.Mana, Operators.AddValue));
                while (true)
                {
                    var selection = this.Console_SelectAbility();
                    if (selection.TryUse())
                    {
                        AbilityLaunched?.Invoke(this, selection);
                        break;
                    }
                    else
                    {
                        ParentBattle.UpdateText();
                    }
                }
            }
            EndOfTurnChecks();
            InTurn = false;
        }
        public virtual void Die()
        {
            AddBattleText(_internalName + ".death", Hp);
            ParentBattle.Kill(this);
            downed = true;
        }
        public void AddBattleText(string key, params object[] args)
        {
            ParentBattle.BattleText.AppendLine(Localization.Translate(key, args));
        }
        public static void QueueInstaAction(Ability ability)
        {
            ability.Parent.ParentBattle.InstaQueue.Add(new CharacterInstaAction(ability));
        }
        internal record CharacterInstaAction(Ability Action) : ITakesAction
        {
            public void TakeAction()
            {
                Action.Parent.TakeInstaAction(Action);
            }
        }
        /// <summary>
        /// Calculates the final value for a given stat by using the base values of the fighter and the currently applied effects.
        /// Trying to calculate <see cref="Stats.None"/> throws an exception.
        /// </summary>
        /// <param name="stat">Stat for which to calculate the final value.</param>
        /// <returns>A double which represents the stat post calculations.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public double GetStat(Stats stat)
        {
            if (stat == Stats.None) throw new ArgumentOutOfRangeException(nameof(stat), "Cannot resolve stat Stats.None.");
            return Base[stat] + this.GetActivesValue(stat);
        }
        /// <summary>
        /// Calculates the final value for a given stat through GetStat() and adds any of the passives that take effect against the given target.
        /// Trying to calculate <see cref="Stats.None"/> throws an exception.
        /// </summary>
        /// <param name="stat">Stat for which to calculate the final value.</param>
        /// <param name="target">Target for which to calculate the passives.</param>
        /// <returns>A double which represents the stat post calculations.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public double GetStatVersus(Stats stat, Character target)
        {
            //Console.WriteLine($"{Name} has {GetPassivesValue(stat, target)} passives value for stat {stat} against {target.Name}");
            return GetStat(stat) + this.GetPassivesValue(stat, target);
        }
        private void StartOfTurnChecks()
        {
            //TurnStarted.Invoke(this, this); not exactly the right place for it
            TakeDoTDamage();
            MarkedForDeath.AddRange(ActiveEffects.FindAll(x => !x.Is(StatusEffect.Flags.StartTick)));
            foreach (var item in ActiveEffects.FindAll(x => x.Is(StatusEffect.Flags.StartTick)))
            {
                if (item.Expire()) ActiveEffects.Remove(item);
            }
        }
        private void EndOfTurnChecks()
        {
            foreach (var item in MarkedForDeath)
            {
                if (item.Expire()) ActiveEffects.Remove(item);
            }
            MarkedForDeath.Clear();
        }
        public IEnumerable<Damage> DoTCalculations()
        {
            return ActiveEffects.Select(x => x.SingleEffect<DamageOverTime>()?.GetDamage()).Where(x => x != null);
        }
        public List<Healing> RegenCalculations()
        {
            return [.. ActiveEffects.Select(x => x.SingleEffect<HealingOverTime>()?.GetHealing()).Where(x => x != null)];
        }
        public void TakeDoTDamage(double ratio = 1)
        {
            var dotList = DoTCalculations();
            double totalDamage = 0;
            foreach (var item in dotList)
            {
                totalDamage += item.Take(ratio);
            }
            if (totalDamage > 0) AddBattleText("character.generic.dot", this, totalDamage);
        }
        public void AddEffect(IAttributeModifier effect)
        {
            switch (effect)
            {
                case StatusEffect SEff:
                    EffectGained?.Invoke(SEff.Source, SEff);
                    SEff.Source?.EffectApplied?.Invoke(this, SEff);
                    var s_idx = ActiveEffects.IndexOf(SEff);
                    if (s_idx > -1)
                    {
                        MarkedForDeath.Remove(ActiveEffects[s_idx]);
                        ActiveEffects[s_idx] = SEff.AddMutables(ActiveEffects[s_idx]);
                    }
                    else ActiveEffects.Add(SEff);
                    break;
                case PassiveEffect PEff:
                    var p_idx = PassiveEffects.IndexOf(PEff);
                    if (p_idx > -1)
                    {
                        PassiveEffects[p_idx] = PEff;
                    }
                    else PassiveEffects.Add(PEff);
                    break;
                default:
                    throw new ArgumentException($"Invalid effect type: {effect.GetType()} (not a StatusEffect nor PassiveEffect)", nameof(effect));
            }
        }
        public void RemoveEffect(IAttributeModifier effect)
        {
            switch (effect)
            {
                case StatusEffect SEff:
                    EffectRemoved?.Invoke(this, SEff);
                    var sameEff = ActiveEffects.Find(x => x == SEff);
                    if (sameEff is not null)
                    {
                        if (sameEff.Properties.HasFlag(StatusEffect.Flags.RemoveStack) && sameEff.Stacks > SEff.Stacks)
                        {
                            sameEff.Stacks -= SEff.Stacks;
                        }
                        else ActiveEffects.Remove(SEff);
                    }
                    break;
                case PassiveEffect PEff:
                    PassiveEffects.Remove(PEff);
                    break;
                default:
                    throw new ArgumentException($"Invalid effect type: {effect.GetType()} (not a StatusEffect nor PassiveEffect)", nameof(effect));
            }
        }
        public void DealDamage(Damage.Snapshot damage)
        {
            DamageDealt?.Invoke(this, damage);
        }
        public void TakeDamage(Damage.Snapshot damage)
        {
            var damageAmount = damage.Amount;
            if (damageAmount <= 0) return;
            DamageTaken?.Invoke(damage.Source, damage);   // YES I FIGURED OUT EVENTS LETS GOO
            if (Barrier > 0)
            {
                Barrier -= 1;
                return;
            }
            if (Shield > 0)
            {
                damageAmount -= Shield;
                Shield = -damageAmount;
            }
            if (damageAmount > 0) Hp -= damageAmount;
        }
        public uint TakeBarrierDamage(Damage.Snapshot damage)
        {
            return Barrier -= 1;
        }
        public double TakeShieldDamage(Damage.Snapshot damage)
        {
            var damageAmount = damage.Amount;
            damageAmount -= Shield;
            Shield = -damageAmount;
            if (damageAmount > 0) return damageAmount;
            else return 0;
        }
        public double TakeHealing(Healing healing, double ratio = 1)
        {
            HealingReceived.Invoke(healing.Source, healing);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Applies a pool change to this fighter.
        /// </summary>
        /// <param name="change">Change to apply.</param>
        public void ApplyChange(IPoolChange change)
        {
            switch (change.Pool)
            {
                case Pools.Hp:
                    Hp += change.Amount;
                    break;
                case Pools.Mana:
                    Mana += change.Amount;
                    break;
                case Pools.Energy:
                    Energy += change.Amount;
                    break;
                case Pools.Special:
                    Special += change.Amount;
                    break;
                case Pools.Shield:
                    Shield += change.Amount;
                    break;
                case Pools.Barrier:
                    Barrier += (uint)change.Amount;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown pool item \"{change.Pool}\"");
            }
            PoolChanged?.Invoke(null, change);
        }
        public Ability SelectAbility(Character target, int selector)
        {
            return abilityList[selector];
        }
        public abstract Ability LoadAbility(Character target, int selector);
        public abstract void LoadAbilities(Character target);
    }
}
