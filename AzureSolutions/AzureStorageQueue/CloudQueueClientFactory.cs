using AzureSolutions.Util.FluentValidation;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Queue;

namespace AzureSolutions.Messaging.AzureStorageQueue
{
    public interface ICloudQueueClientFactory
    {
        CloudQueueClient GetCloudQueueClient();
    }

    public class CloudQueueClientFactory : ICloudQueueClientFactory
    {
        private readonly MessagingConfig _config;

        public CloudQueueClientFactory(MessagingConfig messagingConfig)
        {
            _config = messagingConfig;
        }

        public CloudQueueClient GetCloudQueueClient()
        {
            Requires.That(_config, nameof(_config)).IsNotNull();
            Requires.That(_config, x => x.StorageConnectionString, nameof(_config.StorageConnectionString)).IsNotNullOrEmpty();
            Requires.That(_config, x => x.StorageAccountName, nameof(_config.StorageAccountName)).IsNotNullOrEmpty();
            Requires.That(_config, x => x.StorageAccountKey, nameof(_config.StorageAccountKey)).IsNotNullOrEmpty();

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_config.StorageConnectionString);

            return new CloudQueueClient(storageAccount.QueueStorageUri.PrimaryUri,
                new StorageCredentials(_config.StorageAccountName, _config.StorageAccountKey));
        }
    }
}
