using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FrogBattle.Classes.BattleManagers
{
    internal abstract class BattleManager
    {
        private static readonly Random random = new();
        public static double RNG => random.NextDouble();
        public List<ActionItem> ActionBar { get; private set; } = [];
        public List<ITakesAction> InstaQueue { get; private set; } = [];
        public List<Character> Team1 { get; } = new List<Character>(4);
        public List<Character> Team2 { get; } = new List<Character>(4);
        public Character LeftSide { get => Team1.Single(); }
        public Character RightSide { get => Team2.Single(); }
        public StringBuilder BattleText { get; } = new StringBuilder();
        public Game SourceGame { get; }
        public BattleManager(Game game)
        {
            SourceGame = game;
            Init();
        }

        public abstract void Init();
        public abstract void Update();
        public abstract int GetPlayerInput();

        public int GetPlayerCount() => Team1.Count + Team2.Count;
        public int Run()
        {
            int turns = 0;
            try
            {
                while (true)
                {
                    Update();
                    turns++;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Caught {0} exception in application {1}: \"{2}\"", ex.GetType().FullName, ex.Source, ex.Message);
            }
            return turns;
        }
        public abstract void UpdateInstaQueue();
    }
}
