// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Net.Http;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Storage.Blobs;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Brokers.Storages.Blobs;
using ISL.TPP.Core.Clients.Imports;
using ISL.TPP.Core.Models.Orchestrations.TPP;
using ISL.TPP.Core.Services.Foundations.Documents;
using ISL.TPP.Core.Services.Foundations.Files;
using ISL.TPP.Core.Services.Orchestrations.Tpp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ISL.TPP.Core.Clients
{
    public partial class TppClient : ITppClient
    {
        public TppClient(
            TppConfiguration tppConfiguration,
            ILoggerFactory? loggerFactory = null)
        {
            IServiceProvider serviceProvider = RegisterServices(tppConfiguration, loggerFactory);
            InitialiseClients(serviceProvider);
        }

        public IImportClient Imports { get; private set; }

        private void InitialiseClients(IServiceProvider serviceProvider)
        {
            Imports = serviceProvider.GetRequiredService<IImportClient>();
        }

        private static IServiceProvider RegisterServices(
            TppConfiguration tppConfiguration,
            ILoggerFactory? loggerFactory)
        {

            if (loggerFactory is null)
            {
                loggerFactory = LoggerFactory.Create(builder => { });
            }

            ILogger<LoggingBroker> loggingBroker = loggerFactory.CreateLogger<LoggingBroker>();

            ValidateTppConfiguration(tppConfiguration);

            BlobServiceClient blobServiceClient =
                SetupBlobServiceClient(tppConfiguration);

            var serviceCollection = new ServiceCollection()
                .AddSingleton(blobServiceClient)
                .AddSingleton(tppConfiguration)
                .AddTransient<IDateTimeBroker, DateTimeBroker>()
                .AddTransient(_ => loggingBroker)
                .AddTransient<ILoggingBroker, LoggingBroker>()
                .AddTransient<IBlobStorageBroker, BlobStorageBroker>()
                .AddTransient<IDocumentService, DocumentService>()
                .AddTransient<IFileService, FileService>()
                .AddTransient<ITppOrchestrationService, TppOrchestrationService>();

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            return serviceProvider;
        }

        private static BlobServiceClient SetupBlobServiceClient(
            TppConfiguration tppConfiguration)
        {
            var blobServiceClientOptions = new BlobClientOptions()
            {
                Transport = new HttpClientTransport(new HttpClient { Timeout = new TimeSpan(1, 0, 0) }),
                Retry = { NetworkTimeout = new TimeSpan(1, 0, 0) },
                EnableTenantDiscovery = true
            };

            var blobServiceClient = new BlobServiceClient(
                serviceUri: new Uri(tppConfiguration.BlobStorageSettings.AzureBlobServiceUri),
                credential: new DefaultAzureCredential(
                    new DefaultAzureCredentialOptions
                    {
                        VisualStudioTenantId = tppConfiguration.BlobStorageSettings.AzureTenantId,
                    }),
                options: blobServiceClientOptions);
            return blobServiceClient;
        }
    }
}
