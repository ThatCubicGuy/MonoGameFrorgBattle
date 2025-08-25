using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FrogBattle
{
    internal static class Localization
    {
        public static Dictionary<string, string> strings;
        public static void Load(string json)
        {
            strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
        }
        public static string Translate(string key, params object[] args)
        {
            if (!strings.TryGetValue(key, out var template))
                return key;

            // problem with this is that i don't pass full statuseffects as parameters
            //template = template.Replace("{arrow}", args.Length > 0 && args[0] is Classes.StatusEffect.Effect effect ? (effect.IsBuff ? "↑" : "↓") : string.Empty);

            return string.Format(template, args);
        }
    }
}
