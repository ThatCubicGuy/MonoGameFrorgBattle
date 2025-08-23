using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal abstract class Ability
    {
        protected Fighter source;
        protected Ability()
        {

        }
    }
    internal class Ability_ver1
    {
        private Fighter source;
        public AbilitySettings Settings { get; set; }
        public List<AbilityCost> Costs = [];
        public Ability_ver1(AbilitySettings settings, params AbilityCost[] costs)
        {
            Settings = settings;
            Costs = [.. costs];
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
        public enum Conditions
        {
            none,
            hasBuff,
            
        }
        private Func<Fighter, int> Effect { get; set; }
        private Action<Fighter> Display { get; set; }
        public void Use()
        {
            // no idea lmao, how do you display things
        }
    }
}
