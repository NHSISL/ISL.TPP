// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.IO;
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
        public async Task ShouldThrowValidationExceptionOnMoveFileIfPathsIsInvalidAsync(string invalidPath)
        {
            // given
            string invalidSourcePath = invalidPath;
            string invalidDestinationPath = invalidPath;

            var invalidArgumentFileException =
                new InvalidArgumentFileException(
                    message: "Invalid file argument(s), please correct the errors and try again.");

            invalidArgumentFileException.AddData(
                key: "SourcePath",
                values: "Text is required");

            invalidArgumentFileException.AddData(
                key: "DestinationPath",
                values: "Text is required");

            var expectedFileValidationException =
                new FileValidationException(
                    message: "File validation error occurred, fix the errors and try again.",
                    innerException: invalidArgumentFileException);

            // when
            ValueTask<bool> moveFileTask =
                this.fileService.MoveFileAsync(invalidSourcePath, invalidDestinationPath);

            FileValidationException actualException =
                await Assert.ThrowsAsync<FileValidationException>(moveFileTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedFileValidationException);

            this.fileBrokerMock.Verify(broker =>
                broker.MoveFileAsync(invalidSourcePath, invalidDestinationPath),
                    Times.Never);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnMoveFileIfSourceFileDoesNotExistsAsync()
        {
            // given
            string sourcePath = "C:\\Temp\\Folder1";
            string destinationPath = "C:\\Temp\\Folder2";
            string fileName = $"{GetRandomString()}.txt";
            string fullSourcePath = Path.Combine(sourcePath, fileName);
            string fullDestinationPath = Path.Combine(destinationPath, fileName);

            var invalidArgumentFileException =
                new InvalidArgumentFileException(
                    message: "Invalid file argument(s), please correct the errors and try again.");

            invalidArgumentFileException.AddData(
                key: "SourcePath",
                values: $"Invalid path: {fullSourcePath}");

            var expectedFileValidationException =
                new FileValidationException(
                    message: "File validation error occurred, fix the errors and try again.",
                    innerException: invalidArgumentFileException);

            this.fileBrokerMock.Setup(broker =>
                broker.CheckIfFileExistsAsync(fullSourcePath))
                    .ReturnsAsync(false);

            // when
            ValueTask<bool> moveFileTask =
                this.fileService.MoveFileAsync(fullSourcePath, fullDestinationPath);

            FileValidationException actualException =
                await Assert.ThrowsAsync<FileValidationException>(moveFileTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedFileValidationException);

            this.fileBrokerMock.Verify(broker =>
                broker.CheckIfFileExistsAsync(fullSourcePath),
                    Times.Once);

            this.fileBrokerMock.Verify(broker =>
                broker.MoveFileAsync(fullSourcePath, fullDestinationPath),
                    Times.Never);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }
    }
}
