using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal abstract class Character : IHasTurn
    {
        public readonly Dictionary<Stats, double> Base;
        protected double CurrentHp;
        protected double CurrentMana;
        protected double CurrentEnergy = 0;
        private readonly List<StatusEffect> MarkedForDeath = [];
        public readonly string _internalName;
        
        #region Events
        public event EventHandler<Damage.DamageSnapshot> DamageTaken;  // FINALLY I UNDERSTAND HOW THIS PMO SHIT WORKS
        public event EventHandler<Damage.DamageSnapshot> DamageDealt;
        public event EventHandler<Healing> HealingReceived;
        public event EventHandler<ITakesAction> TurnStarted;
        public event EventHandler<StatusEffect> EffectGained;
        public event EventHandler<StatusEffect> EffectRemoved;
        public event EventHandler<StatusEffect> EffectApplied;
        public event EventHandler<IPoolChange> PoolChanged;
        public event EventHandler<double> HpChanged;
        public event EventHandler<InstaAction> QueueInstantAction;
        public event EventHandler<Ability> AbilityLaunched;
        // Generic Event Builders
        /// <summary>
        /// Create a DoT trigger effect for the ability <typeparamref name="TAbility"/>. Add this to <see cref="AbilityLaunched"/>.
        /// </summary>
        /// <typeparam name="TAbility">The ability which triggers DoT calculations.</typeparam>
        /// <param name="ratio">The ratio at which the DoT damage is taken.</param>
        /// <returns>An <see cref="EventHandler"/> with an <see cref="Ability"/> argument.</returns>
        protected static EventHandler<Ability> DoTTrigger<TAbility>(double ratio) where TAbility : Ability
        {
            return delegate (object sender, Ability e)
            {
                if (e is TAbility)
                {
                    var damages = e.Target.DoTCalculations();
                    foreach (var damage in damages)
                    {
                        e.Target.TakeDamage(damage, ratio);
                    }
                }
            };
        }
        protected static EventHandler<Ability> AdditionalDamage<TAbility>(Damage damage)
        {
            return delegate
            {
                damage.Take();
            };
        }
        #endregion
        public Character(string name, BattleManager battle, bool IS_TEAM_1, Dictionary<Stats, double> overrides = null)
        {
            _internalName = string.Join('.', typeof(Character).Name.camelCase(), GetType().Name.camelCase());
            Name = name;
            ParentBattle = battle;
            if (IS_TEAM_1)
            {
                Team = ParentBattle.Team1;
                EnemyTeam = ParentBattle.Team2;
            }
            else
            {
                EnemyTeam = ParentBattle.Team1;
                Team = ParentBattle.Team2;
            }
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
        public List<Character> Team { get; init; }
        public int PositionInTeam => Team.IndexOf(this);
        public Character LeftTeammate => PositionInTeam != 0 ? Team.ElementAt(PositionInTeam - 1) : null;
        public Character RightTeammate => PositionInTeam != Team.Count - 1 ? Team.ElementAt(PositionInTeam + 1) : null;
        public List<Character> EnemyTeam { get; init; }

        public List<StatusEffect> StatusEffects { get; } = [];
        public List<PassiveEffect> PassiveEffects { get; } = [];

        #region Pools
        public double Hp
        {
            get
            {
                return CurrentHp + GetActives<Overheal>().Sum(x => x.Amount);
            }
            private set
            {
                var diff = Hp - value;
                if (diff > 0)
                {
                    // If we're taking damage (difference between current Hp and target Hp is positive)
                    var overheals = GetActives<Overheal>();
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
                        // Notify any listeners that our HP was changed 
                        HpChanged?.Invoke(this, diff);
                    }
                }
                else
                {
                    // If we're healing, simply add the Hp and check MaxHp
                    CurrentHp = Math.Min(value, GetStat(Stats.MaxHp));
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
                return GetActives().Sum((x) => x.SingleEffect<Shield>()?.Amount ?? 0);
            }
            private set
            {
                if (value <= 0)
                {
                    foreach (var item in GetActives<Shield>())
                        RemoveEffect(item.Parent);
                    return;
                }
                double diff = Shield - value;
                if (diff > 0)
                {
                    var shields = GetActives().SelectMany((x) => x.GetSubeffectsOfType<Shield>().Values);
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
                return (uint)GetActives().Sum((x) => x.SingleEffect<Barrier>()?.Count ?? 0);
            }
            private set
            {
                if ((int)value - Barrier <= 0)
                {
                    uint diff = Barrier - value;
                    foreach (var item in GetActives().SelectMany((x) => x.GetSubeffectsOfType<Barrier>().Values))
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
        public bool IsCrit(Character target) => BattleManager.RNG < GetStatVersus(Stats.CritRate, target);
        public uint Stun => (uint)GetActives<Stun>().Sum(x => x.Count);
        public double BaseActionValue => 10000 / GetStat(Stats.Spd);
        #region Predicates
        public bool InTurn { get; private set; }

        #endregion
        public bool TakeInstaAction(Ability action)
        {
            if (Stun > 0) return true;
            return action.TryUse();
        }
        public void TakeAction()
        {
            InTurn = true;
            StartOfTurnChecks();
            if (Stun > 0)
            {
                ParentBattle.BattleText.AppendLine(Localization.Translate(Generic.Stun, Name, Stun));
            }
            else
            {
                ApplyChange(new Reward(null, this, 5, Pools.Mana, Operators.Additive));
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
        }
        public void AddBattleText(string key, params object[] args)
        {
            ParentBattle.BattleText.AppendLine(Localization.Translate(key, args));
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
            return Base[stat] + GetActivesValue(stat);
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
            return GetStat(stat) + GetPassivesValue(stat, target);
        }
        private void StartOfTurnChecks()
        {
            TakeDoTDamage();
            MarkedForDeath.AddRange(StatusEffects.FindAll(x => !x.Is(StatusEffect.Flags.StartTick)));
            foreach (var item in StatusEffects.FindAll(x => x.Is(StatusEffect.Flags.StartTick)))
            {
                if (item.Expire()) StatusEffects.Remove(item);
            }
        }
        private void EndOfTurnChecks()
        {
            foreach (var item in MarkedForDeath)
            {
                if (item.Expire()) StatusEffects.Remove(item);
            }
            MarkedForDeath.Clear();
        }
        public List<Damage> DoTCalculations()
        {
            return StatusEffects.Select(x => x.SingleEffect<DamageOverTime>()?.GetDamage()).Where(x => x != null).ToList();
        }
        public List<Healing> RegenCalculations()
        {
            return StatusEffects.Select(x => x.SingleEffect<HealingOverTime>()?.GetHealing()).Where(x => x != null).ToList();
        }
        public void TakeDoTDamage(double ratio = 1)
        {
            var dotList = DoTCalculations();
            double totalDamage = 0;
            foreach (var item in dotList)
            {
                totalDamage += item.Amount;
                TakeDamage(item, ratio);
            }
            if (totalDamage > 0) AddBattleText("character.generic.dot", this, totalDamage);
        }
        public void AddEffect(IAttributeModifier effect)
        {
            if (effect is StatusEffect eff)
            {
                if (eff == null) return;
                EffectGained?.Invoke(eff.Source, eff);
                eff.Source.EffectApplied?.Invoke(this, eff);
                var sameEff = GetActives().Find(x => x == eff);
                if (sameEff != null)
                {
                    eff.Stacks += sameEff.Stacks;
                    if (eff.SingleEffect<Shield>() != null) eff.SingleEffect<Shield>().Amount += sameEff.SingleEffect<Shield>().Amount;
                    if (eff.SingleEffect<Barrier>() != null) eff.SingleEffect<Barrier>().Count += sameEff.SingleEffect<Barrier>().Count;
                    StatusEffects[StatusEffects.IndexOf(sameEff)] = eff;
                }
                else StatusEffects.Add(eff);
            }
            else PassiveEffects.Add(effect as PassiveEffect);
        }
        public void RemoveEffect(IAttributeModifier effect)
        {
            if (effect is StatusEffect eff)
            {
                if (eff == null) return;
                EffectRemoved?.Invoke(this, eff);
                var sameEff = StatusEffects.Find(x => x == eff);
                if (sameEff != null)
                {
                    if (sameEff.Properties.HasFlag(StatusEffect.Flags.RemoveStack) && sameEff.Stacks > eff.Stacks)
                    {
                        sameEff.Stacks -= eff.Stacks;
                    }
                    else StatusEffects.Remove(eff);
                }
            }
            else PassiveEffects.Remove(effect as PassiveEffect);
        }
        /// <summary>
        /// Decides whether an attack hits based on the given hit rate and bonuses of the fighter.
        /// Null hit rate guarantees a hit.
        /// </summary>
        /// <param name="hitRate">The base hit rate of the attack.</param>
        /// <returns>True if RNG is less than hitRate or hitRate is null, false otherwise.</returns>
        private bool IsHit(double? hitRate)
        {
            return BattleManager.RNG < hitRate + GetStat(Stats.HitRateBonus) || hitRate == null;
        }
        public double TakeDamage(Damage damage, double ratio = 1)
        {
            if (damage.Target != this) throw new ArgumentException($"Mismatch between Damage instance target {damage.Target} and TakeDamage method target {this}.", nameof(damage));
            return TakeDamage(damage.GetSnapshot(ratio));
        } // reaching obsolescence as i think more and more about just snapshotting damage instead
        public double TakeDamage(Damage.DamageSnapshot damage)
        {
            // to do: implement final damage calculations for additional damage calculations
            var damageAmount = damage.Amount;
            if (damageAmount <= 0) return 0;
            DamageTaken?.Invoke(damage.Source, damage);   // YES I FIGURED OUT EVENTS LETS GOO
            damage.Source.DamageDealt?.Invoke(this, damage);
            if (Barrier > 0)
            {
                Barrier -= 1;
                return 0;
            }
            if (Shield > 0)
            {
                damageAmount -= Shield;
                Shield = -damageAmount;
            }
            if (damageAmount > 0) Hp -= damageAmount;
            return damage.Amount;
        }
        public double TakeHealing(Healing healing, double ratio = 1)
        {
            throw new NotImplementedException();
        }

        #region Effect Methods

        // Actives

        /// <summary>
        /// Searches <see cref="StatusEffects"/> for all effects.
        /// </summary>
        /// <returns>Every effect applied to this fighter.</returns>
        public List<StatusEffect> GetActives()
        {
            return StatusEffects;
        }
        /// <summary>
        /// Searches <see cref="StatusEffects"/> for all effects that contain an effect of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <returns>A list of every <typeparamref name="TResult"/> effect from the fighter's currently applied StatusEffects.</returns>
        public List<TResult> GetActives<TResult>() where TResult : Subeffect
        {
            return [.. StatusEffects.SelectMany((x) => x.GetSubeffectsOfType<TResult>().Values)];
        }
        /// <summary>
        /// Searches <see cref="StatusEffects"/> for all status effects that modify the <see cref="Stats"/> <paramref name="stat"/> in some way.
        /// </summary>
        /// <param name="stat">The modifier type to search for.</param>
        /// <returns>An enumerable of StatusEffects that modify <paramref name="stat"/>.</returns>
        public List<StatusEffect> GetActives(Stats stat)
        {
            return StatusEffects.FindAll((x) => x.GetModifier(stat) != null);
        }
        /// <summary>
        /// Calculates the full modification of a certain stat by every active <see cref="StatusEffect"/>.
        /// This method applies stack counts.
        /// </summary>
        /// <param name="stat">The stat whose modifications to search for.</param>
        /// <returns>A double that represents the modification from the base value of the given stat.</returns>
        public double GetActivesValue(Stats stat)
        {
            return GetActives(stat).Sum((x) => x.GetModifier(stat).Amount * x.Stacks);
        }

        // Passives

        /// <summary>
        /// Searches <see cref="PassiveEffects"/> for all effects that modify the <see cref="Stats"/> <paramref name="stat"/> in some way.
        /// </summary>
        /// <param name="stat">The modifier type to search for.</param>
        /// <returns>An enumerable of ActiveEffects that modify <paramref name="stat"/>.</returns>
        public List<PassiveEffect> GetPassives()
        {
            return PassiveEffects;
        }
        /// <summary>
        /// Searches <see cref="PassiveEffects"/> for all effects that contain an effect of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <returns>A list of every <typeparamref name="TResult"/> effect from the fighter's currently active PassiveEffects.</returns>
        public List<TResult> GetPassives<TResult>() where TResult : Subeffect
        {
            return [.. PassiveEffects.SelectMany((x) => x.GetSubeffectsOfType<TResult>().Values)];
        }
        /// <summary>
        /// Searches <see cref="PassiveEffects"/> for all status effects that modify the <see cref="Stats"/> <paramref name="stat"/> in some way.
        /// </summary>
        /// <param name="stat">The modifier type to search for.</param>
        /// <returns>An enumerable of PassiveEffects that modify <paramref name="stat"/>.</returns>
        public List<PassiveEffect> GetPassives(Stats stat)
        {
            return PassiveEffects.FindAll((x) => x.GetModifier(stat) != null);
        }
        /// <summary>
        /// Calculates the full modification of a certain stat by every active <see cref="PassiveEffect"/>.
        /// This method applies stack counts.
        /// </summary>
        /// <param name="stat">The stat whose modifications to search for.</param>
        /// <returns>A double that represents the modification from the base value of the given stat.</returns>
        public double GetPassivesValue(Stats stat, Character target)
        {
            return GetPassives(stat).Sum(x => x.GetModifier(stat).Amount * x.GetStacks(target));
        }

        // Both cuz im smart

        /// <summary>
        /// Gets the full outgoing generic damage modification for the given type, against the given target.
        /// </summary>
        /// <param name="target">The target for which to calculate passives. Null by default, which won't count passives.</param>
        /// <returns>The modification value. 0 by default.</returns>
        public double GetDamageBonus(Character target = null)
        {
            return GetActives<DamageBonus>().Sum(x => x.Amount * (x.Parent as StatusEffect).Stacks) + GetPassives<DamageBonus>().Sum(x => x.Amount * (x.Parent as PassiveEffect).GetStacks(target));
        }
        /// <summary>
        /// Gets the full incoming generic damage modification for the given type, against the given target.
        /// </summary>
        /// <param name="target">The target for which to calculate passives. Null by default, which won't count passives.</param>
        /// <returns>The modification value. 0 by default.</returns>
        public double GetDamageRES(Character target = null)
        {
            return GetActives<DamageRES>().Sum(x => x.Amount * (x.Parent as StatusEffect).Stacks) + GetPassives<DamageRES>().Sum(x => x.Amount * (x.Parent as PassiveEffect).GetStacks(target));
        }
        /// <summary>
        /// Gets the full outgoing type-specific damage modification for the given type, against the given target.
        /// </summary>
        /// <param name="type">The type of damage to calculate for.</param>
        /// <param name="target">The target for which to calculate passives. Null by default, which won't count passives.</param>
        /// <returns>The modification value. 0 by default.</returns>
        public double GetDamageTypeBonus(DamageTypes type, Character target = null)
        {
            return GetActives<DamageTypeBonus>().FindAll(x => x.Type == type).Sum(x => x.Amount * (x.Parent as StatusEffect).Stacks) + GetPassives<DamageTypeBonus>().FindAll(x => x.Type == type).Sum(x => x.Amount * (x.Parent as PassiveEffect).GetStacks(target));
        }
        /// <summary>
        /// Gets the full incoming type-specific damage modification for the given type, against the given target.
        /// </summary>
        /// <param name="type">The type of damage to calculate for.</param>
        /// <param name="target">The target for which to calculate passives. Null by default, which won't count passives.</param>
        /// <returns>The modification value. 0 by default.</returns>
        public double GetDamageTypeRES(DamageTypes type, Character target = null)
        {
            return GetActives<DamageTypeRES>().FindAll(x => x.Type == type).Sum(x => x.Amount * (x.Parent as StatusEffect).Stacks) + GetPassives<DamageTypeBonus>().FindAll(x => x.Type == type).Sum(x => x.Amount * (x.Parent as PassiveEffect).GetStacks(target));
        }
        /// <summary>
        /// Gets the full outgoing source-specific damage modification for the given type, against the given target.
        /// </summary>
        /// <param name="source">The source of damage to calculate for.</param>
        /// <param name="target">The target for which to calculate passives. Null by default, which won't count passives.</param>
        /// <returns>The modification value. 0 by default.</returns>
        public double GetDamageSourceBonus(DamageSources source, Character target = null)
        {
            return GetActives<DamageSourceBonus>().FindAll(x => x.Source == source).Sum(x => x.Amount * (x.Parent as StatusEffect).Stacks) + GetPassives<DamageSourceBonus>().FindAll(x => x.Source == source).Sum(x => x.Amount * (x.Parent as PassiveEffect).GetStacks(target));
        }
        /// <summary>
        /// Gets the full incoming source-specific damage modification for the given type, against the given target.
        /// </summary>
        /// <param name="source">The source of damage to calculate for.</param>
        /// <param name="target">The target for which to calculate passives. Null by default, which won't count passives.</param>
        /// <returns>The modification value. 0 by default.</returns>
        public double GetDamageSourceRES(DamageSources source, Character target = null)
        {
            return GetActives<DamageSourceRES>().FindAll(x => x.Source == source).Sum(x => x.Amount * (x.Parent as StatusEffect).Stacks) + GetPassives<DamageSourceRES>().FindAll(x => x.Source == source).Sum(x => x.Amount * (x.Parent as PassiveEffect).GetStacks(target));
        }
        #endregion

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
        public abstract Ability SelectAbility(Character target, int selector);
    }
}
