namespace AspireApp.Client;

public static class StringHelper
{
    public static string FormatErrorMessages(this IEnumerable<string> errors) => string.Join(";", errors);
}
