// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using ISL.TPP.Core.Models.Foundations.Files.Exceptions;
using Xeptions;

namespace ISL.TPP.Core.Services.Foundations.Files
{
    internal partial class FileService
    {
        private void ValidateCheckIfFileExistsArguments(string path)
        {
            Validate<InvalidArgumentFileException>(
                message: "Invalid file argument(s), please correct the errors and try again.",
                (Rule: IsInvalid(path), Parameter: nameof(path)));
        }

        private void ValidateWriteToFileArguments(string path, string content)
        {
            Validate<InvalidArgumentFileException>(
                message: "Invalid file argument(s), please correct the errors and try again.",
                (Rule: IsInvalid(path), Parameter: nameof(path)),
                (Rule: IsInvalid(content), Parameter: nameof(content)));
        }

        private void ValidateReadFromFileArguments(string path)
        {
            Validate<InvalidArgumentFileException>(
                message: "Invalid file argument(s), please correct the errors and try again.",
                (Rule: IsInvalid(path), Parameter: nameof(path)));
        }

        private void ValidateDeleteFileArguments(string path)
        {
            Validate<InvalidArgumentFileException>(
                message: "Invalid file argument(s), please correct the errors and try again.",
                (Rule: IsInvalid(path), Parameter: nameof(path)));
        }

        private void ValidateRetrieveListOfFilesArguments(string path, string searchPattern)
        {
            Validate<InvalidArgumentFileException>(
                message: "Invalid file argument(s), please correct the errors and try again.",
                (Rule: IsInvalid(path), Parameter: nameof(path)),
                (Rule: IsInvalid(searchPattern), Parameter: nameof(searchPattern)));
        }

        private void ValidateCheckIfDirectoryExistsArguments(string path)
        {
            Validate<InvalidArgumentFileException>(
                message: "Invalid file argument(s), please correct the errors and try again.",
                (Rule: IsInvalid(path), Parameter: nameof(path)));
        }

        private void ValidateCreateDirectoryArguments(string path)
        {
            Validate<InvalidArgumentFileException>(
                message: "Invalid file argument(s), please correct the errors and try again.",
                (Rule: IsInvalid(path), Parameter: nameof(path)));
        }

        private void ValidateDeleteDirectoryArguments(string path)
        {
            Validate<InvalidArgumentFileException>(
                message: "Invalid file argument(s), please correct the errors and try again.",
                (Rule: IsInvalid(path), Parameter: nameof(path)));
        }

        private static dynamic IsInvalid(string text) => new
        {
            Condition = String.IsNullOrWhiteSpace(text),
            Message = "Text is required"
        };

        private static void Validate<T>(string message, params (dynamic Rule, string Parameter)[] validations)
            where T : Xeption
        {
            var invalidDataException = (T)Activator.CreateInstance(typeof(T), message);

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidDataException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidDataException.ThrowIfContainsErrors();
        }
    }
}
