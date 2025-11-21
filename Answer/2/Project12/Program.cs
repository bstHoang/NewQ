using Microsoft.Extensions.Configuration;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

string ipAddress = configuration["IpAddress"]!;
int port = int.Parse(configuration["Port"]!);

while (true)
{
    Console.Write("Enter operation (format as A X B):");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    if (input.Trim().ToLower() == "exit")
        break;

    try
    {
        using TcpClient client = new TcpClient();
        client.Connect(ipAddress, port);

        using NetworkStream stream = client.GetStream();

        byte[] data = Encoding.UTF8.GetBytes(input);
        stream.Write(data, 0, data.Length);
        stream.Flush();

        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        if (bytesRead == 0)
        {
            Console.WriteLine("Server closed connection unexpectedly.");
            continue;
        }

        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        // JSON array?
        if (response.StartsWith('[') && response.EndsWith(']'))
        {
            var errors = JsonSerializer.Deserialize<string[]>(response)!;
            foreach (var e in errors)
                Console.WriteLine(e);
        }
        else
        {
            // single string
            Console.WriteLine(response);
        }

        stream.Close();
        client.Close();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Server is not running: " + ex.Message);
    }
}
