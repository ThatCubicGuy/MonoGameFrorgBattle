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
        bool TakeAction();
    }

    // anything that has a turn in the ActionBar
    internal interface IHasTurn : ITakesAction
    {
        double ActionValue { get; }
    }

    internal interface IAttack
    {
        Character Parent { get; }
        Character MainTarget { get; }
        double Ratio { get; }
        Stats Scalar { get; }
        double? HitRate { get; }
        bool IndependentHitRate { get; }
        DamageTypes Type { get; }
        double DefenseIgnore { get; }
        double TypeResPen { get; }
        uint[] Split { get; }
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
