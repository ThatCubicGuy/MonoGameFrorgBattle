using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace FrogBattle.Input
{
	public static class GameInput
	{
		private static KeyboardState previousKeyboardState;
		private static KeyboardState currentKeyboardState;
		// Will actually implement this later trust me bro
		private static readonly GamePadState[] previousGamePadStates = new GamePadState[MaximumGamePadCount];
		private static readonly GamePadState[] currentGamePadStates = new GamePadState[MaximumGamePadCount];
		public static void Update()
		{
			previousKeyboardState = currentKeyboardState;
			currentKeyboardState = Keyboard.GetState();
			currentGamePadStates.CopyTo(previousGamePadStates, 0);
			for (int i = 0; i < GamePadCount; ++i)
			{
				currentGamePadStates[i] = GamePad.GetState(i);
			}
        }
        private static bool ActiveCurrentFrame(InputTypes input)
        {
            var keys = KeyboardInterface.InputMap[input];
            return keys.Any(x => currentKeyboardState.IsKeyDown(x));
        }
		private static bool ActiveLastFrame(InputTypes input)
        {
            var keys = KeyboardInterface.InputMap[input];
            return keys.Any(x => previousKeyboardState.IsKeyDown(x));
        }
        public static bool InputActive(InputTypes input) => ActiveCurrentFrame(input);
        public static bool InputInactive(InputTypes input) => !InputActive(input);
		public static bool InputPressed(InputTypes input) =>  ActiveCurrentFrame(input) && !ActiveLastFrame(input);
        public static bool InputUnpressed(InputTypes input) => !ActiveCurrentFrame(input) && ActiveLastFrame(input);
        /// <summary>
        /// Gets or sets the amount of gamepads currently connected.
        /// </summary>
        public static uint GamePadCount { get; set; } = 0;
		public static uint MaximumGamePadCount { get; } = 4;
		public static IKeyboardInterface KeyboardInterface { get; } = new KeyboardInterface([Keys.X, Keys.LeftShift, Keys.Escape], [Keys.Z, Keys.Enter], [Keys.Right, Keys.D], [Keys.Left, Keys.A], [Keys.Up, Keys.W], [Keys.Down, Keys.S]);

		// public static bool InputReceived(InputTypes input) => KeyboardInterface.IsInputDown(input);
		public static async Task<InputTypes> InputReceived()
		{
			// return await currentKeyboardState.GetPressedKeys().Equals(KeyboardInterface.ValidKeys)
			throw new NotImplementedException();
		}
	}
}
