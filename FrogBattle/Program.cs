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
            var game = new FrogBattle.ConsoleFrorgBattle();
            game.Run();
        }
        return 0;
    }
}
