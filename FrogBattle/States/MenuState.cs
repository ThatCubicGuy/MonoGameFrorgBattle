using FrogBattle.UI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace FrogBattle.States
{
    internal class MenuState : IState
    {
        private readonly Dictionary<Keys, bool> inactiveLastFrame = [];
        private readonly SelectableTable table;
        private int _idCounter;
        public MenuState(SelectableTable table)
        {
            this.table = table;
        }

        public int GetId() => _idCounter++;

        public void Update()
        {
            CheckKey(Keys.W, MoveUp);
            CheckKey(Keys.Up, MoveUp);
            CheckKey(Keys.A, MoveLeft);
            CheckKey(Keys.Left, MoveLeft);
            CheckKey(Keys.S, MoveDown);
            CheckKey(Keys.Down, MoveDown);
            CheckKey(Keys.D, MoveRight);
            CheckKey(Keys.Right, MoveRight);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Sprite box in table)
            {
                box.Draw(spriteBatch);
            }
            table.Cursor.Draw(spriteBatch);
        }

        private void CheckKey(Keys key, Action action)
        {

            if (Keyboard.GetState().IsKeyDown(key) && inactiveLastFrame[key])
            {
                inactiveLastFrame[key] = false;
                action.Invoke();
            }
            else if (Keyboard.GetState().IsKeyUp(key)) inactiveLastFrame[key] = true;
        }
        private void MoveUp() => --table.CursorRow;
        private void MoveLeft() => --table.CursorColumn;
        private void MoveDown() => ++table.CursorRow;
        private void MoveRight() => ++table.CursorColumn;
    }
}
