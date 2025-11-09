namespace FrogBattle.Classes
{
    /// <summary>
    /// Some generic text for various common fighter actions.
    /// </summary>
    internal readonly struct Generic
    {
        private const string Base = "character.generic.";
        public static string Skip => Base + "skip";
        public static string Miss => Base + "miss";
        public static string Dodge => Base + "dodge";
        public static string Damage => Base + "damage";
        public static string Healing => Base + "healing";
        public static string Stun => Base + "stun";
        public static string ApplyEffect => Base + "applyEffect";
        public static string EffectRES => Base + "effectRES";
        public static string DoT => Base + "dot";
    }
}
