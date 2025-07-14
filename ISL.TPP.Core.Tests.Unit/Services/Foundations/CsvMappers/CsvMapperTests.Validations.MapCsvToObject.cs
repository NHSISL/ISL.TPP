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
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnMapCsvToObjectIfInputsIsInvalidAndLogItAsync(
            string invalidText)
        {
            // given
            string randomCsvFormattedManifestData = invalidText;
            string inputCsvFormattedManifestData = randomCsvFormattedManifestData;
            List<Manifest> randomOptouts = CreateRandomManifests();
            List<Manifest> expectedManifests = randomOptouts;
            bool withHeaderRecord = true;

            this.csvMapperBrokerMock.Setup(broker =>
                broker.MapCsvToObjectAsync<Manifest>(inputCsvFormattedManifestData, withHeaderRecord))
                    .ReturnsAsync(expectedManifests);

            var invalidCsvMapperArgumentsException = new InvalidCsvMapperArgumentsException(
                message: "Invalid CSV mapper arguments. Please fix the errors and try again.");

            invalidCsvMapperArgumentsException.AddData(
                key: "Data",
                values: "Text is required");

            var expectedCsvMapperValidationException =
                new CsvMapperValidationException(
                    message: "CSV mapper validation errors occurred, fix the errors and try again.",
                    innerException: invalidCsvMapperArgumentsException);

            // when
            ValueTask<List<Manifest>> mapCsvToObjectTask = this.csvMapperService.MapCsvToObjectAsync<Manifest>(
                data: inputCsvFormattedManifestData,
                hasHeaderRecord: withHeaderRecord);

            CsvMapperValidationException actualCsvMapperValidationException =
                await Assert.ThrowsAsync<CsvMapperValidationException>(mapCsvToObjectTask.AsTask);

            // then
            actualCsvMapperValidationException.Should().BeEquivalentTo(expectedCsvMapperValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCsvMapperValidationException))),
                        Times.Once);

            this.csvMapperBrokerMock.Verify(broker =>
                broker.MapCsvToObjectAsync<Manifest>(inputCsvFormattedManifestData, withHeaderRecord),
                    Times.Never());

            this.csvMapperBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
