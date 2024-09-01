using System.Reflection;
using Health.Api.Data;
using Health.Api.Models.Constants;

namespace Health.Api.Repositories;

public interface IDataAuditHistoryRepository
{
    Task RecordNewModelAuditDataAsync<T>(T model
        , string tableName
        , long primaryReferenceId
        , long customerId
        , long appUserId);
    Task RecordEditModelAuditDataAsync<T>(T oldModel
        , T newModel
        , string tableName
        , long primaryReferenceId
        , long customerId
        , long appUserId);
    Task RecordDeactivatedAuditDataAsync(string tableName
        , long primaryReferenceId
        , long customerId
        , long appUserId
        , string columnName = "isactive");
    Task RecordActivatedAuditDataAsync(string tableName
    , long primaryReferenceId
    , long customerId
    , long appUserId
    , string columnName = "isactive");
}

public class DataAuditHistoryRepository : IDataAuditHistoryRepository
{
    private HASDbContext _Context;

    public DataAuditHistoryRepository(HASDbContext context)
    {
        _Context = context;
    }

    private async Task InsertDataAuditHistoryAsync(IList<DataAuditHistory> audits)
    {
        try
        {
            foreach (DataAuditHistory audit in audits)
                _Context.DataAuditHistories.Add(audit);
            await _Context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async Task RecordNewModelAuditDataAsync<T>(T model
        , string tableName
        , long primaryReferenceId
        , long customerId
        , long appUserId)
    {
        List<DataAuditHistory> audits = new List<DataAuditHistory>();
        PropertyInfo[] propertyInfoCollection = typeof(T).GetProperties();
        foreach (PropertyInfo pi in propertyInfoCollection)
        {
            var audit = new DataAuditHistory
            {
                DataAuditHistoryId = Guid.NewGuid().ToString(),
                TableName = tableName.ToLower(),
                ColumnName = pi.Name.ToLower(),
                PrimaryReferenceId = primaryReferenceId,
                OperationType = AuditOperationType.Inserted,
                OldValue = null,
                NewValue = pi.GetValue(model, null)?.ToString(),
                Timestamp = DateTime.Now,
                CustomerId = customerId,
                AppUserId = appUserId
            };
            audits.Add(audit);
        }

        await InsertDataAuditHistoryAsync(audits);
    }

    public async Task RecordEditModelAuditDataAsync<T>(T oldModel
        , T newModel
        , string tableName
        , long primaryReferenceId
        , long customerId
        , long appUserId)
    {
        List<DataAuditHistory> audits = new List<DataAuditHistory>();
        PropertyInfo[] propertyInfoCollection = typeof(T).GetProperties();
        foreach (PropertyInfo pi in propertyInfoCollection)
        {
            string? oldValue = pi.GetValue(oldModel, null)?.ToString();
            string? newValue = pi.GetValue(newModel, null)?.ToString();

            if (!string.Equals(oldValue, newValue))
            {
                var audit = new DataAuditHistory
                {
                    DataAuditHistoryId = Guid.NewGuid().ToString(),
                    TableName = tableName.ToLower(),
                    ColumnName = pi.Name.ToLower(),
                    PrimaryReferenceId = primaryReferenceId,
                    OperationType = AuditOperationType.Edited,
                    OldValue = oldValue,
                    NewValue = newValue,
                    Timestamp = DateTime.Now,
                    CustomerId = customerId,
                    AppUserId = appUserId
                };
                audits.Add(audit);
            }
        }
        await InsertDataAuditHistoryAsync(audits);
    }

    public async Task RecordDeactivatedAuditDataAsync(string tableName
        , long primaryReferenceId
        , long customerId
        , long appUserId
        , string columnName = "isactive")
    {
        const string Active = "1";
        const string Inactive = "0";
        var audit = new DataAuditHistory
        {
            DataAuditHistoryId = Guid.NewGuid().ToString(),
            TableName = tableName.ToLower(),
            ColumnName = columnName.ToLower(),
            PrimaryReferenceId = primaryReferenceId,
            OperationType = AuditOperationType.Deactivated,
            OldValue = Active,
            NewValue = Inactive,
            Timestamp = DateTime.Now,
            CustomerId = customerId,
            AppUserId = appUserId
        };
        await InsertDataAuditHistoryAsync(new List<DataAuditHistory> { audit });
    }

    public async Task RecordActivatedAuditDataAsync(string tableName
        , long primaryReferenceId
        , long customerId
        , long appUserId
        , string columnName = "isactive")
    {
        const string Active = "1";
        const string Inactive = "0";
        var audit = new DataAuditHistory
        {
            DataAuditHistoryId = Guid.NewGuid().ToString(),
            TableName = tableName.ToLower(),
            ColumnName = columnName.ToLower(),
            PrimaryReferenceId = primaryReferenceId,
            OperationType = AuditOperationType.Deactivated,
            OldValue = Inactive,
            NewValue = Active,
            Timestamp = DateTime.Now,
            CustomerId = customerId,
            AppUserId = appUserId
        };
        await InsertDataAuditHistoryAsync(new List<DataAuditHistory> { audit });
    }
}