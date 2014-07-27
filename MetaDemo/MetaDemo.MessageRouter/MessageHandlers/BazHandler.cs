using MetaDemo.Messages;
using System;

namespace MetaDemo.MessageHandlers
{
    public class BazHandler : IMessageHandler<BazMessage>
    {
        public void Handle(BazMessage message)
        {
            Console.WriteLine("HANDLED: BazHandler handled BazMessage");
        }
    }
}
