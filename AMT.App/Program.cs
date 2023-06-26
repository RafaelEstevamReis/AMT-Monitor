using AMT.App;
using Simple.AMT;
using System;
using System.Text.Json;

bool cliMode = false;
bool listenMode = false;
string ip = "";
int port = 9009;
string pwd = "";

var argParser = Simple.BotUtils.Startup.ArgumentParser.Parse(args);

if (argParser.ContainsKey("--help") || argParser.ContainsKey("/h"))
{
    Console.WriteLine("Commands: ");
    Console.WriteLine("--help | /h: show this help");
    Console.WriteLine("--cli: start cli mode");
    Console.WriteLine("--listen: listen mode");
    Console.WriteLine("--ip: defines central ip");
    Console.WriteLine("--port: defines central port");
    Console.WriteLine("--pwd: defines central password");
    Console.WriteLine("--max-sensors: [cli only] max sensors to display");
    return;
}

if (argParser.ContainsKey("--cli")) cliMode = true;
if (argParser.ContainsKey("--listen")) listenMode = true;
ip = argParser.Get("--ip");
pwd = argParser.Get("--pwd");
if(ushort.TryParse(argParser.Get("--port"), out ushort usp)) port = usp;

if (listenMode)
{
    Console.Clear();
    Console.WriteLine("Configure central ip monitoring to this host");
    Console.WriteLine($"Monitor Mode started on port {port}");

    var cnn = new Listener(port);
    cnn.OnEvent += Cnn_OnEvent;
    cnn.OnMessage += (s, m) => Console.WriteLine($"{DateTime.Now} {m}");

    await cnn.StartAsync();
}
else // Connect
{
    Console.WriteLine("Active connection mode");
    if (string.IsNullOrEmpty(ip))
    {
        Console.Write("Central IP: ");
        ip = Console.ReadLine() ?? "";
    }
    if (string.IsNullOrEmpty(pwd))
    {
        Console.Write("Central Password: ");
        pwd = Console.ReadLine() ?? "";
    }

    if (cliMode)
    {
        await UI_CliMode.StartUIAsync(ip, port, pwd, argParser);
    }
    else
    {
        await UI_InteractiveMode.StartUIAsync(ip, port, pwd);
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