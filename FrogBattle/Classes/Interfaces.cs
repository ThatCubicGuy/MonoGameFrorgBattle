using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal interface IExtraTurn
    {
        bool TakeAction();
    }

    // anything that has a turn in the ActionBar
    internal interface ITakesAction
    {
        double ActionValue { get; }
        bool TakeAction();
    }

    internal interface IAttack
    {
        Character MainTarget { get; }
        double Ratio { get; }
        Stats Scalar { get; }
        uint[] Split { get; }
    }
}
