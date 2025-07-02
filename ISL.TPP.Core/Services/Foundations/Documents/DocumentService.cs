// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Brokers.Storages.Blobs;

namespace ISL.TPP.Core.Services.Foundations.Documents
{
    internal partial class DocumentService : IDocumentService
    {
        private readonly IBlobStorageBroker blobStorageBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;

        public DocumentService(
            IBlobStorageBroker blobStorageBroker,
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker)
        {
            this.blobStorageBroker = blobStorageBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask AddDocumentAsync(Stream input, string fileName, string container) =>
            TryCatch(async () =>
            {
                ValidateDocumentOnAdd(input, fileName, container);
                await this.blobStorageBroker.InsertFileAsync(input, fileName, container);
            });

        public ValueTask RetrieveDocumentByFileNameAsync(Stream output, string fileName, string container) =>
             TryCatch(async () =>
             {
                 ValidateArgumentsOnRetrieve(output, fileName, container);
                 await this.blobStorageBroker.SelectByFileNameAsync(output, fileName, container);
                 ValidateStorageDocument(output, fileName);
             });

        public ValueTask RemoveDocumentByFileNameAsync(string fileName, string container) =>
           TryCatch(async () =>
           {
               ValidateDeleteArguments(fileName, container);
               await this.blobStorageBroker.DeleteFileAsync(fileName, container);
           });

        public ValueTask<string> GetDownloadLinkAsync(string fileName, string container) =>
           TryCatch(async () =>
           {
               ValidateGetDownloadLinkArguments(fileName, container);
               var currentDateTimeOffset = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
               var expireOn = currentDateTimeOffset.AddMinutes(5);

               return await this.blobStorageBroker.GetDownloadLinkAsync(fileName, container, expireOn);
           });
    }
}
