﻿using System.Data;
using System.IO.Enumeration;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace http_webserver;

class Program
{
  static readonly string htdocs = "htdocs";
  static readonly int maxMessageSize = 104857600;

  static async Task<RequestModel?> GetRequest(Socket client)
  {
    byte[] buffer = new byte[512];
    StringBuilder requestText = new();

    try
    {
      while (true)
      {
        int amount = await client.ReceiveAsync(buffer);
        string str = Encoding.UTF8.GetString(buffer, 0, amount);
        requestText.Append(str);
        if (requestText.Length >= maxMessageSize) throw new DataException("request text exceeded max size");
        if (requestText.ToString().Contains("\r\n\r\n")) break;
      }

      string[] lines = requestText.ToString().ToLower().Split("\r\n");
      RequestModel requestModel = new();
      if (lines.Length < 1) throw new DataException("incorrect formatting of message");
      if (!requestModel.AddRequestLine(lines[0])) throw new DataException("request line is formatted incorrectly");
      for (int i = 1; i < lines.Length; i++)
      {
        if (lines[i].Equals("")) break;
        if (!requestModel.AddHeader(lines[i])) Console.WriteLine("error adding header");
      }

      Console.WriteLine($"uri: {requestModel.Uri}");
      return requestModel;
    }
    catch (Exception e)
    {
      Console.WriteLine($"failed parsing request: {e.Message}");
    }

    return null;
  }

  static async Task ServeRequest(Socket client)
  {
    RequestModel? request = await GetRequest(client);
    if(request?.Method == RequestMethod.GET)
    {
      await SendFile(client, request);
    }
  }

  static async Task<bool> SendFile(Socket client, RequestModel request)
  {
    string? uri = request.Uri;
    if (uri == null) return false;
    if(uri.Equals("/")) uri = "/index.html";
    string filename = $"{htdocs}/{uri}";

    if (!File.Exists(filename))
    {
      await SendNotFound(client);
      return false;
    }

    try
    {
      using FileStream fs = File.OpenRead(filename);
      long fileLength = fs.Length;
      fs.Close();
      Console.WriteLine($"sending {uri} ({fileLength})");
      string headers = $"HTTP/1.1 200\r\nContent-Type: text/html; charset: UTF-8;\r\nContent-Length: {fileLength}\r\n\r\n";
      await client.SendAsync(Encoding.UTF8.GetBytes(headers));
      await client.SendFileAsync(filename);
      return true;
    }
    catch (Exception e)
    {
      Console.WriteLine($"error sending message: {e.Message}");
    }

    return false;
  }

  static async Task SendNotFound(Socket client)
  {
    string message = "HTTP/1.1 404\r\n\r\n";
    await client.SendAsync(Encoding.UTF8.GetBytes(message));
  }

  static async Task Main()
  {
    int port = 8001;
    var ipEndPoint = IPEndPoint.Parse($"127.0.0.1:{port}");

    try
    {
      using Socket serverSocket = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
      serverSocket.Bind(ipEndPoint);
      serverSocket.Listen(10);
      Console.WriteLine($"server is listening on {ipEndPoint.Address}:{ipEndPoint.Port}");
      while (true)
      {
        using Socket client = await serverSocket.AcceptAsync();
        await ServeRequest(client);
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(e.Message);
    }
  }
}
