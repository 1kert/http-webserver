namespace http_webserver;

class ContentType
{
    private static readonly Dictionary<string, string> _dict = new()
    {
        { "css", "text/css" },
        { "html", "text/html" },
        { "js", "text/javascript" }
    };

    public static string? GetContentType(string filePath)
    {
        string extension = filePath[(filePath.LastIndexOf('/') + 1)..];
        extension = extension[(extension.IndexOf('.') + 1)..];
        return _dict.ContainsKey(extension) ? _dict[extension] : null;
    }
}
