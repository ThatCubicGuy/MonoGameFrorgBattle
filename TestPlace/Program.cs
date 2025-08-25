using static System.Net.Mime.MediaTypeNames;

namespace TestPlace
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            //uint[] split = [];
            //foreach (var i in args)
            //{
            //    split = [.. split, uint.Parse(i)];
            //}
            //var test = Test(1, split);
            //while (test.MoveNext())
            //{
            //    Console.WriteLine(test.Current);
            //}
            double joe = 2.154356465;
            double biden = 345;
            Console.WriteLine("{0:0},{1:0.#}", joe, biden);
        }
        private class Parent
        {
            public string Name { get; set; }
            public Parent()
            {
                Name = GetType().BaseType?.Name + ' ' + GetType().Name;
            }
        }
        private class Child : Parent
        {
            public Child() : base()
            {
            }
        }
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
        private class Something
        {
            private readonly double _ratio;
            public Something(double ratio)
            {
                _ratio = ratio;
            }
        }
        private static string Func(params Something[] stuff)
        {
            if (stuff == null) return "awyep it works";
            if (stuff.Length == 0) return "gay";
            Console.WriteLine(stuff);
            return "done lmfao";
        }
        private class Tree
        {
            public Tree(Tree? current = null, Tree? left = null, Tree? right = null)
            {
                Left = left;
                Middle = current;
                Right = right;
            }
            public Tree? Left { get; set; }
            public Tree? Middle { get; set; }
            public Tree? Right { get; set; }
        }
    }
}
