// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Clients.Exceptions;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Models.Configurations.Retries;

namespace ISL.TPP.Core.Clients
{
    public partial class TppClient
    {
        private static void ValidateTppConfiguration(
            TppConfiguration tppConfiguration)
        {
            if (tppConfiguration == null)
            {
                throw new TppClientValidationException(
                    "TPP client validation error(s) occurred, fix the error(s) and try again: " +
                        "Configuration not defined.");
            }

            Validate(
                (Rule: IsInvalid(tppConfiguration.TppPickupFolder),
                    Parameter: "TppPickupFolder"),

                (Rule: IsInvalid(tppConfiguration.TppManifestFile),
                    Parameter: "TppManifestFile"),

                (Rule: IsInvalid(tppConfiguration.BlobStoragesSettings),
                    Parameter: "BlobStorageSettings"),

                (Rule: IsInvalid(tppConfiguration.RetryConfig),
                    Parameter: "RetryConfig"));

            foreach (BlobStorageSettings blobStorageSettings in tppConfiguration.BlobStoragesSettings)
            {
                Validate(
                (Rule: IsInvalid(blobStorageSettings.AzureBlobServiceUri),
                    Parameter: $"BlobStorageSettings.{blobStorageSettings.Name}.AzureBlobServiceUri"),

                (Rule: IsInvalid(blobStorageSettings.AzureTenantId),
                    Parameter: $"BlobStorageSettings.{blobStorageSettings.Name}.AzureTenantId"),

                (Rule: IsInvalid(blobStorageSettings.AzureClientId),
                    Parameter: $"BlobStorageSettings.{blobStorageSettings.Name}.AzureTenantId"),

                (Rule: IsInvalid(blobStorageSettings.AzureClientSecret),
                    Parameter: $"BlobStorageSettings.{blobStorageSettings.Name}.AzureTenantId"),

                (Rule: IsInvalid(blobStorageSettings.AzureBlobContainer),
                    Parameter: $"BlobStorageSettings.{blobStorageSettings.Name}.Container"));
            }
        }

        private static dynamic IsInvalid(string? text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Configuration value does not exist or is invalid."
        };

        private static dynamic IsInvalid(List<BlobStorageSettings> value) => new
        {
            Condition = value is null,
            Message = "Configuration not defined."
        };

        private static dynamic IsInvalid(RetryConfig? value) => new
        {
            Condition = value is null,
            Message = "Configuration not defined."
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            StringBuilder validationErrors = new StringBuilder();
            validationErrors.AppendLine("Configuration error(s):");
            IDictionary errors = new Dictionary<string, List<string>>();

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    validationErrors.AppendLine(
                        $"{parameter} -> Configuration value does not exist or does not meet validation criteria");

                    if (errors.Contains(parameter))
                    {
                        (errors[parameter] as List<string>)?.Add(rule.Message);
                        return;
                    }

                    errors.Add(parameter, new List<string> { rule.Message });
                }
            }

            var invalidConfigurationException = new TppClientValidationException(
                message: $"TPP client validation error(s) occurred, fix the error(s) and try again:"
                    + Environment.NewLine + validationErrors.ToString(),
                innerException: null!,
                data: errors);

            invalidConfigurationException.ThrowIfContainsErrors();
        }
    }
}
