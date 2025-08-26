using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal abstract class Character : ITakesAction
    {
        public readonly Dictionary<Stats, double> Base;
        protected double CurrentHp;
        protected double CurrentMana;
        protected double CurrentEnergy = 0;
        private readonly List<StatusEffect> StatusEffects = [];
        private readonly List<StatusEffect> MarkedForDeath = [];
        public readonly string internalName;
        public Character(string name, Dictionary<Stats, double> overrides = null)
        {
            internalName = GetType().BaseType.Name.camelCase() + '.' + GetType().Name.camelCase();
            Name = name;
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
        public List<Character> Team { get; } // not gonna implement this rn lmfao
        #region Pools
        public double Hp
        {
            get
            {
                return CurrentHp;
            }
            set
            {
                CurrentHp = value;
                if (CurrentHp > GetStat(Stats.MaxHp)) CurrentHp = GetStat(Stats.MaxHp);
            }
        }
        public double Mana
        {
            get
            {
                return CurrentMana;
            }
            set
            {
                CurrentMana = value;
                if (CurrentMana > GetStat(Stats.MaxMana)) CurrentMana = GetStat(Stats.MaxMana);
            }
        }
        public double Energy
        {
            get
            {
                return CurrentEnergy;
            }
            set
            {
                CurrentEnergy = value;
                if (CurrentEnergy > GetStat(Stats.MaxEnergy)) CurrentEnergy = GetStat(Stats.MaxEnergy);
            }
        }
        public double Special { get; set; }
        public double Shield
        {
            get
            {
                return GetEffects().FindAll((x) => x.GetEffects<StatusEffect.Shield>().Count != 0).Sum((x) => x.GetEffects<StatusEffect.Shield>().Single().Value.Amount);
            }
            set
            {
                double diff = value - Shield;
                if (diff < 0)
                {
                    diff *= -1;
                    foreach (var item in StatusEffects.SelectMany((x) => x.GetEffects<StatusEffect.Shield>().Values))
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
                    throw new NotImplementedException();
                }
            }
        }
        public uint Barrier
        {
            get
            {
                return (uint)GetEffects().FindAll((x) => x.GetEffects<StatusEffect.Barrier>().Count != 0).Sum((x) => x.GetEffects<StatusEffect.Barrier>()[0].Count);
            }
            set
            {
                if ((int)value - Barrier <= 0)
                {
                    uint diff = Barrier - value;
                    foreach (var item in StatusEffects.SelectMany((x) => x.GetEffects<StatusEffect.Barrier>().Values))
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
        public bool IsCrit => Battle.RNG < GetStat(Stats.CritRate);
        public double ActionValue
        {
            get
            {
                return 10000 / GetStat(Stats.Spd);
            }
        }
        public bool TakeAction()
        {
            // HOW
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
            MarkedForDeath.AddRange(StatusEffects.FindAll((x) => !x.Is(StatusEffect.Props.StartTick)));
            foreach (var item in StatusEffects.FindAll((x) => x.Is(StatusEffect.Props.StartTick)))
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
        /// <summary>
        /// Creates one or multiple instances of <see cref="Damage"/> for an attack.
        /// </summary>
        /// <param name="target">The target of the attack.</param>
        /// <param name="scalar">The stat to use for calculating the ratio.</param>
        /// <param name="ratio">The percentage of the scalar to use for the base damage amount.</param>
        /// <param name="type">The type of the damage.</param>
        /// <param name="defIgnore">The amount of the target's defense to ignore.</param>
        /// <param name="typeResPen">The percentage of damage-type-specific resistance of the target to ignore.</param>
        /// <param name="split">The splitting mode of this damage. Each number represents
        /// the amount of damage (from the total sum) that will go to that slice.
        /// Input as many as you want.</param>
        /// <returns>An <see cref="IEnumerator{T}"/> of <see cref="Damage"/>, iterating through every instance of damage.</returns>
        public IEnumerator<Damage> AttackDamage(Character target, Stats scalar, double ratio, DamageTypes type, double defIgnore, double typeResPen, params uint[] split)
        {
            var props = new Damage.Properties(type, DamageSources.Attack, DefenseIgnore: defIgnore, TypeResPen: typeResPen);
            if (split.Length == 0) yield return new(this, target, GetStat(scalar) * ratio, props with { Crit = IsCrit });
            else
            {
                long sum = split.Sum((x) => x);
                foreach (uint i in split)
                {
                    yield return new(this, target, GetStat(scalar) * i * ratio / sum, props with { Crit = IsCrit });
                }
            }
        }
        // to do: implement final damage calculations for additional damage calculations
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
            return [.. StatusEffects.SelectMany((x) => x.GetEffects<TResult>().Values)];
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
        /// <param name="value">Change to apply.</param>
        public void ApplyChange(Ability.PoolChange change)
        {
            switch (change.Pool)
            {
                case Pools.Hp:
                    Hp += change.Op.Apply(change.Amount, Base[change.Pool.Max()]);
                    break;
                case Pools.Mana:
                    Mana += change.Op.Apply(change.Amount, Base[change.Pool.Max()]);
                    break;
                case Pools.Energy:
                    Energy += change.Op.Apply(change.Amount, Base[change.Pool.Max()]);
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
        }
    }
}
