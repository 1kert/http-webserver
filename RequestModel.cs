using System.ComponentModel;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;

namespace http_webserver;

enum RequestMethod
{
    GET
}

class RequestModel
{
    public string? Version { get; private set; }
    public RequestMethod? Method { get; private set; }
    public string? Uri { get; private set; } = null;
    public List<KeyValuePair<string, string>> Headers { get; private set; } = [];

    public bool AddHeader(string headerLine)
    {
        string[] split = headerLine.Split(":").Select(x => x.Trim().ToLower()).ToArray();
        if(split.Length < 2) return false;
        Headers.Add(new(split[0], split[1]));
        return true;
    }

    public bool AddRequestLine(string requestLine)
    {
        string[] split = requestLine.Split(' ');
        if(split.Length != 3) return false;
        if(Enum.TryParse(split[0], out RequestMethod method)) return false;
        Method = method;
        Uri = split[1];
        Version = split[2];

        return true; 
    }
}
