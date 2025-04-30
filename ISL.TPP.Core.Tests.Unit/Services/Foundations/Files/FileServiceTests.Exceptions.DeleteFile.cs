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
        public async Task ShouldThrowDependencyValidationExceptionOnDeleteFileIfDependencyValidationErrorOccursAsync(
            Exception dependencyValidationException)
        {
            // given
            string somePath = GetRandomString();

            var invalidFileServiceDependencyException =
                new InvalidFileServiceDependencyException(
                    message: "Invalid file service dependency validation error occurred.",
                    innerException: dependencyValidationException);

            var expectedFileDependencyValidationException =
                new FileDependencyValidationException(
                    message: "File dependency validation error occurred, contact support.",
                    innerException: invalidFileServiceDependencyException);

            this.fileBrokerMock.Setup(broker =>
                broker.DeleteFileAsync(somePath))
                    .ThrowsAsync(dependencyValidationException);

            // when
            ValueTask<bool> deleteFileTask =
                this.fileService.DeleteFileAsync(somePath);

            FileDependencyValidationException actualException =
                await Assert.ThrowsAsync<FileDependencyValidationException>(deleteFileTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedFileDependencyValidationException);

            this.fileBrokerMock.Verify(broker =>
                broker.DeleteFileAsync(somePath),
                    Times.Once);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(FileServiceDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnDeleteFileIfDependencyErrorOccursAsync(
            Exception dependencyException)
        {
            // given
            string somePath = GetRandomString();

            var failedFileDependencyException =
                new FailedFileDependencyException(
                    message: "Failed file dependency error occurred, contact support.",
                    innerException: dependencyException);

            var expectedFileDependencyException =
                new FileDependencyException(
                    message: "File dependency error occurred, contact support.",
                    innerException: failedFileDependencyException);

            this.fileBrokerMock.Setup(broker =>
                broker.DeleteFileAsync(somePath))
                    .ThrowsAsync(dependencyException);

            // when
            ValueTask<bool> deleteFileTask =
                this.fileService.DeleteFileAsync(somePath);

            FileDependencyException actualException =
                await Assert.ThrowsAsync<FileDependencyException>(deleteFileTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedFileDependencyException);

            this.fileBrokerMock.Verify(broker =>
                broker.DeleteFileAsync(somePath),
                    Times.AtLeastOnce);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShoudThrowServiceExceptionOnDeleteFileIfServiceErrorOccursAsync()
        {
            // given
            string somePath = GetRandomString();
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
                broker.DeleteFileAsync(somePath))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<bool> deleteFileTask =
                this.fileService.DeleteFileAsync(somePath);

            FileServiceException actualException =
                await Assert.ThrowsAsync<FileServiceException>(deleteFileTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedFileServiceException);

            this.fileBrokerMock.Verify(broker =>
                broker.DeleteFileAsync(somePath),
                    Times.Once);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }
    }
}