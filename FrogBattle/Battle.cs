using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrogBattle.Classes;

namespace FrogBattle
{
    internal class Battle
    {
        private static readonly Random random = new();
        public static double RNG => random.NextDouble();
        public List<ActionItem> ActionBar { get; } = [];
        public List<IExtraTurn> InstaQueue { get; } = [];
        public List<Character> Team1 { get; } = new List<Character>(4);
        public List<Character> Team2 { get; } = new List<Character>(4);
        public Battle(Character fighter1, Character fighter2)
        {
            Team1 = [fighter1];
            Team2 = [fighter2];
        }
        public Battle(IEnumerable<Character> team1, IEnumerable<Character> team2)
        {
            Team1 = [.. team1];
            Team2 = [.. team2];
        }
    }
    internal record ActionItem(int ActionValue, ITakesAction ActionEvent) : IComparable<ActionItem>
    {
        public int CompareTo(ActionItem obj)
        {
            if (obj == null) return 1;
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
