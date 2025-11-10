using System;

namespace FrogBattle.Scene
{
    internal class MainMenuScene : IGameScene
    {
        private readonly SelectableTable menu;
        private readonly FrogBattle _game;
        public MainMenuScene(FrogBattle game, Texture2D boxTexture, Texture2D cursorTexture)
        {
            _game = game;
            menu = new SelectableTable(Point.Zero, new(1, 3), boxTexture, new(20), cursorTexture, Point.Zero);
        }

        public void Update()
        {
        }
        public void MoveCursor(Input.Direction dir)
        {
            switch (dir)
            {
                case Input.Direction.Left:
                    menu.MoveCursorLeft();
                    break;
                case Input.Direction.Right:
                    menu.MoveCursorRight();
                    break;
                case Input.Direction.Up:
                    menu.MoveCursorUp();
                    break;
                case Input.Direction.Down:
                    menu.MoveCursorDown();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), $"Unknown direction: {dir}");
            }
        }
    }
}
