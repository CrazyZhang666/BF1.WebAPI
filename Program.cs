using BF1.WebAPI.SDK;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Hello World");

app.MapGet("/getPlayerList", () => API.GetPlayerList());

Task.Run(async () =>
{
    Console.Title = "BF1 LAN WebAPI";

    string HostName = Dns.GetHostName();
    Console.WriteLine($"HostName: {HostName}");
    Console.WriteLine();

    var IpEntry = Dns.GetHostEntry(HostName);
    foreach (var ip in IpEntry.AddressList)
    {
        Console.WriteLine($"{ip.AddressFamily}: {ip}");
    }
    Console.WriteLine();

    while (true)
    {
        if (Process.GetProcessesByName("bf1").Length > 0)
        {
            if (Memory.Bf1ProHandle == IntPtr.Zero)
            {
                Memory.Initialize();
            }
        }
        else
        {
            if (Memory.Bf1ProHandle != IntPtr.Zero)
            {
                Memory.Uninitialize();
            }
        }

        await Task.Delay(1000);
    }
});

app.Run();
