// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
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
            TppClient client = new TppClient(tppConfiguration);

            // when
            List<string> actualFiles = await client.Imports.ProcessFilesAsync();

            // then
            actualFiles.Count.Should().BeGreaterThan(0);
        }
    }
}
