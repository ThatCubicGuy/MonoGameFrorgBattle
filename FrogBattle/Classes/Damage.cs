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
        private readonly Properties _properties;
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
        public record Properties
        (
            DamageTypes type,
            DamageSources source,
            bool crit,
            double critDamage = 1.5,
            double defenseIgnore = 0,
            double typeResPen = 0
        );
    }
}
// order of effects for damage: atk -> type bonus -> crit -> dmg bonus -> type res -> def -> dmg reduction ?