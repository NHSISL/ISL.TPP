// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
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
        public async Task ShouldThrowValidationExceptionOnRetrieveListOfFilesIfPathIsInvalidAsync(string invalidValue)
        {
            // given
            string invalidPath = invalidValue;
            string invalidSearchPattern = invalidValue;

            var invalidArgumentFileException =
                new InvalidArgumentFileException(
                    message: "Invalid file argument(s), please correct the errors and try again.");

            invalidArgumentFileException.AddData(
                key: "path",
                values: "Text is required");

            invalidArgumentFileException.AddData(
                key: "searchPattern",
                values: "Text is required");

            var expectedFileValidationException =
                new FileValidationException(
                    message: "File validation error occurred, fix the errors and try again.",
                    innerException: invalidArgumentFileException);

            // when
            ValueTask<List<string>> retrieveListOfFilesTask =
                this.fileService.RetrieveListOfFilesAsync(invalidPath, invalidSearchPattern);

            FileValidationException actualException =
                await Assert.ThrowsAsync<FileValidationException>(retrieveListOfFilesTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedFileValidationException);

            this.fileBrokerMock.Verify(broker =>
                broker.GetListOfFilesAsync(invalidPath, invalidSearchPattern, It.IsAny<SearchOption>()),
                    Times.Never);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }
    }
}
