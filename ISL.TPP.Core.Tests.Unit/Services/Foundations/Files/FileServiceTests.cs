// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using ISL.TPP.Core.Brokers.Files;
using ISL.TPP.Core.Models.Configurations.Retries;
using ISL.TPP.Core.Services.Foundations.Files;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;
using Xunit;

namespace ISL.TPP.Core.Tests.Unit.Services.Foundations.Files
{
    public partial class FileServiceTests
    {
        private readonly Mock<IFileBroker> fileBrokerMock;
        private readonly IRetryConfig retryConfig;
        private readonly IFileService fileService;

        public FileServiceTests()
        {
            this.fileBrokerMock = new Mock<IFileBroker>();
            int maxRetryAttempts = 3;
            TimeSpan pauseBetweenFailures = TimeSpan.FromMilliseconds(10);
            this.retryConfig = new RetryConfig(maxRetryAttempts, pauseBetweenFailures);

            this.fileService = new FileService(
                fileBroker: this.fileBrokerMock.Object,
                retryConfig: this.retryConfig);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        public static TheoryData FileServiceDependencyValidationExceptions()
        {
            return new TheoryData<Exception>()
            {
                new ArgumentNullException(),
                new ArgumentOutOfRangeException(),
                new ArgumentException()
            };
        }

        public static TheoryData FileServiceDependencyExceptions()
        {
            return new TheoryData<Exception>()
            {
                new SerializationException(),
                new IOException(),
                new OutOfMemoryException(),
                new UnauthorizedAccessException()
            };
        }

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 5).GetValue();

        private static List<string> GetRandomStringList()
        {
            List<string> stringList = new List<string>();

            for (int i = 0; i < GetRandomNumber(); i++)
            {
                stringList.Add(GetRandomString());
            }

            return stringList;
        }

        private static string GetHash(byte[] bytes)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}