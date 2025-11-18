using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace FrogBattle.Input
{
    public interface IKeyboardInterface
    {
        IReadOnlyDictionary<Keys, InputTypes> InputMap { get; }
        IReadOnlySet<Keys> ValidKeys => InputMap.Keys.ToHashSet();
        InputTypes Convert(Keys key);
        bool IsInputDown(InputTypes inputType);
    }
}