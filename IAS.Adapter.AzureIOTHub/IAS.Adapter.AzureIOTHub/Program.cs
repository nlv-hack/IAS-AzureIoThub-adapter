using DataCore.Adapter;
using DataCore.Adapter.RealTimeData;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IAS.Adapter.AzureIOTHub
{
    class Program
    {
        #region [ Properties ]

        private const string AdapterId = "example";

        private const string AdapterDisplayName = "Example Adapter";

        private const string AdapterDescription = "Example adapter, built using the tutorial on GitHub";

        private static IoTHubOptions _options = new IoTHubOptions()
        {

        };

        #endregion

        public static async Task Main(params string[] args)
        {
            using (var userCancelled = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (sender, e) => userCancelled.Cancel();
                try
                {
                    Console.WriteLine("Press CTRL+C to quit");
                    await Run(new DefaultAdapterCallContext(), userCancelled.Token);
                }
                catch (OperationCanceledException) { }
            }
        }

        private static async Task Run(IAdapterCallContext context, CancellationToken cancellationToken)
        {
            //await using (IAdapter adapter = new Adapter(AdapterId, _options, AdapterDisplayName, AdapterDescription))
            //{
            //    await adapter.StartAsync(cancellationToken);

            //    Console.WriteLine();
            //    Console.WriteLine($"[{adapter.Descriptor.Id}]");
            //    Console.WriteLine($"  Name: {adapter.Descriptor.Name}");
            //    Console.WriteLine($"  Description: {adapter.Descriptor.Description}");
            //    Console.WriteLine("  Properties:");
            //    foreach (var prop in adapter.Properties)
            //    {
            //        Console.WriteLine($"    - {prop.Name} = {prop.Value}");
            //    }
            //    Console.WriteLine("  Features:");
            //    foreach (var feature in adapter.Features.Keys)
            //    {
            //        Console.WriteLine($"    - {feature}");
            //    }

            //    var readSnapshotFeature = adapter.GetFeature<IReadSnapshotTagValues>();
            //    var snapshotValues = await readSnapshotFeature.ReadSnapshotTagValues(
            //        context,
            //        new ReadSnapshotTagValuesRequest()
            //        {
            //            Tags = new[] {
            //                "Example 1",
            //                "Example 2"
            //            }
            //        },
            //        cancellationToken
            //    );

            //    Console.WriteLine();
            //    Console.WriteLine("  Snapshot Values:");
            //    await foreach (var value in snapshotValues.ReadAllAsync(cancellationToken))
            //    {
            //        Console.WriteLine($"    [{value.TagName}] - {value.Value}");
            //    }

            //}
        }
    }
}
