using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Health.Api.Data;

[Table("healthvital")]
public class HealthVital
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int HealthVitalId { get; set; }

    [Required]
    [MaxLength(50)]
    public string HealthVitalCode { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string HealthVitalName { get; set; } = default!;

    [MaxLength(50)]
    public string HealthVitalUnit { get; set; } = default!;
}