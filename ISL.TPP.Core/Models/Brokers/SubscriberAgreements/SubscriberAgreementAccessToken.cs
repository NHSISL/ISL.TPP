// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Text.Json.Serialization;

namespace ISL.TPP.Core.Models.Brokers.SubscriberAgreements
{
    internal class SubscriberAgreementAccessToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
