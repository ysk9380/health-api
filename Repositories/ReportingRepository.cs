using Health.Api.Data;
using Health.Api.Models.Constants;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories;

public interface IReportingRepository
{
    Task<int> GetNewPatientsCountAsync(long customerId, int periodInDays);
}

public class ReportingRepository : IReportingRepository
{
    private HASDbContext _Context;

    public ReportingRepository(HASDbContext context)
    {
        _Context = context;
    }

    public async Task<int> GetNewPatientsCountAsync(long customerId, int periodInDays)
    {
        const string PatientTableName = "patient";
        const string PatientColumnName = "patientid";
        DateTime startDate = DateTime.Now.AddDays(-periodInDays);
        return await _Context.DataAuditHistories.CountAsync(d => d.OperationType.Equals(AuditOperationType.Inserted)
                    && d.TableName.Equals(PatientTableName)
                    && d.ColumnName.Equals(PatientColumnName)
                    && d.CustomerId.Equals(customerId)
                    && d.Timestamp > startDate.Date);
    }
}