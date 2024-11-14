using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace http_webserver;

enum ResponseCode
{
    OK = 200,
    NOT_FOUND = 404,
    SERVER_ERROR = 500
}

class ResponseBuilder
{
    private List<KeyValuePair<string, string>> _headers;
    public ResponseCode ResponseCode { get; private set; }

    public ResponseBuilder(ResponseCode responseCode)
    {
        _headers = [];
        ResponseCode = responseCode;
    }

    string GetHeaderString()
    {
        StringBuilder sb = new();
        sb.Append($"HTTP/1.1 {(int) ResponseCode}\r\n");
        foreach(var pair in _headers)
        {
            sb.Append($"{pair.Key}: {pair.Value}\r\n");
        }
        return sb.ToString();
    }

    async Task SendStatusCode(Socket client, ResponseCode responseCode)
    {
        await client.SendAsync(Encoding.UTF8.GetBytes($"HTTP/1.1 {(int) responseCode}\r\n\r\n"));
        Console.WriteLine($"sent status code: {(int) responseCode}");
    }

    public void AddHeader(string key, string value)
    {
        _headers.Add(new(key, value));
    }

    public async Task SendFile(Socket socket, string filePath)
    {
        try
        {
            using FileStream fs = File.OpenRead(filePath);
            long fileSize = fs.Length;
            string? contentType = ContentType.GetContentType(filePath) ?? throw new Exception("content type not supported");
            AddHeader("Content-Type", $"{contentType}; charset: UTF-8");
            AddHeader("Content-Length", fileSize.ToString());
            Console.WriteLine($"sending filetype: {contentType}");

            await socket.SendAsync(Encoding.UTF8.GetBytes($"{GetHeaderString()}\r\n"));
            await socket.SendFileAsync(filePath);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("file not found");
            await SendStatusCode(socket, ResponseCode.NOT_FOUND);
        }
        catch(Exception e)
        {
            Console.WriteLine($"could not send file: {e.Message}");
            await SendStatusCode(socket, ResponseCode.SERVER_ERROR);
        }
    }
}
