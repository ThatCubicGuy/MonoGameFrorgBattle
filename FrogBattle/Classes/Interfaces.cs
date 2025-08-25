using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    internal interface ISubcomponent<T> where T : class
    {
        public T Parent { get; }
        public double Amount { get; }
        public Operators Op { get; }
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
    public enum DamageSources
    {
        None,
        Attack,
        Additional,
        FollowUp,
        DamageOverTime,
    }
    public enum Stats
    {
        None,
        MaxHp,
        MaxMana,
        MaxEnergy,
        Atk,
        Def,
        Spd,
        Dex,
        EffectHitRate,
        EffectRES,
        AllTypeRES,
        ManaCost,
        ManaRegen,
        IncomingHealing,
        OutgoingHealing,
        ShieldToughness,
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
