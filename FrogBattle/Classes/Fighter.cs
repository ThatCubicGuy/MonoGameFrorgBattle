using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal class Fighter
    {
        protected uint BaseHp;
        protected uint BaseAtk;
        protected uint BaseDef;
        protected uint BaseSpd;
        protected uint MaxMana;
        protected uint MaxEnergy;
        protected double CurrentHp;
        protected double CurrentMana;
        protected double CurrentEnergy = 0;
        private List<StatusEffect> StatusEffects;
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
        // i need to understand how event work dawg
        //public EventHandler<EventArgs> HpChanged;
        private void StartOfTurnChecks()
        {
            // DoT Damage & Ticks, Stuns
        }
        private void EndOfTurnChecks()
        {
            // Effect Ticks
        }
        private bool CanUseAbility(Ability x)
        {
            foreach (var cost in x.Costs)
            {
                if ((cost.props & Ability.CostProperties.soft) != 0) continue;
                switch (cost.currency)
                {

                }
            }
        }
        public Damage OutgoingDamage(Stats source, double ratio, uint split = 1)
        {
            return new(source.Resolve(this) * ratio, new(), Damage.Flags.none);
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
            return StatusEffects.FindAll(new((item) => item.GetModifiers().Any((eff) => eff.Attribute == stat)));
        }

    }
}
