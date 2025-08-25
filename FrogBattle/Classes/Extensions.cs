using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal static class EnumExtensions
    {
        public static double Apply(this Operators op, double amount, double baseValue)
        {
            return op switch
            {
                Operators.Additive => amount,
                Operators.Multiplicative => amount * baseValue,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null),
            };
        }
        public static double Apply(this Operators op, double amount, Stats baseStat, Character fighter)
        {
            return op switch
            {
                Operators.Additive => amount,
                Operators.Multiplicative => amount * fighter.Base[baseStat],
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null),
            };
        }
        /// <summary>
        /// Get the corresponding stat value from the <see cref="Pools"/> enum.
        /// </summary>
        /// <param name="source">The fighter for which to resolve the stat.</param>
        /// <param name="stat">The stat to check the value for.</param>
        /// <returns>The current value of the stat requested.</returns>
        public static double Resolve(this Character source, Pools stat)
        {
            return stat switch
            {
                Pools.Hp => source.Hp,
                Pools.Mana => source.Mana,
                Pools.Energy => source.Energy,
                Pools.Special => source.Special,
                Pools.Shield => source.Shield,
                Pools.Barrier => source.Barrier,
                _ => 0
            };
        }
        public static double Resolve(this Pools pool, Character source)
        {
            return source.Resolve(pool);
        }
        public static string camelCase(this string str)
        {
            return str.ToLower()[0] + str[1..];
        }
    }
    internal static class IEnumerableExtensions
    {
        public static double Calculate(this IEnumerable<StatusEffect> list, Stats stat, double baseValue)
        {
            double totalValue = 0;
            foreach (var effect in list)
            {
                var item = effect.GetModifiers()[stat];
                totalValue += item.Op.Apply(item.Amount, baseValue);
            }
            return totalValue;
        }
    }
    internal static class FighterExtensions
    {
        public static string ToConsoleString(this Character src)
        {
            throw new NotImplementedException();
        }
        public static string ToConsoleString(this Character src, string format)
        {
            throw new NotImplementedException();
        }
    }
    internal static class AbilityExtensions
    {
        /// <summary>
        /// Tries to compound the value of a new cost with an already existing cost. This method will not add
        /// the value if the cost doesn't already exist, or if their operators differ.
        /// </summary>
        /// <param name="cost">Cost whose value to add.</param>
        /// <returns>True if the cost was found and the value added, false otherwise.</returns>
        public static bool AddToCost(this Ability self, Cost cost)
        {
            if (self.Costs.TryGetValue(cost.Pool, out var value))
            {
                if (value.Op != cost.Op) return false;
                self.Costs[cost.Pool] = value with { Amount = cost.Amount + value.Amount };
                return true;
            }
            return false;
        }
    }
}
