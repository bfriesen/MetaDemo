using MetaDemo.Messages;
using System;

namespace MetaDemo.MessageHandlers
{
    public class BarHandler : IMessageHandler<BarMessage>
    {
        public void Handle(BarMessage message)
        {
            Console.WriteLine("HANDLED: BarHandler handled BarMessage");
        }
    }
}
