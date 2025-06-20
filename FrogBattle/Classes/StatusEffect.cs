using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    // really not sure what the best course of action is here.
    // shield statuseffects will only ever have one shield effect, but it feels weird to add this
    // special case and leave it as is. maybe i could add different kinds of effects, but then
    // that's redundant. idk tbh.
    internal class StatusEffect
    {
        private readonly List<IEffect> effects;
        public List<IEffect> Effects { get { return effects; } }
    }
    internal interface IEffect;
    //internal abstract class Effect : IEffect
    //{
    //    public enum EffectType
    //    {
    //        CUSTOM,
    //        // here we also do some thinking. do we separate stat modifiers and damage modifiers? idfk.
    //    }
    //}
    // after adding the IModifier interface i'm tempted to split even more stuff tbh. could end up cool?
    internal class StatModifier : IModifier<StatModifier.StatType>
    {
        public enum StatType
        {
            Atk,
            Def,
            Spd,
            EffectHitRate,
            EffectRES,
            AllTypeRES,
        }
        public StatType Property { get; private set; }
        public double Amount { get; private set; }
        public Operator Op { get; }
    }
    internal class MiscModifier : IModifier<MiscModifier.MiscType>
    {
        public enum MiscType
        {
            ManaCost,
            ManaRegen,
            IncomingHealing,
            OutgoingHealing,
        }
        public MiscType Property { get; private set; }
        public double Amount  { get; private set; }
        public Operator Op { get; private set; }
    }
}
