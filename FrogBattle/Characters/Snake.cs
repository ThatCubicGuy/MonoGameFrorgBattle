using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrogBattle.Classes;

namespace FrogBattle.Characters
{
    internal class Snake : Character
    {
        public Snake(string name) : base(name)
        {
        }
        private class ThrowGrenade : Ability
        {
            public ThrowGrenade(Character source) : base(source, new(typeof(ThrowGrenade).Name, Settings.None))
            {
                Init(new Cost(this, Pools.Mana, 15, CostProperties.None));
            }
        }
    }
}
