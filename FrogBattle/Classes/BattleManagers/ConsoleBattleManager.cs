using FrogBattle.Characters;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace FrogBattle.Classes.BattleManagers
{
    internal class ConsoleBattleManager() : BattleManager(null)
    {
        public override void Init()
        {
            // dont think you can have a headless battle manager lmao
            // at least not THAT easily
            Localization.Load("C:\\Users\\Alex\\source\\repos\\MonoGameFrorgBattle\\FrogBattle\\Content\\en.json");
            if (OperatingSystem.IsOSPlatform("windows")) Console.WindowWidth = 160;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
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
                1 => new Snake(name1, this) { Team = Team1, EnemyTeam = Team2 },
                2 => new Rexulti(name1, this) { Team = Team1, EnemyTeam = Team2 },
                _ => throw new Exception("ACTUALLY KILL YOURSELF")
            });
            Team2.Add(p2 switch
            {
                1 => new Snake(name2, this) { Team = Team2, EnemyTeam = Team1 },
                2 => new Rexulti(name2, this) { Team = Team2, EnemyTeam = Team1 },
                _ => throw new Exception("ACTUALLY KILL YOURSELF")
            });
            Console.WriteLine("Fighters ready!");
        }
        public override void Update()
        {
            ActionBar.ForEach(item => { if (item.Actor is Character c && c.Hp <= 0) c.Die(); });
            ActionBar.Sort();
            UpdateInstaQueue();
            UpdateText();
            var nextUp = ActionBar.First();
            double minActionValue = nextUp.ActionValue;
            ActionBar.ForEach(item => { item.ActionValue -= minActionValue; });
            if (nextUp.Actor.StartTurn())
            {
                
            }
            nextUp.Actor.EndTurn();
            ActionBar.Remove(nextUp);
            ActionBar.Add(new(nextUp.Actor));
        }
        private void UpdateText()
        {
            //Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(LeftSide.Console_ToString());
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(RightSide.Console_ToString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(BattleText);
            BattleText.Clear();
        }
        public override void UpdateInstaQueue()
        {
            while (InstaQueue.Count > 0)
            {
                var item = InstaQueue.First();
                item.StartTurn();
                InstaQueue.Remove(item);
            }
        }
        public virtual void Kill(Character character)
        {
            if (!Team1.Remove(character) && !Team2.Remove(character)) throw new InvalidOperationException($"Cannot kill character {character.Name} because they are not part of the battle.");
            if (Team1.Count == 0 || Team2.Count == 0) UpdateText();
            if (Team1.Count == 0) throw new ApplicationException(string.Format(GameFormatProvider.Instance, $"Team 2 ({string.Join(", ", Team2.Select(x => $"{{{Team2.IndexOf(x)}:N}}"))}) won the game!", [.. Team2]));
            if (Team2.Count == 0) throw new ApplicationException(string.Format(GameFormatProvider.Instance, $"Team 1 ({string.Join(", ", Team1.Select(x => $"{{{Team1.IndexOf(x)}:N}}"))}) won the game!", [.. Team1]));
        }

        public AbilityInstance GetPlayerSelection()
        {
            var abilitySelection = GetPlayerInput();
            var enemySelection = GetPlayerInput();
            throw new NotImplementedException();
        }
        public override int GetPlayerInput()
        {
            throw new NotImplementedException();
        }
    }
}
