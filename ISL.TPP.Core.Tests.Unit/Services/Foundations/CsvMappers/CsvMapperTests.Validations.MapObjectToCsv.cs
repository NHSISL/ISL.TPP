// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

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
        public async Task ShouldThrowValidationExceptionOnMapObjectToCsvIfInputsIsInvalidAndLogItAsync()
        {
            // given
            List<Manifest> nullManifests = null;
            string randomCsvFormattedManifestData = GetRandomString();
            string expectedCsvFormattedManifestData = randomCsvFormattedManifestData;
            bool withHeaderRecord = true;
            bool shouldAddTrailingComma = true;

            this.csvMapperBrokerMock.Setup(broker =>
                broker.MapObjectToCsvAsync<Manifest>(nullManifests, withHeaderRecord, shouldAddTrailingComma))
                    .ReturnsAsync(expectedCsvFormattedManifestData);

            var invalidCsvMapperArgumentsException = new InvalidCsvMapperArgumentsException(
                message: "Invalid CSV mapper arguments. Please fix the errors and try again.");

            invalidCsvMapperArgumentsException.AddData(
                key: "Object",
                values: "Object is required");

            var expectedCsvMapperValidationException =
                new CsvMapperValidationException(
                    message: "CSV mapper validation errors occurred, fix the errors and try again.",
                    innerException: invalidCsvMapperArgumentsException);

            // when
            ValueTask<string> mapObjectToCsvTask = this.csvMapperService.MapObjectToCsvAsync<Manifest>(
                @object: nullManifests,
                addHeaderRecord: withHeaderRecord,
                shouldAddTrailingComma);

            CsvMapperValidationException actualCsvMapperValidationException =
                await Assert.ThrowsAsync<CsvMapperValidationException>(mapObjectToCsvTask.AsTask);

            // then
            actualCsvMapperValidationException.Should().BeEquivalentTo(expectedCsvMapperValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCsvMapperValidationException))),
                        Times.Once);

            this.csvMapperBrokerMock.Verify(broker =>
                broker.MapObjectToCsvAsync<Manifest>(nullManifests, withHeaderRecord, shouldAddTrailingComma),
                    Times.Never());

            this.csvMapperBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
