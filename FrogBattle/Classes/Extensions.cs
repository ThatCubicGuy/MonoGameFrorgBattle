using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                Operators.AddValue => amount,
                Operators.MultiplyBase => amount * baseValue,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null),
            };
        }
        public static double? Apply(this Operators op, double? amount, double baseValue)
        {
            if (amount == null) return null;
            return op switch
            {
                Operators.AddValue => amount,
                Operators.MultiplyBase => amount * baseValue,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null),
            };
        }
        public static double Apply(this Operators op, double amount, Stats baseStat, Character fighter)
        {
            return op switch
            {
                Operators.AddValue => amount,
                Operators.MultiplyBase => amount * fighter.Base[baseStat],
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
        public static string GetLocalizedText(this Stats stat)
        {
            return Localization.Translate(string.Join('.', typeof(Stats).Name.FirstLower(), stat.ToString().FirstLower()));
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
        #region Effect Methods


        // Actives

        /// <summary>
        /// Searches <see cref="ActiveEffects"/> for all effects that contain an effect of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <returns>A list of every <typeparamref name="TResult"/> effect from the fighter's currently applied StatusEffects.</returns>
        public static List<TResult> GetActives<TResult>(this ICanHaveActives ch) where TResult : Subeffect
        {
            return [.. ch.ActiveEffects.SelectMany((x) => x.GetSubeffectsOfType<TResult>().Values)];
        }
        /// <summary>
        /// Searches <see cref="ActiveEffects"/> for all status effects that modify the <see cref="Stats"/> <paramref name="stat"/> in some way.
        /// </summary>
        /// <param name="stat">The modifier type to search for.</param>
        /// <returns>An enumerable of StatusEffects that modify <paramref name="stat"/>.</returns>
        public static List<StatusEffect> GetActives(this ICanHaveActives ch, Stats stat)
        {
            return ch.ActiveEffects.FindAll((x) => x.GetModifier(stat) != null);
        }
        /// <summary>
        /// Calculates the full modification of a certain stat by every active <see cref="StatusEffect"/>.
        /// This method applies stack counts.
        /// </summary>
        /// <param name="stat">The stat whose modifications to search for.</param>
        /// <returns>A double that represents the modification from the base value of the given stat.</returns>
        public static double GetActivesValue(this ICanHaveActives ch, Stats stat)
        {
            return ch.GetActives(stat).Sum((x) => x.GetModifier(stat).Amount * x.Stacks);
        }

        // Passives

        /// <summary>
        /// Searches <see cref="PassiveEffects"/> for all effects that contain an effect of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <returns>A list of every <typeparamref name="TResult"/> effect from the fighter's currently active PassiveEffects.</returns>
        public static List<TResult> GetPassives<TResult>(this ICanHavePassives ch) where TResult : Subeffect
        {
            return [.. ch.PassiveEffects.SelectMany((x) => x.GetSubeffectsOfType<TResult>().Values)];
        }
        /// <summary>
        /// Searches <see cref="PassiveEffects"/> for all effects that modify the <see cref="Stats"/> <paramref name="stat"/> in some way.
        /// </summary>
        /// <param name="stat">The modifier type to search for.</param>
        /// <returns>An enumerable of PassiveEffects that modify <paramref name="stat"/>.</returns>
        public static List<PassiveEffect> GetPassives(this ICanHavePassives ch, Stats stat)
        {
            return ch.PassiveEffects.FindAll((x) => x.GetModifier(stat) != null);
        }
        /// <summary>
        /// Calculates the full modification of a certain stat by every active <see cref="PassiveEffect"/>.
        /// This method applies stack counts.
        /// </summary>
        /// <param name="stat">The stat whose modifications to search for.</param>
        /// <returns>A double that represents the modification from the base value of the given stat.</returns>
        public static double GetPassivesValue(this ICanHavePassives ch, Stats stat, Character target)
        {
            return ch.GetPassives(stat).Sum(x => x.GetModifier(stat).Amount * x.GetStacks(target));
        }

        // Both cuz im smart

        /// <summary>
        /// Gets the full outgoing generic damage modification for the given type, against the given target.
        /// </summary>
        /// <param name="target">The target for which to calculate passives. Null by default, which won't count passives.</param>
        /// <returns>The modification value. 0 by default.</returns>
        public static double GetDamageBonus(this ISupportsEffects ch, Character target = null)
        {
            return ch.GetActives<DamageBonus>().Sum(x => x.Amount * (x.Parent as StatusEffect).Stacks) + ch.GetPassives<DamageBonus>().Sum(x => x.Amount * (x.Parent as PassiveEffect).GetStacks(target));
        }
        /// <summary>
        /// Gets the full incoming generic damage modification for the given type, against the given target.
        /// </summary>
        /// <param name="target">The target for which to calculate passives. Null by default, which won't count passives.</param>
        /// <returns>The modification value. 0 by default.</returns>
        public static double GetDamageRES(this ISupportsEffects ch, Character target = null)
        {
            return ch.GetActives<DamageRES>().Sum(x => x.Amount * (x.Parent as StatusEffect).Stacks) + ch.GetPassives<DamageRES>().Sum(x => x.Amount * (x.Parent as PassiveEffect).GetStacks(target));
        }
        /// <summary>
        /// Gets the full outgoing type-specific damage modification for the given type, against the given target.
        /// </summary>
        /// <param name="type">The type of damage to calculate for.</param>
        /// <param name="target">The target for which to calculate passives. Null by default, which won't count passives.</param>
        /// <returns>The modification value. 0 by default.</returns>
        public static double GetDamageTypeBonus(this ISupportsEffects ch, DamageTypes type, Character target = null)
        {
            return ch.GetActives<DamageTypeBonus>().FindAll(x => x.Type == type).Sum(x => x.Amount * (x.Parent as StatusEffect).Stacks) + ch.GetPassives<DamageTypeBonus>().FindAll(x => x.Type == type).Sum(x => x.Amount * (x.Parent as PassiveEffect).GetStacks(target));
        }
        /// <summary>
        /// Gets the full incoming type-specific damage modification for the given type, against the given target.
        /// </summary>
        /// <param name="type">The type of damage to calculate for.</param>
        /// <param name="target">The target for which to calculate passives. Null by default, which won't count passives.</param>
        /// <returns>The modification value. 0 by default.</returns>
        public static double GetDamageTypeRES(this ISupportsEffects ch, DamageTypes type, Character target = null)
        {
            return ch.GetActives<DamageTypeRES>().FindAll(x => x.Type == type).Sum(x => x.Amount * (x.Parent as StatusEffect).Stacks) + ch.GetPassives<DamageTypeBonus>().FindAll(x => x.Type == type).Sum(x => x.Amount * (x.Parent as PassiveEffect).GetStacks(target));
        }
        /// <summary>
        /// Gets the full outgoing source-specific damage modification for the given type, against the given target.
        /// </summary>
        /// <param name="source">The source of damage to calculate for.</param>
        /// <param name="target">The target for which to calculate passives. Null by default, which won't count passives.</param>
        /// <returns>The modification value. 0 by default.</returns>
        public static double GetDamageSourceBonus(this ISupportsEffects ch, DamageSources source, Character target = null)
        {
            return ch.GetActives<DamageSourceBonus>().FindAll(x => x.Source == source).Sum(x => x.Amount * (x.Parent as StatusEffect).Stacks) + ch.GetPassives<DamageSourceBonus>().FindAll(x => x.Source == source).Sum(x => x.Amount * (x.Parent as PassiveEffect).GetStacks(target));
        }
        /// <summary>
        /// Gets the full incoming source-specific damage modification for the given type, against the given target.
        /// </summary>
        /// <param name="source">The source of damage to calculate for.</param>
        /// <param name="target">The target for which to calculate passives. Null by default, which won't count passives.</param>
        /// <returns>The modification value. 0 by default.</returns>
        public static double GetDamageSourceRES(this ISupportsEffects ch, DamageSources source, Character target = null)
        {
            return ch.GetActives<DamageSourceRES>().FindAll(x => x.Source == source).Sum(x => x.Amount * (x.Parent as StatusEffect).Stacks) + ch.GetPassives<DamageSourceRES>().FindAll(x => x.Source == source).Sum(x => x.Amount * (x.Parent as PassiveEffect).GetStacks(target));
        }

        #endregion
    }
    internal static class AbilityExtensions
    {
        public static bool IsHit(this IAttack ability, Character Target)
        {
            return ability.AttackInfo.HitRate == null || BattleManager.RNG < (ability.AttackInfo.HitRate + ability.Parent.GetStatVersus(Stats.HitRateBonus, Target) - (Target.GetStatVersus(Stats.Dex, ability.Parent) / 100));
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
                string.Join(' ', src.ActiveEffects.Where(x => !x.Is(StatusEffect.Flags.Hidden)).Select(x => string.Format(GameFormatProvider.Instance, "{0:xs}", x)));
        }
        public static Ability Console_SelectAbility(this Character src)
        {
            while (true)
            {
                Console.Write("{0} selects ability... ", src.Name);
                try
                {
                    int selector = int.Parse(Console.ReadLine());
                    return src.SelectAbility(src.EnemyTeam.Single(), selector);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
    internal static class UniversalExtensions
    {
        public static string FirstLower(this string str)
        {
            return str.ToLower()[0] + (str.Length > 1 ? str[1..] : string.Empty);
        }
        public static string FirstUpper(this string str)
        {
            return str.ToUpper()[0] + (str.Length > 1 ? str[1..] : string.Empty);
        }
    }
}
