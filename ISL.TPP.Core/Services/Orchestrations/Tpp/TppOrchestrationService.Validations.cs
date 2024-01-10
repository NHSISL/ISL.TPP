// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions;

namespace ISL.TPP.Core.Services.Orchestrations.Tpp
{
    internal partial class TppOrchestrationService : ITppOrchestrationService
    {
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

                (Rule: IsInvalid(this.tppConfiguration.BlobStorageSettings),
                    Parameter: "BlobStorageSettings.AzureBlobContainer"));
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

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is required"
        };

        private static dynamic IsInvalid(byte[] data) => new
        {
            Condition = data.Length < 1,
            Message = "File has no data"
        };

        private static dynamic IsInvalid(BlobStorageSettings? config) => new
        {
            Condition = config is null || string.IsNullOrWhiteSpace(config.AzureBlobContainer),
            Message = "BlobStorageSettings.AzureBlobContainer is required"
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
