using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static FrogBattle.Classes.StatusEffect;

namespace FrogBattle.Classes
{
    internal class Damage
    {
        private readonly double baseAmount;
        /// <summary>
        /// Initializes a new instance of <see cref="Damage"/>.
        /// </summary>
        /// <param name="source">Character which launched this damage.</param>
        /// <param name="amount">The amount of damage, pre-calculations. Usually ratio * scalar.</param>
        /// <param name="props">Damage properties like type, source, crit, crit damage, defense ignore, and type res pen.</param>
        public Damage(Character source, Character target, double amount, Properties props)
        {
            Source = source;
            Target = target;
            baseAmount = amount;
            Props = props;
        }
        /// <summary>
        /// Automatically calculates outgoing bonuses from the source fighter,
        /// such as damage type boosts, and incoming resistances on the target.
        /// </summary>
        public double Amount
        {
            get
            {
                var total = baseAmount;
                // Outgoing bonuses
                if (Source != null)
                {
                    total += total * Source.GetEffects<DamageTypeBonus>().FindAll((x) => x.Type == Props.Type).Sum((x) => x.Amount);
                    total += Props.Crit ? total * Source.GetStat(Stats.CritDamageBonus) : 0;
                    total += total * Source.GetEffects<DamageBonus>().Sum((x) => x.Amount);
                }
                // Incoming bonuses
                if (Target != null)
                {
                    total -= total * Target.GetEffects<DamageTypeRES>().FindAll((x) => x.Type == Props.Type).Sum((x) => x.Amount) * (1 - Props.TypeResPen);
                    total -= Target.GetStat(Stats.Def) * (1 - Props.DefenseIgnore);
                    total -= total * Source.GetEffects<DamageRES>().Sum((x) => x.Amount);
                }
                return total;
            }
        }
        public Character Source { get; }
        public Character Target { get; }
        public Properties Props { get; }
        public Damage Clone()
        {
            return MemberwiseClone() as Damage;
        }
        public record Properties
        (
            DamageTypes Type,
            DamageSources Source,
            bool Crit = false,
            double DefenseIgnore = 0,
            double TypeResPen = 0
        );
    }
}
// order of effects for damage: atk -> type bonus -> crit -> dmg bonus -> type res -> def -> dmg reduction