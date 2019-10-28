using System.Collections.Concurrent;
using AzureSolutions.Util;
using AzureSolutions.Util.FluentValidation;
using Microsoft.Azure.ServiceBus.Core;

namespace AzureSolutions.Messaging.AzureServiceBus
{
    public interface IMessageSenderFactory
    {
        IMessageSender GetMessageSender(string queueName);
    }

    public class AsbMessageSenderFactory : IMessageSenderFactory
    {
        private readonly MessagingConfig _config;

        // We want to reuse MessageSenders.
        // Although ConcurrentDictionary is thread-safe, it does not guarantee GetOrAdd valueFactory delegate will be called only once during
        // concurrent calls, since the internal locking happens after the value is created by the delegate.
        // Each message sender holds one connection so we don't want MessageSenders being created vainly,
        // therefore we use LazyInitializer<T> which handles locking on its .Value creation.
        
        private readonly ConcurrentDictionary<string, LazyInitializer<IMessageSender>> _messageSendersCache
            = new ConcurrentDictionary<string, LazyInitializer<IMessageSender>>();

        public AsbMessageSenderFactory(MessagingConfig config)
        {
            _config = config;
        }

        public IMessageSender GetMessageSender(string queueName)
        {
            Requires.That(queueName, nameof(queueName)).IsNotNullOrEmpty();
            
            return _messageSendersCache.GetOrAdd(queueName, CreateLazyMessageSender).Value;
        }

        private LazyInitializer<IMessageSender> CreateLazyMessageSender(string queueName)
        {
            System.Diagnostics.Trace.WriteLine($"{nameof(CreateLazyMessageSender)} invoked for queue name '{queueName}'");

            Requires.That(_config, c => c.ServiceBusConnectionString, nameof(_config.ServiceBusConnectionString)).IsNotNullOrEmpty();

            return new LazyInitializer<IMessageSender>(() => new MessageSender(_config.ServiceBusConnectionString, queueName));
        }
    }
}
