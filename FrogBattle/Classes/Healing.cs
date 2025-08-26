using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static FrogBattle.Classes.StatusEffect;

namespace FrogBattle.Classes
{
    internal class Healing
    {
        private readonly double baseAmount;
        /// <summary>
        /// Initializes a new instance of <see cref="Healing"/>.
        /// </summary>
        /// <param name="source">Character which sent this heal.</param>
        /// <param name="amount">The amount of healing, pre-calculations. Usually ratio * scalar, or just a flat value.</param>
        public Healing(Character source, Character target, double amount)
        {
            Source = source;
            Target = target;
            baseAmount = amount;
        }
        /// <summary>
        /// Automatically calculates outgoing healing from the source fighter and incoming healing on the target.
        /// </summary>
        public double Amount
        {
            get
            {
                var total = baseAmount;
                // Outgoing bonuses
                if (Source != null)
                {
                    total *= Source.GetStat(Stats.OutgoingHealing);
                }
                // Incoming bonuses
                if (Target != null)
                {
                    total *= Target.GetStat(Stats.IncomingHealing);
                }
                return total;
            }
        }
        public Character Source { get; }
        public Character Target { get; }
        public Healing Clone()
        {
            return MemberwiseClone() as Healing;
        }
    }
}
// order of effects for damage: atk -> type bonus -> crit -> dmg bonus -> type res -> def -> dmg reduction