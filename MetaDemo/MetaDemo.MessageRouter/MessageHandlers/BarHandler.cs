using MetaDemo.Messages;
using System;

namespace MetaDemo.MessageHandlers
{
    public class BarHandler
    {
        public void Handle(BarMessage message)
        {
            Console.WriteLine("HANDLED: BarHandler handled BarMessage");
        }
    }
}
