// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Models.Foundations.Documents;
using Moq;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Orchestrations.Tpp
{
    public partial class TppOrchestrationTests
    {
        [Fact]
        public async Task ShouldNotProcessFilesIfManifestFileNotPresentAsync()
        {
            // given
            int numberOfFiles = GetRandomNumber();
            List<string> files = GetRandomFileList(numberOfFiles);
            List<string> expectedFiles = new List<string>();
            byte[] fileBytes = Encoding.UTF8.GetBytes(GetRandomString());

            foreach (string reportingGroup in this.tppConfiguration.ReportingGroups)
            {
                string pickupFolder = Path.Combine(this.tppConfiguration.TppPickupFolder, reportingGroup);
                List<string> reportingGroupFiles = files.Select(file => Path.Combine(pickupFolder, file)).ToList();

                this.fileServiceMock.Setup(service =>
                    service.RetrieveListOfFilesAsync(pickupFolder, "*"))
                        .ReturnsAsync(reportingGroupFiles);
            }

            // when
            List<string> actualFiles = await this.tppOrchestrationService.ProcessFilesAsync();

            // then
            expectedFiles.Should().BeEquivalentTo(actualFiles);

            foreach (string reportingGroup in this.tppConfiguration.ReportingGroups)
            {
                string pickupFolder = Path.Combine(this.tppConfiguration.TppPickupFolder, reportingGroup);
                List<string> reportingGroupFiles = files.Select(file => Path.Combine(pickupFolder, file)).ToList();

                this.fileServiceMock.Verify(service =>
                    service.RetrieveListOfFilesAsync(pickupFolder, "*"),
                        Times.Once);
            }

            this.fileServiceMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldProcessFilesIfManifestFileIsPresentAsync()
        {
            // given
            int numberOfFiles = GetRandomNumber();
            List<string> files = GetRandomFileList(numberOfFiles);
            files.Add(tppConfiguration.TppManifestFile);
            List<string> expectedFiles = new List<string>();
            byte[] fileBytes = Encoding.UTF8.GetBytes(GetRandomString());
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffset())
                    .Returns(randomDateTimeOffset);

            foreach (string reportingGroup in this.tppConfiguration.ReportingGroups)
            {
                string pickupFolder = Path.Combine(this.tppConfiguration.TppPickupFolder, reportingGroup);
                List<string> reportingGroupFiles = files.Select(file => Path.Combine(pickupFolder, file)).ToList();

                this.fileServiceMock.Setup(service =>
                    service.RetrieveListOfFilesAsync(pickupFolder, "*"))
                        .ReturnsAsync(reportingGroupFiles);

                foreach (string file in reportingGroupFiles)
                {
                    this.fileServiceMock.Setup(service =>
                        service.ReadFromFileAsync(file))
                            .ReturnsAsync(fileBytes);
                }
            }

            // when
            List<string> actualFiles = await this.tppOrchestrationService.ProcessFilesAsync();

            // then
            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffset(),
                    Times.Exactly(this.tppConfiguration.ReportingGroups.Count));

            foreach (string reportingGroup in this.tppConfiguration.ReportingGroups)
            {
                string pickupFolder = Path.Combine(this.tppConfiguration.TppPickupFolder, reportingGroup);
                List<string> reportingGroupFiles = files.Select(file => Path.Combine(pickupFolder, file)).ToList();

                this.fileServiceMock.Verify(service =>
                    service.RetrieveListOfFilesAsync(pickupFolder, "*"),
                        Times.Once);

                foreach (string file in reportingGroupFiles)
                {
                    this.fileServiceMock.Verify(service =>
                        service.ReadFromFileAsync(file),
                            Times.Once);

                    var fileName =
                        $"{reportingGroup}\\{randomDateTimeOffset.ToString("yyyyMMddHHmmss")}\\" +
                        $"{file.Replace(pickupFolder, "")}";

                    fileName = fileName.Replace("\\\\", "\\");

                    var doc = new Document
                    {
                        FileName = fileName,
                        DocumentData = fileBytes
                    };

                    expectedFiles.Add(doc.FileName);

                    this.documentServiceMock.Verify(service =>
                        service.AddDocumentAsync(
                            It.Is(SameDocumentAs(doc)),
                            this.tppConfiguration.BlobStorageSettings.AzureBlobContainer),
                                Times.Once);

                    this.fileServiceMock.Verify(service =>
                        service.DeleteFileAsync(file),
                            Times.Once);
                }
            }

            this.fileServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
