using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrogBattle.Classes;

namespace FrogBattle.Characters
{
    internal class Snake : Character
    {
        private readonly FrozenDictionary<int, Type> _abilities = new Dictionary<int, Type>()
        {
            { 1, typeof(ThrowGrenade) },
        }
        .ToFrozenDictionary();
        public Snake(string name, Battle battle) : base(name, battle)
        {

        }
        public class ThrowGrenade : BlastAttack
        {
            public static Damage.Properties DamageProps => new
                (
                    Type: DamageTypes.Blast,
                    Source: DamageSources.Attack,
                    DefenseIgnore: 0,
                    TypeResPen: 0
                );
            public ThrowGrenade(Character source, Character target) : base(source, new(typeof(ThrowGrenade).Name, false), target,

                ratio: 1.15, scalar: Stats.Atk, hitRate: 0.80, independentHitRate: false, props: DamageProps, split: [], falloff: 0.5) {
                WithGenericManaCost(13);
            }
        }
        protected override Ability SelectAbility(Character target, object selector)
        {
            return selector switch
            {
                1 => new ThrowGrenade(this, target),
                _ => throw new ArgumentOutOfRangeException(nameof(selector), $"Invalid ability number: {selector}")
            };
        }
    }
}
