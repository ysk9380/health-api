using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("DataAuditHistory")]
public class DataAuditHistory
{
    public string DataAuditHistoryId { get; set; } = default!;
    public string TableName { get; set; } = default!;
    public string ColumnName { get; set; } = default!;
    public long PrimaryReferenceId { get; set; }
    public string OperationType { get; set; } = default!;
    public string? OldValue { get; set; } = default!;
    public string? NewValue { get; set; } = default!;
    public DateTime Timestamp { get; set; }
    public long CustomerId { get; set; }
    public long AppUserId { get; set; }
}