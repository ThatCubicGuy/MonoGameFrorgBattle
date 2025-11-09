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
        internal class Thing : PublishinMyShi
        {

        }
        static void Main(string[] args)
        {
            var thing = new Thing();
            Console.WriteLine("Name: {0}\nFull name: {1}\nDeclaring type: {2}\nBase type: {3}\nAssembly: {4}", thing.GetType().Name, thing.GetType().FullName, thing.GetType().DeclaringType?.Name, thing.GetType().BaseType, thing.GetType().Assembly);
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
