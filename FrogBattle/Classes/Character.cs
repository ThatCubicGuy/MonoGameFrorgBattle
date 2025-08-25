using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal abstract class Character
    {
        public Dictionary<Stats, double> Base;
        protected double CurrentHp;
        protected double CurrentMana;
        protected double CurrentEnergy = 0;
        private readonly List<StatusEffect> StatusEffects = [];
        private readonly List<StatusEffect> MarkedForDeath = [];
        public string Name { get; protected set; }
        public readonly string internalName;
        #region Pools
        public double Hp
        {
            get
            {
                return CurrentHp;
            }
            set
            {
                CurrentHp= value;
                if (CurrentHp> GetStat(Stats.MaxHp)) CurrentHp = GetStat(Stats.MaxHp);
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
                return GetEffects<Shield>().Sum((x) => x.GetShield());
            }
        }
        public double Barrier
        {
            get
            {
                return GetEffects<Barrier>().Sum((x) => x.GetBarrier());
            }
        }
        #endregion
        public double ActionValue
        {
            get
            {
                return 10000 / GetStat(Stats.Spd);
            }
        }
        public Character(string name, Dictionary<Stats, double> overrides = null)
        {
            internalName = GetType().BaseType.Name + '.' + GetType().Name;
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
        public double GetStat(Stats stat)
        {
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
        }
        public IEnumerator<Damage> OutgoingDamage(Stats source, double ratio, Damage.Properties props, params uint[] split)
        {
            if (split.Length == 0) yield return new(GetStat(source) * ratio, props);
            else
            {
                double sum = split.Sum((x) => (double)x);
                foreach (uint i in split)
                {
                    yield return new(GetStat(source) * i * ratio / sum, props);
                }
            }
        }
        /// <summary>
        /// Searches <see cref="StatusEffects"/> for all effects.
        /// </summary>
        /// <returns>Every effect applied to this fighter.</returns>
        public IEnumerable<StatusEffect> GetEffects()
        {
            return StatusEffects;
        }
        /// <summary>
        /// Searches <see cref="StatusEffects"/> for all effects that match the given predicate.
        /// </summary>
        /// <param name="predicate">The condition that a <see cref="StatusEffect"/> must match.</param>
        /// <returns>An enumerable of effects that match the given predicate.</returns>
        public IEnumerable<StatusEffect> GetEffects(Predicate<StatusEffect> predicate)
        {
            return StatusEffects.FindAll(predicate);
        }
        /// <summary>
        /// Searches <see cref="StatusEffects"/> for all effects that modify the <see cref="Stats"/> <paramref name="stat"/> in some way.
        /// </summary>
        /// <param name="stat">The <see cref="StatusEffect.Modifier.Stat"/> effects to search for.</param>
        /// <returns>An enumerable of StatusEffects that modify <paramref name="stat"/>.</returns>
        public IEnumerable<StatusEffect> GetEffects(Stats stat)
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
            return GetEffects(stat).Sum((x) => x.GetModifiers()[stat].Op.Apply(x.GetModifiers()[stat].Amount, Base[stat]));
        }
        /// <summary>
        /// Searches StatusEffects for all effects that match the given type.
        /// </summary>
        /// <typeparam name="T">Type inheriting <see cref="StatusEffect"/> to search for.</typeparam>
        /// <returns>An enumerable of type <typeparamref name="T"/>.</returns>
        public IEnumerable<T> GetEffects<T>()
        {
            return (IEnumerable<T>)StatusEffects.FindAll((x) => x is T);
        }
        /// <summary>
        /// Checks whether a character has the resources available to expend for an ability cost.
        /// </summary>
        /// <param name="value">The ability cost to check for.</param>
        /// <returns>True if the character can afford the cost, false otherwise.</returns>
        internal bool CanAfford(Ability.Cost value)
        {
            if (value.Properties.HasFlag(Ability.CostProperties.Soft)) return true;
            else if (value.Properties.HasFlag(Ability.CostProperties.Reverse))
            {
                if (this.Resolve(value.Currency) > value.Op.Apply(value.Amount, this.Resolve(value.Currency))) return false;
                else return true;
            }
            else if (this.Resolve(value.Currency) < value.Op.Apply(value.Amount, this.Resolve(value.Currency))) return false;
            else return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        internal void Expend(Ability.Cost value)
        {

        }
    }
}
