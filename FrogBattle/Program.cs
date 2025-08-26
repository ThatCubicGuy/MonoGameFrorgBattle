using FrogBattle;
using System;

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
                var game = new ConsoleFrorgBattle();
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
            Console.WriteLine($"Invalid argument(s): {args}");
        }
        return 0;
    }
}
