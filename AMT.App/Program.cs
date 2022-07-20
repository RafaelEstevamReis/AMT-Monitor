using AMT.Lib;
using System.Text.Json;

Console.WriteLine("START");

var cnn = new Connection(9009);
cnn.OnEvent += Cnn_OnEvent;
cnn.OnMessage += (s, m) => Console.WriteLine(m);

await cnn.StartAsync();


void Cnn_OnEvent(object? sender, AMT.Lib.Models.EventInformation e)
{
    var json = JsonSerializer.Serialize(e);
    Console.WriteLine(json); 
}