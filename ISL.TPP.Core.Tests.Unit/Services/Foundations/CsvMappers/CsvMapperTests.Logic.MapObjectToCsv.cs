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
        public async Task ShouldMapObjectToCsvAsync()
        {
            // given
            List<Manifest> randomOptouts = CreateRandomManifests();
            List<Manifest> inputManifests = randomOptouts;
            string randomCsvFormattedManifestData = GetRandomString();
            string expectedCsvFormattedManifestData = randomCsvFormattedManifestData;
            bool withHeaderRecord = true;
            bool shouldAddTrailingComma = true;

            this.csvMapperBrokerMock.Setup(broker =>
                broker.MapObjectToCsvAsync<Manifest>(inputManifests, withHeaderRecord, shouldAddTrailingComma))
                    .ReturnsAsync(expectedCsvFormattedManifestData);

            // when
            string actualCsvFormattedManifestData = await this.csvMapperService.MapObjectToCsvAsync<Manifest>(
                @object: inputManifests,
                addHeaderRecord: withHeaderRecord,
                shouldAddTrailingComma);

            // then
            actualCsvFormattedManifestData.Should().BeEquivalentTo(expectedCsvFormattedManifestData);

            this.csvMapperBrokerMock.Verify(broker =>
                broker.MapObjectToCsvAsync<Manifest>(inputManifests, withHeaderRecord, shouldAddTrailingComma),
                    Times.Once());

            this.csvMapperBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
