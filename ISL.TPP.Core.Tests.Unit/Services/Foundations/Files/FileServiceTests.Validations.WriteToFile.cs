// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Foundations.Files.Exceptions;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Files
{
    public partial class FileServiceTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ShouldThrowValidationExceptionOnWriteToFileIfArgumentsIsInvalidAsync(string invalidValue)
        {
            // given
            string invalidPath = invalidValue;
            string invalidContent = invalidValue;

            var invalidArgumentFileException =
                new InvalidArgumentFileException(
                    message: "Invalid file argument(s), please correct the errors and try again.");

            invalidArgumentFileException.AddData(
                key: "path",
                values: "Text is required");

            invalidArgumentFileException.AddData(
                key: "content",
                values: "Text is required");

            var expectedFileValidationException =
                new FileValidationException(
                    message: "File validation error occurred, fix the errors and try again.",
                    innerException: invalidArgumentFileException);

            // when
            ValueTask<bool> writeToFileTask =
                this.fileService.WriteToFileAsync(invalidPath, invalidContent);

            FileValidationException actualException =
                await Assert.ThrowsAsync<FileValidationException>(writeToFileTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedFileValidationException);

            this.fileBrokerMock.Verify(broker =>
                broker.WriteToFileAsync(invalidPath, invalidContent),
                        Times.Never);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }
    }
}
