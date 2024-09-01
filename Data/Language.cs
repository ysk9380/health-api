using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("Language")]
public class Language
{
    public byte LanguageId { get; set; }
    public string LanguageCode { get; set; } = default!;
    public string LanguageName { get; set; } = default!;
}