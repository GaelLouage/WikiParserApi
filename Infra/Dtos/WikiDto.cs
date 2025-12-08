using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Dtos
{
    public class WikiDto
    {
        public List<string> Errors { get; set; } = new List<string>();
        public string? PdfByte64String { get; set; } 
    }

}
