using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    public interface IEffect
    {
        string Name { get; }
        public double Amount { get; set; }
    }
    public enum DamageTypes
    {
        None,
        Blunt,
        Slash,
        Pierce,
        Bullet,
        Blast,
        Magic,
        True
    }
    public enum Stats
    {
        None,
        MaxHp,
        Atk,
        Def,
        Spd,
        EffectHitRate,
        EffectRES,
        AllTypeRES,
        ManaCost,
        ManaRegen,
        IncomingHealing,
        OutgoingHealing,
    }
    public enum Pools
    {
        Hp,
        Mana,
        Energy,
        Special,
        Shield,
        Barrier,
        Custom,
    }
    public enum Operators
    {
        Additive,
        Multiplicative
    }
}
