// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Models;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Models.Foundations.Documents;
using ISL.TPP.Core.Models.Foundations.Documents.Exceptions;
using ISL.TPP.Core.Models.Foundations.Files.Exceptions;
using ISL.TPP.Core.Services.Foundations.CsvMappers;
using ISL.TPP.Core.Services.Foundations.Documents;
using ISL.TPP.Core.Services.Foundations.Files;
using ISL.TPP.Core.Services.Foundations.SubscriberAgreements;
using ISL.TPP.Core.Services.Orchestrations.Tpp;
using KellermanSoftware.CompareNetObjects;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Orchestrations.Tpp
{
    public partial class TppOrchestrationTests
    {
        private readonly Mock<IFileService> fileServiceMock;
        private readonly Mock<IDocumentService> documentServiceMock;
        private readonly Mock<ISubscriberAgreementService> subscriberAgreementServiceMock;
        private readonly Mock<ICsvMapperService> csvMapperServiceMock;
        private readonly TppConfiguration tppConfiguration;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private ITppOrchestrationService tppOrchestrationService;
        private readonly ICompareLogic compareLogic;

        public TppOrchestrationTests()
        {
            this.fileServiceMock = new Mock<IFileService>();
            this.documentServiceMock = new Mock<IDocumentService>();
            this.subscriberAgreementServiceMock = new Mock<ISubscriberAgreementService>();
            this.csvMapperServiceMock = new Mock<ICsvMapperService>();
            this.tppConfiguration = CreateRandomTppConfiguration(count: 1);
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            compareLogic = new CompareLogic();

            this.tppOrchestrationService = new TppOrchestrationService(
                fileService: this.fileServiceMock.Object,
                documentService: this.documentServiceMock.Object,
                subscriberAgreementService: this.subscriberAgreementServiceMock.Object,
                csvMapperService: this.csvMapperServiceMock.Object,
                tppConfiguration: this.tppConfiguration,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private Expression<Func<Stream, bool>> SameStreamAs(Stream expectedStream)
        {
            return actualStream =>
                IsSameStream(expectedStream, actualStream);
        }

        private static bool IsSameStream(Stream expectedStream, Stream actualStream)
        {
            byte[] expectedBytes = ReadAllBytesFromStream(expectedStream);
            byte[] actualBytes = ReadAllBytesFromStream(actualStream);

            return new CompareLogic().Compare(expectedBytes, actualBytes).AreEqual;
        }

        private static byte[] ReadAllBytesFromStream(Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Func<Xeption, bool> IsSameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        public static TheoryData<Xeption> TppDependencyValidationExceptions()
        {
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;
            var innerException = new Xeption(exceptionMessage);

            return new TheoryData<Xeption>
            {
                new DocumentValidationException(
                    message: "Document validation errors occured, please try again",
                    innerException),

                new DocumentDependencyValidationException(
                    message: "Document dependency validation occurred, please try again.",
                    innerException),

                new FileValidationException(
                    message: "File validation errors occurred, please try again.",
                    innerException),

                new FileDependencyValidationException(
                    message: "File dependency validation occurred, please try again.",
                    innerException),
            };
        }

        public static TheoryData<Xeption> TppDependencyExceptions()
        {
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;
            var innerException = new Xeption(exceptionMessage);

            return new TheoryData<Xeption>
            {
                new DocumentDependencyException(
                    message: "Document dependency error occurred, contact support.",
                    innerException),

                new DocumentServiceException(
                    message: "Document service error occurred, contact support.",
                    innerException),

                new FileDependencyException(
                    message: "File dependency error occurred, contact support.",
                    innerException),

                new FileServiceException(
                    message: "File service error occurred, contact support.",
                    innerException)
            };
        }

        private Expression<Func<Document, bool>> SameDocumentAs(
            Document expectedDocument)
        {
            return actualDocument =>
                this.compareLogic.Compare(expectedDocument, actualDocument)
                    .AreEqual;
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static TppConfiguration CreateRandomTppConfiguration(int count) =>
            CreateRandomTppConfigurationFiller(count).Create();

        private static TppConfiguration CreateRandomTppConfiguration() =>
            CreateRandomTppConfigurationFiller(GetRandomNumber()).Create();

        private static Filler<TppConfiguration> CreateRandomTppConfigurationFiller(int count)
        {
            var filler = new Filler<TppConfiguration>();
            filler.Setup()
                .OnProperty(config => config.BlobStoragesSettings).Use(() => GetRandomBlobStorages(count))
                .OnProperty(config => config.TppPickupFolder).Use(() => $"c:\\{GetRandomString()}")
                .OnProperty(config => config.TppManifestFile).Use(() => $"{GetRandomString()}Manifest.csv");

            return filler;
        }

        private static List<BlobStorageSettings> GetRandomBlobStorages(int count) =>
            Enumerable.Range(0, count).Select(_ => GetRandomBlobStorage()).ToList();

        private static BlobStorageSettings GetRandomBlobStorage() =>
            GetRandomBlobStorageFiller().Create();

        private static Filler<BlobStorageSettings> GetRandomBlobStorageFiller()
        {
            var filler = new Filler<BlobStorageSettings>();
            filler.Setup().OnProperty(setting => setting.Enabled).Use(() => true);

            return filler;
        }

        private static List<string> GetRandomFileList(int count)
        {
            List<string> files = new List<string>();

            for (int i = 0; i < count; i++)
            {
                files.Add($"{GetRandomString()}.csv");
            }

            return files;
        }

        private static List<string> GetRandomStringList(int count)
        {
            return CreateRandomFileListFiller()
                .Create(count)
                    .ToList();
        }

        private static Filler<string> CreateRandomFileListFiller()
        {
            var filler = new Filler<string>();
            filler.Setup();

            return filler;
        }

        private static List<Document> CreateRandomDocuments(int count)
        {
            return CreateDocumentFiller()
                .Create(count)
                    .ToList();
        }

        private static Filler<Document> CreateDocumentFiller()
        {
            var filler = new Filler<Document>();
            string filename = GetRandomString();
            filler.Setup().OnProperty(document => document.FileName).Use(() => filename);

            return filler;
        }

        private static List<Manifest> CreateRandomManifests(int count)
        {
            return CreateManifestFiller(dateTimeOffset: GetRandomDateTimeOffset())
                .Create(count)
                    .ToList();
        }

        private static List<Manifest> CreateRandomManifests()
        {
            return CreateManifestFiller(dateTimeOffset: GetRandomDateTimeOffset())
                .Create(count: GetRandomNumber())
                    .ToList();
        }

        private static Filler<Manifest> CreateManifestFiller(DateTimeOffset dateTimeOffset)
        {
            DateTime fromDate = dateTimeOffset.UtcDateTime.AddDays(-1);
            DateTime toDate = dateTimeOffset.UtcDateTime;

            var filler = new Filler<Manifest>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(manifest => manifest.FileName).Use(() => $"{GetRandomString()}.csv")
                .OnProperty(manifest => manifest.IsDelta).Use("Y")
                .OnProperty(manifest => manifest.IsReference).Use("N")
                .OnProperty(manifest => manifest.DateExtractFrom).Use($"{fromDate:yyyyMMdd}_{fromDate:HHmm}")
                .OnProperty(manifest => manifest.DateExtractTo).Use($"{toDate:yyyyMMdd}_{toDate:HHmm}");

            return filler;
        }
    }
}
