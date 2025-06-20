using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static FrogBattle.Classes.Damage.Modifier;
using static FrogBattle.FrorgBattle;

namespace FrogBattle.Classes
{
    internal class Damage : ICloneable
    {
        public static Damage operator +(Damage a, Damage b)
        {
            return new Damage().Amount(a.amount + b.amount).CritRate(a.Props.CritRate + b.Props.CritRate).CritDamage(a.Props.CritDamage + b.Props.CritDamage).IgnoreDefense(a.Props.DefenseIgnore + b.Props.DefenseIgnore);
        }
        public static Damage operator *(Damage a, Damage b)
        {
            return new Damage().Amount((a.amount + b.amount) / 2).CritRate((1 - a.Props.CritRate) * (1 - b.Props.CritRate)).CritDamage(Math.Sqrt(a.Props.CritDamage * b.Props.CritDamage)).IgnoreDefense((a.Props.DefenseIgnore + b.Props.DefenseIgnore) / 2);
        }
        private double amount = 0;
        public Properties Props { get; private set; }
        public object Clone()
        {
            return MemberwiseClone();
        }
        public Damage Amount(double count)
        {
            amount = count;
            return this;
        }
        public Damage CritRate(double critRate)
        {
            Props.CritChance(critRate);
            return this;
        }
        public Damage CritDamage(double critDmg)
        {
            Props.CritDmg(critDmg);
            return this;
        }
        public Damage IgnoreDefense(double ignoreDefense)
        {
            Props.IgnoreDefense(ignoreDefense);
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
        internal struct Properties
        {
            private DamageType type = DamageType.None;
            public DamageType GetDamageType() => type;
            public double CritRate { get; private set; } = 0;
            public double CritDamage { get; private set; } = 2;
            public double DefenseIgnore { get; private set; } = 0;
            public Properties(DamageType dmgType, double cr, double cd, double defIgn)
            {
                type = dmgType;
                CritRate = cr;
                CritDamage = cd;
                DefenseIgnore = defIgn;
            }
            public Properties @Type(DamageType newType)
            {
                type = newType;
                return this;
            }
            public Properties CritChance(double critRate)
            {
                CritRate = Math.Min(critRate, 1);
                return this;
            }
            public Properties CritDmg(double critDmg)
            {
                CritDamage = critDmg;
                return this;
            }
            public Properties IgnoreDefense(double ignoreDefense)
            {
                DefenseIgnore = Math.Min(ignoreDefense, 1);
                return this;
            }
        }
        internal class Modifier : IModifier<PropType>
        {
            public enum PropType
            {
                Amount,
                CritRate,
                CritDamage,
                IgnoreDefense,
            }
            public double Amount { get; private set; }
            public Operator Op { get; }
            public PropType Property { get; }
            public Modifier(double amount, PropType prop, Operator op)
            {
                Amount = amount;
                Property = prop;
                Op = op;
            }
            public Damage Apply(Damage dmg)
            {
                var output = dmg.Clone() as Damage;
                return Property switch
                {
                    PropType.Amount => output.Amount(Op.Apply(Amount, dmg.amount)),
                    PropType.CritRate => output.CritRate(Op.Apply(Amount, dmg.Props.CritRate)),
                    PropType.CritDamage=> output.CritDamage(Op.Apply(Amount, dmg.Props.CritDamage)),
                    PropType.IgnoreDefense => output.IgnoreDefense(Op.Apply(Amount, dmg.Props.DefenseIgnore)),
                    _ => throw new ArgumentOutOfRangeException(nameof(Property), Property, null)
                };
            }
        } // devilish but genius idea. trust
        public bool Crits()
        {
            return RNG < Props.CritRate;
        }
    }
}
