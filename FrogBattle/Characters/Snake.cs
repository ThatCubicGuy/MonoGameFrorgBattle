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
        private readonly List<Ability> abilities = [];
        public Snake(string name) : base(name)
        {
            abilities.Add(new ThrowGrenade(this));
        }
        private class ThrowGrenade : Ability
        {
            public ThrowGrenade(Character source) : base(source, new(typeof(ThrowGrenade).Name, AbilitySettings.None))
            {
                Init(new Cost(this, Pools.Mana, 15, CostProperties.None));
            }
        }
    }
}
