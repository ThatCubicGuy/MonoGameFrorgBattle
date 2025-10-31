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
        Reflect,
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
        CritDamage,
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
        AddValue,
        MultiplyBase,
        MultiplyTotal
    }
    public enum ChanceTypes
    {
        Fixed,
        Base
    }
    public enum EffectTypes
    {
        Custom,
        Modifier,
        Shield,
        Barrier,
        Drain,
        DamageOverTime
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
        Healing,
        Miss,
        End
    }
    public enum ConditionTypes
    {
        EffectStacks,
        EffectsTypeCount,
        EffectTypeStacks,
        // For every 1000 atk, get 10% crit rate ahh
        StatValue,
        // When above 1000 atk, get 10% crit rate ahh
        StatThreshold,
        // For every 100 HP / 10% of MaxHP, get 10% crit rate
        PoolValue,
        // When above 100 HP / 10% of MaxHP, get 10% crit rate
        PoolThreshold
    }
    public enum AdditionalEffectTypes
    {
        AdditionalDamage,
        TriggersDot,
        FollowUp,
    }
    [Flags] public enum DoTTypes
    {
        None = 0,
        Bleed   = 1 << 0,
        Burn    = 1 << 1,
        Shock   = 1 << 2,
        WindShear = 1 << 3,
    }
}
