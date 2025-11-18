using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FrogBattle.Input
{
	internal record class KeyboardInterface() : IKeyboardInterface
	{
		public KeyboardInterface(Keys[] cancelKeys, Keys[] confirmKeys, Keys[] rightKeys, Keys[] leftKeys, Keys[] upKeys, Keys[] downKeys) : this()
		{
			var result = new Dictionary<Keys, InputTypes>();
			Array.ForEach(confirmKeys, item => result.Add(item, InputTypes.Confirm));
			Array.ForEach(cancelKeys, item => result.Add(item, InputTypes.Cancel));
			Array.ForEach(rightKeys, item => result.Add(item, InputTypes.Right));
			Array.ForEach(leftKeys, item => result.Add(item, InputTypes.Left));
			Array.ForEach(upKeys, item => result.Add(item, InputTypes.Up));
			Array.ForEach(downKeys, item => result.Add(item, InputTypes.Down));
			InputMap = result;
		}

		public IReadOnlyDictionary<Keys, InputTypes> InputMap { get; }
		
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
