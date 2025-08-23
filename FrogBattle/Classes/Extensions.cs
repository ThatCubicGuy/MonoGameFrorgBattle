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
        /// <summary>
        /// Get the corresponding stat value from the <see cref="Stats"/> enum.
        /// </summary>
        /// <param name="source">The fighter for which to resolve the stat.</param>
        /// <param name="stat">The stat to check the value for.</param>
        /// <returns>The current value of the stat requested.</returns>
        public static double Resolve(this Fighter source, Stats stat)
        {
            return stat switch
            {
                Stats.MaxHp => source.MaxHp,
                Stats.Atk => source.Atk,
                Stats.Def => source.Def,
                Stats.Spd => source.Spd,
                Stats.EffectHitRate => source.EffectHitRate,
                Stats.EffectRES => source.EffectRES,
                Stats.AllTypeRES => source.AllTypeRES,
                Stats.ManaCost => source.ManaCost,
                Stats.ManaRegen => source.ManaRegen,
                Stats.IncomingHealing => source.IncomingHealing,
                Stats.OutgoingHealing => source.OutgoingHealing,
                _ => 0
            };
        }
        public static double Resolve(this Stats stat, Fighter source)
        {
            return source.Resolve(stat);
        }
    }
    internal static class EnumerableExtensions
    {
        public static double Calculate(this IEnumerable<StatusEffect> list, Stats stat, double baseValue)
        {
            double totalValue = 0;
            foreach (var effect in list)
            {
                foreach(var item in effect.GetModifiers(stat))
                {
                    totalValue += item.Op.Apply(item.Amount, baseValue);
                }
            }
            return totalValue;
        }
    }
    internal static class FighterExtensions
    {
        public static string ToConsoleString(this Fighter src)
        {
            throw new NotImplementedException();
        }
        public static string ToConsoleString(this Fighter src, string format)
        {
            throw new NotImplementedException();
        }
    }
}
