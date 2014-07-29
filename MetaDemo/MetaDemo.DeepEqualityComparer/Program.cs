using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaDemo
{
    class Program
    {
        public class Foo
        {
            public Bar[] Bars { get; set; }
            public string Baz { get; set; }
        }

        public class Bar
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        static void Main(string[] args)
        {
            var foo1 = new Foo {
                Bars = new[] {
                    new Bar { Id = 123, Value = "abc" },
                    new Bar { Id = 789, Value = "xyz" } },
                Baz = "Hello, world!" };

            var foo2 = new Foo {
                Bars = new[] {
                    new Bar { Id = 123, Value = "abc" },
                    new Bar { Id = 789, Value = "xyz" } },
                Baz = "Hello, world!" };

            var foo3 = new Foo {
                Bars = new[] {
                    new Bar { Id = 123, Value = "abc" },
                    new Bar { Id = 789, Value = "xyz" } },
                Baz = "Good-bye, cruel world!" };

            var foo4 = new Foo {
                Bars = new[] {
                    new Bar { Id = 123, Value = "abc" },
                    new Bar { Id = 789, Value = "WTF!?" } },
                Baz = "Hello, world"
            };

            var foo5 = new Foo
            {
                Bars = new[] {
                    new Bar { Id = 123, Value = "abc" },
                    new Bar { Id = 789, Value = "xyz" },
                    new Bar { Id = 789, Value = "omg" } },
                Baz = "Hello, world!"
            };

            ShowEquals(foo1, foo2, "foo1", "foo2");
            ShowEquals(foo1, foo3, "foo1", "foo3");
            ShowEquals(foo1, foo4, "foo1", "foo4");
            ShowEquals(foo1, foo5, "foo1", "foo5");

            Console.WriteLine();
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static void ShowEquals(Foo lhs, Foo rhs, string lhsLabel, string rhsLabel)
        {
            var equals = DeepEqualityComparer.Instance.Equals(lhs, rhs);
            Console.WriteLine("({0}, {1}): {2}", lhsLabel, rhsLabel, equals);
        }

        private static bool Equals(Foo lhs, Foo rhs)
        {
            if (lhs == null && rhs == null)
            {
                return true;
            }

            if (lhs == null || rhs == null)
            {
                return false;
            }

            if ((lhs.Bars == null) != (rhs.Bars == null))
            {
                return false;
            }

            if (lhs.Bars != null)
            {
                if (lhs.Bars.Length != rhs.Bars.Length)
                {
                    return false;
                }

                for (int i = 0; i < lhs.Bars.Length; i++)
                {
                    if (lhs.Bars[i].Id != rhs.Bars[i].Id
                        || lhs.Bars[i].Value != rhs.Bars[i].Value)
                    {
                        return false;
                    }
                }
            }

            if (lhs.Baz != rhs.Baz)
            {
                return false;
            }

            return true;
        }
    }
}
