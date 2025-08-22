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
        public AbilitySettings Settings { get; set; }
        public List<AbilityCost> Costs = [];
        public Ability(AbilitySettings settings, params AbilityCost[] costs)
        {
            Settings = settings;
            Costs.AddRange(costs);
        }
        // rewrite #5 gazillion lmfao
        internal struct AbilityCost
        {
            public double amount;
            public Pools currency;
            public CostProperties props;
        }
        internal struct AbilitySettings
        {
            public bool repeatsTurn;
        }
        [Flags]
        public enum Properties
        {
            repeatsTurn   = 1 << 0,

        }
        [Flags]
        public enum CostProperties
        {
            soft =      1 << 0,
            reverse =   1 << 1,
            keep =      1 << 2,
        }
        private Func<Fighter, int> Effect { get; set; }
        private Action<Fighter> Display { get; set; }
        public void Use()
        {
            // no idea lmao, how do you display things
        }
    }
}
