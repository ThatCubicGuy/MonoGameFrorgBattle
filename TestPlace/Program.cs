using System;
using System.Text;

namespace TestPlace
{
    internal static class Extensions
    {
        public static string camelCase(this string str)
        {
            return str.ToLower()[0] + str[1..];
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            foreach (var item in Enum.GetValues(typeof(Stats)))
            {
                Console.WriteLine($"\"stats.{item.ToString()?.camelCase()}\": \"\",");
            }
        }
        public enum Stats
        {
            None,
            MaxHp,
            MaxMana,
            MaxEnergy,          // Positive = Bad
            Atk,
            Def,
            Spd,
            Dex,
            CritRate,
            CritDamageBonus,
            HitRateBonus,
            EffectHitRate,
            EffectRES,
            ManaCost,           // Positive = Bad
            ManaRegen,
            EnergyRecharge,
            IncomingHealing,
            OutgoingHealing,
            ShieldToughness,
        }
        private static IEnumerator<string> Enumerator(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                yield return "lol. lmao even.";
            }
            yield break;
        }
        internal class PublishinMyShi
        {

        }
        internal class LikeCommentSubscribe
        {
            //public EventHandler<EventArgs> EventHandler { get; set; }
        }
        private enum AMOGUS
        {
            First,
            Things,
            Second,
            Right,
            these,
            KEYS,
            Are,
            Kinda,
            U_n_i_q_u_e_2,
            You,
            Know
        }
    }
}
