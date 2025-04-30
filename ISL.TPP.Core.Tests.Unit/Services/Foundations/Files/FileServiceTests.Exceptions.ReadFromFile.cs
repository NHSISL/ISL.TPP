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
        public async Task ShouldThrowDependencyValidationExceptionOnReadFromFileIfDependencyValidationErrorOccursAsync(
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
                broker.ReadFileAsync(somePath))
                    .ThrowsAsync(dependencyValidationException);

            // when
            ValueTask<byte[]> readFileTask =
                this.fileService.ReadFromFileAsync(somePath);

            FileDependencyValidationException actualException =
                await Assert.ThrowsAsync<FileDependencyValidationException>(readFileTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedFileDependencyValidationException);

            this.fileBrokerMock.Verify(broker =>
                broker.ReadFileAsync(somePath),
                    Times.Once);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(FileServiceDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnReadFromFileIfDependencyErrorOccursAsync(
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
                broker.ReadFileAsync(somePath))
                    .ThrowsAsync(dependencyException);

            // when
            ValueTask<byte[]> readFromFileTask =
                this.fileService.ReadFromFileAsync(somePath);

            FileDependencyException actualException =
                await Assert.ThrowsAsync<FileDependencyException>(readFromFileTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedFileDependencyException);

            this.fileBrokerMock.Verify(broker =>
                broker.ReadFileAsync(somePath),
                    Times.AtLeastOnce);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShoudThrowServiceExceptionOnReadFromFileIfServiceErrorOccursAsync()
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
                broker.ReadFileAsync(somePath))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<byte[]> readFromFileTask =
                this.fileService.ReadFromFileAsync(somePath);

            FileServiceException actualException =
                await Assert.ThrowsAsync<FileServiceException>(readFromFileTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedFileServiceException);

            this.fileBrokerMock.Verify(broker =>
                broker.ReadFileAsync(somePath),
                    Times.Once);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }
    }
}