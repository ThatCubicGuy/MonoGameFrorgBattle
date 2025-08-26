using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal static class DoubleExtensions
    {
        /// <summary>
        /// Checks whether <paramref name="value"/> is within the inclusive interval [<paramref name="minValue"/>, <paramref name="maxValue"/>].
        /// Null values imply no bounding on that side.
        /// </summary>
        /// <param name="value">The value to check for.</param>
        /// <param name="minValue">The lower bound of the interval.</param>
        /// <param name="maxValue">The upper bound of the interval.</param>
        /// <returns>True if <paramref name="value"/> is within the interval, false otherwise.</returns>
        public static bool IsWithinBounds(this double value, double? minValue, double? maxValue)
        {
            return !(value < minValue || value > maxValue);
        }
    }
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
        public static double? Apply(this Operators op, double? amount, double baseValue)
        {
            if (amount == null) return null;
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
        /// <param name="pool">The stat to check the value for.</param>
        /// <returns>The current value of the stat requested.</returns>
        public static double Resolve(this Character source, Pools pool)
        {
            return pool switch
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
        /// <summary>
        /// Finds the Stat that corresponds to the maximum value of this <see cref="Pools"/> item.
        /// Returns <see cref="Stats.None"/> for pools that do not have a maximum value.
        /// </summary>
        /// <param name="pool">Pool whose corresponding max stat to find.</param>
        /// <returns></returns>
        public static Stats Max(this Pools pool)
        {
            return pool switch
            {
                Pools.Hp => Stats.MaxHp,
                Pools.Mana => Stats.MaxMana,
                Pools.Energy => Stats.MaxEnergy,
                _ => Stats.None
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
                totalValue += item.Amount;
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
        /// <param name="change">Cost whose value to add.</param>
        /// <returns>True if the cost was found and the value added, false otherwise.</returns>
        public static bool AddToChange(this Ability self, Ability.PoolChange change)
        {
            if (self.PoolChanges.TryGetValue(change.Pool, out var value))
            {
                if (value.Op != change.Op) return false;
                self.PoolChanges[change.Pool] = value with { Amount = change.Amount + value.Amount };
                return true;
            }
            return false;
        }
    }
}
