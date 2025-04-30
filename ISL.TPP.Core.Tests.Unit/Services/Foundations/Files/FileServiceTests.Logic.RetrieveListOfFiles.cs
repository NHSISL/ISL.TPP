// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Files
{
    public partial class FileServiceTests
    {
        [Fact]
        public async Task ShouldRetrieveListOfFilesAsync()
        {
            // given
            string randomFilePath = GetRandomString();
            string inputFilePath = randomFilePath;
            string randomSearchPattern = GetRandomString();
            string inputSearchPattern = randomSearchPattern;
            List<string> randomResult = GetRandomStringList();
            List<string> outputResult = randomResult;
            List<string> expectedResult = randomResult;

            this.fileBrokerMock.Setup(broker =>
                broker.GetListOfFilesAsync(inputFilePath, inputSearchPattern, It.IsAny<SearchOption>()))
                    .ReturnsAsync(outputResult);

            // when
            List<string> actualResult =
                await this.fileService.RetrieveListOfFilesAsync(inputFilePath, inputSearchPattern);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.fileBrokerMock.Verify(broker =>
                broker.GetListOfFilesAsync(inputFilePath, inputSearchPattern, It.IsAny<SearchOption>()),
                    Times.Once);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }
    }
}
