// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Threading.Tasks;
using ISL.TPP.Core.Models.Foundations.Documents;

namespace ISL.TPP.Core.Services.Foundations.Documents
{
    internal interface IDocumentService
    {
        ValueTask AddDocumentAsync(Document document, string container);
        ValueTask<Document> RetrieveDocumentByFileNameAsync(string fileName, string container);
        ValueTask RemoveDocumentByFileNameAsync(string filename, string container);
        ValueTask<string> GetDownloadLinkAsync(string fileName, string container);
    }
}
