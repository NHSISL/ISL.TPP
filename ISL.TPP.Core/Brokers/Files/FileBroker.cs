// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ISL.TPP.Core.Brokers.Files
{
    internal class FileBroker : IFileBroker
    {
        public async ValueTask<bool> CheckIfFileExistsAsync(string path) =>
            File.Exists(path);

        public async ValueTask<bool> WriteToFileAsync(string path, string content)
        {
            File.WriteAllText(path, content);

            return true;
        }

        public async ValueTask<byte[]> ReadFileAsync(string path) =>
            await File.ReadAllBytesAsync(path);

        public async ValueTask ReadFromFileAsync(Stream output, string path)
        {
            await using var fileStream = File.OpenRead(path);
            await fileStream.CopyToAsync(output);
        }

        public async ValueTask<bool> DeleteFileAsync(string path)
        {
            File.Delete(path);

            return true;
        }

        public async ValueTask<bool> CopyFileAsync(string sourcePath, string destinationPath)
        {
            File.Copy(sourcePath, destinationPath, overwrite: true);

            return true;
        }

        public async ValueTask<bool> MoveFileAsync(string sourcePath, string destinationPath)
        {
            File.Move(sourcePath, destinationPath, overwrite: true);

            return true;
        }

        public async ValueTask<List<string>> GetListOfFilesAsync(
            string path,
            string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            Directory.GetFiles(path, searchPattern, searchOption).ToList();

        public async ValueTask<List<string>> GetListOfSubFoldersAsync(
            string path,
            string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            Directory.GetDirectories(path, searchPattern, searchOption).ToList();

        public async ValueTask<bool> CheckIfDirectoryExistsAsync(string path) =>
            Directory.Exists(path);

        public async ValueTask<bool> CreateDirectoryAsync(string path)
        {
            Directory.CreateDirectory(path);

            return true;
        }

        public async ValueTask<bool> DeleteDirectoryAsync(string path, bool recursive = false)
        {
            Directory.Delete(path, recursive);

            return true;
        }

        public async ValueTask<string> GetDirectoryAsync(string path)
        {
            FileInfo fileInfo = new FileInfo(path);

            return fileInfo.DirectoryName;
        }

        public async ValueTask<string> GetTempFileNameAsync() =>
            Path.GetTempFileName();
    }
}
