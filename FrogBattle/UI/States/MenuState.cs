using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace FrogBattle.UI
{
    internal class MenuState
    {
        private Dictionary<Keys, bool> inactiveLastFrame = [];
        private SelectableTable menu;
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
        private void CheckKey(Keys key, Action action)
        {

            if (Keyboard.GetState().IsKeyDown(key) && inactiveLastFrame[key])
            {
                inactiveLastFrame[key] = false;
                action.Invoke();
            }
            else inactiveLastFrame[key] = true;
        }
        private void MoveUp() => --menu.CursorRow;
        private void MoveLeft() => --menu.CursorColumn;
        private void MoveDown() => ++menu.CursorRow;
        private void MoveRight() => ++menu.CursorColumn;
    }
}
