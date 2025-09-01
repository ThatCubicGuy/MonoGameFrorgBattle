using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static FrogBattle.Classes.StatusEffect;

namespace FrogBattle.Classes
{
    internal class GameFormatProvider : IFormatProvider, ICustomFormatter
    {
        public static GameFormatProvider Instance { get; } = new GameFormatProvider();
        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
                return this;
            return null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            ArgumentNullException.ThrowIfNull(arg, nameof(arg));
            if (format == null || format.Length == 0) return arg.ToString();
            else format = format.ToLower();
            return arg switch
            {
                //string.Join(' ', format.Select(x => x switch { })), was the compounding option. could've worked tbh
                StatusEffect ef => format[0] switch
                {
                    // n - Name
                    'n' => ef.Name,
                    // e - Effects
                    'e' => ef.ToString(),
                    // t - Turns
                    't' => ef.Is(Flags.Infinite) ? "∞" : ef.Turns.ToString(),
                    // b - Buff
                    'b' => ef.Is(Flags.Debuff) ? "↓" : "↑",
                    // x - Auto localize
                    'x' => format.Length == 1 ? throw new FormatException("Unable to use auto localization without second parameter") : format[1] switch
                    {
                        // s - Simple
                        's' => Localization.Translate("effect.display.simple", ef, ef.Is(Flags.Infinite) ? null :
                        Localization.Translate("effect.display.simple.turnAddon", ef)),
                        // t - Text (Natural Language)
                        't' => Localization.Translate("effect.display.text", ef, ef.Is(Flags.Infinite) ? null :
                        Localization.Translate("effect.display.text.turnAddon", ef)),
                        // n - Nameless
                        'n' => Localization.Translate("effect.display.nameless", ef, ef.Is(Flags.Infinite) ? null :
                        Localization.Translate("effect.display.nameless.turnAddon", ef)),
                        _ => throw new FormatException($"Unknown format string parameter: {format[1]}")
                    },
                    _ => throw new FormatException($"Unknown format string parameter: {format[0]}")
                },
                Character ch => format[0] switch
                {
                    'p' => format.Length == 1 ? throw new FormatException("Pronoun key requires selector") : ch.Pronouns.PronArray[int.Parse([format[1]])],
                    's' => ch.Pronouns.Extra_S ? "s" : string.Empty,
                    'n' => ch.ToString(),
                    _ => throw new FormatException($"Unknown format string parameter: {format[0]}")
                },
                _ => arg.ToString()
            };
        }
    }
}
