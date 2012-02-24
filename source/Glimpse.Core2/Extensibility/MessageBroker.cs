using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Glimpse.Core2.Framework;

namespace Glimpse.Core2.Extensibility
{
    public class MessageBroker : IMessageBroker
    {
        internal IDictionary<Type, List<Subscriber>> Subscriptions { get; set; }
        public ILogger Logger { get; set; }

        public MessageBroker(ILogger logger)
        {
            Contract.Requires<ArgumentNullException>(logger != null, "logger");

            Subscriptions = new Dictionary<Type, List<Subscriber>>();
            Logger = logger;
        }

        public void Publish<T>(T message)
        {
            foreach (var subscriber in GetSubscriptions(typeof (T)))
            {
                try
                {
                    subscriber.Execute(message);
                }
                catch(Exception exception)
                {
                    Logger.Error("Exception calling subscriber with message of type '{0}'.", exception, typeof(T));
                }
            }
        }

        public Guid Subscribe<T>(Action<T> action)
        {
            Contract.Requires<ArgumentNullException>(action!=null, "action");

            var subscriptions = GetSubscriptions(typeof (T));

            var subscriptionId = Guid.NewGuid();
            subscriptions.Add(new Subscriber<T>(action, subscriptionId));

            Logger.Debug("Method '{0}' on type '{1}' has been subscribed to all '{2}' messages.", action.Method.Name, action.Method.DeclaringType, typeof(T)); //TODO: Move to resource

            return subscriptionId;
        }

        public void Unsubscribe<T>(Guid subscriptionId)
        {
            var subscriptions = GetSubscriptions(typeof (T));
            subscriptions.RemoveAll(i => i.SubscriptionId == subscriptionId);
        }

        private List<Subscriber> GetSubscriptions(Type type)
        {
            if (Subscriptions.ContainsKey(type))
                return Subscriptions[type];

            var result = new List<Subscriber>();
            Subscriptions.Add(type, result);

            return result;
        }
    }
}