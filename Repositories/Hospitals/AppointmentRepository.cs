using Health.Api.Data;
using Health.Api.Models.Hospitals;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Hospitals;

public interface IAppointmentRepository
{
    public Task<IList<AppointmentDetail>> GetAppointmentDetailsByDateAsync(long customerId, DateTime appointmentDate);
    public Task<IList<AppointmentDetail>> GetAppointmentDetailssAsync(long customerId, long appUserId);
}

public class AppointmentRepository : IAppointmentRepository
{
    private HASDbContext _Context;

    public AppointmentRepository(HASDbContext context)
    {
        _Context = context;
    }

    public async Task<IList<AppointmentDetail>> GetAppointmentDetailsByDateAsync(long customerId, DateTime appointmentDate)
    {
        var query = from appointment in _Context.Appointments
                    join customer in _Context.Customers on appointment.CustomerId equals customer.CustomerId
                    join appUser in _Context.AppUsers on appointment.AppUserId equals appUser.AppUserId
                    join patient in _Context.Patients on appointment.PatientId equals patient.PatientId
                    join status in _Context.AppointmentStatuses on appointment.AppointmentStatusId equals status.AppointmentStatusId
                    where appointment.CustomerId == customerId && appointment.AppointmentTimestamp.Date.Equals(appointmentDate.Date)
                    select new AppointmentDetail
                    {
                        AppointmentId = appointment.AppointmentId,
                        CustomerId = appointment.CustomerId,
                        AppUserId = appointment.AppUserId,
                        AppUserFirstName = appUser.Firstname,
                        AppUserLastName = appUser.Lastname,
                        PatientId = appointment.PatientId,
                        PatientFirstName = patient.Firstname,
                        PatientLastName = patient.Lastname,
                        CaseNumberId = null,
                        AppointmentTimestamp = appointment.AppointmentTimestamp,
                        AppointmentStartTime = appointment.AppointmentStartTime,
                        AppointmentEndTime = appointment.AppointmentEndTime,
                        AppointmentStatusId = appointment.AppointmentStatusId,
                        AppointmentStatusCode = status.AppointmentStatusCode,
                        AppointmentStatusName = status.AppointmentStatusName
                    };

        return await query.ToListAsync();
    }

    public async Task<IList<AppointmentDetail>> GetAppointmentDetailssAsync(long customerId, long appUserId)
    {
        var query = from appointment in _Context.Appointments
                    join customer in _Context.Customers on appointment.CustomerId equals customer.CustomerId
                    join appUser in _Context.AppUsers on appointment.AppUserId equals appUser.AppUserId
                    join patient in _Context.Patients on appointment.PatientId equals patient.PatientId
                    join status in _Context.AppointmentStatuses on appointment.AppointmentStatusId equals status.AppointmentStatusId
                    where appointment.CustomerId == customerId && appointment.AppUserId.Equals(appUserId)
                    select new AppointmentDetail
                    {
                        AppointmentId = appointment.AppointmentId,
                        CustomerId = appointment.CustomerId,
                        AppUserId = appointment.AppUserId,
                        AppUserFirstName = appUser.Firstname,
                        AppUserLastName = appUser.Lastname,
                        PatientId = appointment.PatientId,
                        PatientFirstName = patient.Firstname,
                        PatientLastName = patient.Lastname,
                        CaseNumberId = null,
                        AppointmentTimestamp = appointment.AppointmentTimestamp,
                        AppointmentStartTime = appointment.AppointmentStartTime,
                        AppointmentEndTime = appointment.AppointmentEndTime,
                        AppointmentStatusId = appointment.AppointmentStatusId,
                        AppointmentStatusCode = status.AppointmentStatusCode,
                        AppointmentStatusName = status.AppointmentStatusName
                    };

        return await query.ToListAsync();
    }
}