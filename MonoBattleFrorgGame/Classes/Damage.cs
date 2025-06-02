using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoBattleFrorgGame.Classes
{
    internal class Damage : ICloneable
    {
        private double amount = 0;
        public DamageProperties Properties { get; private set; }
        public object Clone()
        {
            var clone = MemberwiseClone() as Damage;
            clone.Properties = Properties.Clone() as DamageProperties;
            return clone;
        }
        public Damage Amount(double count)
        {
            amount = count;
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
        public IEnumerable<Damage> Split(params double[] ratios)
        {
            if (ratios.Sum() <= 0.99 && ratios.Sum() >= 1.01) throw new ArgumentException("Damage splits should total to 1.");
            IEnumerable<Damage> result = new List<Damage>(ratios.Length);
            foreach (var i in ratios)
            {
                result = result.Append((Clone() as Damage).Amount(amount * i));
            }
            return result;
        }
        internal class DamageProperties : ICloneable
        {
            private DamageType type = DamageType.None;
            private double critRate = 0;
            private double critDamage = 2;
            private double ignoreDefense = 0;
            public DamageType GetDamageType() => type;
            public double GetCritRate() => critRate;
            public double GetCritDamage() => critDamage;
            public double GetDefenseIgnore() => ignoreDefense;
            public object Clone() { return MemberwiseClone(); }
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
        internal class DamageModifier
        {
            public enum Property
            {
                Amount,
                CritRate,
                CritDamage,
                IgnoreDefense
            }
            public enum Operation
            {
                Add,
                Multiply
            }
            public Property damageProperty;
            public Operation operation;
            public DamageModifier(Property prop, Operation op)
            {
                damageProperty = prop;
                operation = op;
            }
        } // devilish but genius idea. trust
        public bool Crits()
        {
            return 1 < Properties.GetCritRate(); // an RNG is supposed to go here but I haven't made that yet
        }
    }
}
