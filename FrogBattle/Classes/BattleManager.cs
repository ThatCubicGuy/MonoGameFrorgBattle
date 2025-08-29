using FrogBattle.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle.Classes
{
    // name different to file name just cuz i wanna be able to swap it all out more easily lmao.
    // thats why it even implements shit like Team1 and Team2
    internal class BattleManager
    {
        private static readonly Random random = new();
        public static double RNG => random.NextDouble();
        public List<ActionItem> ActionBar { get; } = [];
        public List<ITakesAction> InstaQueue { get; } = [];
        public List<Character> Team1 { get; } = new List<Character>(4);
        public List<Character> Team2 { get; } = new List<Character>(4);
        public Character LeftSide { get => Team1.Single(); }
        public Character RightSide { get => Team2.Single(); }
        public StringBuilder BattleText { get; } = new StringBuilder();
        public BattleManager()
        {
            Init();
        }
        public virtual void Init()
        {
            // dont think you can have a headless battle manager lmao
            // at least not THAT easily
            Localization.Load("C:\\Users\\Alex\\source\\repos\\MonoGameFrorgBattle\\FrogBattle\\Content\\en.json");
            if (OperatingSystem.IsOSPlatform("windows")) Console.WindowWidth = 160;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("helphlephllbklbhbpblpblb\n");
            Console.Write("Select player 1: ");
            int p1 = int.Parse(Console.ReadLine());
            Console.Write("Select player 2: ");
            int p2 = int.Parse(Console.ReadLine());
            string[][] names =
            [
                ["Solid Snake", "Big Boss"],
                ["Rexulti", "Sephiroth"]
            ];
            int[] indeces = [0, 0, 0, 0, 0, 0, 0];
            string name1 = names[p1 - 1][indeces[p1]++];
            string name2 = names[p2 - 1][indeces[p2]++];
            Team1.Add(p1 switch
            {
                1 => new Snake(name1, this, true),
                2 => new Rexulti(name1, this, true),
                _ => throw new Exception("ACTUALLY KILL YOURSELF")
            });
            Team2.Add(p2 switch
            {
                1 => new Snake(name2, this, false),
                2 => new Rexulti(name2, this, false),
                _ => throw new Exception("ACTUALLY KILL YOURSELF")
            });
            Console.WriteLine("Fighters ready!");
        }
        public int Run()
        {
            int turns = 0;
            foreach (var item in (List<Character>)[.. Team1, .. Team2])
            {
                ActionBar.Add(new(item));
            }
            try
            {
                while (true)
                {
                    //Console.Clear();
                    ActionBar.Sort();
                    while (InstaQueue.Count > 0)
                    {
                        var item = InstaQueue.First();
                        item.TakeAction();
                        InstaQueue.Remove(item);
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(LeftSide.Console_ToString());
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(RightSide.Console_ToString());
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(BattleText.ToString());
                    BattleText.Clear();
                    var nextUp = ActionBar.First();
                    double minActionValue = nextUp.ActionValue;
                    ActionBar.ForEach(item => { item.ActionValue -= minActionValue; });
                    nextUp.Actor.TakeAction();
                    ActionBar.Remove(nextUp);
                    ActionBar.Add(new(nextUp.Actor));
                    turns += 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("[From Battle.Run()]");
            }
            return turns;
        }
    }
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
