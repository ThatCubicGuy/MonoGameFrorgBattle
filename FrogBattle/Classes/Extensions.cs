using System;
using System.Collections.Generic;
using System.Linq;
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
        public static double Resolve(this Stats stat, Fighter source)
        {
            return stat switch
            {
                Stats.MaxHp => source.MaxHp,
                Stats.Atk   => source.Atk,
                Stats.Def   => source.Def,
                Stats.Spd   => source.Spd,
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
}
