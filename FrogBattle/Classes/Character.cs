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
        private readonly List<StatusEffect> StatusEffects = [];
        private readonly List<StatusEffect> MarkedForDeath = [];
        public readonly string _internalName;
        
        #region Events
        public event EventHandler<Damage> DamageTaken;  // FINALLY I UNDERSTAND HOW THIS PMO SHIT WORKS
        public event EventHandler<Damage> DamageDealt;
        public event EventHandler<Healing> HealingReceived;
        public event EventHandler<ITakesAction> TurnStarted;
        public event EventHandler<StatusEffect> EffectGained;
        public event EventHandler<StatusEffect> EffectRemoved;
        public event EventHandler<IPoolChange> PoolChanged;
        // Generic Events
        public event EventHandler<double> HpChanged;
        #endregion
        public Character(string name, BattleManager battle, bool IS_TEAM_1, Dictionary<Stats, double> overrides = null)
        {
            _internalName = typeof(Character).Name.camelCase() + '.' + GetType().Name.camelCase();
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
        public string Name { get; protected set; }
        public BattleManager ParentBattle { get; }
        // crack cocaine team stuff that isn't even implemented yet lmfao
        public List<Character> Team { get; }
        public int PositionInTeam => Team.IndexOf(this);
        public Character LeftTeammate => PositionInTeam != 0 ? Team.ElementAt(PositionInTeam - 1) : null;
        public Character RightTeammate => PositionInTeam != Team.Count - 1 ? Team.ElementAt(PositionInTeam + 1) : null;
        public List<Character> EnemyTeam { get; }

        #region Pools
        public double Hp
        {
            get
            {
                return CurrentHp + GetEffects<StatusEffect.Overheal>().Sum(x => x.Amount);
            }
            private set
            {
                var diff = Hp - value;
                if (diff > 0)
                {
                    // If we're taking damage (difference between current Hp and target Hp is positive)
                    var overheals = GetEffects<StatusEffect.Overheal>();
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
                        StatusEffects.Remove(item.Parent);
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
                return GetEffects().FindAll((x) => x.GetSubeffects<StatusEffect.Shield>().Count != 0).Sum((x) => x.GetSubeffects<StatusEffect.Shield>().Single().Value.Amount);
            }
            private set
            {
                if (value <= 0)
                {
                    foreach (var item in GetEffects<StatusEffect.Shield>())
                        StatusEffects.Remove(item.Parent);
                    return;
                }
                double diff = Shield - value;
                if (diff > 0)
                {
                    var shields = StatusEffects.SelectMany((x) => x.GetSubeffects<StatusEffect.Shield>().Values);
                    foreach (var item in shields)
                    {
                        if (diff >= item.Amount)
                        {
                            diff -= item.Amount;
                            StatusEffects.Remove(item.Parent);
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
                return (uint)GetEffects().FindAll((x) => x.GetSubeffects<StatusEffect.Barrier>().Count != 0).Sum((x) => x.GetSubeffects<StatusEffect.Barrier>()[0].Count);
            }
            private set
            {
                if ((int)value - Barrier <= 0)
                {
                    uint diff = Barrier - value;
                    foreach (var item in StatusEffects.SelectMany((x) => x.GetSubeffects<StatusEffect.Barrier>().Values))
                    {
                        if (diff >= item.Count)
                        {
                            diff -= item.Count;
                            StatusEffects.Remove(item.Parent);
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
        public bool IsCrit => BattleManager.RNG < GetStat(Stats.CritRate);
        public uint Stun => (uint)GetEffects<StatusEffect.Stun>().Sum(x => x.Count);
        public double BaseActionValue => 10000 / GetStat(Stats.Spd);
        public bool TakeAction(Ability action)
        {
            if (Stun > 0) return true;
            return action.TryUse();
        }
        public record InstaAction(Ability Action) : ITakesAction
        {
            public void TakeAction()
            {
                Action.TryUse();
            }
        }
        public void TakeAction()
        {
            StartOfTurnChecks();
            if (Stun > 0)
            {
                ParentBattle.BattleText.AppendLine(Localization.Translate("character.generic.stun", Name, Stun));
            }
            else
            {
                ApplyChange(new Reward(this, this, 5, Pools.Mana, Operators.Additive));
                while (!this.Console_SelectAbility().TryUse());
            }
            EndOfTurnChecks();
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
            return Base[stat] + GetEffectsValue(stat);
        }
        private void StartOfTurnChecks()
        {
            var dotList = DoTCalculations();
            double totalDamage = 0;
            foreach (var item in dotList)
            {
                totalDamage += item.Amount;
                TakeDamage(item);
            }
            if (totalDamage > 0) AddBattleText("character.generic.dot", Name, EnemyTeam.Single().Name, totalDamage);
            MarkedForDeath.AddRange(StatusEffects.FindAll((x) => !x.Is(StatusEffect.Flags.StartTick)));
            foreach (var item in StatusEffects.FindAll((x) => x.Is(StatusEffect.Flags.StartTick)))
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
            var damages = new List<Damage>();
            foreach (var item in StatusEffects.Select(x => x.SingleEffect<StatusEffect.DamageOverTime>()))
            {
                damages.Add(item.GetDamage());
            }
            return damages;
        }
        public void AddEffect(StatusEffect effect)
        {
            if (effect == null) return;
            EffectGained?.Invoke(this, effect);
            var sameEff = StatusEffects.Find(x => x == effect);
            if (sameEff != null)
            {
                effect.Stacks += sameEff.Stacks;
                StatusEffects[StatusEffects.IndexOf(sameEff)] = effect;
            }
            else StatusEffects.Add(effect);
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
        public void TakeDamage(Damage damage, double ratio = 1)
        {
            // to do: implement final damage calculations for additional damage calculations
            if (damage.Target != this) throw new ArgumentException($"Mismatch between Damage instance target {damage.Target} and TakeDamage method target {this}.", nameof(damage));
            var damageAmount = damage.Amount * ratio;
            this.DamageTaken?.Invoke(damage.Source, damage);   // YES I FIGURED OUT EVENTS LETS GOO
            damage.Source.DamageDealt?.Invoke(this, damage);
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
        public void TakeHealing(Healing healing, double ratio = 1)
        {

        }
        /// <summary>
        /// Searches <see cref="StatusEffects"/> for all effects.
        /// </summary>
        /// <returns>Every effect applied to this fighter.</returns>
        public List<StatusEffect> GetEffects()
        {
            return StatusEffects;
        }
        /// <summary>
        /// Searches <see cref="StatusEffects"/> for all effects that contain an effect of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <returns>A list of every <typeparamref name="TResult"/> effect from the fighter's currently applied StatusEffects.</returns>
        public List<TResult> GetEffects<TResult>() where TResult : StatusEffect.Effect
        {
            return [.. StatusEffects.SelectMany((x) => x.GetSubeffects<TResult>().Values)];
        }
        /// <summary>
        /// Searches <see cref="StatusEffects"/> for all effects that modify the <see cref="Stats"/> <paramref name="stat"/> in some way.
        /// </summary>
        /// <param name="stat">The <see cref="StatusEffect.Modifier.Stat"/> effects to search for.</param>
        /// <returns>An enumerable of StatusEffects that modify <paramref name="stat"/>.</returns>
        public List<StatusEffect> GetEffects(Stats stat)
        {
            return StatusEffects.FindAll((x) => x.GetModifiers().ContainsKey(stat));
        }
        /// <summary>
        /// Calculates the full modification of a certain stat by every active <see cref="StatusEffect"/>.
        /// </summary>
        /// <param name="stat">The stat whose modifications to search for.</param>
        /// <returns>A double that represents the modification from the base value of the given stat.</returns>
        public double GetEffectsValue(Stats stat)
        {
            return GetEffects(stat).Sum((x) => x.GetModifiers()[stat].Amount);
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
        public abstract Ability SelectAbility(Character target, int selector);
        public override string ToString()
        {
            return Name ?? "[UNNAMED]";
        }
    }
}
