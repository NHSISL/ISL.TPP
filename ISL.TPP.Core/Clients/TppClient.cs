// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Net.Http;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Storage.Blobs;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Files;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Brokers.Storages.Blobs;
using ISL.TPP.Core.Clients.Imports;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Models.Configurations.Retries;
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

        internal TppClient(
         TppConfiguration tppConfiguration,
         IServiceCollection serviceCollection,
         ILoggerFactory? loggerFactory = null)
        {
            IServiceProvider serviceProvider =
                RegisterServices(tppConfiguration, loggerFactory, serviceCollection, acceptanceTests: true);

            InitialiseClients(serviceProvider);
        }

        public IImportClient Imports { get; private set; }

        private void InitialiseClients(IServiceProvider serviceProvider)
        {
            Imports = serviceProvider.GetRequiredService<IImportClient>();
        }

        private static IServiceProvider RegisterServices(
            TppConfiguration tppConfiguration,
            ILoggerFactory? loggerFactory,
            IServiceCollection? serviceCollection = null,
            bool acceptanceTests = false)
        {

            if (loggerFactory is null)
            {
                loggerFactory = LoggerFactory.Create(builder => { });
            }

            ILogger<LoggingBroker> loggingBroker = loggerFactory.CreateLogger<LoggingBroker>();

            ValidateTppConfiguration(tppConfiguration);

            if (serviceCollection is null)
            {
                serviceCollection = new ServiceCollection();
            }

            if (!acceptanceTests)
            {
                BlobServiceClient blobServiceClient =
                    SetupBlobServiceClient(tppConfiguration);

                serviceCollection
                    .AddSingleton(blobServiceClient)
                    .AddTransient<IBlobStorageBroker, BlobStorageBroker>()
                    .AddTransient<IFileBroker, FileBroker>();
            }

            serviceCollection
                .AddSingleton(tppConfiguration)

                .AddSingleton<IRetryConfig>(_ =>
                {
                    var retryConfig = tppConfiguration.RetryConfig;

                    return new RetryConfig(
                        maxRetryAttempts: retryConfig.MaxRetryAttempts,
                        pauseBetweenFailures: retryConfig.PauseBetweenFailures
                    );
                })

                .AddTransient(_ => tppConfiguration.BlobStorageSettings)
                .AddTransient<IDateTimeBroker, DateTimeBroker>()
                .AddTransient(_ => loggingBroker)
                .AddTransient<ILoggingBroker, LoggingBroker>()
                .AddTransient<IFileService, FileService>()
                .AddTransient<IDocumentService, DocumentService>()
                .AddTransient<IImportClient, ImportClient>()
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
