using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FrogBattle
{
    internal static class Localization
    {
        public static Dictionary<string, string> strings = [];
        public static void Load(string json)
        {
            strings = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(json)) ?? [];
        }
        public static string Translate(string key, params object[] args)
        {
            if (!strings.TryGetValue(key, out var template))
                return key;
            //Console.WriteLine(template);
            //foreach (var item in args) Console.WriteLine(item);
            return string.Format(template, args);
        }
        public static string Translate(KeyValuePair<string, object[]> kvp, params object[] args)
        {
            return Translate(kvp.Key, [..kvp.Value, ..args]);
        }
        public static string Translate((string, object[]) tuple, params object[] args)
        {
            return Translate(tuple.Item1, [..tuple.Item2, ..args]);
        }
    }
}
