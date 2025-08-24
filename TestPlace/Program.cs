using static System.Net.Mime.MediaTypeNames;

namespace TestPlace
{
    internal class Program
    {
        public static IEnumerator<double> Test(double ratio, params uint[] split)
        {
            if (split.Length == 0) yield return -999999;
            else
            {
                double sum = split.Sum((x) => (double)x);
                foreach (uint i in split)
                {
                    yield return i * ratio / sum;
                }
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            uint[] split = [];
            foreach (var i in args)
            {
                split = [.. split, uint.Parse(i)];
            }
            var test = Test(1, split);
            while (test.MoveNext())
            {
                Console.WriteLine(test.Current);
            }
        }
    }
}
