using Infra.Enums;

namespace Infra.Models
{
    public class WikiRequestEntity
    {
        public string Topic { get; set; }
        public LanguageType Language { get; set; } = LanguageType.English;
    }
}
