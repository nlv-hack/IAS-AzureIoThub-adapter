using System.Collections.Generic;

namespace IAS.Adapter.AzureIOTHub.Model
{
    public class Message
    {
        public string FormatId { get; set; }

        public int ApiVersion { get; set; }

        public int CollectionId { get; set; }

        public IEnumerable<TagData> TagData { get; set; }
    }
}
