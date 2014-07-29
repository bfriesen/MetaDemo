using MetaDemo.MessageHandlers;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MetaDemo
{
    public class MessageRouter
    {
        private static readonly MethodInfo _deserializeMethod =
            typeof(MessageRouter).GetMethod("Deserialize", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly ConcurrentDictionary<string, Action<string>> _handleFunctions =
            new ConcurrentDictionary<string, Action<string>>();

        public void Route(string xmlMessage)
        {
            try
            {
                var messageType = GetMessageType(xmlMessage);
                var handle = _handleFunctions.GetOrAdd(messageType, mt => CreateHandleFunction(mt, xmlMessage));
                handle(xmlMessage);
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        private Action<string> CreateHandleFunction(string messageTypeName, string xmlMessage)
        {
            var messageType = GetTypeOfMessage(messageTypeName);
            var handlerType = GetTypeOfMessageHandler(messageType);
            var handleMethod = GetHandleMethodInfo(messageType);

            var xmlMessageParameter = Expression.Parameter(typeof(string), "xmlMessage");

            var callDeserialize =
                Expression.Call(
                    Expression.Constant(this),
                    _deserializeMethod,
                    Expression.Constant(messageType),
                    xmlMessageParameter);

            var newMessageHandler = Expression.New(handlerType);

            var callHandle =
                Expression.Call(
                    newMessageHandler,
                    handleMethod,
                    Expression.Convert(callDeserialize, messageType));

            var lambda = Expression.Lambda<Action<string>>(callHandle, xmlMessageParameter);
            return lambda.Compile();
        }

        private static Type GetTypeOfMessage(string messageTypeName)
        {
            return
                Assembly.GetExecutingAssembly().GetTypes()
                    .SingleOrDefault(t => t.Namespace == "MetaDemo.Messages" && t.Name == messageTypeName);
        }

        private static Type GetTypeOfMessageHandler(Type messageType)
        {
            return
                Assembly.GetExecutingAssembly().GetTypes()
                    .SingleOrDefault(
                        t =>
                        t.Namespace == "MetaDemo.MessageHandlers"
                        && !t.IsAbstract
                        && t.GetInterfaces().Any(
                            i =>
                            i.IsGenericType
                            && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>)
                            && i.GetGenericArguments()[0] == messageType));
        }

        private static MethodInfo GetHandleMethodInfo(Type messageType)
        {
            return
                typeof(IMessageHandler<>).MakeGenericType(messageType)
                    .GetMethods()
                    .Single();
        }

        #region Plumbing (i.e. not relevant to the talk)

        private string GetMessageType(string xmlMessage)
        {
            var doc = System.Xml.Linq.XDocument.Parse(xmlMessage);
            return doc.Root.Name.LocalName;
        }

        private object Deserialize(Type type, string xmlMessage)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(type);
            using (var reader = new System.IO.StringReader(xmlMessage))
            {
                return serializer.Deserialize(reader);
            }
        }

        private void LogError(string message)
        {
            Console.WriteLine("ERROR: " + message);
        }

        #endregion
    }
}
