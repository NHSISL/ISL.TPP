// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Files
{
    public partial class FileServiceTests
    {
        [Fact]
        public async Task ShouldMoveToFileToDestinationIfFolderDoesNotExistsAsync()
        {
            // given
            string sourcePath = "C:\\Temp\\Folder1";
            string destinationPath = "C:\\Temp\\Folder2";
            string fileName = $"{GetRandomString()}.txt";
            string fullSourcePath = Path.Combine(sourcePath, fileName);
            string fullDestinationPath = Path.Combine(destinationPath, fileName);

            this.fileBrokerMock.Setup(broker =>
                broker.GetDirectoryAsync(fullDestinationPath))
                    .ReturnsAsync(destinationPath);

            this.fileBrokerMock.Setup(broker =>
                broker.CheckIfDirectoryExistsAsync(destinationPath))
                    .ReturnsAsync(false);

            this.fileBrokerMock.Setup(broker =>
                broker.CheckIfFileExistsAsync(fullSourcePath))
                    .ReturnsAsync(true);

            this.fileBrokerMock.Setup(broker =>
                broker.CreateDirectoryAsync(destinationPath))
                    .ReturnsAsync(true);

            this.fileBrokerMock.Setup(broker =>
                broker.CheckIfFileExistsAsync(fullDestinationPath))
                    .ReturnsAsync(false);

            this.fileBrokerMock.Setup(broker =>
                broker.MoveFileAsync(fullSourcePath, fullDestinationPath))
                    .ReturnsAsync(true);

            // when
            await this.fileService.MoveFileAsync(fullSourcePath, fullDestinationPath);

            // then
            this.fileBrokerMock.Verify(broker =>
                broker.GetDirectoryAsync(fullDestinationPath),
                    Times.Once);

            this.fileBrokerMock.Verify(broker =>
                broker.CheckIfDirectoryExistsAsync(destinationPath),
                    Times.Once);

            this.fileBrokerMock.Verify(broker =>
                broker.CheckIfFileExistsAsync(fullSourcePath),
                    Times.Once);

            this.fileBrokerMock.Verify(broker =>
                broker.CheckIfFileExistsAsync(fullDestinationPath),
                    Times.Once);

            this.fileBrokerMock.Verify(broker =>
                broker.CreateDirectoryAsync(destinationPath),
                    Times.Once);

            this.fileBrokerMock.Verify(broker =>
                broker.MoveFileAsync(fullSourcePath, fullDestinationPath),
                    Times.Once);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldMoveToFileToDestinationIfFolderExistsAsync()
        {
            // given
            string sourcePath = "C:\\Temp\\Folder1";
            string destinationPath = "C:\\Temp\\Folder2";
            string fileName = $"{GetRandomString()}.txt";
            string fullSourcePath = Path.Combine(sourcePath, fileName);
            string fullDestinationPath = Path.Combine(destinationPath, fileName);

            this.fileBrokerMock.Setup(broker =>
                broker.GetDirectoryAsync(fullDestinationPath))
                    .ReturnsAsync(destinationPath);

            this.fileBrokerMock.Setup(broker =>
                broker.CheckIfDirectoryExistsAsync(destinationPath))
                    .ReturnsAsync(true);

            this.fileBrokerMock.Setup(broker =>
                broker.CheckIfFileExistsAsync(fullSourcePath))
                    .ReturnsAsync(true);

            this.fileBrokerMock.Setup(broker =>
                broker.CheckIfFileExistsAsync(fullDestinationPath))
                    .ReturnsAsync(false);

            this.fileBrokerMock.Setup(broker =>
                broker.MoveFileAsync(fullSourcePath, fullDestinationPath))
                    .ReturnsAsync(true);

            // when
            await this.fileService.MoveFileAsync(fullSourcePath, fullDestinationPath);

            // then
            this.fileBrokerMock.Verify(broker =>
                broker.GetDirectoryAsync(fullDestinationPath),
                    Times.Once);

            this.fileBrokerMock.Verify(broker =>
                broker.CheckIfDirectoryExistsAsync(destinationPath),
                    Times.Once);

            this.fileBrokerMock.Verify(broker =>
                broker.CheckIfFileExistsAsync(fullSourcePath),
                    Times.Once);

            this.fileBrokerMock.Verify(broker =>
                broker.CheckIfFileExistsAsync(fullDestinationPath),
                    Times.Once);

            this.fileBrokerMock.Verify(broker =>
                broker.MoveFileAsync(fullSourcePath, fullDestinationPath),
                    Times.Once);

            this.fileBrokerMock.VerifyNoOtherCalls();
        }
    }
}