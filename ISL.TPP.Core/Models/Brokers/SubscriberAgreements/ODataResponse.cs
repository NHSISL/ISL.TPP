// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ISL.TPP.Core.Models.Brokers.SubscriberAgreements
{
    internal class ODataResponse<T>
    {
        [JsonPropertyName("value")]
        public List<T> Value { get; set; } = new List<T>();

        [JsonPropertyName("@odata.nextLink")]
        public string NextLink { get; set; }
    }
}
