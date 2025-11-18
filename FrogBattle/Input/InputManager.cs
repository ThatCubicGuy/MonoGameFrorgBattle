using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace FrogBattle.Input
{
	public static class InputManager
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
			for (var i = 0; i < GamePadCount; i++)
			{
				currentGamePadStates[i] = GamePad.GetState(i);
			}
        }
		
		public static List<InputTypes> GetActions()
		{
			var actions = new HashSet<InputTypes>(6);
			foreach (var item in currentKeyboardState.GetPressedKeys())
			{
				actions.Add(KeyboardInterface.InputMap[item]);
			}
			return actions.ToList();
		}
		
        /// <summary>
        /// Gets or sets the amount of gamepads currently connected.
        /// </summary>
        public static uint GamePadCount { get; set; } = 0;
		public static uint MaximumGamePadCount => 4;
		public static IKeyboardInterface KeyboardInterface { get; } = new KeyboardInterface([Keys.X, Keys.LeftShift, Keys.Escape], [Keys.Z, Keys.Enter], [Keys.Right, Keys.D], [Keys.Left, Keys.A], [Keys.Up, Keys.W], [Keys.Down, Keys.S]);
	}
}
