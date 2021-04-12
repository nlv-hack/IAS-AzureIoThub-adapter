using DataCore.Adapter;
using DataCore.Adapter.RealTimeData;
using IntelligentPlant.BackgroundTasks;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventHubs.Processor;
using Azure.Messaging.EventHubs.Consumer;

namespace IAS.Adapter.AzureIOTHub.Features
{
    public class SnapshotTagValuePushImp_EventProcessor : BaseFeature, ISnapshotTagValuePush
    {
        #region [ Properties ]

        private readonly SnapshotTagValuePush _push;

        #endregion

        #region [ Constructor(s) ]

        public SnapshotTagValuePushImp_EventProcessor(Adapter adapter)
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
        }

        #endregion

        #region [ Events ]

        public async Task processEventHandler(ProcessEventArgs args)
        {
            try
            {
                // TODO: general message type parser here

                await _push
                    .ValueReceived(new TagValueQueryResult("tag-id", "tag-name", TagValueBuilder.Create().Build()), _adapter.StopToken);
            }
            catch (Exception ex)
            {
                // Handle the exception from handler code
                _adapter.Logger.LogError(ex, "Encountered and error in process event handler");
                throw;
            }
        }

        public async Task processErrorHandler(ProcessErrorEventArgs args)
        {
            try
            {
                _adapter.Logger.LogError("Error in the EventProcessorClient");
                _adapter.Logger.LogError($"\tOperation: { args.Operation ?? "Unknown" }");
                _adapter.Logger.LogError($"\tPartition: { args.PartitionId ?? "None" }");
                _adapter.Logger.LogError($"\tException: { args.Exception }");

                // If processing stopped and this handler determined
                // the error to be non-fatal, restart processing.

                //if ((!_adapter.Options.ProcessingClient.IsRunning)
                //    && (!_push.BackgroundTaskService cancellationSource.IsCancellationRequested))
                //{
                //    // To be safe, request that processing stop before
                //    // requesting the start; this will ensure that any
                //    // processor state is fully reset.

                //    await _adapter.Options.ProcessingClient.StopProcessingAsync();
                //    await _adapter.Options.ProcessingClient.StartProcessingAsync(cancellationSource.Token);
                //}

                // Perform the application-specific processing for an error.
                // await DoSomethingWithTheError(eventArgs.Exception);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // Handle the exception from handler code
                _adapter.Logger.LogError(ex, "Encountered and error in error event handler");
                throw;
            }
        }

        public async Task initializeEventHandler(PartitionInitializingEventArgs args)
        {
            try
            {
                if (args.CancellationToken.IsCancellationRequested)
                {
                    await Task.CompletedTask;
                }

                _adapter.Logger.LogDebug($"Initialize partition: { args.PartitionId }");

                // If no checkpoint was found, start processing
                // events enqueued now or in the future.

                EventPosition startPositionWhenNoCheckpoint =
                    EventPosition.FromEnqueuedTime(DateTimeOffset.UtcNow);

                args.DefaultStartingPosition = startPositionWhenNoCheckpoint;
            }
            catch (Exception ex)
            {
                // Take action to handle the exception.
                // It is important that all exceptions are
                // handled and none are permitted to bubble up.
                _adapter.Logger.LogError(ex, "Encountered and error in Event processor init method.");
                throw;
            }

            await Task.CompletedTask;
        }

        #endregion

        #region [ Helpers ]

        #endregion
    }
}
