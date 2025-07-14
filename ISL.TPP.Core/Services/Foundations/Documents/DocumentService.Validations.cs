// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.IO;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Foundations.Documents.Exceptions;

namespace ISL.TPP.Core.Services.Foundations.Documents
{
    internal partial class DocumentService
    {
        private static void ValidateDocumentOnAdd(
            Stream input,
            string fileName,
            BlobStorageSettings blobStorageSettings)
        {
            Validate(
                (Rule: IsInvalidInputStream(input), Parameter: "Input"),
                (Rule: IsInvalid(fileName), Parameter: "FileName"),
                (Rule: IsInvalid(blobStorageSettings), Parameter: nameof(BlobStorageSettings)));

            Validate(
                (Rule: IsInvalid(blobStorageSettings.AzureBlobServiceUri),
                    Parameter: nameof(BlobStorageSettings.AzureBlobServiceUri)),

                (Rule: IsInvalid(blobStorageSettings.AzureTenantId),
                    Parameter: nameof(BlobStorageSettings.AzureTenantId)),

                (Rule: IsInvalid(blobStorageSettings.AzureBlobContainer),
                    Parameter: nameof(BlobStorageSettings.AzureBlobContainer)));
        }

        private static void ValidateArgumentsOnRetrieve(
            Stream output,
            string fileName,
            BlobStorageSettings blobStorageSettings)
        {
            Validate(
                (Rule: IsInvalidOutputStream(output), Parameter: "Output"),
                (Rule: IsInvalid(blobStorageSettings), Parameter: nameof(BlobStorageSettings)),
                (Rule: IsInvalid(fileName), Parameter: "FileName"));

            Validate(
                (Rule: IsInvalid(blobStorageSettings.AzureBlobServiceUri),
                    Parameter: nameof(BlobStorageSettings.AzureBlobServiceUri)),

                (Rule: IsInvalid(blobStorageSettings.AzureTenantId),
                    Parameter: nameof(BlobStorageSettings.AzureTenantId)),

                (Rule: IsInvalid(blobStorageSettings.AzureBlobContainer),
                    Parameter: nameof(BlobStorageSettings.AzureBlobContainer)));
        }

        private void ValidateDeleteArguments(string fileName, BlobStorageSettings blobStorageSettings)
        {
            Validate(
               (Rule: IsInvalid(blobStorageSettings), Parameter: nameof(BlobStorageSettings)),
               (Rule: IsInvalid(fileName), Parameter: "FileName"));

            Validate(
                (Rule: IsInvalid(blobStorageSettings.AzureBlobServiceUri),
                    Parameter: nameof(BlobStorageSettings.AzureBlobServiceUri)),

                (Rule: IsInvalid(blobStorageSettings.AzureTenantId),
                    Parameter: nameof(BlobStorageSettings.AzureTenantId)),

                (Rule: IsInvalid(blobStorageSettings.AzureBlobContainer),
                    Parameter: nameof(BlobStorageSettings.AzureBlobContainer)));
        }

        private void ValidateGetDownloadLinkArguments(string fileName, BlobStorageSettings blobStorageSettings)
        {
            Validate(
               (Rule: IsInvalid(blobStorageSettings), Parameter: nameof(BlobStorageSettings)),
               (Rule: IsInvalid(fileName), Parameter: "FileName"));

            Validate(
                (Rule: IsInvalid(blobStorageSettings.AzureBlobServiceUri),
                    Parameter: nameof(BlobStorageSettings.AzureBlobServiceUri)),

                (Rule: IsInvalid(blobStorageSettings.AzureTenantId),
                    Parameter: nameof(BlobStorageSettings.AzureTenantId)),

                (Rule: IsInvalid(blobStorageSettings.AzureBlobContainer),
                    Parameter: nameof(BlobStorageSettings.AzureBlobContainer)));
        }

        private static void ValidateStorageDocument(
            Stream data,
            string fileName)
        {
            if (data is null || data.Length == 0)
            {
                throw new NotFoundDocumentException(message: $"Couldn't find documents with fileName: {fileName}.");
            }
        }

        private static dynamic IsInvalidInputStream(Stream? stream) => new
        {
            Condition = stream is null || stream.Length == 0,
            Message = "Stream is required"
        };

        private static dynamic IsInvalidOutputStream(Stream? stream) => new
        {
            Condition = stream is null || stream.Length > 0,
            Message = "Stream is required"
        };

        private static dynamic IsInvalid(string? text) => new
        {
            Condition = String.IsNullOrWhiteSpace(text),
            Message = "Text is required"
        };

        private static dynamic IsInvalid(BlobStorageSettings blobStorageSettings) => new
        {
            Condition = blobStorageSettings is null,
            Message = "Settings is required"
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidDocumentException = new InvalidDocumentException(
                message: "Invalid document. Please correct the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidDocumentException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidDocumentException.ThrowIfContainsErrors();
        }
    }
}
