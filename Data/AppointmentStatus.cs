using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Models
{
    [Table("appointmentstatus")]
    public class AppointmentStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte AppointmentStatusId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AppointmentStatusCode { get; set; } = default!;

        [Required]
        [MaxLength(100)]
        public string AppointmentStatusName { get; set; } = default!;
    }
}