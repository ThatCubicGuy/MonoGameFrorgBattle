using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal interface ITakesAction
    {
        /// <summary>
        /// The action of the ITakesAction entity.
        /// </summary>
        /// <returns>True if the turn should end, false otherwise.</returns>
        void TakeAction();
    }

    // anything that has a turn in the ActionBar
    internal interface IHasTurn : ITakesAction
    {
        double BaseActionValue { get; }
    }

    internal interface IHasTarget
    {
        Character Parent { get; }
        Character Target { get; }
    }

    internal interface IAttack : IHasTarget
    {
        AttackInfo AttackInfo { get; }
    }

    internal interface IAppliesEffect : IHasTarget
    {
        EffectInfo EffectInfo { get; }
        bool ApplyEffect(Character target);
    }

    internal interface IPoolChange
    {
        public Character Source { get; }
        public Character Target { get; }
        /// <summary>
        /// Automatically calculates the final value of the amount through the attached fighters.
        /// </summary>
        public double Amount { get; }
        public Pools Pool { get; }
        public Operators Op { get; }
    }
}
