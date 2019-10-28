using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureSolutions.Messaging.AzureStorageQueue;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using NSubstitute;
using Xunit;

// disable "call not awaited" compiler warnings when using NSubstitute .Received() for async methods
#pragma warning disable 4014

namespace AzureSolutions.Messaging.UnitTests
{
    public class CloudQueueFactoryTest
    {
        private readonly ICloudQueueFactory _cloudQueueFactory; // class under test
        private readonly ICloudQueueClientFactory _cloudQueueClientFactoryMock; // a mock
        private readonly CloudQueueClient _cloudQueueClientMock;

        public CloudQueueFactoryTest()
        {
            // Note: the tests here heavily depend on "real" Azure.Storage entities - we do mock the virtual methods,
            // but all constructors are "real" code.
            // Therefore, a change in next version of Azure.Storage SDK may break our tests (in that case maybe wrap any Azure.Storage entity
            // in a proxy or a wrapper class with the public interface and test that)

            _cloudQueueClientFactoryMock = Substitute.For<ICloudQueueClientFactory>();

            // Mock entire CloudQueueClient - the factory code for it havily depends on MS Azure.Storage entities
            // whose logic we do not want to test
            // What we do want to test is how many times .GetCloudQueueClient() or .GetQueueReference() has been called
            {
                _cloudQueueClientMock = Substitute.ForPartsOf<CloudQueueClient>(
                        new StorageUri(new Uri("https://test.com")),
                        new Microsoft.Azure.Storage.Auth.StorageCredentials(),
                        null);

                _cloudQueueClientMock.When(c => c.GetQueueReference(Arg.Any<string>())).DoNotCallBase();
                _cloudQueueClientMock.GetQueueReference(Arg.Any<string>())
                    .Returns(x =>
                    {
                        CloudQueue queue = Substitute.ForPartsOf<CloudQueue>(new Uri($"http://test.com/{x.Arg<string>()}"));
                        queue.When(q => q.CreateIfNotExistsAsync()).DoNotCallBase();
                        queue.CreateIfNotExistsAsync().Returns(Task.FromResult(true));
                        return queue;
                    });

                _cloudQueueClientFactoryMock.GetCloudQueueClient().Returns(_ => _cloudQueueClientMock);
            }

            _cloudQueueFactory = new CloudQueueFactory(_cloudQueueClientFactoryMock);
        }

        [Fact]
        public async Task Creates_CouldQueue_ForQueueName()
        {
            // execute
            var q = await _cloudQueueFactory.GetCloudQueue("test-queue-1");

            // check
            Assert.NotNull(q);
            Assert.Equal("test-queue-1", q.Name);
        }

        [Fact]
        public async Task Creates_TwoCouldQueues_ForDifferentNames_OnSerialCalls()
        {
            // execute
            var q = await _cloudQueueFactory.GetCloudQueue("test-queue-1");
            var q2 = await _cloudQueueFactory.GetCloudQueue("test-queue-2");

            // check
            Assert.NotNull(q);
            Assert.NotNull(q2);

            Assert.Equal("test-queue-1", q.Name);
            Assert.Equal("test-queue-2", q2.Name);

            Assert.False(object.ReferenceEquals(q, q2)); // not the same things
        }

        [Fact]
        public async Task Reuses_CloudQueue_ForSameQueueName_OnSerialCalls()
        {
            // execute
            var q = await _cloudQueueFactory.GetCloudQueue("test-queue-1");
            var q2 = await _cloudQueueFactory.GetCloudQueue("test-queue-1");

            // check
            Assert.NotNull(q);
            Assert.NotNull(q2);

            Assert.Equal("test-queue-1", q.Name);
            Assert.Equal("test-queue-1", q2.Name);

            // q & q2 should be the same objects - q2 is pulled from the cache
            Assert.True(object.ReferenceEquals(q, q2));
        }

        [Fact]
        public async Task Reuses_CloudQueue_ForSameQueueName_OnConcurentCalls()
        {
            // prepare
            const string queueName = "test-queue-1";
            ConcurrentBag<CloudQueue> queueBag = new ConcurrentBag<CloudQueue>();

            int count = 10;

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < count; i++)
                tasks.Add(Task.Run(async () => queueBag.Add(await _cloudQueueFactory.GetCloudQueue(queueName))));

            // execute
            await Task.WhenAll(tasks);

            // check

            // 10 queues created 
            Assert.Equal(10, queueBag.Count);

            // all queues have the same name
            foreach (var q in queueBag)
                Assert.Equal(queueName, q.Name);

            // all queues are the same object 
            var firstQueue = queueBag.First();
            foreach (var q in queueBag)
                Assert.True(object.ReferenceEquals(firstQueue, q));

            // .CreateIfNotExistsAsync() is invoked exactly once 
            _cloudQueueClientMock.Received(1).GetQueueReference(Arg.Any<string>());
            firstQueue.Received(1).CreateIfNotExistsAsync(); 
        }

        [Fact]
        public async Task Reuses_CloudQueueClient_ForMultipleCloudQueues_OnConcurrentCalls()
        {
            // prepare
            const string queueNamePrefix = "test-queue-";
            ConcurrentBag<CloudQueue> queueBag = new ConcurrentBag<CloudQueue>();

            int count = 10;

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < count; i++)
            {
                var y = i;
                tasks.Add(Task.Run(async () => queueBag.Add(await _cloudQueueFactory.GetCloudQueue($"{queueNamePrefix}{y}"))));
            }
            
            // execute
            await Task.WhenAll(tasks);

            // check

            // 10 queues created 
            Assert.Equal(10, queueBag.Count());

            // not two queues have the same name
            Assert.Equal(10, queueBag.GroupBy(x => x.Name).Count());

            // all queues are different object 
            var firstQueue = queueBag.First();
            foreach (var q in queueBag.Skip(1))
                Assert.False(object.ReferenceEquals(firstQueue, q));

            // ONLY ONE underlying CloudQueueClient has been created!
            _cloudQueueClientFactoryMock.Received(1).GetCloudQueueClient();

            // CouldQueueClient has been reused to create each queue
            _cloudQueueClientMock.Received(10).GetQueueReference(Arg.Any<string>());
        }
    }
}
