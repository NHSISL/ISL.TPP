// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ISL.TPP.Core.Brokers.Files;
using ISL.TPP.Core.Models.Configurations.Retries;

namespace ISL.TPP.Core.Services.Foundations.Files
{
    internal partial class FileService : IFileService
    {
        private readonly IFileBroker fileBroker;
        private readonly IRetryConfig retryConfig;

        public FileService(IFileBroker fileBroker, IRetryConfig retryConfig)
        {
            this.fileBroker = fileBroker;
            this.retryConfig = retryConfig;
        }

        public ValueTask<bool> CheckIfFileExistsAsync(string path) =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateCheckIfFileExistsArguments(path);

                    return await this.fileBroker.CheckIfFileExistsAsync(path);
                });
            });

        public ValueTask<bool> WriteToFileAsync(string path, string content) =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateWriteToFileArguments(path, content);

                    return await this.fileBroker.WriteToFileAsync(path, content);
                });
            });

        public ValueTask<byte[]> ReadFromFileAsync(string path) =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateReadFromFileArguments(path);

                    return await this.fileBroker.ReadFileAsync(path);
                });
            });

        public ValueTask<bool> DeleteFileAsync(string path) =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateDeleteFileArguments(path);
                    return await this.fileBroker.DeleteFileAsync(path);
                });
            });

        public ValueTask<bool> MoveFileAsync(string sourcePath, string destinationPath) =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateMoveFileArguments(sourcePath, destinationPath);

                    string destinationFolder = await this.fileBroker.GetDirectoryAsync(destinationPath);
                    bool destinationFolderExists = await this.fileBroker.CheckIfDirectoryExistsAsync(destinationFolder);

                    if (!destinationFolderExists)
                    {
                        await this.fileBroker.CreateDirectoryAsync(destinationFolder);
                    }

                    if (await this.fileBroker.CheckIfFileExistsAsync(destinationPath))
                    {
                        await this.fileBroker.DeleteFileAsync(destinationPath);
                    }

                    return await this.fileBroker.MoveFileAsync(sourcePath, destinationPath);
                });
            });

        public ValueTask<List<string>> RetrieveListOfFilesAsync(string path, string searchPattern = "*") =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateRetrieveListOfFilesArguments(path, searchPattern);
                    return await this.fileBroker.GetListOfFilesAsync(path, searchPattern);
                });
            });

        public ValueTask<List<string>> RetrieveListOfSubFoldersAsync(string path, string searchPattern = "*") =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateRetrieveListOfFilesArguments(path, searchPattern);
                    return await this.fileBroker.GetListOfSubFoldersAsync(path, searchPattern) ?? new List<string>();
                });
            });

        public ValueTask<bool> CheckIfDirectoryExistsAsync(string path) =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateCheckIfDirectoryExistsArguments(path);

                    return await this.fileBroker.CheckIfDirectoryExistsAsync(path);
                });
            });

        public ValueTask<bool> CreateDirectoryAsync(string path) =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateCreateDirectoryArguments(path);

                    return await this.fileBroker.CreateDirectoryAsync(path);
                });
            });

        public ValueTask<bool> DeleteDirectoryAsync(string path, bool recursive = false) =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateDeleteDirectoryArguments(path);

                    return await this.fileBroker.DeleteDirectoryAsync(path, recursive);
                });
            });

        public ValueTask<string> ComputeSHA256Hash(string path) =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateReadFromFileArguments(path);

                    var bytes = await this.fileBroker.ReadFileAsync(path);

                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] hashBytes = sha256.ComputeHash(bytes);
                        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    }
                });
            });
    }
}
