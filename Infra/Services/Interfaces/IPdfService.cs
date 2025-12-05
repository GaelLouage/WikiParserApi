using Infra.Dtos;
using Infra.Models;

namespace Infra.Services.Interfaces
{
    public interface IPdfService
    {
        Task<WikiDto?> GeneratePdfFromWikiEntityAsync(WikiEntity wikiEntity);
    }
}