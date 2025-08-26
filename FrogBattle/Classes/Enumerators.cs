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
        HitRate,
        EffectHitRate,
        EffectRES,
        AllTypeRES,
        ManaCost,           // Positive = Bad
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
    }
    public enum Operators
    {
        Additive,
        Multiplicative
    }
    public enum Chances
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
}
