namespace Health.Api.Models.Hospitals
{
    public class AppointmentDetail
    {
        public long AppointmentId { get; set; }
        public long CustomerId { get; set; }
        public long AppUserId { get; set; }
        public string AppUserFirstName { get; set; } = default!;
        public string AppUserLastName { get; set; } = default!;
        public long PatientId { get; set; }
        public string PatientFirstName { get; set; } = default!;
        public string PatientLastName { get; set; } = default!;
        public long? CaseNumberId { get; set; }
        public DateTime AppointmentTimestamp { get; set; }
        public DateTime? AppointmentStartTime { get; set; }
        public DateTime? AppointmentEndTime { get; set; }
        public byte AppointmentStatusId { get; set; }
        public string AppointmentStatusCode { get; set; } = default!;
        public string AppointmentStatusName { get; set; } = default!;
    }
}