using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    /// <summary>
    /// Rewards generally given after using abilities.
    /// </summary>
    internal class Reward : IPoolChange
    {
        private readonly double _baseAmount;
        /// <summary>
        /// Initializes a new instance of <see cref="Reward"/>.
        /// </summary>
        /// <param name="source">Character which sent this reward.</param>
        /// <param name="amount">The amount (of a certain pool), pre-calculations. Usually a flat value.</param>
        public Reward(Character source, Character target, double amount, Pools pool, Operators op)
        {
            // You can use the cost / reward system for gaining and consuming HP.
            // Prefer not to use it for actual healing and damage.
            Source = source;
            Target = target;
            _baseAmount = amount;
            Pool = pool;
            Op = op;
        }
        public Character Source { get; }
        public Character Target { get; }
        /// <summary>
        /// Automatically calculates outgoing healing from the source fighter and incoming healing on the target.
        /// </summary>
        public double Amount
        {
            get
            {
                var total = Op.Apply(_baseAmount, Target.Resolve(Pool));
                // Outgoing bonuses
                if (Source != null)
                {
                    // Nothing for now lol
                }
                // Incoming bonuses
                if (Target != null)
                {
                    total *= Pool switch
                    {
                        Pools.Mana => Target.GetStat(Stats.ManaRegen),
                        Pools.Energy => Target.GetStat(Stats.EnergyRecharge),
                        _ => 1
                    };
                }
                return total;
            }
        }
        public Pools Pool { get; }
        public Operators Op { get; }
        public Reward Clone() => MemberwiseClone() as Reward;
    }
    /// <summary>
    /// The opposite of rewards. Positive values are deducted. Taxed before using the ability.
    /// </summary>
    internal class Cost : IPoolChange
    {
        private readonly double _baseAmount;
        public Cost(Character source, Character target, double amount, Pools pool, Operators op)
        {
            Source = source;
            Target = target;
            _baseAmount = -1 * amount;
            Pool = pool;
            Op = op;
        }
        public Character Source { get; }
        public Character Target { get; }
        public double Amount
        {
            get
            {
                var total = Op.Apply(_baseAmount, Target.Resolve(Pool));
                // Outgoing bonuses
                if (Source != null)
                {
                    // Nothing for now lol
                }
                // Incoming bonuses
                if (Target != null)
                {
                    total *= Pool switch
                    {
                        Pools.Mana => Target.GetStat(Stats.ManaCost),
                        _ => 1
                    };
                }
                return total;
            }
        }
        public Pools Pool { get; }
        public Operators Op { get; }
        public Cost Clone() => MemberwiseClone() as Cost;
    }
}
