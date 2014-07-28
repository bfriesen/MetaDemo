using MetaDemo.MessageHandlers;
using MetaDemo.Messages;
using System;

namespace MetaDemo
{
    public class MessageRouter
    {
        public void Route(string xmlMessage)
        {
            try
            {
                var messageType = GetMessageType(xmlMessage);

                switch (messageType)
                {
                    case "FooMessage":
                    {
                        var message = Deserialize<FooMessage>(xmlMessage);
                        var handler = new FooHandler();
                        handler.Handle(message);
                        break;
                    }
                    case "BarMessage":
                    {
                        var message = Deserialize<BarMessage>(xmlMessage);
                        var handler = new BarHandler();
                        handler.Handle(message);
                        break;
                    }
                    case "BazMessage":
                    {
                        var message = Deserialize<BazMessage>(xmlMessage);
                        var handler = new BazHandler();
                        handler.Handle(message);
                        break;
                    }
                    // Imagine dozens or hundreds of messages here.
                    default:
                        LogError("Unknown message type: " + messageType);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        #region Plumbing (i.e. not relevant to the talk)

        private string GetMessageType(string xmlMessage)
        {
            var doc = System.Xml.Linq.XDocument.Parse(xmlMessage);
            return doc.Root.Name.LocalName;
        }

        private T Deserialize<T>(string xmlMessage)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using (var reader = new System.IO.StringReader(xmlMessage))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        private void LogError(string message)
        {
            Console.WriteLine("ERROR: " + message);
        }

        #endregion
    }
}
