namespace TaskPage.Infrastructure.Persistence;

/// <summary>
/// Maps SQL Server JSON scalar functions so LINQ queries can use them.
/// The methods only serve as translation placeholders for EF Core and are
/// never executed in memory.
/// </summary>
public static class SqlServerJsonFunctions
{
    /// <summary>Placeholder for SQL Server JSON_VALUE(expression, path).</summary>
    /// <param name="expression">JSON document column to read.</param>
    /// <param name="path">JSON path of the scalar value to extract.</param>
    /// <returns>The extracted scalar value.</returns>
    public static string? JsonValue(string? expression, string path) =>
        throw new NotSupportedException("This method is only translated by EF Core for SQL Server.");
}
