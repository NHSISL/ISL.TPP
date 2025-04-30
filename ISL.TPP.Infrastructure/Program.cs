// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using ISL.TPP.Infrastructure.Services;

namespace ISL.TPP.Infrastructure
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var scriptGenerationService = new ScriptGenerationService();

            scriptGenerationService.GenerateBuildScript(
                branchName: "main",
                projectName: "ISL.TPP.Core",
                dotNetVersion: "9.0.100");

            scriptGenerationService.GeneratePrLintScript("main");
        }
    }
}
