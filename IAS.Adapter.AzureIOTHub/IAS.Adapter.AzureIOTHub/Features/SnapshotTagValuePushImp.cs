using DataCore.Adapter;
using DataCore.Adapter.RealTimeData;
using IntelligentPlant.BackgroundTasks;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text;
using Azure.Messaging.EventHubs.Consumer;
using System.Collections.Generic;

namespace IAS.Adapter.AzureIOTHub.Features
{
    public class SnapshotTagValuePushImp : BaseFeature, ISnapshotTagValuePush
    {
        #region [ Properties ]

        private readonly SnapshotTagValuePush _push;

        #endregion

        #region [ Constructor(s) ]

        public SnapshotTagValuePushImp(Adapter adapter)
        {
            this._adapter = adapter;

            _push = new SnapshotTagValuePush(new SnapshotTagValuePushOptions(), _adapter.BackgroundTaskService, _adapter.Logger);

        }

        #endregion

        #region [ Interface implementation ]

        /// <inheritdoc/>
        public IBackgroundTaskService BackgroundTaskService => _push.BackgroundTaskService;

        public Task<ChannelReader<TagValueQueryResult>> Subscribe(IAdapterCallContext context, 
            CreateSnapshotTagValueSubscriptionRequest request, 
            ChannelReader<TagValueSubscriptionUpdate> channel, 
            CancellationToken cancellationToken)
        {
            return _push.Subscribe(context, request, channel, cancellationToken);

            //// Begin reading events for all partitions, starting with the first event in each partition and waiting indefinitely for
            //// events to become available. Reading can be canceled by breaking out of the loop when an event is processed or by
            //// signaling the cancellation token.
            ////
            //// The "ReadEventsAsync" method on the consumer is a good starting point for consuming events for prototypes
            //// and samples. For real-world production scenarios, it is strongly recommended that you consider using the
            //// "EventProcessorClient" from the "Azure.Messaging.EventHubs.Processor" package.
            ////
            //// More information on the "EventProcessorClient" and its benefits can be found here:
            ////   https://github.com/Azure/azure-sdk-for-net/blob/master/sdk/eventhub/Azure.Messaging.EventHubs.Processor/README.md
            //await foreach (PartitionEvent partitionEvent in _adapter.Options.ConsumerClient.ReadEventsAsync(cancellationToken))
            //{
            //    Console.WriteLine($"\nMessage received on partition {partitionEvent.Partition.PartitionId}:");

            //    string data = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
            //    Console.WriteLine($"\tMessage body: {data}");

            //    Console.WriteLine("\tApplication properties (set by device):");
            //    foreach (KeyValuePair<string, object> prop in partitionEvent.Data.Properties)
            //    {
            //        Console.WriteLine($"\t\t{prop.Key}: {prop.Value}");
            //    }

            //    Console.WriteLine("\tSystem properties (set by IoT Hub):");
            //    foreach (KeyValuePair<string, object> prop in partitionEvent.Data.SystemProperties)
            //    {
            //        Console.WriteLine($"\t\t{prop.Key}: {prop.Value}");
            //    }
            //}
        }

        #endregion

        #region [ Helpers ]

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            //_adapter.Options.ConsumerClient
            await foreach (PartitionEvent partitionEvent in _adapter.Options.ConsumerClient.ReadEventsAsync(cancellationToken))
            {
                //await _push.ValueReceived(new TagValueQueryResult("", "", new TagValueExtended()))

                await _push
                    .ValueReceived(new TagValueQueryResult("tag-id", "tag-name", TagValueBuilder.Create().Build()), cancellationToken);

                //Console.WriteLine($"\nMessage received on partition {partitionEvent.Partition.PartitionId}:");

                //string data = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
                //Console.WriteLine($"\tMessage body: {data}");

                //Console.WriteLine("\tApplication properties (set by device):");
                //foreach (KeyValuePair<string, object> prop in partitionEvent.Data.Properties)
                //{
                //    Console.WriteLine($"\t\t{prop.Key}: {prop.Value}");
                //}

                //Console.WriteLine("\tSystem properties (set by IoT Hub):");
                //foreach (KeyValuePair<string, object> prop in partitionEvent.Data.SystemProperties)
                //{
                //    Console.WriteLine($"\t\t{prop.Key}: {prop.Value}");
                //}
            }
        }

        #endregion
    }
}
