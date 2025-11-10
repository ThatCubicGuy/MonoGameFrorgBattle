using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace FrogBattle.Input
{
    public static class KeyboardInput
    {
        private static KeyboardState previousState;
        private static KeyboardState currentState;

        public static void Update()
        {
            previousState = currentState;
            currentState = Keyboard.GetState();
        }
    }
    public class GamepadInput
    {
        private GamePadState previousState;
        private GamePadState currentState;

        public GamepadInput(int index)
        {
            Index = index;
        }

        public void Update()
        {
            previousState = currentState;
            currentState = GamePad.GetState(Index);
        }

        public int Index { get; }
    }
}
