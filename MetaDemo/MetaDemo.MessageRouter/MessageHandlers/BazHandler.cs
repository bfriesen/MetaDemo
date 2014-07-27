using MetaDemo.Messages;
using System;

namespace MetaDemo.MessageHandlers
{
    public class BazHandler
    {
        public void Handle(BazMessage message)
        {
            Console.WriteLine("HANDLED: BazHandler handled BazMessage");
        }
    }
}
