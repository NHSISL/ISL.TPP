// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using ISL.TPP.Core.Models.Foundations.CsvMappers.Exceptions;
using Xeptions;

namespace ISL.TPP.Core.Services.Foundations.CsvMappers
{
    internal partial class CsvMapperService
    {
        private static void ValidateMapCsvToObjectArguments(string data, bool hasHeaderRecord)
        {
            Validate<InvalidCsvMapperArgumentsException>(
                    message: "Invalid CSV mapper arguments. Please fix the errors and try again.",
                    (Rule: IsInvalid(data), Parameter: "Data"));
        }

        private static void ValidateMapObjectToCsvArguments<T>(T @object, bool addHeaderRecord)
        {
            Validate<InvalidCsvMapperArgumentsException>(
                    message: "Invalid CSV mapper arguments. Please fix the errors and try again.",
                    (Rule: IsInvalid(@object), Parameter: "Object"));
        }

        private static dynamic IsInvalid(string text) => new
        {
            Condition = String.IsNullOrWhiteSpace(text),
            Message = "Text is required"
        };

        private static dynamic IsInvalid(object @object) => new
        {
            Condition = @object is null,
            Message = "Object is required"
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
