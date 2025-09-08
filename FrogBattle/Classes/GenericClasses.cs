using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal record PositiveInterval(double Min = 0, double Max = double.PositiveInfinity)
    {
        public bool Contains(double number) { return number >= Min && number <= Max; }
        public double Width => Max - Min;
    }
    internal record NullableInterval(double? Min = null, double? Max = null)
    {
        public bool Contains(double number) { return !(number < Min || number > Max); }
        public double Width => Min.HasValue && Max.HasValue ? Max.Value - Min.Value : double.PositiveInfinity;
    }
}
