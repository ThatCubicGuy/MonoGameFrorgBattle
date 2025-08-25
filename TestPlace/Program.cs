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
            Child amogus = new();
            Console.WriteLine(amogus.Name);
            Parent amogi = new();
            Console.WriteLine(amogi.Name);
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
    }
}
