using System.Text.Json;

namespace TaskPage.Application.Common;

/// <summary>
/// Shared JSON validation helper aligned with the SQL Server ISJSON behavior,
/// which accepts objects and arrays as valid JSON documents.
/// </summary>
internal static class JsonGuard
{
    /// <summary>Determines whether the value is a valid JSON object or array.</summary>
    /// <param name="value">Text to validate, expected to be non-empty.</param>
    /// <returns>True when the value is a valid JSON object or array.</returns>
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(value);
            return document.RootElement.ValueKind is JsonValueKind.Object or JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
