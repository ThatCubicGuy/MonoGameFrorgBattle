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
        public static StringBuilder BattleText = new();
        public int Run()
        {
            if (OperatingSystem.IsOSPlatform("windows")) Console.WindowWidth = 160;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            return 0;
        }
    }
}
