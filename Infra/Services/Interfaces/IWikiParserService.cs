using Infra.Dtos;
using Infra.Enums;
using Infra.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infra.Services.Interfaces
{
    public interface IWikiParserService
    {
        Task<(WikiEntity, List<string> Errors)> ExtractPageAsync(
            string topic,
             LanguageType language =
            LanguageType.English,
            bool fullContent = false,
            int paragraphCount = 1
           );
    }
}
