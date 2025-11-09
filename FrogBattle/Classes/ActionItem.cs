using System;

namespace FrogBattle.Classes
{
    internal class ActionItem(IHasTurn Actor) : IComparable<ActionItem>
    {
        public double ActionValue { get; set; } = Actor.BaseActionValue;
        public IHasTurn Actor { get; set; } = Actor;
        public int CompareTo(ActionItem obj)
        {
            if (obj == null) return 1;  // debating this ngl
            if (obj == this) return 0;
            return ActionValue.CompareTo(obj.ActionValue);
        }
        public static bool operator <(ActionItem a, ActionItem b)
        {
            return a.CompareTo(b) < 0;
        }
        public static bool operator >(ActionItem a, ActionItem b)
        {
            return a.CompareTo(b) > 0;
        }
        public static bool operator <=(ActionItem a, ActionItem b)
        {
            return a.CompareTo(b) <= 0;
        }
        public static bool operator >=(ActionItem a, ActionItem b)
        {
            return a.CompareTo(b) >= 0;
        }
    }
}
