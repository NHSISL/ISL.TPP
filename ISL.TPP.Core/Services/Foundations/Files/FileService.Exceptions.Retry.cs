// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ISL.TPP.Core.Services.Foundations.Files
{
    internal partial class FileService
    {
        private readonly List<Type> retryExceptionTypes =
            new List<Type>()
            {
                typeof(IOException)
            };

        private async ValueTask<bool> WithRetry(ReturningBooleanFunction returningBooleanFunction)
        {
            var attempts = 0;

            while (true)
            {
                try
                {
                    attempts++;
                    return await returningBooleanFunction();
                }
                catch (Exception ex)
                {
                    if (retryExceptionTypes.Any(exception => exception == ex.GetType()))
                    {
                        if (attempts == this.retryConfig.MaxRetryAttempts)
                        {
                            throw;
                        }

                        Task.Delay(this.retryConfig.PauseBetweenFailures).Wait();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private async ValueTask<string> WithRetry(ReturningStringFunction returningStringFunction)
        {
            var attempts = 0;

            while (true)
            {
                try
                {
                    attempts++;
                    return await returningStringFunction();
                }
                catch (Exception ex)
                {
                    if (retryExceptionTypes.Any(exception => exception == ex.GetType()))
                    {
                        if (attempts == this.retryConfig.MaxRetryAttempts)
                        {
                            throw;
                        }

                        Task.Delay(this.retryConfig.PauseBetweenFailures).Wait();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private async ValueTask<byte[]> WithRetry(ReturningByteArrayFunction returningByteArrayFunction)
        {
            var attempts = 0;

            while (true)
            {
                try
                {
                    attempts++;
                    return await returningByteArrayFunction();
                }
                catch (Exception ex)
                {
                    if (retryExceptionTypes.Any(exception => exception == ex.GetType()))
                    {
                        if (attempts == this.retryConfig.MaxRetryAttempts)
                        {
                            throw;
                        }

                        Task.Delay(this.retryConfig.PauseBetweenFailures).Wait();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private async ValueTask<List<string>> WithRetry(ReturningStringListFunction returningStringListFunction)
        {
            var attempts = 0;

            while (true)
            {
                try
                {
                    attempts++;
                    return await returningStringListFunction();
                }
                catch (Exception ex)
                {
                    if (retryExceptionTypes.Any(exception => exception == ex.GetType()))
                    {
                        if (attempts == this.retryConfig.MaxRetryAttempts)
                        {
                            throw;
                        }

                        Task.Delay(this.retryConfig.PauseBetweenFailures).Wait();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
