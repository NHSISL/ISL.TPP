// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Services.Orchestrations.Tpp;
using Moq;
using Xeptions;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Orchestrations.Tpp
{
    public partial class TppOrchestrationTests
    {
        [Fact]
        public async Task ShouldThrowAggregateExceptionOnProcessReportingGroupReprocessFolderFilesAsync()
        {
            // given
            string randomReportingGroupFolder = GetRandomString();
            string inputReportingGroupFolder = randomReportingGroupFolder;
            List<string> randomFolderList = GetRandomStringList(GetRandomNumber());
            Xeption someException = new Xeption("Some exception");
            var exceptions = new List<Exception>();

            this.fileServiceMock.Setup(service =>
                service.RetrieveListOfSubFoldersAsync(inputReportingGroupFolder, "*"))
                    .ReturnsAsync(randomFolderList);

            var tppOrchestrationServiceMock = new Mock<TppOrchestrationService>(
                this.fileServiceMock.Object,
                this.documentServiceMock.Object,
                this.csvMapperServiceMock.Object,
                this.tppConfiguration,
                this.dateTimeBrokerMock.Object,
                this.loggingBrokerMock.Object)
            {
                CallBase = true
            };

            string pickupFolder =
                Path.Combine(inputReportingGroupFolder, tppConfiguration.TppWorkingFolders.ReProcess);

            fileServiceMock.Setup(service =>
                service.RetrieveListOfSubFoldersAsync(pickupFolder, "*"))
                    .ReturnsAsync(randomFolderList);

            foreach (string folder in randomFolderList)
            {
                exceptions.Add(someException);

                tppOrchestrationServiceMock.Setup(service =>
                    service.ProcessReportingGroupFilesAsync(folder))
                        .ThrowsAsync(someException);
            }

            AggregateException expectedAggregateException =
                new AggregateException($"Unable to land document(s)", exceptions);

            // when
            ValueTask processReportingGroupReprocessFolderFilesTask = tppOrchestrationServiceMock.Object
                .ProcessReportingGroupReprocessFolderFilesAsync(inputReportingGroupFolder);

            AggregateException actualAggregateException =
                await Assert.ThrowsAsync<AggregateException>(processReportingGroupReprocessFolderFilesTask.AsTask);

            // then
            actualAggregateException.Should().BeEquivalentTo(expectedAggregateException);

            fileServiceMock.Verify(service =>
                service.RetrieveListOfSubFoldersAsync(pickupFolder, "*"),
                    Times.Once);

            foreach (string folder in randomFolderList)
            {
                tppOrchestrationServiceMock.Verify(service =>
                    service.ProcessReportingGroupFilesAsync(folder),
                        Times.Once);
            }

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(someException))),
                    Times.Exactly(randomFolderList.Count));

            tppOrchestrationServiceMock.Verify(service =>
                service.ProcessReportingGroupReprocessFolderFilesAsync(inputReportingGroupFolder),
                    Times.Once);

            tppOrchestrationServiceMock.VerifyNoOtherCalls();
            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
