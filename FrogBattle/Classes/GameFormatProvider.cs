using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static FrogBattle.Classes.StatusEffectDefinition;

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
                Damage dm => Format(format, dm.GetSnapshot(1), formatProvider),
                Damage.Snapshot ds => format[0] == 'x' ? (format.Length == 1 ? throw new FormatException("Unable to use auto formatting without a second token") : Localization.Translate("damage.display." + format[1] switch
                {
                    // f - Full
                    'f' => "full",
                    // s - Short
                    's' => "short",
                    // Unknown
                    _ => throw new FormatException($"Unknown format string token: {format[1]}")
                }, ds)) : string.Join(' ', format.Select(x => x switch
                {
                    // a - Amount
                    'a' => ds.Amount.ToString("F0"),
                    // t - Type
                    't' => Localization.Translate("damage.type." + ds.Info.Type.ToString().FirstLower()),
                    // s - Source
                    's' => Localization.Translate("damage.source." + ds.Info.Source.ToString().FirstLower()),
                    // c - Critical
                    'c' => ds.IsCrit ? Localization.Translate("damage.critical") : null,
                    // Unknown
                    _ => throw new FormatException($"Unknown format string token: {x}")
                }).Where(x => x != null)),

                StatusEffectInstance ef => format[0] == 'x' ? (format.Length == 1 ? throw new FormatException("Unable to use auto formatting without second token") : Localization.Translate("effect.display." + format[1] switch
                {
                    // s - Simple
                    's' => "simple",
                    // t - Text (Natural Language)
                    't' => "text",
                    // o - Turns only
                    'o' => "onlyTurns",
                    // n - Nameless
                    'n' => "nameless",
                    // e - Effects only
                    'e' => "effects",
                    // Unknown
                    _ => throw new FormatException($"Unknown format string token: {format[1]}")
                }, ef, ef.Is(Flags.Infinite) ? null : Localization.Translate("effect.display." + (format[1] == 's' ? "simple" : "generic") + ".turnAddon", ef))) : string.Join(' ', format.Select(x => x switch
                {
                    // n - Name
                    'n' => ef.Name,
                    // e - Effects
                    'e' => string.Join(", ", ef.Subeffects.Values.Select(x => x.GetLocalizedText())),
                    // t - Turns
                    't' => ef.Is(Flags.Infinite) ? "∞" : ef.Turns.ToString(),
                    // s - Stacks + Buff / Debuff arrow
                    's' => ef.Stacks < 5 ? new string(ef.Is(Flags.Debuff) ? '↓' : '↑', (int)ef.Stacks) : ((ef.Is(Flags.Debuff) ? '↓' : '↑') + ef.Stacks.ToString()),
                    // b - Buff / Debuff (word)
                    'b' => ef.Is(Flags.Debuff) ? "debuff" : "buff",
                    // Unknown
                    _ => throw new FormatException($"Unknown format string token: {format[0]}")
                })),

                Character ch => format[0] switch
                {
                    // c - Caps
                    'c' => string.Format(Instance, $"{{0:{format[1..]}}}", arg).ToUpper(),
                    // u - Uppercase first letter
                    'u' => string.Format(Instance, $"{{0:{format[1..]}}}", arg).FirstUpper(),
                    // p - Pronoun
                    'p' => format.Length == 1 ? throw new FormatException("Pronoun token requires selector!") : ch.Pronouns.PronArray[int.Parse([format[1]])],
                    // n - Name
                    'n' => ch.Name,
                    // v - Verb related bullshit
                    'v' => format.Length == 1 ? throw new FormatException("Verb token requires selector!") : format[1] switch
                    {
                        // i - is / are
                        'i' => ch.Pronouns.Singular ? "is" : "are",
                        // w - was / were
                        'w' => ch.Pronouns.Singular ? "was" : "were",
                        // h - has / have
                        'h' => ch.Pronouns.Singular ? "has" : "have",
                        // y - Distinction between -y and -ies
                        'y' => ch.Pronouns.Singular ? "ies" : "y",
                        // e - Extra "es" for the verbs of some pronouns
                        'e' => ch.Pronouns.Singular ? "es" : string.Empty,
                        // s - Extra 's' for the verbs of some pronouns
                        's' => ch.Pronouns.Singular ? "s" : string.Empty,
                        // Unknown
                        _ => throw new FormatException($"Unknown format string token: {format[1]}")
                    },
                    // Unknown
                    _ => throw new FormatException($"Unknown format string token: {format[0]}")
                },
                // Double it and pass it to the next user!
                _ => string.Format($"{{0:{format}}}", arg)
            };
        }
    }
}
