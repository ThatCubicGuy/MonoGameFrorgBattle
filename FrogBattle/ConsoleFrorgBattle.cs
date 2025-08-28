using FrogBattle.Characters;
using FrogBattle.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattle
{
    internal class ConsoleFrorgBattle
    {
        public StringBuilder BattleText = new();
        public int Run()
        {
            if (OperatingSystem.IsOSPlatform("windows")) Console.WindowWidth = 160;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Select player 1: ");
            char p1 = (char)Console.Read();
            Console.WriteLine("Select player 2: ");
            char p2 = (char)Console.Read();
            Character Player1, Player2;
            string[] playerNames = { "Solid Snake" };
            switch (p1)
            {
                case 'a' or 'A':
                    Player1 = new Snake(playerNames[0]);
                    playerNames[0] = "Big Boss";
                    break;
                default:
                    throw new Exception("what in the what are you even doing.");
            }
            switch (p2)
            {
                case 'a':
                    Player2 = new Snake(playerNames[0]);
                    break;
                default:
                    throw new Exception("what in the what are you even doing.");
            }
            var game = new Battle(Player1, Player2);
            game.Run();
            return 0;
        }
    }
}
