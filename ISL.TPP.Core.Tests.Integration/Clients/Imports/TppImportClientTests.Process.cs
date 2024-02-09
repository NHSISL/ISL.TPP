// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.TPP.Core.Clients;
using ISL.TPP.Core.Tests.Integrations;

namespace ISL.TPP.Core.Tests.Integration.Clients.Imports
{
    public partial class TppImportClientTests
    {
        [ReleaseCandidateFact]
        public async Task ShouldProcessNewFilesIfManifestFilePresentAsync()
        {
            // given
            string randomFileName = $"{GetRandomString()}.txt";
            string manifestToDate = DateTime.Now.ToString("yyyyMMdd_HHmm");
            StringBuilder csvManifest = new StringBuilder();
            csvManifest.AppendLine("FileName,IsDelta,IsReference,DateExtractFrom,DateExtractTo");
            csvManifest.AppendLine($"{randomFileName},Y,N,20231209_2245,{manifestToDate}");

            foreach (string reportingGroup in this.tppConfiguration.ReportingGroups)
            {
                string reportingGroupFolder = Path.Combine(this.tppConfiguration.TppPickupFolder, reportingGroup);
                string manifestFile = Path.Combine(reportingGroupFolder, this.tppConfiguration.TppManifestFile);
                string randomFile = Path.Combine(reportingGroupFolder, $"{GetRandomString(1)}.txt");

                if (!Directory.Exists(reportingGroupFolder))
                {
                    Directory.CreateDirectory(reportingGroupFolder);
                }

                foreach (string file in Directory.EnumerateFiles(reportingGroupFolder))
                {
                    File.Delete(file);
                    Console.WriteLine($"File '{file}' deleted.");
                }

                File.WriteAllText(manifestFile, csvManifest.ToString());
                File.WriteAllText(randomFile, randomFileName);
            }

            TppClient client = new TppClient(tppConfiguration);

            // when
            List<string> actualFiles = await client.Imports.ProcessFilesAsync();

            // then
            actualFiles.Count.Should().Be(2);
        }
    }
}
