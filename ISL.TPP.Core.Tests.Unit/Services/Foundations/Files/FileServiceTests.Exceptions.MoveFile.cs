// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
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
        [MemberData(nameof(FileServiceDependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnMoveFileIfDependencyValidationErrorOccursAsync(
            Exception dependencyValidationException)
        {
            // given
            string someSourcePath = GetRandomString();
            string someDestinationPath = GetRandomString();

            var invalidFileServiceDependencyException =
                new InvalidFileServiceDependencyException(
                    message: "Invalid file service dependency validation error occurred.",
                    innerException: dependencyValidationException);

            var expectedFileDependencyValidationException =
                new FileDependencyValidationException(
                    message: "File dependency validation error occurred, contact support.",
                    innerException: invalidFileServiceDependencyException);

            this.fileBrokerMock.Setup(broker =>
                broker.GetDirectoryAsync(It.IsAny<string>()))
                    .ThrowsAsync(dependencyValidationException);

            // when
            ValueTask<bool> moveFileTask =
                this.fileService.MoveFileAsync(someSourcePath, someDestinationPath);

            FileDependencyValidationException actualException =
                await Assert.ThrowsAsync<FileDependencyValidationException>(moveFileTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedFileDependencyValidationException);

            this.fileBrokerMock.Verify(broker =>
                broker.GetDirectoryAsync(It.IsAny<string>()),
                    Times.Once);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(FileServiceDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnMoveFileIfDependencyErrorOccursAsync(
            Exception dependencyException)
        {
            // given
            string someSourcePath = GetRandomString();
            string someDestinationPath = GetRandomString();

            var failedFileDependencyException =
                new FailedFileDependencyException(
                    message: "Failed file dependency error occurred, contact support.",
                    innerException: dependencyException);

            var expectedFileDependencyException =
                new FileDependencyException(
                    message: "File dependency error occurred, contact support.",
                    innerException: failedFileDependencyException);

            this.fileBrokerMock.Setup(broker =>
                broker.GetDirectoryAsync(It.IsAny<string>()))
                    .ThrowsAsync(dependencyException);

            // when
            ValueTask<bool> moveFileTask =
                this.fileService.MoveFileAsync(someSourcePath, someDestinationPath);

            FileDependencyException actualException =
                await Assert.ThrowsAsync<FileDependencyException>(moveFileTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedFileDependencyException);

            this.fileBrokerMock.Verify(broker =>
                broker.GetDirectoryAsync(It.IsAny<string>()),
                    Times.AtLeastOnce);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShoudThrowServiceExceptionOnMoveFileIfServiceErrorOccursAsync()
        {
            // given
            string someSourcePath = GetRandomString();
            string someDestinationPath = GetRandomString();
            var serviceException = new Exception();

            var failedFileServiceException =
                new FailedFileServiceException(
                    message: "Failed file service error occurred, contact support.",
                    innerException: serviceException);

            var expectedFileServiceException =
                new FileServiceException(
                    message: "File service error occurred, contact support.",
                    innerException: failedFileServiceException);

            this.fileBrokerMock.Setup(broker =>
                broker.GetDirectoryAsync(It.IsAny<string>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<bool> moveFileTask = this.fileService.MoveFileAsync(someSourcePath, someDestinationPath);

            FileServiceException actualException =
                await Assert.ThrowsAsync<FileServiceException>(moveFileTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedFileServiceException);

            this.fileBrokerMock.Verify(broker =>
                broker.GetDirectoryAsync(It.IsAny<string>()),
                    Times.Once);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }
    }
}