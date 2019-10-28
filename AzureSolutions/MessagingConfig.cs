namespace AzureSolutions
{
    public class MessagingConfig
    {
        public string ServiceBusConnectionString { get; set; }
        public string ServiceBusPartnerDispatchQueueName { get; set; }
        public string StorageConnectionString { get; set; }
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
        public double MessageConsideredStaleAfter { get; set; }
    }
}