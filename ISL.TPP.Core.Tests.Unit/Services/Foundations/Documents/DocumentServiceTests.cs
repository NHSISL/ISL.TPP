// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using Azure;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Services.Foundations.Documents;
using Microsoft.Extensions.Configuration;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {
        private readonly Mock<IBlobStorageBroker> blobStorageBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IConfiguration inMemoryConfiguration;
        private readonly IDocumentService documentService;

        public DocumentServiceTests()
        {
            this.blobStorageBrokerMock = new Mock<IBlobStorageBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.documentService = new DocumentService(
                blobStorageBroker: this.blobStorageBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static RequestFailedException GetBlobException() =>
           (RequestFailedException)FormatterServices.GetUninitializedObject(typeof(RequestFailedException));

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
           actualException => actualException.SameExceptionAs(expectedException);

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static string ComputeSHA256Hash(byte[] bytes)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        private static BlobStorageSettings CreateRandomBlobStorageSettings() =>
            CreateRandomBlobStorageSettingsFiller().Create();

        private static Filler<BlobStorageSettings> CreateRandomBlobStorageSettingsFiller()
        {
            var filler = new Filler<BlobStorageSettings>();
            filler.Setup();

            return filler;
        }
    }
}