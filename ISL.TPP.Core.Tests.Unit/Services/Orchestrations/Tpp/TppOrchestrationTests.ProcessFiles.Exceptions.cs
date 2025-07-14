// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions;
using ISL.TPP.Core.Services.Orchestrations.Tpp;
using Moq;
using Xeptions;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Orchestrations.Tpp
{
    public partial class TppOrchestrationTests
    {
        [Theory]
        [MemberData(nameof(TppDependencyValidationExceptions))]
        public async Task ShouldThrowAggregateExceptionOnProcessFilesIfDependencyValidationOccursAndLogItAsync(
            Xeption dependancyValidationException)
        {
            // given
            string randomFileName = GetRandomString();
            List<Exception> exceptions = new List<Exception>();

            var tppOrchestrationServiceMock = new Mock<TppOrchestrationService>(
                this.fileServiceMock.Object,
                this.documentServiceMock.Object,
                this.csvMapperServiceMock.Object,
                tppConfiguration,
                this.dateTimeBrokerMock.Object,
                this.loggingBrokerMock.Object)
            {
                CallBase = true
            };

            foreach (string reportingGroup in this.tppConfiguration.ReportingGroups)
            {
                exceptions.Add(dependancyValidationException);
                exceptions.Add(dependancyValidationException);
            }

            var aggregateException =
                new AggregateException($"Unable to land {exceptions.Count} document(s)", exceptions);

            var failedTppOrchestrationServiceException =
                new FailedTppOrchestrationServiceException(
                    message: "Failed TPP orchestration service occurred, please contact support",
                    innerException: aggregateException);

            var expectedTppOrchestrationServiceException =
                new TppOrchestrationServiceException(
                    message: "TPP orchestration service error occurred, contact support.",
                    innerException: failedTppOrchestrationServiceException);

            tppOrchestrationServiceMock.Setup(service =>
                service.ProcessReportingGroupFilesAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(dependancyValidationException);

            tppOrchestrationServiceMock.Setup(service =>
                service.ProcessReportingGroupReprocessFolderFilesAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(dependancyValidationException);

            // when
            ValueTask processTask = tppOrchestrationServiceMock.Object.ProcessFilesAsync();

            TppOrchestrationServiceException actualTppOrchestrationServiceException =
                await Assert.ThrowsAsync<TppOrchestrationServiceException>(processTask.AsTask);

            // then
            actualTppOrchestrationServiceException.Should()
                .BeEquivalentTo(expectedTppOrchestrationServiceException);

            tppOrchestrationServiceMock.Verify(service =>
                service.ProcessReportingGroupFilesAsync(It.IsAny<string>(), It.IsAny<string>()),
                    Times.Exactly(this.tppConfiguration.ReportingGroups.Count));

            tppOrchestrationServiceMock.Verify(service =>
                service.ProcessReportingGroupReprocessFolderFilesAsync(It.IsAny<string>(), It.IsAny<string>()),
                    Times.Exactly(this.tppConfiguration.ReportingGroups.Count));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTppOrchestrationServiceException))),
                        Times.Once);

            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(TppDependencyExceptions))]
        public async Task ShouldThrowAggregateExceptionOnProcessFilesIfDependencyExceptionOccursAndLogItAsync(
            Xeption dependancyException)
        {
            // given
            string randomFileName = GetRandomString();
            List<Exception> exceptions = new List<Exception>();

            var tppOrchestrationServiceMock = new Mock<TppOrchestrationService>(
                this.fileServiceMock.Object,
                this.documentServiceMock.Object,
                this.csvMapperServiceMock.Object,
                tppConfiguration,
                this.dateTimeBrokerMock.Object,
                this.loggingBrokerMock.Object)
            {
                CallBase = true
            };

            foreach (string reportingGroup in this.tppConfiguration.ReportingGroups)
            {
                exceptions.Add(dependancyException);
                exceptions.Add(dependancyException);
            }

            var aggregateException =
                new AggregateException($"Unable to land {exceptions.Count} document(s)", exceptions);

            var failedTppOrchestrationServiceException =
                new FailedTppOrchestrationServiceException(
                    message: "Failed TPP orchestration service occurred, please contact support",
                    innerException: aggregateException);

            var expectedTppOrchestrationServiceException =
                new TppOrchestrationServiceException(
                    message: "TPP orchestration service error occurred, contact support.",
                    innerException: failedTppOrchestrationServiceException);

            tppOrchestrationServiceMock.Setup(service =>
                service.ProcessReportingGroupFilesAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(dependancyException);

            tppOrchestrationServiceMock.Setup(service =>
                service.ProcessReportingGroupReprocessFolderFilesAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(dependancyException);

            // when
            ValueTask processTask = tppOrchestrationServiceMock.Object.ProcessFilesAsync();

            TppOrchestrationServiceException actualException =
                await Assert.ThrowsAsync<TppOrchestrationServiceException>(processTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedTppOrchestrationServiceException);

            tppOrchestrationServiceMock.Verify(service =>
                service.ProcessReportingGroupFilesAsync(It.IsAny<string>(), It.IsAny<string>()),
                    Times.Exactly(this.tppConfiguration.ReportingGroups.Count));

            tppOrchestrationServiceMock.Verify(service =>
                service.ProcessReportingGroupReprocessFolderFilesAsync(It.IsAny<string>(), It.IsAny<string>()),
                    Times.Exactly(this.tppConfiguration.ReportingGroups.Count));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTppOrchestrationServiceException))),
                        Times.Once);

            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowAggregateExceptionOnProcessFilesIfServiceErrorOccursAndLogItAsync()
        {
            //Given
            string randomFileName = GetRandomString();
            var serviceException = new Exception();
            List<Exception> exceptions = new List<Exception>();

            var tppOrchestrationServiceMock = new Mock<TppOrchestrationService>(
                this.fileServiceMock.Object,
                this.documentServiceMock.Object,
                this.csvMapperServiceMock.Object,
                tppConfiguration,
                this.dateTimeBrokerMock.Object,
                this.loggingBrokerMock.Object)
            {
                CallBase = true
            };



            foreach (string reportingGroup in this.tppConfiguration.ReportingGroups)
            {
                exceptions.Add(serviceException);
                exceptions.Add(serviceException);
            }

            var aggregateException =
                new AggregateException($"Unable to land {exceptions.Count} document(s)", exceptions);

            var outerFailedTppOrchestrationServiceException =
                new FailedTppOrchestrationServiceException(
                    message: "Failed TPP orchestration service occurred, please contact support",
                    innerException: aggregateException);

            var expectedTppOrchestrationServiceException =
                new TppOrchestrationServiceException(
                    message: "TPP orchestration service error occurred, contact support.",
                    innerException: outerFailedTppOrchestrationServiceException);

            tppOrchestrationServiceMock.Setup(service =>
                service.ProcessReportingGroupFilesAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(serviceException);

            tppOrchestrationServiceMock.Setup(service =>
                service.ProcessReportingGroupReprocessFolderFilesAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask processTask = tppOrchestrationServiceMock.Object.ProcessFilesAsync();

            TppOrchestrationServiceException actualException =
                await Assert.ThrowsAsync<TppOrchestrationServiceException>(processTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedTppOrchestrationServiceException);

            tppOrchestrationServiceMock.Verify(service =>
                service.ProcessReportingGroupFilesAsync(It.IsAny<string>(), It.IsAny<string>()),
                    Times.Exactly(this.tppConfiguration.ReportingGroups.Count));

            tppOrchestrationServiceMock.Verify(service =>
                service.ProcessReportingGroupReprocessFolderFilesAsync(It.IsAny<string>(), It.IsAny<string>()),
                    Times.Exactly(this.tppConfiguration.ReportingGroups.Count));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTppOrchestrationServiceException))),
                        Times.Once);

            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
