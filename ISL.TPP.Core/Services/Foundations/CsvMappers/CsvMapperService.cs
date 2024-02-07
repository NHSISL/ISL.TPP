// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Models.Brokers.CsvMappers;

namespace ISL.TPP.Core.Services.Foundations.CsvMappers
{
    internal partial class CsvMapperService : ICsvMapperService
    {
        private readonly ICsvMapperBroker csvMapperBroker;
        private readonly ILoggingBroker loggingBroker;

        public CsvMapperService(ICsvMapperBroker csvMapperBroker, ILoggingBroker loggingBroker)
        {
            this.csvMapperBroker = csvMapperBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<List<T>> MapCsvToObjectAsync<T>(string data, bool hasHeaderRecord) =>
            TryCatch(async () =>
            {
                ValidateMapCsvToObjectArguments(data, hasHeaderRecord);

                return await this.csvMapperBroker.MapCsvToObjectAsync<T>(data, hasHeaderRecord);
            });

        public ValueTask<string> MapObjectToCsvAsync<T>(
            List<T> @object,
            bool addHeaderRecord,
            bool shouldAddTrailingComma) =>
        TryCatch(async () =>
        {
            ValidateMapObjectToCsvArguments(@object, addHeaderRecord);

            return await this.csvMapperBroker
                .MapObjectToCsvAsync(@object, addHeaderRecord, shouldAddTrailingComma);
        });
    }
}
