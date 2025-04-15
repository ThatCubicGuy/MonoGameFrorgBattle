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
        internal class Cost(int hp = 0, int mana = 0, int energy = 0, int specialStat = 0)
        {
            public int HpCost = hp;
            public int ManaCost = mana;
            public int EnergyCost = energy;
            public int SpecialStatCost = specialStat;
        }
        //internal class PercentCost(double hp = 0, double mana = 0, double energy = 0, double specialStat = 0) : Cost
        //{
        //    // i wanna unify things so idk if i should use this
        //    // will keep the declaration but maybe avoid for now
        //}
        protected Cost AbilityCost { get; set; }
        public Cost GetCost()
        {
            return AbilityCost; // protecting cause it's easier (Ability.GetCost() instead of Ability.AbilityCost);
        }
        public void UpdateCost(int? hp = null, int? mana = null, int? energy = null, int? specialStat = null)
        {
            AbilityCost.HpCost = hp ?? AbilityCost.HpCost;
            AbilityCost.ManaCost = mana ?? AbilityCost.ManaCost;
            AbilityCost.EnergyCost = energy ?? AbilityCost.EnergyCost;
            AbilityCost.SpecialStatCost = specialStat ?? AbilityCost.SpecialStatCost;
        }
        private Func<Fighter, int> Effect { get; set; }
        private Action<Fighter> Display { get; set; }
        public void Use()
        {
            // no idea lmao, how do you display things
        }
    }
}
