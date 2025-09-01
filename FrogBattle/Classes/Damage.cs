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
        /// <param name="target">Character to which this damage is going to be applied.</param>
        /// <param name="amount">The amount of damage, pre-calculations. Usually ratio * scalar.</param>
        /// <param name="info">Damage properties like type, source, crit, crit damage, defense ignore, and type res pen.</param>
        public Damage(Character source, Character target, double amount, DamageInfo info)
        {
            Source = source;
            Target = target;
            baseAmount = amount;
            DamageInfo = info;
            if (info.CanCrit) Crit = source.IsCrit(target);
            else Crit = false;
        }
        /// <summary>
        /// Automatically calculates outgoing bonuses from the source fighter,
        /// such as damage type boosts, and incoming resistances on the target.
        /// <br/>Order: base -> Type and Source Bonus -> Crit Damage -> Damage Bonus -> Type and Source RES -> DEF -> Damage RES
        /// <br/>(Damage stacks after each calculation.)
        /// </summary>
        public double Amount
        {
            get
            {
                var total = baseAmount;
                // Outgoing bonuses
                if (Source != null)
                {
                    total += total * (
                        + Source.GetActives<DamageTypeBonus>().FindAll(x => x.Type == DamageInfo.Type).Sum(x => x.Amount)
                        + Source.GetActives<DamageSourceBonus>().FindAll(x => x.Source == DamageInfo.Source).Sum(x => x.Amount)

                        + Source.GetPassives<DamageTypeBonus>().FindAll(x => x.Type == DamageInfo.Type).Sum(x => x.Amount)
                        + Source.GetPassives<DamageSourceBonus>().FindAll(x => x.Source == DamageInfo.Source).Sum(x => x.Amount)
                        );
                    total += Crit ? total * Source.GetStatVersus(Stats.CritDamageBonus, Target) : 0;
                    total += total * Source.GetActives<DamageBonus>().Sum(x => x.Amount);
                }
                // Incoming bonuses
                if (Target != null)
                {
                    total -= total * (Target.GetActives<DamageTypeRES>().FindAll((x) => x.Type == DamageInfo.Type).Sum(x => x.Amount) * (1 - DamageInfo.TypeResPen) + Target.GetActives<DamageSourceRES>().FindAll((x) => x.Source == DamageInfo.Source).Sum(x => x.Amount)/* * (1 - Props.SourceResPen) // idk maybe one day :pensive:*/);
                    total -= Target.GetStat(Stats.Def) * (1 - DamageInfo.DefenseIgnore);
                    total -= total * Source.GetActives<DamageRES>().Sum(x => x.Amount);
                }
                return total;
            }
        }
        public Character Source { get; }
        public Character Target { get; }
        public DamageInfo DamageInfo { get; }
        public bool Crit { get; }
        public DamageSnapshot GetSnapshot() => new(Amount, DamageInfo, Source);
        public void Take(double ratio) => Target.TakeDamage(this, ratio);
        public Damage Clone() => MemberwiseClone() as Damage;
        public record DamageSnapshot(double Amount, DamageInfo Info, Character Source = null);
    }
}
