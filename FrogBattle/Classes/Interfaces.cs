using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoBattleFrorgGame.Classes
{
    internal interface IModifier
    {
        double Amount { get; }
        Operation Op { get; }
        enum Property;
        enum Operation
        {
            Add,
            Multiply
        }
    }
}
