namespace AMT.App;

using Simple.AMT;
using System;
using System.Threading.Tasks;

internal static class UI_CliMode
{
    public static async Task StartUIAsync(string ip, int port, string passwd, Simple.BotUtils.Startup.Arguments argParser)
    {
        Console.Clear();
        Console.WriteLine($"Connecting to {ip}:{port}...");
        AMT8000 amt = new AMT8000(new Simple.AMT.AMTModels.ConnectionInfo()
        {
            IP = ip,
            Port = port,
            Password = passwd,
        });
        await amt.ConnectAsync();
        Console.CursorVisible = false;

        if (!int.TryParse(argParser.Get("--max-sensors"), out int maxSensors)) maxSensors = 64;

        while (true)
        {
            var centralInfo = await amt.GetCentralStatusAsync();
            var sensorNames = await amt.GetZonesNamesAsync();
            //var events = await amt.GetEventsAsync();

            // Central Data
            Console.WriteLine($"Central Information");
            Console.WriteLine($" DateTime: {centralInfo.CentralDateTime:g} Firmware: {centralInfo.Firmware}");
            Console.WriteLine($" ETH: {centralInfo.IsEth}  WiFi: {centralInfo.IsWifi}  GPRS: {centralInfo.IsGprs} ");
            Console.WriteLine($" Battery: {centralInfo.BatteryLevel}  Problems: {centralInfo.HasProblems}  Siren: {centralInfo.HasSiren}");
            Console.WriteLine($" All Closed:  {centralInfo.AllZonesClosed}  Any Bypassed: {centralInfo.AnyZoneByPassed}  Any Triggered: {centralInfo.AnyZoneTriggered} ");

            Console.WriteLine("Sensors:");
            int countdown = 60;
            while (countdown-- > 0)
            {
                var oppenedZones = await amt.GetOpenedZonesAsync();

                int sensors = Math.Min(64, maxSensors);

                for (int offset = 0; offset < sensors; offset++)
                {
                    Console.WriteLine($"{offset + 1:00}: [{(centralInfo.Zones[offset].Open ? "O" : "C")}{(centralInfo.Zones[offset].ByPass ? "B" : " ")}{(centralInfo.Zones[offset].Trigger ? "T" : " ")}] {sensorNames[offset].Name.PadRight(16)} ");
                }

                Console.WriteLine("--------------------------");
                await Task.Delay(1000);
            }
            Console.WriteLine("==========================");
        }
    }
}
