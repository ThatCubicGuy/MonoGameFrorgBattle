using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoBattleFrorgGame.Classes
{
    // really not sure what the best course of action is here.
    // shield statuseffects will only ever have one shield effect, but it feels weird to add this
    // special case and leave it as is. maybe i could add different kinds of effects, but then
    // that's redundant. idk tbh.
    internal class StatusEffect
    {
        internal class Shield : StatusEffect
        {

        }
    }
    internal abstract class Effect
    {
        public enum EffectType
        {
            CUSTOM,
            Atk,
            Def,
            Spd,
            // here we also do some thinking. do we separate stat modifiers and damage modifiers? idfk.
        }
    }
    // after adding the IModifier interface i'm tempted to split even more stuff tbh. could end up cool?
}
