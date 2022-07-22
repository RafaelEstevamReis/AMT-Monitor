using AMT.Lib;
using System.Text.Json;

Console.WriteLine("START");

AMT8000 amt = new AMT8000(new AMT.Lib.AMTModels.ConnectionInfo()
{
    IP = "192.168.1.0",
    Port = 9876,
    Password = "123456",
});
await amt.ConnectAsync();





return;

var cnn = new Listener(9009);
cnn.OnEvent += Cnn_OnEvent;
cnn.OnMessage += (s, m) => Console.WriteLine($"{DateTime.Now} {m}");

await cnn.StartAsync();


void Cnn_OnEvent(object? sender, AMT.Lib.ListenerModels.EventInformation e)
{
    var json = JsonSerializer.Serialize(e);
    Console.WriteLine(json);

    string qual;
    if (e.Qualifier == 1) qual = "*";
    else if (e.Qualifier == 3) qual = "RST";
    else qual = $"{e.Qualifier}";

    Console.WriteLine($"{DateTime.Now} [{e.Channel}] [{e.MessageType}/{e.ContactId}] Zone: {e.Zone} [{qual}]{e.Code} {e.CodeName}");
}