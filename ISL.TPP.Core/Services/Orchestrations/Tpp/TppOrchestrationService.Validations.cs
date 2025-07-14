// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions;

namespace ISL.TPP.Core.Services.Orchestrations.Tpp
{
    internal partial class TppOrchestrationService : ITppOrchestrationService
    {
        private void ValidateArgumentsOnCleanupFiles(
            List<string> fileList,
            string reportingGroup,
            string reportingGroupFolder,
            string manifestDateTime,
            string tppSubmissionFolder)
        {
            Validate(
                (Rule: IsInvalid(fileList), Parameter: "fileList"),
                (Rule: IsInvalid(reportingGroup), Parameter: "reportingGroup"),
                (Rule: IsInvalid(reportingGroupFolder), Parameter: "reportingGroupFolder"),
                (Rule: IsInvalid(manifestDateTime), Parameter: "manifestDateTime"),
                (Rule: IsInvalid(tppSubmissionFolder), Parameter: "tppSubmissionFolder"));
        }

        private void ValidateFolderExistOnCleanupFiles(bool folderExists, string reportingGroupFolder)
        {
            Validate((Rule: IsInvalidPath(folderExists, reportingGroupFolder), Parameter: "reportingGroupFolder"));
        }

        private void ValidateFileExistOnCleanupFiles(bool fileExists, string filePath)
        {
            Validate((Rule: IsInvalidPath(fileExists, filePath), Parameter: "filePath"));
        }

        private void ValidateFile(byte[] file)
        {
            Validate(
                (Rule: IsInvalid(file), Parameter: "File"));
        }

        private void ValidateConfigurationSettings()
        {
            ValidateTppConfigurationIsNotNull();

            Validate(
                (Rule: IsInvalid(this.tppConfiguration.TppManifestFile),
                    Parameter: "TppManifestFile"),

                (Rule: IsInvalid(this.tppConfiguration.TppPickupFolder),
                    Parameter: "TppPickupFolder"),

                (Rule: IsInvalid(this.tppConfiguration.BlobStoragesSettings),
                    Parameter: "BlobStoragesSettings"));
        }

        private void ValidateTppConfigurationIsNotNull()
        {
            if (this.tppConfiguration is null)
            {
                throw new InvalidArgumentTppOrchestrationException(
                    message: "Null configuration TPP orchestration exception, " +
                        "please correct the errors and try again.");
            }
        }

        private static dynamic IsInvalidPath(bool exists, string path) => new
        {
            Condition = exists != true,
            Message = $"Path does not exist: '{path}'"
        };

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is required"
        };

        private static dynamic IsInvalid(List<string> list) => new
        {
            Condition = list == null,
            Message = "Items is required"
        };

        private static dynamic IsInvalid(byte[] data) => new
        {
            Condition = data.Length < 1,
            Message = "File has no data"
        };

        private static dynamic IsInvalid(List<BlobStorageSettings> config) => new
        {
            Condition = config is null,
            Message = "BlobStoragesSettings is required"
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidArgumentTppOrchestrationException = new InvalidArgumentTppOrchestrationException(
                message: "Invalid TPP orchestration argument(s), please correct the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidArgumentTppOrchestrationException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidArgumentTppOrchestrationException.ThrowIfContainsErrors();
        }
    }
}
