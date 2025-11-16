using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace FrogBattle.Input
{
	internal record class KeyboardInterface() : IKeyboardInterface
	{
		public KeyboardInterface(Keys[] cancelKeys, Keys[] confirmKeys, Keys[] rightKeys, Keys[] leftKeys, Keys[] upKeys, Keys[] downKeys) : this()
		{
			InputMap = new Dictionary<InputTypes, Keys[]>()
			{
				{ InputTypes.Cancel, cancelKeys },
				{ InputTypes.Confirm, confirmKeys },
				{ InputTypes.Right, rightKeys },
				{ InputTypes.Left, leftKeys },
				{ InputTypes.Up, upKeys },
				{ InputTypes.Down, downKeys },
			}.AsReadOnly();
		}

		public IReadOnlyDictionary<InputTypes, Keys[]> InputMap { get; }

		public InputTypes Convert(Keys key)
		{
			throw new NotImplementedException();
		}

		public bool IsInputDown(InputTypes inputType)
		{
			throw new NotImplementedException();
		}
	}
}
