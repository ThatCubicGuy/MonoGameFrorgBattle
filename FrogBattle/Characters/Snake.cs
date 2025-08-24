using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrogBattle.Classes;

namespace FrogBattle.Characters
{
    internal class Snake : Fighter
    {
        public Snake(string name) : base(name, Registry.DefaultHp, Registry.DefaultAtk, Registry.DefaultDef, Registry.DefaultSpd, Registry.DefaultDex, Registry.DefaultMaxMana, Registry.DefaultMaxEnergy)
        {

        }
    }
}
