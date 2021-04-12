using DataCore.Adapter;
using DataCore.Adapter.RealTimeData;
using IntelligentPlant.BackgroundTasks;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IAS.Adapter.AzureIOTHub.Features
{
    public class ReadSnapshotTagValuesImp : BaseFeature, IReadSnapshotTagValues
    {
        #region [ Properties ]

        #endregion

        #region [ Constructor(s) ]

        public ReadSnapshotTagValuesImp(Adapter adapter)
        {
            this._adapter = adapter;
        }

        #endregion

        #region [ Interface implementation ]

        /// <inheritdoc/>
        public IBackgroundTaskService BackgroundTaskService => _adapter.BackgroundTaskService;

        public Task<ChannelReader<TagValueQueryResult>> ReadSnapshotTagValues(IAdapterCallContext context,
            ReadSnapshotTagValuesRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var resultChannel = Channel.CreateUnbounded<TagValueQueryResult>();

                var now = DateTime.UtcNow;

                resultChannel.Writer.RunBackgroundOperation(async (ch, ct) =>
                {
                    //var data = await _wonderwareSQLAdapter.wwSQLConnection
                    //.ExecuteQueryAsync(q.Query,
                    //        q.Parameters,
                    //        async dbReader =>
                    //        {
                    //            // each row is a tag name with its value
                    //            var tagValue = new TagValueQueryResult(dbReader.GetInt32(0).ToString(),
                    //                dbReader.GetString(1),
                    //                TagValueBuilder
                    //                    .Create()
                    //                    .WithUtcSampleTime(DateTime.UtcNow)
                    //                    .WithValue(dbReader.GetDouble(2))
                    //                    .WithStatus(dbReader.GetInt32(3) == (int)Resources.OPCDataQualityCodes.Good
                    //                        ? TagValueStatus.Good
                    //                        : TagValueStatus.Uncertain)
                    //                    .Build()
                    //            );

                    //            await resultChannel.Writer
                    //                .WriteAsync(tagValue, cancellationToken) // trywrite ??
                    //                .ConfigureAwait(false);
                    //        },
                    //        cancellationToken)
                    //.ConfigureAwait(false);

                    foreach (var tag in request.Tags)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            break;
                        }
                        if (string.IsNullOrWhiteSpace(tag))
                        {
                            continue;
                        }

                        ch.TryWrite(new TagValueQueryResult(
                            tag,
                            tag,
                            TagValueBuilder
                                .Create()
                                .WithUtcSampleTime(now)
                                .WithValue(SinusoidWave(now, TimeSpan.Zero, 1, 1))
                                .Build()
                        ));
                    }

                }, true, _adapter.BackgroundTaskService, cancellationToken);

                return Task.FromResult(resultChannel.Reader);
            }
            catch (Exception ex)
            {
                _adapter.Logger.LogError("Failed performing spanshot query.", ex);

                throw;
            }
        }

        #endregion

        #region [ Helpers ]

        private static DateTime CalculateSampleTime(DateTime queryTime)
        {
            var offset = queryTime.Ticks % TimeSpan.TicksPerSecond;
            return queryTime.Subtract(TimeSpan.FromTicks(offset));
        }

        private static double SinusoidWave(DateTime sampleTime, TimeSpan offset, double period, double amplitude)
        {
            var time = (sampleTime - DateTime.UtcNow.Date.Add(offset)).TotalSeconds;
            return amplitude * (Math.Sin(2 * Math.PI * (1 / period) * time));
        }

        #endregion
    }
}
