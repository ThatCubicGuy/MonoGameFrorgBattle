namespace FrogBattle.Classes
{
    internal class Healing
    {
        private readonly double _baseAmount;
        /// <summary>
        /// Initializes a new instance of <see cref="Healing"/>.
        /// </summary>
        /// <param name="source">Character which sent this heal.</param>
        /// <param name="amount">The amount of healing, pre-calculations. Usually ratio * scalar, or just a flat value.</param>
        public Healing(Character source, Character target, HealingInfo info)
        {
            Source = source;
            Target = target;
            Info = info;
            _baseAmount = info.Amount;
        }
        /// <summary>
        /// Automatically calculates outgoing healing from the source fighter and incoming healing on the target.
        /// </summary>
        public double Amount
        {
            get
            {
                var total = _baseAmount; // Op.Apply(_baseAmount, Target.Hp); if we decide to use Op
                // Outgoing bonuses
                if (Source != null)
                {
                    total *= Source.GetStatVersus(Stats.OutgoingHealing, Target);
                }
                // Incoming bonuses
                if (Target != null)
                {
                    total *= Target.GetStatVersus(Stats.IncomingHealing, Target);
                }
                return total;
            }
        }
        public Character Source { get; }
        public Character Target { get; }
        public HealingInfo Info { get; }
        public double Take(double ratio = 1)
        {
            // ?
            var snapshot = GetSnapshot(ratio);
            Target.TakeHealing(this, ratio);
            return snapshot;
        }
        public double GetSnapshot(double ratio) => Amount * ratio;
        public Healing Clone() => MemberwiseClone() as Healing;
    }
}
