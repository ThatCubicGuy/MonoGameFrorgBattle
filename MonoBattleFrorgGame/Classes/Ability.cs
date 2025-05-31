using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonoBattleFrorgGame.Classes
{
    internal class Ability
    {
        internal class AbilityCost()
        {
            private double hpCost = 0;
            private double manaCost = 0;
            private double energyCost = 0;
            private double extraCost = 0;
            public double HpCost => hpCost;
            public double ManaCost => manaCost;
            public double EnergyCost => energyCost;
            public double ExtraCost => extraCost;
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
            public AbilityCostProvider Provider = null;
            internal class AbilityCostProvider
            {
                private static double @Default() { return 0; }
                private readonly Func<double> calcHp;
                private readonly Func<double> calcMana;
                private readonly Func<double> calcEnergy;
                private readonly Func<double> calcExtra;
                public AbilityCostProvider(Func<double> HP = null, Func<double> MP = null, Func<double> ENG = null, Func<double> EX = null)
                {
                    calcHp = HP ?? @Default;
                    calcMana = MP ?? @Default;
                    calcEnergy = ENG ?? @Default;
                    calcExtra = EX ?? @Default;
                }

                public AbilityCost Get()
                {
                    return new AbilityCost().Hp(calcHp()).Mana(calcMana()).Energy(calcEnergy()).Extra(calcExtra());
                }
            }
        }
        internal class AbilitySettings(bool percent = false, bool dynamic = false, bool repeats = false)
        {
            public bool IsPercent = percent;
            public bool IsDynamic = dynamic;
            public bool RepeatsTurn = repeats;
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
