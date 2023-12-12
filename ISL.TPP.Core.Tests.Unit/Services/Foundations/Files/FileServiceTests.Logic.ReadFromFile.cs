// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Files
{
    public partial class FileServiceTests
    {
        [Fact]
        public async Task ShouldReadFromFileAsync()
        {
            // given
            string randomFilePath = GetRandomString();
            string inputFilePath = randomFilePath;
            byte[] outputResult = ASCIIEncoding.UTF8.GetBytes(GetRandomString());
            byte[] expectedResult = outputResult;

            this.fileBrokerMock.Setup(broker =>
                broker.ReadFileAsync(inputFilePath))
                    .ReturnsAsync(outputResult);

            // when
            byte[] actualResult = await this.fileService.ReadFromFileAsync(inputFilePath);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.fileBrokerMock.Verify(broker =>
                broker.ReadFileAsync(inputFilePath),
                    Times.Once);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }
    }
}
