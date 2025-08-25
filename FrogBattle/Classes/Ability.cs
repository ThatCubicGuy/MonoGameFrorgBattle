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
        private readonly Character source;
        private readonly Dictionary<Pools, Cost> costs = [];
        public Ability(Character source, AbilityProperties settings, params Cost[] costs)
        {
            this.source = source;
            Settings = settings;
            foreach (var cost in costs)
            {
                this.costs[cost.Currency] = cost;
            }
        }
        /// <summary>
        /// Initialise the current ability with at least one cost.
        /// </summary>
        /// <param name="costs">Costs to affix to the ability.</param>
        /// <exception cref="ArgumentNullException">Thrown when trying to initialise with no costs.</exception>
        protected void Init(params Cost[] costs)
        {
            if (costs.Length == 0) throw new ArgumentNullException(nameof(costs), "Unable to have a costless ability.");
            foreach (var item in costs)
            {
                this.costs[item.Currency] = item;
            }
        }
        // rewrite #7 gazillion lmfao
        internal record AbilityProperties
        (
            string name,
            AbilitySettings settings
        );
        internal class Cost : ISubcomponent<Ability>
        {
            public Cost(Ability parent, Pools currency, double amount, CostProperties properties)
            {
                Parent = parent;
                Amount = amount;
                Currency = currency;
                Properties = properties;
            }

            public Ability Parent { get; }
            public Pools Currency { get; }
            public double Amount { get; }
            public Operators Op { get; }
            public CostProperties Properties { get; }
        }
        [Flags] public enum AbilitySettings
        {
            None = 0,
            RepeatsTurn = 1 << 0,
        }
        [Flags] public enum CostProperties
        {
            None = 0,
            Soft    = 1 << 0,
            Reverse = 1 << 1,
        }
        public StringBuilder Text { get; } = new StringBuilder();
        public AbilityProperties Settings { get; set; }
        private List<Action<Character, Character>> Effects { get; set; }
        private Action<Character> Display { get; set; }
        public bool TryUse(Character target)
        {
            foreach (var cost in costs)
            {
                if (!source.CanAfford(cost.Value)) return false;
            }
            foreach (var cost in costs)
            {
                source.Expend(cost.Value);
            }
            foreach (var effect in Effects)
            {
                effect(source, target);
            }
            return true;
        }
    }
    internal static class AbilityActions
    {
        public static Action<Character, Character> SingleTargetDamage(this Ability ability, Stats stat, double ratio, double hitRate, Damage.Properties props, params uint[] split)
        {
            return (source, target) =>
            {
                double snap = FrorgBattle.RNG;
                if (snap < hitRate)
                {
                    if (snap < hitRate - Math.Max(0, target.GetStat(Stats.Dex) / 100))
                    {
                        source.OutgoingDamage(stat, ratio, props, split);
                    }
                    else ability.Text.AppendLine("character.generic.dodge");
                }
                else ability.Text.AppendLine("character.generic.miss");
            };
        }
    }
}
