using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal class HpChange : EventArgs
    {
        public double Amount { get; init; }
    }
    internal class DamageTake : EventArgs
    {
        public IDamageSource Source { get; init; }
        public Damage.Snapshot Damage { get; init; }
    }
}
