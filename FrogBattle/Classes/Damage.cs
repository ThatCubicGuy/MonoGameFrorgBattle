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
            Info = info;
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
                    total += total * (Source.GetDamageTypeBonus(Info.Type, Target) + Source.GetDamageSourceBonus(Info.Source, Target));
                    total += Crit ? total * Source.GetStatVersus(Stats.CritDamage, Target) : 0;
                    total += total * Source.GetDamageBonus(Target);
                }
                // Incoming bonuses
                if (Target != null)
                {
                    total -= total * (Target.GetDamageTypeRES(Info.Type, Source) * (1 - Info.TypeResPen) + Target.GetDamageSourceRES(Info.Source, Target));
                    total -= Target.GetStatVersus(Stats.Def, Source) * (1 - Info.DefenseIgnore);
                    total -= total * Target.GetDamageRES(Source);
                }
                return total;
            }
        }
        public Character Source { get; }
        public Character Target { get; }
        public DamageInfo Info { get; }
        public bool Crit { get; }
        public DamageSnapshot GetSnapshot(double ratio) => new(Amount * ratio, Info, Crit, Source, Target);
        public double Take(double ratio = 1)
        {
            // ?
            var snapshot = GetSnapshot(ratio);
            Target.TakeDamage(this, ratio);
            return snapshot.Amount;
        }
        public Damage Clone() => MemberwiseClone() as Damage;
        public record DamageSnapshot(double Amount, DamageInfo Info, bool IsCrit, Character Source = null, Character Target = null);
    }
}
