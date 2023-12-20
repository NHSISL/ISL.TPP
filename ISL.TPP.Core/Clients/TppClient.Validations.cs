// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Clients.Exceptions;
using ISL.TPP.Core.Models.Orchestrations.TPP;

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

                (Rule: IsInvalid(tppConfiguration.BlobStorageSettings),
                    Parameter: "BlobStorageSettings"));

            Validate(
                (Rule: IsInvalid(tppConfiguration.BlobStorageSettings.AzureBlobServiceUri),
                    Parameter: "BlobStorageSettings.AzureBlobServiceUri"),

                (Rule: IsInvalid(tppConfiguration.BlobStorageSettings.AzureTenantId),
                    Parameter: "BlobStorageSettings.AzureTenantId"),

                (Rule: IsInvalid(tppConfiguration.BlobStorageSettings.AzureClientId),
                    Parameter: "BlobStorageSettings.AzureTenantId"),

                (Rule: IsInvalid(tppConfiguration.BlobStorageSettings.AzureClientSecret),
                    Parameter: "BlobStorageSettings.AzureTenantId"),

                (Rule: IsInvalid(tppConfiguration.BlobStorageSettings.AzureBlobContainer),
                    Parameter: "BlobStorageSettings.Container"));
        }

        private static dynamic IsInvalid(string? text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Configuration value does not exist or is invalid."
        };

        private static dynamic IsInvalid(BlobStorageSettings? value) => new
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
