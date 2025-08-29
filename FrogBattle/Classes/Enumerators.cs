using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
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
        MaxEnergy,          // Positive = Bad
        Atk,
        Def,
        Spd,
        Dex,
        CritRate,
        CritDamageBonus,
        HitRateBonus,
        EffectHitRate,
        EffectRES,
        ManaCost,           // Positive = Bad
        ManaRegen,
        EnergyRecharge,
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
        Barrier
    }
    public enum Operators
    {
        Additive,
        Multiplicative
    }
    public enum ChanceTypes
    {
        Fixed,
        Base
    }
    public enum EffectType
    {
        Custom,
        Modifier,
        Shield,
        Barrier,
        Drain,
        DoT
    }
    public enum Scalars
    {
        Light,
        Medium,
        Heavy,
        Huge,
        Massive,
        Mental,
        Deranged,
        Irrational,
        Outrageous,
        Bonkers,
        Baffling,
        Obscene,
        Crikey,
        Cringe
    }
    public enum TextTypes
    {
        Start,
        Damage1 = 1,
        Damage2 = 2,
        Damage3 = 3,
        Damage4 = 4,
        Damage5 = 5,
        Damage6 = 6,
        Damage7 = 7,
        Damage8 = 8,
        Damage9 = 9,
        Damage10 = 10,
        Damage11 = 11,
        Damage12 = 12,
        Damage13 = 13,
        Damage14 = 14,
        Damage15 = 15,
        Damage16 = 16,
        ApplyEffect,
        Miss,
        End
    }
}
