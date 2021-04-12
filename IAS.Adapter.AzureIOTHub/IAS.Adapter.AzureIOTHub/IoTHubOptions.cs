using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using DataCore.Adapter;

namespace IAS.Adapter.AzureIOTHub
{
    /// <summary>
    /// Azure IoT Hub adapter options definition.
    /// </summary>
    public class IoTHubOptions : AdapterOptions
    {
        #region [ Properties ]

        internal const string IotHubSharedAccessKeyName = "service";

        /// <summary>
        /// Indicates whether to use the adapter with event processor implementation.
        /// More information on the "EventProcessorClient" and its benefits can be found here:
        /// https://github.com/Azure/azure-sdk-for-net/blob/master/sdk/eventhub/Azure.Messaging.EventHubs.Processor/README.md
        /// </summary>
        public bool UseEventProcessor { get; set; } = false;

        #region [ Additional setting if event processor implementation is used ]

        /// <summary>
        /// Name of the event hub consumer group
        /// </summary>
        public string ConsumerGroup { get; set; }

        /// <summary>
        /// Storage connection string.
        /// </summary>
        public string StorageConnectionString { get; set; }

        /// <summary>
        /// Name of the blob container.
        /// </summary>
        public string BlobContainerName { get; set; }

        #endregion

        /// <summary>
        /// "The event hub-compatible endpoint from your IoT Hub instance. Use `az iot hub show --query properties.eventHubEndpoints.events.endpoint --name {your IoT Hub name}` to fetch via the Azure CLI.")
        /// </summary>
        public string EventHubCompatibleEndpoint { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string EventHubName { get; set; }

        /// <summary>
        /// A primary or shared access key from your IoT Hub instance, with the 'service' permission. Use `az iot hub policy show --name service --query primaryKey --hub-name {your IoT Hub name}` to fetch via the Azure CLI."
        /// </summary>
        public string SharedAccessKey { get; set; }

        /// <summary>
        /// The connection string to the event hub-compatible endpoint. Use the Azure portal to get this parameter. If this value is provided, all the others are not necessary.")
        /// </summary>
        public string EventHubConnectionString { get; set; }

        internal string GetEventHubConnectionString()
        {
            return EventHubConnectionString ?? $"Endpoint={EventHubCompatibleEndpoint};SharedAccessKeyName={IotHubSharedAccessKeyName};SharedAccessKey={SharedAccessKey}";
        }

        #endregion

        #region [ Clients ]

        /// <summary>
        /// Consumer client.
        /// </summary>
        public EventHubConsumerClient ConsumerClient { get; set; }

        /// <summary>
        /// Event processing client.
        /// </summary>
        public EventProcessorClient ProcessingClient { get; set; }

        #endregion
    }
}
