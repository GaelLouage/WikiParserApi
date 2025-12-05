using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Models
{
    public class WikiEntity
    {
        public string Topic { get; set; }
        public string Title { get; set; }
        public List<string> Body { get; set; }
        public List<string> ImageUrl { get; set; }
    }
}
