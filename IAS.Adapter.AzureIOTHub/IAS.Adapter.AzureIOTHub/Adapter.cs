using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Storage.Blobs;
using DataCore.Adapter;
using DataCore.Adapter.Diagnostics;
using IntelligentPlant.BackgroundTasks;
using Microsoft.Extensions.Logging;

namespace IAS.Adapter.AzureIOTHub
{
    public class Adapter : AdapterBase<IoTHubOptions>
    {
        #region [ Properties ]

        /// <summary>
        /// Gets the adapter options.
        /// </summary>
        internal new IoTHubOptions Options
        {
            get { return base.Options; }
        }

        /// <summary>
        /// Logging.
        /// </summary>
        internal new ILogger Logger => base.Logger;

        private readonly Features.SnapshotTagValuePushImp snapshotTagValuePushImp;

        private readonly Features.SnapshotTagValuePushImp_EventProcessor snapshotTagValuePushImp_EventProcessor;

        #endregion

        #region [ Constructor(s) ]

        public Adapter(
            // Unique identifier for the adapter instance
            string id,
            // adapter options
            IoTHubOptions options,           
            // Used to allow the adapter to run background operations.
            IBackgroundTaskService backgroundTaskService = null,
            // Logging
            ILogger<Adapter> logger = null
        ) : base(id, options, backgroundTaskService, logger) 
        {
            snapshotTagValuePushImp = new Features.SnapshotTagValuePushImp(this);
            snapshotTagValuePushImp_EventProcessor = new Features.SnapshotTagValuePushImp_EventProcessor(this);

            // data read
            AddFeatures(new Features.ReadSnapshotTagValuesImp(this));
            AddFeatures(snapshotTagValuePushImp);
        }

        #endregion

        #region [ Implementation ]

        protected override async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitAsync().ConfigureAwait(false);

            AddProperty("Successfully connected to ", Options.EventHubName);
            AddProperty("Startup Time", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));

            BackgroundTaskService.QueueBackgroundWorkItem(snapshotTagValuePushImp.RunAsync);

            if (Options.UseEventProcessor)
            {
                // assign handlers
                Options.ProcessingClient.ProcessEventAsync += snapshotTagValuePushImp_EventProcessor.processEventHandler;
                Options.ProcessingClient.ProcessErrorAsync += snapshotTagValuePushImp_EventProcessor.processErrorHandler;
                Options.ProcessingClient.PartitionInitializingAsync += snapshotTagValuePushImp_EventProcessor.initializeEventHandler;

                await Options.ProcessingClient
                    .StartProcessingAsync(cancellationToken);
            }
        }

        protected override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (Options.UseEventProcessor)
            {
                try
                {
                    await Options.ProcessingClient
                        .StartProcessingAsync(cancellationToken);
                }
                finally
                {
                    // To prevent leaks, the handlers should be removed when processing is complete.
                    Options.ProcessingClient.ProcessEventAsync -= snapshotTagValuePushImp_EventProcessor.processEventHandler;
                    Options.ProcessingClient.ProcessErrorAsync -= snapshotTagValuePushImp_EventProcessor.processErrorHandler;

                    Options.ProcessingClient.PartitionInitializingAsync -= snapshotTagValuePushImp_EventProcessor.initializeEventHandler;
                }
            }
        }

        protected override Task<IEnumerable<HealthCheckResult>> CheckHealthAsync(
            IAdapterCallContext context,
            CancellationToken cancellationToken
        )
        {
            return Task.FromResult<IEnumerable<HealthCheckResult>>(new[] {
                HealthCheckResult.Healthy("All systems normal!")
            });
        }

        protected override void Dispose(bool disposing)
        {
            // ??
            //await Options.ConsumerClient.DisposeAsync();

            base.Dispose(disposing);
        }

        #endregion

        #region [ Helpers ]

        /// <summary>
        /// Initialise connection to azure iot hub and create a consumer client.
        /// </summary>
        /// <returns></returns>
        private async Task InitAsync()
        {
            // general init here

            if (Options.UseEventProcessor)
                InitEventProcessorClient();
            else
                await InitConsumerClient();
        }

        public async Task InitConsumerClient()
        {
            try
            {
                // Either the connection string must be supplied, or the set of endpoint, name, and shared access key must be.
                if (string.IsNullOrWhiteSpace(Options.EventHubConnectionString)
                    && (string.IsNullOrWhiteSpace(Options.EventHubCompatibleEndpoint)
                        || string.IsNullOrWhiteSpace(Options.EventHubName)
                        || string.IsNullOrWhiteSpace(Options.SharedAccessKey)))
                {
                    // TODO: some more appropriate error messages
                    throw new Exception("Connection options not provided");
                }

                // create a test consumer to see if it works
                string connectionString = Options.GetEventHubConnectionString();

                // Create the consumer using the default consumer group using a direct connection to the service.
                // Information on using the client with a proxy can be found in the README for this quick start, here:
                // https://github.com/Azure-Samples/azure-iot-samples-csharp/tree/master/iot-hub/Quickstarts/ReadD2cMessages/README.md#websocket-and-proxy-support
                var consumer = new EventHubConsumerClient(
                    EventHubConsumerClient.DefaultConsumerGroupName,
                    connectionString,
                    Options.EventHubName);

                Options.ConsumerClient = consumer;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void InitEventProcessorClient()
        {
            try
            {
                // settings null checks

                BlobContainerClient storageClient = new BlobContainerClient(Options.StorageConnectionString, Options.BlobContainerName);

                EventProcessorClient processor = new EventProcessorClient
                (
                    storageClient,
                    Options.ConsumerGroup,
                    Options.GetEventHubConnectionString(),
                    Options.EventHubName
                );

                Options.ProcessingClient = processor;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion
    }
}
