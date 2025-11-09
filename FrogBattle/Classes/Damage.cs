using System;

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
        public Damage(IDamageSource source, IDamageable target, double amount, DamageInfo info)
        {
            Source = source;
            Target = target;
            baseAmount = amount;
            Info = info;
            if (!(source is null || info is null)
                && info.CanCrit) Crit = source.IsCrit(target);
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
                if (Source is Character sc)
                {
                    total += total * (sc.CalcDamageTypeBonus(Info.Type, Target as Character) + sc.CalcDamageSourceBonus(Info.Source, Target as Character));
                    total += Crit ? total * sc.GetStatVersus(Stats.CritDamage, Target as Character) : 0;
                    total += total * sc.CalcDamageBonus(Target as Character);
                }
                // Incoming bonuses
                if (Target is Character tg)
                {
                    total -= total * (tg.CalcDamageTypeRES(Info.Type, Source as Character) * Math.Max(0, 1 - Info.TypeResPen) + tg.CalcDamageSourceRES(Info.Source, Source as Character));
                    total -= tg.GetStatVersus(Stats.Def, Source as Character) * Math.Max(0, 1 - Info.DefenseIgnore);
                    total -= total * tg.CalcDamageRES(Source as Character);
                }
                return total;
            }
        }
        public IDamageSource Source { get; }
        public IDamageable Target { get; }
        public DamageInfo Info { get; }
        public bool Crit { get; }
        public Snapshot GetSnapshot(double ratio) => new(Amount * ratio, Info, Crit, Source, Target);
        public double Take(double ratio = 1)
        {
            // ?
            var snapshot = GetSnapshot(ratio);
            Source.DealDamage(snapshot);
            Target.TakeDamage(snapshot);
            return snapshot.Amount;
        }
        public Damage Clone() => MemberwiseClone() as Damage;
        public record Snapshot(double Amount, DamageInfo Info, bool IsCrit, IDamageSource Source = null, IDamageable Target = null) : IHasTarget
        {
            Character IHasTarget.User => Source as Character;

            Character IHasTarget.Target => Target as Character;

            public void Take(double ratio = 1) => Target.TakeDamage(this with { Amount = Amount * ratio });
        }
        public static readonly Damage Missed = null;
        // uuuuuuuuugh
        public sealed class MissedDamage : Damage
        {
            public MissedDamage() : base(null, null, 0, null) { }
        }
    }
}
