using FrogBattle.Classes.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

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
                Operators.MultiplyTotal => throw new InvalidOperationException("Can only multiply total when operating on pools."),
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }
        public static double Apply(this Operators op, double amount, Pools pool, Character fighter)
        {
            return op switch
            {
                Operators.AddValue => amount,
                Operators.MultiplyBase => amount * fighter.Base[pool.Max()],
                Operators.MultiplyTotal => amount * fighter.Resolve(pool),
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
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
        public static string GetLocalizedText(this Stats stat)
        {
            return Localization.Translate(string.Join('.', typeof(Stats).Name.FirstLower(), stat.ToString().FirstLower()));
        }
    }
    internal static class EffectExtensions
    {
        #region Actives

        /// <summary>
        /// Searches <see cref="Character.ActiveEffects"/> for all effects which contain subeffects that
        /// meet the required criteria given by the <paramref name="predicate"/>.
        /// </summary>
        /// <param name="ch">The character on which to search for subeffects.</param>
        /// <param name="predicate">The conditions for filtering subeffects.</param>
        /// <returns>A list of <see cref="StaticSubeffectInstance"/> that meet the criteria.</returns>
        public static List<ISubeffectInstance> GetActives(this ICanHaveActives ch, Func<ISubeffectInstance, bool> predicate)
        {
            return [.. ch.ActiveEffects.SelectMany(x => x.Subeffects.Values.Where(predicate))];
        }
        /// <summary>
        /// Searches <see cref="Character.ActiveEffects"/> for any effects which contain subeffects of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of subeffect to search for.</typeparam>
        /// <param name="ch">The character on which to search for subeffects.</param>
        /// <returns>A list of every <see cref="StaticSubeffectInstance"/> with a definition of type <typeparamref name="TResult"/>.</returns>
        public static List<ISubeffectInstance> GetActives<TResult>(this ICanHaveActives ch) where TResult : ISubeffect
        {
            return ch.GetActives(x => x.Definition is TResult);
        }
        /// <summary>
        /// Calculates the full modification of a certain stat by every active <see cref="StatusEffectInstance"/>.
        /// This method applies stack counts.
        /// </summary>
        /// <param name="stat">The stat whose modifications to search for.</param>
        /// <returns>A double that represents the modification from the base value of the given stat.</returns>
        public static double GetActivesValue(this ICanHaveActives ch, Stats stat)
        {
            return ch.ActiveEffects.Sum(x => (x.GetModifier(stat)?.Amount ?? 0) * x.Stacks);
        }
        /// <summary>
        /// Determines whether an active effect is currently attached to the <see cref="ICanHaveActives"/> entity.
        /// </summary>
        /// <typeparam name="TEffect">The type of the StatusEffect to check for.</typeparam>
        /// <param name="ch">The character for which to run the test.</param>
        /// <returns>True if the effect is present, false otherwise.</returns>
        public static bool EffectIsActive<TEffect>(this ICanHaveActives ch) where TEffect : StatusEffectDefinition
        {
            return ch.ActiveEffects.Any(x => x.Definition is TEffect);
        }

        #endregion

        #region Passives

        /// <summary>
        /// Searches <see cref="PassiveEffects"/> for all effects that contain an effect of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <returns>A list of every <typeparamref name="TResult"/> effect from the fighter's currently active PassiveEffects.</returns>
        public static List<ISubeffectInstance> GetPassives<TResult>(this ICanHavePassives ch) where TResult : ISubeffect
        {
            return [.. ch.PassiveEffects.SelectMany(x => x.Subeffects.Values.Where(x => x.Definition is TResult))];
        }
        /// <summary>
        /// Calculates the full modification of a certain stat by every active <see cref="PassiveEffect"/>.
        /// This method applies stack counts.
        /// </summary>
        /// <param name="stat">The stat whose modifications to search for.</param>
        /// <returns>A double that represents the modification from the base value of the given stat.</returns>
        public static double GetPassivesValue(this ICanHavePassives ch, Stats stat, Character target)
        {
            return ch.PassiveEffects.Sum(x => (x.GetModifier(stat)?.Amount ?? 0) * x.GetStacks(target));
        }

        #endregion

        #region Both

        /// <summary>
        /// Gets the subeffect that modifies <paramref name="stat"/> in some way.
        /// </summary>
        /// <param name="ef">The effect for which to check modifiers.</param>
        /// <param name="stat">The stat of the modifier for which to search.</param>
        /// <returns>A <see cref="StaticSubeffectInstance"/> with a <see cref="Modifier"/> definition that acts on <paramref name="stat"/>.</returns>
        public static ISubeffectInstance GetModifier(this IAttributeModifier ef, Stats stat)
        {
            return ef.Subeffects.TryGetValue(Modifier.GetKeyStatic(stat), out var result) ? result : null;
        }

        public static ISubeffectInstance GetDamageBonus(this IAttributeModifier ef)
        {
            return ef.Subeffects.TryGetValue(DamageBonus.GetKeyStatic(), out var result) ? result : null;
        }

        public static ISubeffectInstance GetDamageTypeBonus(this IAttributeModifier ef, DamageTypes type)
        {
            return ef.Subeffects.TryGetValue(DamageTypeBonus.GetKeyStatic(type), out var result) ? result : null;
        }

        public static ISubeffectInstance GetDamageSourceBonus(this IAttributeModifier ef, DamageSources source)
        {
            return ef.Subeffects.TryGetValue(DamageSourceBonus.GetKeyStatic(source), out var result) ? result : null;
        }

        public static ISubeffectInstance GetDamageRES(this IAttributeModifier ef)
        {
            return ef.Subeffects.TryGetValue(DamageRES.GetKeyStatic(), out var result) ? result : null;
        }

        public static ISubeffectInstance GetDamageTypeRES(this IAttributeModifier ef, DamageTypes type)
        {
            return ef.Subeffects.TryGetValue(DamageTypeRES.GetKeyStatic(type), out var result) ? result : null;
        }

        public static ISubeffectInstance GetDamageSourceRES(this IAttributeModifier ef, DamageSources source)
        {
            return ef.Subeffects.TryGetValue(DamageSourceRES.GetKeyStatic(source), out var result) ? result : null;
        }

        /// <summary>
        /// Get all subeffects of type <typeparamref name="TResult"/> within this <see cref="PassiveEffect"/>.
        /// </summary>
        /// <returns>A dictionary containing every effect of type <typeparamref name="TResult"/>.</returns>
        public static Dictionary<object, ISubeffectInstance> GetSubeffectsOfType<TResult>(this IAttributeModifier ef) where TResult : ISubeffect
        {
            return ef.Subeffects.Where(x => x.Value.Definition is TResult).ToDictionary();
        }
        /// <summary>
        /// Returns the only <see cref="ISubeffectInstance"/> with a definition of type <typeparamref name="TResult"/>,
        /// or a default value if there is none, and throws an exception if there is more than one element that satisfies this condition.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="ef"></param>
        /// <returns>The only <see cref="ISubeffectInstance"/> of type <typeparamref name="TResult"/>, or a default value if there is none.</returns>
        public static ISubeffectInstance SingleEffect<TResult>(this IAttributeModifier ef) where TResult : ISubeffect
        {
            return ef.Subeffects.Values.SingleOrDefault(x => x is TResult);
        }
        #endregion
    }
    internal static class FighterExtensions
    {
        /// <summary>
        /// Calculates the full damage bonus against a certain target.
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="target"></param>
        /// <returns>A double representing the percent of extra damage to be dealt. 0 by default.</returns>
        public static double CalcDamageBonus(this ISupportsEffects ch, Character target)
        {
            return ch.ActiveEffects.Sum(x => x.GetDamageBonus().Amount * x.Stacks) + ch.PassiveEffects.Sum(x => x.GetDamageBonus().Amount * x.GetStacks(target));
        }
        /// <summary>
        /// Calculates the full damage resistance against a certain target.
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="target"></param>
        /// <returns>A double representing the percent of damage to be resisted. 0 by default.</returns>
        public static double CalcDamageRES(this ISupportsEffects ch, Character target)
        {
            return ch.ActiveEffects.Sum(x => x.GetDamageRES().Amount * x.Stacks) + ch.PassiveEffects.Sum(x => x.GetDamageBonus().Amount * x.GetStacks(target));
        }
        /// <summary>
        /// Calculates the full type-specific damage bonus against a certain target.
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <returns>A double representing the percent of extra damage to be dealt. 0 by default.</returns>
        public static double CalcDamageTypeBonus(this ISupportsEffects ch, DamageTypes type, Character target)
        {
            return ch.ActiveEffects.Sum(x => x.GetDamageTypeBonus(type).Amount * x.Stacks) + ch.PassiveEffects.Sum(x => x.GetDamageBonus().Amount * x.GetStacks(target));
        }
        /// <summary>
        /// Calculates the full type-specific damage resistance against a certain target.
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <returns>A double representing the percent of damage to be resisted. 0 by default.</returns>
        public static double CalcDamageTypeRES(this ISupportsEffects ch, DamageTypes type, Character target)
        {
            return ch.ActiveEffects.Sum(x => x.GetDamageTypeRES(type).Amount * x.Stacks) + ch.PassiveEffects.Sum(x => x.GetDamageBonus().Amount * x.GetStacks(target));
        }
        /// <summary>
        /// Calculates the full source-specific damage bonus against a certain target.
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns>A double representing the percent of extra damage to be dealt. 0 by default.</returns>
        public static double CalcDamageSourceBonus(this ISupportsEffects ch, DamageSources source, Character target)
        {
            return ch.ActiveEffects.Sum(x => x.GetDamageSourceBonus(source).Amount * x.Stacks) + ch.PassiveEffects.Sum(x => x.GetDamageBonus().Amount * x.GetStacks(target));
        }
        /// <summary>
        /// Calculates the full source-specific damage resistance against a certain target.
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns>A double representing the percent of damage to be resisted. 0 by default.</returns>
        public static double CalcDamageSourceRES(this ISupportsEffects ch, DamageSources source, Character target)
        {
            return ch.ActiveEffects.Sum(x => x.GetDamageSourceRES(source).Amount * x.Stacks) + ch.PassiveEffects.Sum(x => x.GetDamageBonus().Amount * x.GetStacks(target));
        }
    }
    internal static class SourceTargetExtensions
    {
        public static ISourceTargetContext Swap(this ISourceTargetContext ctx) => new UserTargetWrapper(ctx.Target, ctx.Source);
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
                string.Join(' ', src.ActiveEffects.Where(x => !x.Is(EffectFlags.Hidden)).Select(x => string.Format(GameFormatProvider.Instance, "{0:xs}", x)));
        }
        public static AbilityInstance Console_SelectAbility(this Character src)
        {
            while (true)
            {
                Console.Write("{0} selects ability... ", src.Name);
                try
                {
                    int selector = int.Parse(Console.ReadLine());
                    return src.LaunchAbility(src.EnemyTeam.Single(), selector);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
    internal static class StringExtensions
    {
        public static string FirstLower(this string str)
        {
            return char.ToLower(str[0]) + (str.Length > 1 ? str[1..] : string.Empty);
        }
        public static string FirstUpper(this string str)
        {
            return char.ToUpper(str[0]) + (str.Length > 1 ? str[1..] : string.Empty);
        }
        public static string camelCase(this string str, char delimiter)
        {
            return string.Join('.', str.Split(delimiter).Select(x => x.FirstLower()));
        }
    }
}
