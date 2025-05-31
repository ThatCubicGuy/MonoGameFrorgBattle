using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoBattleFrorgGame.Classes
{
    internal class Damage
    {
        private double amount = 0;
        public Damage Amount(double count)
        {
            amount = count;
            return this;
        }
        private Damage SplitAmount(double ratio)
        {
            amount *= ratio;
            return this;
        }
        internal enum DamageType
        {
            None,
            Blunt,
            Slash,
            Pierce,
            Bullet,
            Blast,
            Magic
        }
        internal class DamageProperties()
        {
            private DamageType type = DamageType.None;
            private double critRate = 0;
            private double critDamage = 2;
            private double ignoreDefense = 0;
            public DamageProperties @Type(DamageType newType)
            {
                type = newType;
                return this;
            }
            public DamageProperties CritRate(double critRate)
            {
                this.critRate = critRate;
                return this;
            }
            public DamageProperties CritDmg(double critDmg)
            {
                critDamage = critDmg;
                return this;
            }
            public DamageProperties IgnoreDefense(double ignoreDefense)
            {
                this.ignoreDefense = ignoreDefense;
                return this;
            }
        }
        public IEnumerable<Damage> Split(params double[] ratios)
        {
            if (ratios.Sum() <= 0.99 && ratios.Sum() >= 1.01) throw new ArgumentException("Damage splits should total to 1.");
            IEnumerable<Damage> result = new List<Damage>(ratios.Length);
            foreach (var i in ratios)
            {
                result = result.Append(SplitAmount(i));
            }
            return result;
        }
    }
}
