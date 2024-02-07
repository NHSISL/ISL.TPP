// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace ISL.TPP.Core.Models.Brokers.CsvMappers
{
    public class CsvMapperBroker : ICsvMapperBroker
    {
        public async ValueTask<List<T>> MapCsvToObjectAsync<T>(string data, bool hasHeaderRecord)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = hasHeaderRecord,
                MissingFieldFound = null
            };

            using (var reader = new StringReader(data))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<T>().ToList();
                return await ValueTask.FromResult(records);
            }
        }

        public async ValueTask<string> MapObjectToCsvAsync<T>(
            List<T> @object,
            bool addHeaderRecord,
            bool shouldAddTrailingComma)
        {
            var csvWriterConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = addHeaderRecord,
                NewLine = Environment.NewLine
            };

            using (var stringWriter = new StringWriter())
            using (var csvWriter = new CsvWriter(stringWriter, csvWriterConfig))
            {
                if (addHeaderRecord)
                {
                    csvWriter.WriteHeader<T>();
                    csvWriter.NextRecord();
                }

                foreach (var item in @object)
                {
                    csvWriter.WriteRecord(item);
                    if (shouldAddTrailingComma) { csvWriter.WriteField(""); }
                    csvWriter.NextRecord();
                }

                var csv = stringWriter.ToString();

                return await ValueTask.FromResult(csv);
            }
        }
    }
}
