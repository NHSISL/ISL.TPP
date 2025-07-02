// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models;
using ISL.TPP.Core.Models.Foundations.CsvMappers.Exceptions;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.CsvMappers
{
    public partial class CsvMapperTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnMapCsvToObjectIfServiceErrorOccursAndLogItAsync()
        {
            // given
            string randomCsvFormattedManifestData = GetRandomString();
            string inputCsvFormattedManifestData = randomCsvFormattedManifestData;
            bool withHeaderRecord = true;
            var serviceException = new Exception();

            var failedCsvMapperServiceException =
                new FailedCsvMapperServiceException(
                    message: "Failed CSV mapper service error occurred, contact support.",
                    innerException: serviceException);

            var expectedCsvMapperServiceException =
                new CsvMapperServiceException(
                    message: "CSV mapper service error occurred, contact support.",
                    innerException: failedCsvMapperServiceException);

            this.csvMapperBrokerMock.Setup(broker =>
                broker.MapCsvToObjectAsync<Manifest>(inputCsvFormattedManifestData, withHeaderRecord))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<List<Manifest>> mapCsvToObjectTask = this.csvMapperService.MapCsvToObjectAsync<Manifest>(
                data: inputCsvFormattedManifestData,
                hasHeaderRecord: withHeaderRecord);

            CsvMapperServiceException actualCsvMapperServiceException =
                await Assert.ThrowsAsync<CsvMapperServiceException>(mapCsvToObjectTask.AsTask);

            // then
            actualCsvMapperServiceException.Should().BeEquivalentTo(expectedCsvMapperServiceException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCsvMapperServiceException))),
                        Times.Once);

            this.csvMapperBrokerMock.Verify(broker =>
                broker.MapCsvToObjectAsync<Manifest>(inputCsvFormattedManifestData, withHeaderRecord),
                    Times.Once());

            this.csvMapperBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
