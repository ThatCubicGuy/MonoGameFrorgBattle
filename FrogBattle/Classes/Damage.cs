using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal class Damage : ICloneable
    {
        readonly Properties _properties;
        readonly Flags _flags;
        public Damage(double amount, Properties props, Flags flags)
        {
            Amount = amount;
            _properties = props;
            _flags = flags;
        }
        public double Amount { get; private set; }
        public object Clone()
        {
            return MemberwiseClone();
        }
        public struct Properties
        {
            readonly double defenseIgnore;
            readonly double typeResPen;
            readonly DamageTypes type;
        }
        [Flags] public enum Flags
        {
            none,
            crit,
        }
    }
}
