// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Models.Foundations.Documents;
using ISL.TPP.Core.Models.Foundations.Documents.Exceptions;
using ISL.TPP.Core.Models.Foundations.Files.Exceptions;
using ISL.TPP.Core.Services.Foundations.Documents;
using ISL.TPP.Core.Services.Foundations.Files;
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
        private readonly TppConfiguration tppConfiguration;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private ITppOrchestrationService tppOrchestrationService;
        private readonly ICompareLogic compareLogic;

        public TppOrchestrationTests()
        {
            this.fileServiceMock = new Mock<IFileService>();
            this.documentServiceMock = new Mock<IDocumentService>();
            this.tppConfiguration = CreateRandomTppConfiguration();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            compareLogic = new CompareLogic();

            this.tppOrchestrationService = new TppOrchestrationService(
                fileService: this.fileServiceMock.Object,
                documentService: this.documentServiceMock.Object,
                tppConfiguration: this.tppConfiguration,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        public static TheoryData TppDependencyValidationExceptions()
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

        public static TheoryData TppDependencyExceptions()
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

        private static TppConfiguration CreateRandomTppConfiguration() =>
            CreateRandomTppConfigurationFiller().Create();

        private static Filler<TppConfiguration> CreateRandomTppConfigurationFiller()
        {
            var filler = new Filler<TppConfiguration>();
            filler.Setup();

            return filler;
        }

        private static List<string> GetRandomFileList(int count)
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
    }
}
