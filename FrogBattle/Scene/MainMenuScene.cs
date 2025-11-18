using FrogBattle.UI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using FrogBattle.Input;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace FrogBattle.Scene
{
    internal class MainMenuScene : IGameScene
    {
        private readonly SelectableTable menu;
        private readonly FrogBattle _game;
        private readonly ISelectableCell[] _cells;
        private readonly Texture2D cellTexture;
        private readonly Texture2D cursorTexture;
        public MainMenuScene(FrogBattle game)
        {
            _game = game;
            cellTexture = _game.Content.Load<Texture2D>("UI/MainMenu/Button");
            cursorTexture = _game.Content.Load<Texture2D>("UI/MainMenu/ButtonHover");
            _cells =
            [
                new SelectableCell(cellTexture, cursorTexture),
                new SelectableCell(cellTexture, cursorTexture),
                new SelectableCell(cellTexture, cursorTexture),
            ];
            var emptyCell = new SelectableCell(cursorTexture, cellTexture);

            menu = new SelectableTable(_cells, new Point(1, 3), Point.Zero, cellTexture.Bounds.Size, new Point(20));
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IGameEntity> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void MoveCursor(InputTypes dir)
        {
            switch (dir)
            {
                case InputTypes.Left:
                    menu.MoveCursorLeft();
                    break;
                case InputTypes.Right:
                    menu.MoveCursorRight();
                    break;
                case InputTypes.Up:
                    menu.MoveCursorUp();
                    break;
                case InputTypes.Down:
                    menu.MoveCursorDown();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), $"Unknown direction: {dir}");
            }
        }
        public void TryMoveCursor(InputTypes direction)
        {
            try
            {
                MoveCursor(direction);
            }
            catch
            {
                Debug.WriteLine(message: "WARNING: Tried moving cursor in an invalid direction", category: "WARNING");
            }
        }

        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
