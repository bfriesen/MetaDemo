using System;

namespace ConsoleApplication17
{
    class Program
    {
        static void Main(string[] args)
        {
            Foo foo;

            foo = new Foo
            {
                Bar = new Bar
                {
                    Qux = 123
                },
                Baz = "abc"
            };

            foo.Dump();

            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }
    }

    public class Foo
    {
        public Bar Bar { get; set; }
        public string Baz { get; set; }
    }

    public class Bar
    {
        public int Qux { get; set; }
    }
}
