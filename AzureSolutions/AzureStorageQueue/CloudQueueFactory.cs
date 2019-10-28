using System.Collections.Concurrent;
using System.Threading.Tasks;
using AzureSolutions.Util;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;

namespace AzureSolutions.Messaging.AzureStorageQueue
{
    public interface ICloudQueueFactory
    {
        Task<CloudQueue> GetCloudQueue(string queueName);
    }

    /// <summary>
    /// Implemented as an instantiable but in essence must be singleton because of caching. We will rely on DI container to inject this
    /// as singleton.
    /// </summary>
    public class CloudQueueFactory : ICloudQueueFactory
    {
        private readonly ILogger _logger;

        private readonly LazyInitializer<CloudQueueClient> _cloudQueueClientLazy;

        private readonly ConcurrentDictionary<string, LazyInitializer<Task<CloudQueue>>> _cloudQueueCache 
            = new ConcurrentDictionary<string, LazyInitializer<Task<CloudQueue>>>();
        
        public CloudQueueFactory(ICloudQueueClientFactory cloudQueueClientFactory)
        {
            _cloudQueueClientLazy = new LazyInitializer<CloudQueueClient>(() =>
            {
                System.Diagnostics.Trace.WriteLine("LazyInitializer<CloudQueueClient> delegate executed");
                return cloudQueueClientFactory.GetCloudQueueClient();
            });
        }
        
        public async Task<CloudQueue> GetCloudQueue(string queueName)
        {
            // When the dictionary key does not exist, multiple concurent threads 
            // may execute MakeCloudQueueLazy factory method
            // The Lazy instance of the thread that executes first will be taken, others dismissed 
            // We are ok with this, because the Lazy itself is not a heavy object (a simple wrapper around the delegate)
            var queueWrapperLazy = _cloudQueueCache.GetOrAdd(queueName, MakeCloudQueueLazy);
            // This is where the synchronization happens 
            // Only one thread may execute the Lazy delegate when .Value is called 
            // Other threads will get that same value 
            return await queueWrapperLazy.Value;
        }

        private LazyInitializer<Task<CloudQueue>> MakeCloudQueueLazy(string queueName)
        {
            // We only create a LazyInitializer<> wrapper here and provide the delegate for the value 
            // This delegate will be invoked in thread-safe manner when the .Value property is accessed
            return new LazyInitializer<Task<CloudQueue>>(async () =>
            {
                System.Diagnostics.Trace.WriteLine($"LazyInitializer<Task<CloudQueue>> delegate executed for queueName='{queueName}'");
                var queueReference = _cloudQueueClientLazy.Value.GetQueueReference(queueName);
                await queueReference.CreateIfNotExistsAsync();
                return queueReference;
            });
        }
    }
}
