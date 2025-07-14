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
        public async Task ShouldThrowServiceExceptionOnMapObjectToCsvIfServiceErrorOccursAndLogItAsync()
        {
            // given
            List<Manifest> randomManifests = CreateRandomManifests();
            List<Manifest> inputManifests = randomManifests;
            bool withHeaderRecord = true;
            bool shouldAddTrailingComma = true;
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
                broker.MapObjectToCsvAsync<Manifest>(inputManifests, withHeaderRecord, shouldAddTrailingComma))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<string> mapCsvToObjectTask = this.csvMapperService.MapObjectToCsvAsync<Manifest>(
                @object: inputManifests,
                addHeaderRecord: withHeaderRecord,
                shouldAddTrailingComma);

            CsvMapperServiceException actualCsvMapperServiceException =
                await Assert.ThrowsAsync<CsvMapperServiceException>(mapCsvToObjectTask.AsTask);

            // then
            actualCsvMapperServiceException.Should().BeEquivalentTo(expectedCsvMapperServiceException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCsvMapperServiceException))),
                        Times.Once);

            this.csvMapperBrokerMock.Verify(broker =>
                broker.MapObjectToCsvAsync<Manifest>(inputManifests, withHeaderRecord, shouldAddTrailingComma),
                    Times.Once());

            this.csvMapperBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
