using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;

var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

string ipAddress = configuration["IpAddress"]!;
int port = int.Parse(configuration["Port"]!);

TcpListener listener = new TcpListener(IPAddress.Parse(ipAddress), port);
listener.Start();

Console.WriteLine("Waiting for a connection from client");

while (true)
{
    TcpClient client = listener.AcceptTcpClient();
    Console.WriteLine("Client connected");

    using NetworkStream stream = client.GetStream();

    try
    {
        byte[] buffer = new byte[1024];
        int bytesRead;

        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            string input = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

            var errors = ValidateInput(input, out int A, out string op, out int B);

            if (errors.Count > 0)
            {
                string jsonError = System.Text.Json.JsonSerializer.Serialize(errors);
                byte[] errorBytes = Encoding.UTF8.GetBytes(jsonError);
                stream.Write(errorBytes, 0, errorBytes.Length);
                continue;
            }

            double result = op switch
            {
                "+" => A + B,
                "-" => A - B,
                "*" => A * B,
                "/" => (double)A / B,
                _ => 0
            };

            double formattedResult = Math.Truncate(result * 100) / 100;
            byte[] resultBytes = Encoding.UTF8.GetBytes(formattedResult.ToString());
            stream.Write(resultBytes, 0, resultBytes.Length);
        }

        Console.WriteLine("Client disconnected");
    }
    catch
    {
        Console.WriteLine("Error while handling client");
    }
    finally
    {
        client.Close();
    }
}

List<string> ValidateInput(string input, out int a, out string op, out int b)
{
    var errors = new List<string>();
    a = 0;
    b = 0;
    op = "";

    var lengthInput = input.Split(' ');

    if (lengthInput.Length != 3)
    {
        errors.Add("A X B is not correct format");
    }

    string[] parts = input.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    if (parts.Length >= 3)
    {
        string A = parts[0];
        op = parts[1];
        string B = parts[2];

        if (!int.TryParse(A, out a) || !int.TryParse(B, out b))
        {
            errors.Add("A or B is not an integer");
        }

        if (op != "+" && op != "-" && op != "*" && op != "/")
        {
            errors.Add("X is not a valid operator (+,-,*,/)");
        }

        if (op == "/" && int.TryParse(B, out b) && b == 0)
        {
            errors.Add("Divide by 0");
        }
    }

    return errors;
}
