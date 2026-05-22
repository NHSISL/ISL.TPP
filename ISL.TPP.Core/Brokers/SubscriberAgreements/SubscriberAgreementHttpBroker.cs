// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ISL.TPP.Core.Models.Brokers.SubscriberAgreements;
using ISL.TPP.Core.Models.Foundations.SubscriberAgreements;

namespace ISL.TPP.Core.Brokers.SubscriberAgreements
{
    internal class SubscriberAgreementHttpBroker : ISubscriberAgreementHttpBroker
    {
        private readonly SubscriberAgreementConfiguration configuration;
        private readonly HttpClient httpClient;
        private readonly HttpClient tokenHttpClient;
        private readonly SemaphoreSlim tokenGate = new SemaphoreSlim(1, 1);
        private string accessToken = string.Empty;
        private DateTimeOffset tokenExpiry = DateTimeOffset.MinValue;

        private static readonly JsonSerializerOptions jsonOptions =
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public SubscriberAgreementHttpBroker(SubscriberAgreementConfiguration configuration)
        {
            this.configuration = configuration;

            this.httpClient = new HttpClient
            {
                BaseAddress = new Uri(this.configuration.BaseUrl),
                Timeout = TimeSpan.FromSeconds(this.configuration.TimeoutInSeconds),

                MaxResponseContentBufferSize =
                    this.configuration.MaxResponseContentBufferSizeInMegaBytes * 1024 * 1024
            };

            this.tokenHttpClient = new HttpClient();
        }

        public async ValueTask<List<SubscriberAgreement>> GetActiveSubscriberAgreementsAsync()
        {
            await EnsureAccessTokenAsync(CancellationToken.None).ConfigureAwait(false);

            string filter = BuildODataFilter();
            string? nextUrl = $"{this.configuration.ActiveAgreementsRelativeUrl}?$filter={filter}";
            var results = new List<SubscriberAgreement>();

            while (nextUrl is not null)
            {
                var response = await this.httpClient
                    .GetAsync(nextUrl)
                    .ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                string json = await response.Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                var page =
                    JsonSerializer.Deserialize<ODataResponse<SubscriberAgreement>>(json, jsonOptions)
                        ?? new ODataResponse<SubscriberAgreement>();

                results.AddRange(page.Value);
                nextUrl = page.NextLink;
            }

            return results;
        }

        private string BuildODataFilter()
        {
            string isActiveFilter = "IsActive eq true";

            if (this.configuration.SupplierIds is null || !this.configuration.SupplierIds.Any())
            {
                return isActiveFilter;
            }

            string supplierIdFilter = string.Join(
                " or ",
                this.configuration.SupplierIds.Select(
                    id => $"SupplierId eq {id}"));

            return $"({supplierIdFilter}) and {isActiveFilter}";
        }

        private async ValueTask EnsureAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(this.accessToken)
                && DateTimeOffset.UtcNow < this.tokenExpiry)
            {
                return;
            }

            await this.tokenGate.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (string.IsNullOrEmpty(this.accessToken)
                    || DateTimeOffset.UtcNow >= this.tokenExpiry)
                {
                    await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

                    this.httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", this.accessToken);
                }
            }
            finally
            {
                this.tokenGate.Release();
            }
        }

        private async ValueTask GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            string tokenUrl =
                $"https://login.microsoftonline.com/{this.configuration.TenantId}/oauth2/v2.0/token";

            var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = this.configuration.ClientId,
                ["client_secret"] = this.configuration.ClientSecret,
                ["scope"] = this.configuration.Scope,
                ["grant_type"] = "client_credentials"
            });

            var response = await this.tokenHttpClient
                .PostAsync(tokenUrl, formContent, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string json = await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            SubscriberAgreementAccessToken token =
                JsonSerializer.Deserialize<SubscriberAgreementAccessToken>(json, jsonOptions)
                    ?? throw new InvalidOperationException(
                        "Failed to deserialise access token response.");

            this.accessToken = token.AccessToken;
            this.tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn - 30);
        }
    }
}