// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.CsvMappers
{
    public partial class CsvMapperTests
    {
        [Fact]
        public async Task ShouldMapCsvToObjectAsync()
        {
            // given
            string randomCsvFormattedManifestData = GetRandomString();
            string inputCsvFormattedManifestData = randomCsvFormattedManifestData;
            List<Manifest> randomOptouts = CreateRandomManifests();
            List<Manifest> expectedManifests = randomOptouts;
            bool withHeaderRecord = true;

            this.csvMapperBrokerMock.Setup(broker =>
                broker.MapCsvToObjectAsync<Manifest>(inputCsvFormattedManifestData, withHeaderRecord))
                    .ReturnsAsync(expectedManifests);

            // when
            List<Manifest> actualManifests = await this.csvMapperService.MapCsvToObjectAsync<Manifest>(
                data: inputCsvFormattedManifestData,
                hasHeaderRecord: withHeaderRecord);

            // then
            actualManifests.Should().BeEquivalentTo(expectedManifests);

            this.csvMapperBrokerMock.Verify(broker =>
                broker.MapCsvToObjectAsync<Manifest>(inputCsvFormattedManifestData, withHeaderRecord),
                    Times.Once());

            this.csvMapperBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
