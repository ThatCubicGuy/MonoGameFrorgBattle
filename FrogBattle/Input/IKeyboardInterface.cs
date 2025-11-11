using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace FrogBattle.Input
{
    public interface IKeyboardInterface
    {
        IReadOnlyDictionary<InputTypes, Keys[]> InputMap { get; }
        IReadOnlySet<Keys> ValidKeys => InputMap.Values.SelectMany(x => x.AsEnumerable()).ToHashSet();
        InputTypes Convert(Keys key);
        bool IsInputDown(InputTypes inputType);
    }
}