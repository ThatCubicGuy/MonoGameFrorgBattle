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
		/// <summary>
		/// Gets or sets the amount of gamepads currently connected.
		/// </summary>
		public static uint GamePadCount { get; set; } = 0;
		public static uint MaximumGamePadCount { get; } = 4;
		public static IKeyboardInterface KeyboardInterface { get; }

		// public static bool InputReceived(InputTypes input) => KeyboardInterface.IsInputDown(input);
		public static async Task<InputTypes> InputReceived()
		{
			// return await currentKeyboardState.GetPressedKeys().Equals(KeyboardInterface.ValidKeys)
			throw new System.NotImplementedException();
		}
	}
}
