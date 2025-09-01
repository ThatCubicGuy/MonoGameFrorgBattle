using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static FrogBattle.Classes.StatusEffect;

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
        /// <param name="pool">The pool to check the value for.</param>
        /// <returns>The current value of the pool requested.</returns>
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
                var item = effect.GetModifier(stat);
                totalValue += item.Amount;
            }
            return totalValue;
        }
    }
    internal static class FighterExtensions
    {
        public static string ToConsoleString(this Character src, string format)
        {
            throw new NotImplementedException();
        }
    }
    internal static class AbilityExtensions
    {
        public static bool IsHit(this IAttack ability, Character Target)
        {
            return ability.AttackInfo.HitRate == null || BattleManager.RNG < (ability.AttackInfo.HitRate + ability.Parent.GetStat(Stats.HitRateBonus) - (Target.GetStat(Stats.Dex) / 100));
        }
        /// <summary>
        /// Creates a list containing every instance of damage dealt
        /// </summary>
        /// <param name="ability">The ability for which to calculate the damage(s).</param>
        /// <param name="target">Target of the damages.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException">Ability damage split should not be longer than 16.</exception>
        public static List<Damage> AttackDamage(this IAttack ability, Character target)
        {
            if (ability.AttackInfo.Split?.Length > 16) throw new InvalidDataException("Cannot split damage into more than 16 parts!");
            var result = new List<Damage>();
            if (!ability.AttackInfo.IndependentHitRate && !ability.IsHit(target))
            {
                return null;
            }
            if (ability.AttackInfo.Split == null || ability.AttackInfo.Split.Length == 0)
            {
                // If it's not IHR then hit or not doesn't matter, we calculated that already.
                // Though here it technically shouldn't matter at all. What's the point of
                // having IndependentHitRate when your split is 0...
                result.Add(!ability.AttackInfo.IndependentHitRate || ability.IsHit(target) ? new(ability.Parent, target,
                ability.AttackInfo.Ratio * ability.Parent.GetStatVersus(ability.AttackInfo.Scalar, ability.Target), ability.AttackInfo.DamageInfo) : null);
            }
            else
            {
                long sum = ability.AttackInfo.Split.Sum((x) => x);
                foreach (var i in ability.AttackInfo.Split)
                {
                    result.Add(!ability.AttackInfo.IndependentHitRate || ability.IsHit(target) ? new(ability.Parent, target,
                    ability.AttackInfo.Ratio * i / sum, ability.AttackInfo.DamageInfo) : null);
                }
            }
            return result.All(x => x == null) ? null : result;
        }
    }
    internal static class ConsoleBattleExtensions
    {
        public static string Console_ToString(this Character src)
        {
            return string.Join(' ', $"{src.Name}{(src.Name.Length <= 7 ? '\t' : string.Empty)}\t\t[{src.Hp:0} HP,",
                $"{src.GetStat(Stats.Atk):0} ATK,",
                $"{src.GetStat(Stats.Def):0} DEF,",
                $"{src.GetStat(Stats.Spd):0} SPD,",
                $"{src.GetStat(Stats.Dex):0} DEX,",
                $"{src.Mana:0} MP,",
                $"{src.Energy:0}/{src.GetStat(Stats.MaxEnergy)}] ") + 
                string.Join(' ', src.GetActives().Select((x) => string.Format(GameFormatProvider.Instance, "{0:xs}", x)));
        }
        public static Ability Console_SelectAbility(this Character src)
        {
            Ability result = null;
            bool repeat = true;
            while (repeat)
            {
                repeat = false;
                Console.Write("{0} selects ability... ", src.Name);
                try
                {
                    int selector = int.Parse(Console.ReadLine());
                    result = src.SelectAbility(src.EnemyTeam.Single(), selector);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    repeat = true;
                }
            }
            return result;
        }
    }
}
