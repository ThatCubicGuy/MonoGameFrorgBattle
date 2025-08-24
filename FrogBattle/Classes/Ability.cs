using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static FrogBattle.Classes.Ability;

namespace FrogBattle.Classes
{
    internal class Ability
    {
        private Fighter source;
        public AbilitySettings Settings { get; set; }
        public List<AbilityCost> Costs = [];
        public Ability(Fighter source, AbilitySettings settings, params AbilityCost[] costs)
        {
            this.source = source;
            Settings = settings;
            Costs = [.. costs];
        }
        // rewrite #6 gazillion lmfao
        internal struct AbilityCost
        {
            public double amount;
            public Pools currency;
            public CostProperties props;
        }
        [Flags]
        public enum AbilitySettings
        {
            repeatsTurn = 1 << 0,
        }
        [Flags]
        public enum CostProperties
        {
            soft = 1 << 0,
            reverse = 1 << 1,
            keep = 1 << 2,
        }
        private Func<Fighter, int> Effect { get; set; }
        private Action<Fighter> Display { get; set; }
        public void Use()
        {
            // no idea lmao, how do you display things
        }
    }
    internal class AbilityBuilder
    {
        public AbilityBuilder()
        {

        }
        public AbilityBuilder SingleTargetDamage(Stats source, double ratio, Damage.Properties props, params uint[] split)
        {
            
            return this;
        }
    }
}
