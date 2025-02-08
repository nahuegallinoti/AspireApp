namespace AspireApp.Web;

public static class StringHelper
{
    public static string FormatErrorMessages(this IEnumerable<string> errors) => string.Join(";", errors);
}
