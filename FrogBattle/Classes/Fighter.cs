using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal abstract class Fighter
    {
        protected uint BaseHp;
        protected uint BaseAtk;
        protected uint BaseDef;
        protected uint BaseSpd;
        protected uint BaseDex;
        protected uint MaxMana;
        protected uint MaxEnergy;
        protected double CurrentHp;
        protected double CurrentMana;
        protected double CurrentEnergy = 0;
        private List<StatusEffect> StatusEffects = [];
        private List<StatusEffect> MarkedForDeath = [];
        public string Name { get; protected set; }
        #region Stats
        public double MaxHp
        {
            get
            {
                return BaseHp + GetEffects().Calculate(Stats.MaxHp, BaseHp);
            }
        }
        public double Atk
        {
            get
            {
                return BaseAtk + GetEffects().Calculate(Stats.Atk, BaseAtk);
            }
        }
        public double Def
        {
            get
            {
                return BaseDef + GetEffects().Calculate(Stats.Def, BaseDef);
            }
        }
        public double Spd
        {
            get
            {
                return BaseSpd + GetEffects().Calculate(Stats.Spd, BaseSpd);
            }
        }
        public double Dex
        {
            get
            {
                return BaseDex + GetEffects().Calculate(Stats.Dex, BaseDex);
            }
        }
        public double EffectHitRate
        {
            get
            {
                return GetEffects().Calculate(Stats.EffectHitRate, 0);
            }
        }
        public double EffectRES
        {
            get
            {
                return GetEffects().Calculate(Stats.EffectRES, 0);
            }
        }
        public double AllTypeRES
        {
            get
            {
                return GetEffects().Calculate(Stats.AllTypeRES, 0);
            }
        }
        public double ManaCost
        {
            get
            {
                return 1 + GetEffects().Calculate(Stats.ManaCost, 0);
            }
        }
        public double ManaRegen
        {
            get
            {
                return 1 + GetEffects().Calculate(Stats.ManaRegen, 0);
            }
        }
        public double IncomingHealing
        {
            get
            {
                return 1 + GetEffects().Calculate(Stats.IncomingHealing, 0);
            }
        }
        public double OutgoingHealing
        {
            get
            {
                return 1 + GetEffects().Calculate(Stats.OutgoingHealing, 0);
            }
        }
        #endregion
        #region Pools
        public double Hp
        {
            get
            {
                return CurrentHp;
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
                if (CurrentMana > MaxMana) CurrentMana = MaxMana;
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
                if (CurrentEnergy > MaxEnergy) CurrentEnergy = MaxEnergy;
            }
        }
        public double Special { get; set; }
        public double Shield
        {
            get
            {
                double shield = 0;
                foreach (var effect in GetEffects((x) => x is Shield))
                {
                    shield += ((Shield)effect).GetShield();
                }
                return shield;
            }
        }
        public double Barrier
        {
            get
            {
                double barrier = 0;
                foreach (var effect in GetEffects((x) => x is Barrier))
                {
                    barrier += ((Barrier)effect).GetBarrier();
                }
                return barrier;
            }
        }
        #endregion
        // i need to understand how event work dawg
        //public EventHandler<EventArgs> HpChanged;
        public Fighter(string name, uint baseHp, uint baseAtk, uint baseDef, uint baseSpd, uint baseDex, uint maxMana, uint maxEnergy)
        {
            Name = name;
            BaseHp = baseHp;
            BaseAtk = baseAtk;
            BaseDef = baseDef;
            BaseSpd = baseSpd;
            BaseDex = baseDex;
            MaxMana = maxMana;
            MaxEnergy = maxEnergy;
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
            if (split.Length == 0) yield return new(this.Resolve(source) * ratio, props);
            else
            {
                double sum = split.Sum((x) => (double)x);
                foreach (uint i in split)
                {
                    yield return new(this.Resolve(source) * i * ratio / sum, props);
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
        /// <returns>An enumerable of StatusEffects that modify <paramref name="stat"/></returns>
        public IEnumerable<StatusEffect> GetEffects(Stats stat)
        {
            return StatusEffects.FindAll((item) => item.GetModifiers().Any((eff) => eff.Attribute == stat));
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
    }
}
