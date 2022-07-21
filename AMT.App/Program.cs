using AMT.Lib;
using System.Text.Json;

Console.WriteLine("START");

var cnn = new Listener(9009);
cnn.OnEvent += Cnn_OnEvent;
cnn.OnMessage += (s, m) => Console.WriteLine($"{DateTime.Now} {m}");

await cnn.StartAsync();


void Cnn_OnEvent(object? sender, AMT.Lib.Models.EventInformation e)
{
    var json = JsonSerializer.Serialize(e);
    Console.WriteLine(json);

    string qual;
    if (e.Qualifier == 1) qual = "*";
    else if (e.Qualifier == 3) qual = "RST";
    else qual = $"{e.Qualifier}";

    Console.WriteLine($"{DateTime.Now} [{e.Channel}] [{e.MessageType}/{e.ContactId}] Zone: {e.Zone} [{qual}]{e.Code} {e.CodeName}");
}