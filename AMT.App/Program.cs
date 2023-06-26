using Simple.AMT;
using System.Text.Json;

Console.WriteLine("START");

bool listen = false;

if(listen)
{
    var cnn = new Listener(9009);
    cnn.OnEvent += Cnn_OnEvent;
    cnn.OnMessage += (s, m) => Console.WriteLine($"{DateTime.Now} {m}");

    await cnn.StartAsync();
}
else // Connect
{
    AMT8000 amt = new AMT8000(new Simple.AMT.AMTModels.ConnectionInfo()
    {
        IP = "192.168.1.0",
        Port = 9876,
        Password = "123456",
    });
    await amt.ConnectAsync();



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