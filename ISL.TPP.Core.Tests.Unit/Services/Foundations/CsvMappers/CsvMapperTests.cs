// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Models;
using ISL.TPP.Core.Models.Brokers.CsvMappers;
using ISL.TPP.Core.Services.Foundations.CsvMappers;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.CsvMappers
{
    public partial class CsvMapperTests
    {
        private readonly Mock<ICsvMapperBroker> csvMapperBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly CsvMapperService csvMapperService;

        public CsvMapperTests()
        {
            this.csvMapperBrokerMock = new Mock<ICsvMapperBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.csvMapperService = new CsvMapperService(
                csvMapperBroker: this.csvMapperBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static List<Manifest> CreateRandomManifests()
        {
            return CreateManifestFiller(dateTimeOffset: GetRandomDateTimeOffset())
                .Create(count: GetRandomNumber())
                    .ToList();
        }

        private static Filler<Manifest> CreateManifestFiller(DateTimeOffset dateTimeOffset)
        {
            DateTime fromDate = dateTimeOffset.UtcDateTime.AddDays(-1);
            DateTime toDate = dateTimeOffset.UtcDateTime;

            var filler = new Filler<Manifest>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(manifest => manifest.FileName).Use(() => $"{GetRandomString()}.csv")
                .OnProperty(manifest => manifest.IsDelta).Use("Y")
                .OnProperty(manifest => manifest.IsReference).Use("N")
                .OnProperty(manifest => manifest.DateExtractFrom).Use($"{fromDate:yyyyMMdd}_{fromDate:HHmm}")
                .OnProperty(manifest => manifest.DateExtractTo).Use($"{toDate:yyyyMMdd}_{toDate:HHmm}");

            return filler;
        }
    }
}
