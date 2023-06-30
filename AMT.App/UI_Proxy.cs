namespace AMT.App;

using Simple.AMT;
using Simple.AMT.AMTPackets;
using System;
using System.IO;
using System.Threading;

internal class UI_Proxy
{
    public static void StartUI(string ip, int port, Simple.BotUtils.Startup.Arguments argParser)
    {
        AmtProxyObserver observer = new AmtProxyObserver(port, ip, port);
        observer.LogEvents += Observer_LogEvents;
        observer.DataTraffic += Observer_DataTraffic;

        void Observer_DataTraffic(object? sender, ProxyTraffic e)
        {
            var decoder = new TrafficDecoder(e);
            Console.WriteLine($"{DateTime.Now:G} {(e.SentToCentral ? ">>" : "<<")} [{decoder.PacketLen:000}] Cmd: {(DataPacket.Commands)decoder.PacketCommand} {messageDataStart(decoder.GetPacketData(), 32)} ");
            string data = $"{DateTime.Now:G} {(e.SentToCentral ? ">>" : "<<")} [{decoder.PacketLen:000}] Cmd: {(DataPacket.Commands)decoder.PacketCommand} {messageDataStart(decoder.GetPacketData(), 9999)}\n";
            File.AppendAllText("proxy_data.txt", data);
        }
        void Observer_LogEvents(object? sender, string msg)
        {
            Console.WriteLine($"{DateTime.Now:G} {msg}");
        }
        string messageDataStart(byte[] data, int maxBytes)
        {
            return BitConverter.ToString(data, 0, Math.Min(data.Length, maxBytes)).Replace('-', ' ');
        }

        observer.Start();
        Console.WriteLine("Proxy active");
        while (true) { Thread.Sleep(100); }

    }
}
