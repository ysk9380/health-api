using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

public class PatientHealthVital
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PatientHealthVitalId { get; set; }

    [Required]
    public long PatientId { get; set; }

    [Required]
    public int HealthVitalId { get; set; }

    [MaxLength(1000)]
    public string? Content { get; set; }
}