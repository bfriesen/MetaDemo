using System;

namespace MetaDemo
{
    class Program
    {
        private static readonly MessageRouter _router = new MessageRouter();

        static void Main(string[] args)
        {
            ReceiveMessage("<FooMessage/>");
            ReceiveMessage("<BarMessage/>");
            ReceiveMessage("<BazMessage/>");
            
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }

        static void ReceiveMessage(string xmlMessage)
        {
            Console.WriteLine("RECEIVED: " + xmlMessage);

            _router.Route(xmlMessage);

            Console.WriteLine();
        }
    }
}
