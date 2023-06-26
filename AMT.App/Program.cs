using Simple.AMT;
using System.Text.Json;

int port = 9009;
bool listen;

while (true)
{
    Console.Clear();
    Console.WriteLine("Select option:");
    Console.WriteLine("1. Listen Mode - IP monitoring");
    Console.WriteLine("2. Active mode - Connects to central");
    Console.Write("> ");
    string response = Console.ReadLine() ?? "";

    if (!int.TryParse(response, out int result)) continue;

    if (result == 1)
    {
        listen = true;
        break;
    }
    if (result == 2)
    {
        listen = false;
        break;
    }
}


if (listen)
{
    Console.Clear();
    Console.WriteLine($"Monitor Mode started on port {port}");
    Console.WriteLine("Configure central ip monitoring to this host");

    var cnn = new Listener(port);
    cnn.OnEvent += Cnn_OnEvent;
    cnn.OnMessage += (s, m) => Console.WriteLine($"{DateTime.Now} {m}");

    await cnn.StartAsync();
}
else // Connect
{
    Console.Clear();

    Console.WriteLine("Active connection mode");
    Console.Write("Central IP: ");
    string ip = Console.ReadLine() ?? "";
    Console.WriteLine();

    Console.Write("Central Password: ");
    string passwd = Console.ReadLine() ?? "";
    Console.WriteLine();

    Console.WriteLine("Connecting...");
    AMT8000 amt = new AMT8000(new Simple.AMT.AMTModels.ConnectionInfo()
    {
        IP = ip,
        Port = port,
        Password = passwd,
    });
    await amt.ConnectAsync();

    Console.Clear();
    Console.WriteLine("Connected to CENTRAL");
    var centralInfo = await amt.GetCentralStatusAsync();
    var sensorNames = await amt.GetZonesNamesAsync();


    var types = await amt.GetZoneTypesAsync();

    var status = await amt.GetCentralStatusAsync();
    var sensors = await amt.GetSensorConfigurationAsync();

    var zones = await amt.GetZonesNamesAsync();
    var users = await amt.GetUserNamesAsync();

    var events = await amt.GetEventsAsync();
    while (true)
    {
        Thread.Sleep(1000);
        Console.Clear();
        var oppenedZones = await amt.GetOpenedZonesAsync();

        for (int i = 0; i < 16; i++)
        {
            Console.WriteLine($"[{i + 1:00}] {zones[i].Name.PadRight(16)} {(oppenedZones[i] ? "OPEN " : "CLOSE")} {(status.Zones[i].ByPass ? "[BP]" : "  ")}");
        }
    }
}

void Cnn_OnEvent(object? sender, Simple.AMT.ListenerModels.EventInformation e)
{
    var json = JsonSerializer.Serialize(e);
    Console.WriteLine(json);

    string qual;
    if (e.Qualifier == 1) qual = "*";
    else if (e.Qualifier == 3) qual = "RST";
    else qual = $"{e.Qualifier}";

    Console.WriteLine($"{DateTime.Now} [{e.Channel}] [{e.MessageType}/{e.ContactId}] Zone: {e.Zone} [{qual}]{e.Code} {e.CodeName}");
}