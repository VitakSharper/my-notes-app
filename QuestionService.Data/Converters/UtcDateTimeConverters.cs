using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace QuestionService.Data.Converters;

/// <summary>
/// SQL Server datetime2 carries no offset, so EF hands values back with Kind=Unspecified and
/// System.Text.Json then serialises them without a trailing Z. The browser reads that as local
/// time and every relative date is off by the timezone offset. Everything stored here is UTC, so
/// the kind is stamped back on the way out.
/// </summary>
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}

public class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter() : base(
        v => v.HasValue
            ? v.Value.Kind == DateTimeKind.Utc ? v : v.Value.ToUniversalTime()
            : v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
    {
    }
}
