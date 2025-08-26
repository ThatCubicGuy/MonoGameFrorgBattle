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
        public class ThrowGrenade : BlastAttack
        {
            public ThrowGrenade(Character source, Character target) : base(source, new(typeof(ThrowGrenade).Name, false), target,

                ratio: 1.1, scalar: Stats.Atk, split: [1], falloff: 0.5) {
                WithCost(new(this, 13, Pools.Mana, Operators.Additive));
            }
        }
    }
}
