using Microsoft.Extensions.Configuration;
using System.Net.Sockets;
using System.Text;

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

        NetworkStream stream = client.GetStream();

        // Gửi input thẳng đến server
        byte[] data = Encoding.UTF8.GetBytes(input);
        stream.Write(data, 0, data.Length);

        // Nhận dữ liệu từ server
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        // Chuyển về string
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        string[] lines = response.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }

        stream.Close();
        client.Close();
    }
    catch
    {
        Console.WriteLine("Server is not running");
    }
}
