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

            return string.Format(template, args);
        }
    }
}
