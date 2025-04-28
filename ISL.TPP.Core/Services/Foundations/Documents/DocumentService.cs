// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Foundations.Documents;

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

        public ValueTask AddDocumentAsync(Document document, BlobStorageSettings blobStorageSettings) =>
            TryCatch(async () =>
            {
                ValidateDocumentOnAdd(document, blobStorageSettings);

                await this.blobStorageBroker.UploadFileAsync(
                   fileName: document.FileName,
                   data: document.DocumentData,
                   blobStorageSettings);
            });

        public ValueTask<Document> RetrieveDocumentByFileNameAsync(
            string fileName,
            BlobStorageSettings blobStorageSettings) =>
             TryCatch(async () =>
             {
                 ValidateDocumentOnRetrieve(fileName, blobStorageSettings);

                 byte[] retrievedDocument = await this.blobStorageBroker
                     .DownloadByFileNameAsync(fileName, blobStorageSettings);

                 ValidateStorageDocument(retrievedDocument, fileName);

                 using (SHA256 sha256 = SHA256.Create())
                 {
                     byte[] hashBytes = sha256.ComputeHash(retrievedDocument);
                     var sha256Hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                     var document = new Document
                     {
                         FileName = fileName,
                         DocumentData = retrievedDocument,
                         SHA256Hash = sha256Hash
                     };

                     return document;
                 }
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
               var expireOn = this.dateTimeBroker.GetCurrentDateTimeOffset().AddMinutes(5);

               return await this.blobStorageBroker.GetDownloadLinkAsync(fileName, blobStorageSettings, expireOn);
           });
    }
}
