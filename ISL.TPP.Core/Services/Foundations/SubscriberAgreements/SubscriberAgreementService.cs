// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;

namespace ISL.TPP.Core.Services.Foundations.Documents
{
    internal partial class SubscriberAgreementService : ISubscriberAgreementService
    {
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;

        public SubscriberAgreementService(
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker)
        {
            this.dateTimeBroker = dateTimeBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask AddDocumentAsync(Stream input, string fileName, BlobStorageSettings blobStorageSettings) =>
        TryCatch(async () =>
        {
            ValidateDocumentOnAdd(input, fileName, blobStorageSettings);
            await this.blobStorageBroker.InsertFileAsync(input, fileName, blobStorageSettings);
        });

        public ValueTask RetrieveDocumentByFileNameAsync(
            Stream output,
            string fileName,
            BlobStorageSettings blobStorageSettings) =>
        TryCatch(async () =>
        {
            ValidateArgumentsOnRetrieve(output, fileName, blobStorageSettings);
            await this.blobStorageBroker.SelectByFileNameAsync(output, fileName, blobStorageSettings);
            ValidateStorageDocument(output, fileName);
        });

        public ValueTask RemoveDocumentByFileNameAsync(string fileName, BlobStorageSettings blobStorageSettings) =>
        TryCatch(async () =>
        {
            ValidateDeleteArguments(fileName, blobStorageSettings);
            await this.blobStorageBroker.DeleteFileAsync(fileName, blobStorageSettings);
        });

        public ValueTask<string> GetDownloadLinkAsync(string fileName, BlobStorageSettings blobStorageSettings) =>
        TryCatch(async () =>
        {
            ValidateGetDownloadLinkArguments(fileName, blobStorageSettings);
            var currentDateTimeOffset = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
            var expireOn = currentDateTimeOffset.AddMinutes(5);

            return await this.blobStorageBroker.GetDownloadLinkAsync(fileName, blobStorageSettings, expireOn);
        });
    }
}
