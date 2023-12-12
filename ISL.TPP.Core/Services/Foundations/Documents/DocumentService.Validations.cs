// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using ISL.TPP.Core.Models.Foundations.Documents;
using ISL.TPP.Core.Models.Foundations.Documents.Exceptions;

namespace ISL.TPP.Core.Services.Foundations.Documents
{
    public partial class DocumentService
    {
        private static void ValidateDocumentOnAdd(Document document, string container)
        {
            ValidateDocumentIsNotNull(document);

            Validate(
                (Rule: IsInvalid(container), Parameter: "Container"),
                (Rule: IsInvalid(document.DocumentData), Parameter: nameof(Document.DocumentData)),
                (Rule: IsInvalid(document.FileName), Parameter: nameof(Document.FileName)));
        }

        private static void ValidateDocumentOnRetrieve(string fileName, string container)
        {
            Validate(
                (Rule: IsInvalid(container), Parameter: "Container"),
                (Rule: IsInvalid(fileName), Parameter: "FileName"));
        }

        private void ValidateDeleteArguments(string fileName, string container)
        {
            Validate(
               (Rule: IsInvalid(container), Parameter: "Container"),
               (Rule: IsInvalid(fileName), Parameter: "FileName"));
        }

        private void ValidateGetDownloadLinkArguments(string fileName, string container)
        {
            Validate(
               (Rule: IsInvalid(container), Parameter: "Container"),
               (Rule: IsInvalid(fileName), Parameter: "FileName"));
        }

        private static void ValidateDocumentIsNotNull(Document document)
        {
            if (document is null)
            {
                throw new NullDocumentException(message: "Document is Null");
            }
        }

        private static void ValidateStorageDocument(
            byte[] maybeRetrievedDocument,
            string fileName)
        {
            if (maybeRetrievedDocument is null)
            {
                throw new NotFoundDocumentException(message: $"Couldn't find documents with fileName: {fileName}.");
            }
        }

        private static dynamic IsInvalid(byte[] data) => new
        {
            Condition = (data == null || data.Length == 0),
            Message = "Data is required"
        };

        private static dynamic IsInvalid(string text) => new
        {
            Condition = String.IsNullOrWhiteSpace(text),
            Message = "Text is required"
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
