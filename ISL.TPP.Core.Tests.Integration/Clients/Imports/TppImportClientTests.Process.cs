// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Clients;
using Xunit;

namespace ISL.TPP.Core.Tests.Integration.Clients.Imports
{
    public partial class TppImportClientTests
    {
        //[ReleaseCandidateFact]
        [Fact]
        public async Task ShouldProcessNewFilesIfManifestFilePresentAsync()
        {
            // given
            string randomString = GetRandomString();

            string manifestFile = Path.Combine(
                this.tppConfiguration.TppPickupFolder,
                this.tppConfiguration.TppManifestFile);

            string randomFile = Path.Combine(
                this.tppConfiguration.TppPickupFolder,
                $"{GetRandomString(1)}.txt");

            foreach (string file in Directory.EnumerateFiles(this.tppConfiguration.TppPickupFolder))
            {
                File.Delete(file);
                Console.WriteLine($"File '{file}' deleted.");
            }

            File.WriteAllText(manifestFile, randomString);
            File.WriteAllText(randomFile, randomString);

            TppClient client = new TppClient(tppConfiguration);

            // when
            List<string> actualFiles = await client.Imports.ProcessFilesAsync();

            // then
            actualFiles.Count.Should().BeGreaterThan(0);
        }
    }
}
