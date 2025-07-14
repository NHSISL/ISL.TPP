// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
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

        public ValueTask ReadFromFileAsync(Stream output, string path) =>
            TryCatch(async () =>
            {
                await WithRetry(async () =>
                {
                    ValidateReadFromFileArguments(path);
                    await this.fileBroker.ReadFromFileAsync(output, path);
                });
            });

        public ValueTask<Stream> OpenReadStreamAsync(string path) =>
        TryCatch(async () =>
        {
            return await WithRetry(async () =>
            {
                ValidateReadFromFileArguments(path);
                Stream stream = await this.fileBroker.OpenReadStreamAsync(path);

                return stream;
            });
        });

        public ValueTask WriteToFileAsync(Stream input, string path, bool overwrite = true) =>
            TryCatch(async () =>
            {
                await WithRetry(async () =>
                {
                    ValidateWriteToFileAsyncArguments(input, path);

                    FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;

                    await using FileStream outputStream = new FileStream(
                        path,
                        mode,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize: 81920,
                        useAsync: true);

                    await input.CopyToAsync(outputStream);
                    await outputStream.FlushAsync();
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

        public ValueTask<bool> CopyFileAsync(string sourcePath, string destinationPath) =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateMoveFileArguments(sourcePath, destinationPath);
                    bool sourceFileExists = await this.fileBroker.CheckIfFileExistsAsync(sourcePath);
                    ValidateSourcePath(sourcePath, sourceFileExists);
                    string destinationFolder = await this.fileBroker.GetDirectoryAsync(destinationPath);
                    bool destinationFolderExists = await this.fileBroker.CheckIfDirectoryExistsAsync(destinationFolder);

                    if (!destinationFolderExists)
                    {
                        await this.fileBroker.CreateDirectoryAsync(destinationFolder);
                    }

                    return await this.fileBroker.CopyFileAsync(sourcePath, destinationPath);
                });
            });

        public ValueTask<bool> MoveFileAsync(string sourcePath, string destinationPath) =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateMoveFileArguments(sourcePath, destinationPath);
                    bool sourceFileExists = await this.fileBroker.CheckIfFileExistsAsync(sourcePath);
                    ValidateSourcePath(sourcePath, sourceFileExists);
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

        public ValueTask<bool> MoveDirectoryAsync(string sourceDirectory, string destinationDirectory) =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateMoveFileArguments(sourceDirectory, destinationDirectory);
                    bool sourceFileExists = await this.fileBroker.CheckIfDirectoryExistsAsync(sourceDirectory);
                    ValidateSourcePath(sourceDirectory, sourceFileExists);

                    bool destinationFolderExists =
                        await this.fileBroker.CheckIfDirectoryExistsAsync(destinationDirectory);

                    if (!destinationFolderExists)
                    {
                        await this.fileBroker.CreateDirectoryAsync(destinationDirectory);
                    }

                    List<string> sourceFiles = await this.fileBroker.GetListOfFilesAsync(sourceDirectory);

                    foreach (string file in sourceFiles)
                    {
                        string destFile = Path.Combine(destinationDirectory, Path.GetFileName(file));

                        if (await this.fileBroker.CheckIfFileExistsAsync(destFile))
                        {
                            await this.fileBroker.DeleteFileAsync(destFile);
                        }

                        await this.fileBroker.MoveFileAsync(file, destFile);
                    }

                    List<string> sourceSubDirectories =
                        await this.fileBroker.GetListOfSubFoldersAsync(sourceDirectory);

                    foreach (string sourceDirectory in sourceSubDirectories)
                    {
                        string destinationSubDirectory =
                            Path.Combine(destinationDirectory, Path.GetFileName(sourceDirectory));

                        await MoveDirectoryAsync(sourceDirectory, destinationSubDirectory);
                    }

                    return await this.fileBroker.DeleteDirectoryAsync(sourceDirectory, recursive: true);
                });
            });

        public ValueTask<List<string>> RetrieveListOfFilesAsync(
            string path,
            string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateRetrieveListOfFilesArguments(path, searchPattern);
                    return await this.fileBroker.GetListOfFilesAsync(path, searchPattern, searchOption);
                });
            });

        public ValueTask<List<string>> RetrieveListOfSubFoldersAsync(
            string path,
            string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateRetrieveListOfFilesArguments(path, searchPattern);
                    return await this.fileBroker.GetListOfSubFoldersAsync(path, searchPattern, searchOption) ?? new List<string>();
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

        public ValueTask<string> GetDirectoryAsync(string path) =>
            TryCatch(async () =>
            {
                return await WithRetry(async () =>
                {
                    ValidateReadFromFileArguments(path);

                    return await this.fileBroker.GetDirectoryAsync(path);
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

        public ValueTask<string> GetTempFileNameAsync() =>
            TryCatch(async () =>
            {
                return await this.fileBroker.GetTempFileNameAsync();
            });
    }
}
