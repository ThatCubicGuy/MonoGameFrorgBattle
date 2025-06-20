using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal class Ability
    {
        internal class AbilityCost : ICloneable
        {
            private double hpCost = 0;
            private double manaCost = 0;
            private double energyCost = 0;
            private double extraCost = 0;
            private readonly CostProperties costProperties;
            public AbilityCost(CostProperties costProperties)
            {
                this.costProperties = costProperties;
            }
            public double HpCost => costProperties.CalcHp(hpCost);
            public double ManaCost => costProperties.CalcMana(manaCost);
            public double EnergyCost => costProperties.CalcEnergy(energyCost);
            public double ExtraCost => costProperties.CalcExtra(extraCost);
            object ICloneable.Clone()
            {
                return MemberwiseClone();
            }
            public AbilityCost Hp(double newHpCost)
            {
                hpCost = newHpCost;
                return this;
            }
            public AbilityCost Mana(double newManaCost)
            {
                manaCost = newManaCost;
                return this;
            }
            public AbilityCost Energy(double newEnergyCost)
            {
                energyCost = newEnergyCost;
                return this;
            }
            public AbilityCost Extra(double newSpecialStatCost)
            {
                extraCost = newSpecialStatCost;
                return this;
            }
            internal class CostProperties
            {
                public CostType SoftCost { get; private set; }
                public CostType ReverseCost { get; private set; }
                public CostType ThresholdCost { get; private set; }
                public CostType PercentCost { get; private set; }
                private static double @Default(double val) { return val; }
                public Func<double, double> CalcHp { get; private set; } = @Default;
                public Func<double, double> CalcMana { get; private set; } = @Default;
                public Func<double, double> CalcEnergy { get; private set; } = @Default;
                public Func<double, double> CalcExtra { get; private set; } = @Default;
                public CostProperties Soft(CostType costType)
                {
                    SoftCost = costType;
                    return this;
                }
                public CostProperties Reverse(CostType costType) { 
                    ReverseCost = costType;
                    return this;
                }
                public CostProperties Threshold(CostType costType)
                {
                    ThresholdCost = costType;
                    return this;
                }
                public CostProperties Percent(CostType costType)
                {
                    PercentCost = costType;
                    return this;
                }
                public CostProperties DynamicHp(Func<double, double> func)
                {
                    CalcHp = func ?? @Default;
                    return this;
                }
                public CostProperties DynamicMana(Func<double, double> func)
                {
                    CalcMana = func ?? @Default;
                    return this;
                }
                public CostProperties DynamicEnergy(Func<double, double> func)
                {
                    CalcEnergy = func ?? @Default;
                    return this;
                }
                public CostProperties DynamicExtra(Func<double, double> func)
                {
                    CalcExtra = func ?? @Default;
                    return this;
                }
            }
        }
        internal class AbilitySettings : ICloneable
        {
            public bool RepeatsTurn { get; private set; }
            public AbilitySettings(bool repeats = false)
            {
                RepeatsTurn = repeats;
            }
            object ICloneable.Clone()
            {
                return MemberwiseClone();
            }
            public AbilitySettings Repeat(bool val)
            {
                RepeatsTurn = val;
                return this;
            }
        }
        [Flags]
        public enum CostType
        {
            hp =        1 << 0,   // 1
            mana =      1 << 1,   // 2
            energy =    1 << 2,   // 4
            extra =     1 << 3,   // 8
        }
        protected AbilityCost Cost { get; set; }
        public AbilityCost GetCost()
        {
            return Cost;
        }
        private Func<Fighter, int> Effect { get; set; }
        private Action<Fighter> Display { get; set; }
        public void Use()
        {
            // no idea lmao, how do you display things
        }
    }
}
