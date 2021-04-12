using System;
using System.Collections.Generic;

namespace IAS.Adapter.AzureIOTHub.Model
{
    public class TagData
    {
        public DateTime Time { get; set; }

        public Dictionary<string, double> Values { get; set; }
    }
}
