using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal class Damage
    {
        readonly Properties _properties;
        public Damage(double amount, Properties props)
        {
            Amount = amount;
            _properties = props;
        }
        public double Amount { get; private set; }
        public Damage Clone()
        {
            return MemberwiseClone() as Damage;
        }
        public struct Properties
        {
            readonly double defenseIgnore;
            readonly double typeResPen;
            readonly DamageTypes type;
            readonly bool crit;
        }
    }
}
// order of effects for damage: atk -> type bonus -> crit -> dmg bonus -> type res -> def -> dmg reduction ?