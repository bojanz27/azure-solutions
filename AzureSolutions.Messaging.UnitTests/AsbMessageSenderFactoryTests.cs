using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureSolutions.Messaging.AzureServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Xunit;

namespace AzureSolutions.Messaging.UnitTests
{
    public class AsbMessageSenderFactoryTests
    {
        private readonly IMessageSenderFactory _messageSenderFactory;

        public AsbMessageSenderFactoryTests()
        {
            var config = new MessagingConfig
            {
                ServiceBusConnectionString = "Endpoint=sb://nonexistingconnection.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=test",
            };

            _messageSenderFactory = new AsbMessageSenderFactory(config);
        }

        [Fact]
        public void Creates_MessageSender_ForQueueName()
        {
            // prepare 
            const string queueName = "test-queue";

            // execute
            var messageSender = _messageSenderFactory.GetMessageSender(queueName);
            
            // test
            Assert.NotNull(messageSender);
            Assert.Equal(queueName, messageSender.Path);
        }

        [Fact]
        public void Creates_TwoMessageSenders_ForTwoQueueNames()
        {
            // prepare 
            const string queueName1 = "test-queue-1";
            const string queueName2 = "test-queue-2";

            // execute
            var messageSender1 = _messageSenderFactory.GetMessageSender(queueName1);
            var messageSender2 = _messageSenderFactory.GetMessageSender(queueName2);

            // test
            Assert.NotNull(messageSender1);
            Assert.NotNull(messageSender2);

            Assert.Equal(queueName1, messageSender1.Path);
            Assert.Equal(queueName2, messageSender2.Path);

            Assert.NotEqual(messageSender1.ClientId, messageSender2.ClientId);
            Assert.False(object.ReferenceEquals(messageSender1, messageSender2));
        }

        [Fact]
        public void Throws_ArgumentException_IfQueueNameIsNullOrEmpty()
        {
            // prepare 
            const string queueName = "";

            // execute and test
            var ex = Assert.Throws<ArgumentException>(() => _messageSenderFactory.GetMessageSender(queueName));
            Assert.NotNull(ex);
            Assert.Equal(expected: "queueName cannot be null or empty.", actual: ex.Message);
        }

        [Fact]
        public void Throws_Exception_IfServiceBusConnectionString_IsNullOrEmpty()
        {
            // prepare 
            const string queueName = "test-queue";

            var config = new MessagingConfig
            {
                ServiceBusConnectionString = "",
            };

            var messageSenderFactory = new AsbMessageSenderFactory(config);

            // execute and test
            var ex = Assert.Throws<ArgumentException>(() => messageSenderFactory.GetMessageSender(queueName));
            Assert.NotNull(ex);
            Assert.Equal(expected: $"ServiceBusConnectionString cannot be null or empty.", actual: ex.Message);
        }

        [Fact]
        public void Reuses_MessageSender_ForTheSameQueueName_OnSequentualCalls()
        {
            // prepare 
            const string queueName = "test-queue";

            // execute
            var messageSender1 = _messageSenderFactory.GetMessageSender(queueName);
            var messageSender2 = _messageSenderFactory.GetMessageSender(queueName);

            // test
            Assert.NotNull(messageSender1);
            Assert.NotNull(messageSender2);

            Assert.Equal(queueName, messageSender1.Path);
            Assert.Equal(queueName, messageSender2.Path);

            Assert.Equal(messageSender1.ClientId, messageSender2.ClientId);
            Assert.True(object.ReferenceEquals(messageSender1, messageSender2));
        }

        [Fact]
        public void Reuses_MessageSender_ForTheSameQueueName_OnConcurrentThreads()
        {
            // prepare 
            const string queueName = "test-queue";

            IMessageSender messageSender1 = null; 
            IMessageSender messageSender2 = null; 
            IMessageSender messageSender3 = null; 
            IMessageSender messageSender4 = null; 
            IMessageSender messageSender5 = null;

            Thread thread1 = new Thread(new ThreadStart(
                () => messageSender1 = _messageSenderFactory.GetMessageSender(queueName)));
            Thread thread2 = new Thread(new ThreadStart(
                () => messageSender2 = _messageSenderFactory.GetMessageSender(queueName)));
            Thread thread3 = new Thread(new ThreadStart(
                () => messageSender3 = _messageSenderFactory.GetMessageSender(queueName)));
            Thread thread4 = new Thread(new ThreadStart(
                () => messageSender4 = _messageSenderFactory.GetMessageSender(queueName)));
            Thread thread5 = new Thread(new ThreadStart(
                () => messageSender5 = _messageSenderFactory.GetMessageSender(queueName)));

            // execute
            thread1.Start();
            thread2.Start();
            thread3.Start();
            thread4.Start();
            thread5.Start();

            thread1.Join();
            thread2.Join();
            thread3.Join();
            thread4.Join();
            thread5.Join();

            // test
            Assert.NotNull(messageSender1);
            Assert.NotNull(messageSender2);
            Assert.NotNull(messageSender3);
            Assert.NotNull(messageSender4);
            Assert.NotNull(messageSender5);

            // each message sender is for the same queue
            Assert.Equal(queueName, messageSender1.Path); 
            Assert.Equal(queueName, messageSender2.Path); 
            Assert.Equal(queueName, messageSender3.Path); 
            Assert.Equal(queueName, messageSender4.Path); 
            Assert.Equal(queueName, messageSender5.Path);

            // each message sender has the same client Id
            Assert.Equal(messageSender1.ClientId, messageSender2.ClientId); 
            Assert.Equal(messageSender1.ClientId, messageSender3.ClientId); 
            Assert.Equal(messageSender1.ClientId, messageSender4.ClientId); 
            Assert.Equal(messageSender1.ClientId, messageSender5.ClientId);

            // finally check that each message sender is indeed the same object
            Assert.True(object.ReferenceEquals(messageSender1, messageSender2));
            Assert.True(object.ReferenceEquals(messageSender1, messageSender3));
            Assert.True(object.ReferenceEquals(messageSender1, messageSender4));
            Assert.True(object.ReferenceEquals(messageSender1, messageSender5));
        }

        [Fact]
        public async void Reuses_MessageSender_ForTheSameQueueName_OnConcurrentTasks()
        {
            // prepare 
            const string queueName = "test-queue";

            int taskCount = 5;

            IMessageSender[] messageSenders =new IMessageSender[taskCount];
            Task[] tasks = new Task[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                var y = i; // CLOSURE - we dont want to capture i as it is changing outside of the delegate!!!
                tasks[i] = Task.Run(
                    () => messageSenders[y] = _messageSenderFactory.GetMessageSender(queueName));
            }

            // execute
            await Task.WhenAll(tasks);

            // test
            foreach (var ms in messageSenders)
                Assert.NotNull(ms);

            // each message sender is for the same queue
            var groupedByPath = messageSenders.GroupBy(x => x.Path);
            Assert.Single(groupedByPath);

            // each message sender has the same client Id
            var groupedByClientId = messageSenders.GroupBy(x => x.ClientId);
            Assert.Single(groupedByClientId);

            // finally check that each message sender is indeed the same object
            var firstMs = messageSenders[0];
            for (int i = 1; i < taskCount; i++)
                Assert.True(object.ReferenceEquals(firstMs, messageSenders[i]));
        }

        [Fact]
        public async void Reuses_MessageSenders_ForMultipleQueueNames_OnConcurrentTasks()
        {
            // prepare 
            const string queueName0 = "test-queue-0";
            const string queueName1 = "test-queue-1";

            int taskCount = 6;

            IMessageSender[] messageSenders = new IMessageSender[taskCount];
            Task[] tasks = new Task[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                var y = i; // CLOSURE - we dont want to capture i as it is changing outside of the delegate!!!
                tasks[i] = Task.Run(
                    () => {
                        // each "even" message sender gets test-queue-0
                        // each "odd" message sender gets test-queue-1
                        var suffix = y % 2 == 0 ? 0 : 1;
                        messageSenders[y] = _messageSenderFactory.GetMessageSender($"test-queue-{suffix}"); });
            }

            // execute
            await Task.WhenAll(tasks);

            // test
            foreach (var ms in messageSenders)
                Assert.NotNull(ms);

            // each message sender is for the same queue
            var groupedByPath = messageSenders.GroupBy(x => x.Path);
            Assert.Equal(2, groupedByPath.Count());

            // check all even senders are the same object
            var evenSenders = messageSenders.Where(s => s.Path.Equals(queueName0)).ToList();
            var evenSender0 = evenSenders[0];
            Assert.True(object.ReferenceEquals(evenSender0, evenSenders[1]));
            Assert.True(object.ReferenceEquals(evenSender0, evenSenders[2]));

            // check all odd senders are the same object
            var oddSenders = messageSenders.Where(s => s.Path.Equals(queueName1)).ToList();
            var oddSender0 = oddSenders[0];
            Assert.True(object.ReferenceEquals(oddSender0, oddSenders[1]));
            Assert.True(object.ReferenceEquals(oddSender0, oddSenders[2]));

            // check odd and even senders are different objects
            Assert.False(object.ReferenceEquals(evenSender0, oddSender0));
        }
    }
}
