using System;
using System.Text;

namespace TestPlace
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine('\u2191' == '↑');
            foreach (var item in Enum.GetValues(typeof(AMOGUS)))
            {
                Console.WriteLine(item.ToString());
            }
            var bruh = int.Parse(Console.ReadLine() ?? string.Empty);
            var buddy = Enumerator(bruh);
            while (buddy.MoveNext())
            {
                Console.WriteLine(buddy.Current);
            }
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
