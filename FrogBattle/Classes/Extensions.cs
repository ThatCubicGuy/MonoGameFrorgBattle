using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal static class OperatorExtensions
    {
        public static double Apply(this Operator op, double v1, double v2)
        {
            return op switch
            {
                Operator.Add => v1 + v2,
                Operator.Multiply => v1 * v2,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null),
            };
        }
    }
}
