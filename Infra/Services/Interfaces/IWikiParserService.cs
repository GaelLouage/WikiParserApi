using Infra.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Services.Interfaces
{
    public interface IWikiParserService
    {
        Task<WikiEntity> ExtractFirstParagraphAsync(string topic);
    }
}
