using MetaDemo.Messages;
using System;

namespace MetaDemo.MessageHandlers
{
    public class FooHandler : IMessageHandler<FooMessage>
    {
        public void Handle(FooMessage message)
        {
            Console.WriteLine("HANDLED: FooHandler handled FooMessage");
        }
    }
}
