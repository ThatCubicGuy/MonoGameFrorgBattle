using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal interface IModifier<TProp> : IEffect where TProp : struct, Enum
    {
        TProp Property { get; }
        double Amount { get; }
        Operator Op { get; }
    }
    enum Operator
    {
        Add,
        Multiply
    }
}
