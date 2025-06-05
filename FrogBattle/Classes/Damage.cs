using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MonoBattleFrorgGame.FrorgBattle;

namespace MonoBattleFrorgGame.Classes
{
    internal class Damage : ICloneable
    {
        private double amount = 0;
        public Properties Props { get; private set; }
        public object Clone()
        {
            var clone = MemberwiseClone() as Damage;
            clone.Props = Props.Clone() as Properties;
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
        public struct DamageInstance // detonates self with mind
        {
            public double Amount;
            public DamageType Type;
            public bool Crit;
            public DamageInstance(Damage damage)
            {
                Amount = damage.amount;
                Type = damage.Props.GetDamageType();
                Crit = damage.Crits();
            }
        }
        public DamageInstance GetInstance(params Modifier[] mods)
        {
            // idk i initially thought adding five gazillion types was a good idea i guess...
            return new DamageInstance(this);
        }
        internal class Properties : ICloneable
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
            public Properties @Type(DamageType newType)
            {
                type = newType;
                return this;
            }
            public Properties CritRate(double critRate)
            {
                this.critRate = critRate;
                return this;
            }
            public Properties CritDmg(double critDmg)
            {
                critDamage = critDmg;
                return this;
            }
            public Properties IgnoreDefense(double ignoreDefense)
            {
                this.ignoreDefense = ignoreDefense;
                return this;
            }
        }
        internal class Modifier : IModifier
        {
            public double Amount { get; private set; }
            public IModifier.Operation Op { get; }
            public enum Property
            {
                Amount,
                CritRate,
                CritDamage,
                IgnoreDefense
            }
            public Property damageProperty;
            public Modifier(double amount, Property prop, IModifier.Operation op)
            {
                Amount = amount;
                damageProperty = prop;
                Op = op;
            }
        } // devilish but genius idea. trust
        public bool Crits()
        {
            return RNG < Props.GetCritRate();
        }
    }
}
