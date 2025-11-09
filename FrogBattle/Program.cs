using FrogBattle.Classes;
using System;
using System.Linq;

internal class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            using var game = new FrogBattle.FrorgBattle();
            game.Run();
        }
        else if (args[0] == "-c" || args[0] == "--console")
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Starting ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Frog");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Battle");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" in Console Mode...\n");
                
                var game = new ConsoleBattleManager();
                return game.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("\nRough.");
            }
        }
        else
        {
            Console.WriteLine($"Invalid argument(s): {args.ToList()}");
        }
        return 0;
    }
}
