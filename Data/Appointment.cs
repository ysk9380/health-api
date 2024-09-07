using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Models
{
    [Table("appointment")]
    public class Appointment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AppointmentId { get; set; }

        [Required]
        public long CustomerId { get; set; }

        [Required]
        public long AppUserId { get; set; }

        [Required]
        public DateTime AppointmentTimestamp { get; set; }

        [Required]
        public long PatientId { get; set; }

        [MaxLength(45)]
        public string? CaseNumberId { get; set; }

        [MaxLength(45)]
        public string? AppointmentStartTime { get; set; }

        [MaxLength(45)]
        public string? AppointmentEndTime { get; set; }

        [Required]
        public byte AppointmentStatusId { get; set; }
    }
}