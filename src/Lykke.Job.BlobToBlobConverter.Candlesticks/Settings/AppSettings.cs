using System;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.BlobToBlobConverter.Candlesticks.Settings
{
    public class AppSettings
    {
        public BlobToBlobConverterCandlesticksSettings BlobToBlobConverterCandlesticksJob { get; set; }

        public SlackNotificationsSettings SlackNotifications { get; set; }

        public MonitoringServiceClientSettings MonitoringServiceClient { get; set; }
    }

    public class SlackNotificationsSettings
    {
        public AzureQueuePublicationSettings AzureQueue { get; set; }
    }

    public class AzureQueuePublicationSettings
    {
        public string ConnectionString { get; set; }

        public string QueueName { get; set; }
    }

    public class MonitoringServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string MonitoringServiceUrl { get; set; }
    }

    public class BlobToBlobConverterCandlesticksSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }

        [AzureBlobCheck]
        public string InputBlobConnString { get; set; }

        public string InputContainer { get; set; }

        [AzureBlobCheck]
        public string OutputBlobConnString { get; set; }

        public TimeSpan BlobScanPeriod { get; set; }
    }
}
