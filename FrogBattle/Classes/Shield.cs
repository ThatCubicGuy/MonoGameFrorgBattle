using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal class CollectiveShield : IDamageable
    {
        public event EventHandler<Damage.Snapshot> DamageTaken;
        public double Hp { get; private set; }
        public void TakeDamage(Damage.Snapshot damage)
        {
            Hp -= damage.Amount;
        }
    }
}
