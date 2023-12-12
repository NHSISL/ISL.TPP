// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Foundations.Documents;

namespace ISL.TPP.Core.Services.Foundations.Documents
{
    public partial class DocumentService : IDocumentService
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

        public ValueTask AddDocumentAsync(Document document, string container) =>
            TryCatch(async () =>
            {
                ValidateDocumentOnAdd(document, container);

                await this.blobStorageBroker.InsertFileAsync(
                   fileName: document.FileName,
                   stream: new MemoryStream(document.DocumentData),
                   container);
            });

        public ValueTask<Document> RetrieveDocumentByFileNameAsync(string fileName, string container) =>
             TryCatch(async () =>
             {
                 ValidateDocumentOnRetrieve(fileName, container);

                 byte[] retrievedDocument = await this.blobStorageBroker
                     .SelectByFileNameAsync(fileName, container);

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
               var expireOn = this.dateTimeBroker.GetCurrentDateTimeOffset().AddMinutes(5);

               return await this.blobStorageBroker.GetDownloadLinkAsync(fileName, container, expireOn);
           });
    }
}
