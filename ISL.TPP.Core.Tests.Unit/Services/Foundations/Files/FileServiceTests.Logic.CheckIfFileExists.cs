// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Files
{
    public partial class FileServiceTests
    {
        [Fact]
        public async Task ShouldCheckIfFileExistsAsync()
        {
            // given
            string randomFilePath = GetRandomString();
            string inputFilePath = randomFilePath;
            bool outputResult = true;
            bool expectedResult = outputResult;

            this.fileBrokerMock.Setup(broker =>
                broker.CheckIfFileExistsAsync(inputFilePath))
                    .ReturnsAsync(outputResult);

            // when
            bool actualResult = await this.fileService.CheckIfFileExistsAsync(inputFilePath);

            // then
            actualResult.Should().Be(expectedResult);

            this.fileBrokerMock.Verify(broker =>
                broker.CheckIfFileExistsAsync(inputFilePath),
                    Times.Once);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }
    }
}
