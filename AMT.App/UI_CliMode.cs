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

        bool continuous = argParser.Has("--continuous");
        if (continuous)
        {
            await runContinuousMode(amt, maxSensors);
        }
        else
        {
            await runSingleMode(argParser, amt, maxSensors);
        }
    }

    private static async Task runSingleMode(Simple.BotUtils.Startup.Arguments argParser, AMT8000 amt, int maxSensors)
    {
        var centralInfo = await amt.GetCentralStatusAsync();

        if (argParser.Has("--central-info"))
        {
            Console.WriteLine($"Central Information:");
            Console.WriteLine($" DateTime: {centralInfo.CentralDateTime:g}");
            Console.WriteLine($" Firmware: {centralInfo.Firmware}");
            Console.WriteLine($" ETH: {centralInfo.IsEth}");
            Console.WriteLine($" WiFi: {centralInfo.IsWifi}");
            Console.WriteLine($" GPRS: {centralInfo.IsGprs} ");
            Console.WriteLine($" Battery: {centralInfo.BatteryLevel}");
            Console.WriteLine($" Problems: {centralInfo.HasProblems}");
            Console.WriteLine($" Siren: {centralInfo.HasSiren}");
            Console.WriteLine($" All Closed:  {centralInfo.AllZonesClosed}");
            Console.WriteLine($" Any Bypassed: {centralInfo.AnyZoneByPassed}");
            Console.WriteLine($" Any Triggered: {centralInfo.AnyZoneTriggered} ");
        }

        Console.WriteLine($"Sensors:");
        var sensorNames = await amt.GetZonesNamesAsync();
        int sensors = Math.Min(64, maxSensors);
        for (int offset = 0; offset < sensors; offset++)
        {
            Console.WriteLine($"{offset + 1:00}: [{(centralInfo.Zones[offset].Open ? "O" : "C")}{(centralInfo.Zones[offset].ByPass ? "B" : " ")}{(centralInfo.Zones[offset].Trigger ? "T" : " ")}] {sensorNames[offset].Name}");
        }

        if (argParser.Has("--update"))
        {
            Console.WriteLine($"Updates:");
            var mon = new SensorMonitor(amt);
            mon.Setup();

            while (true)
            {
                await Task.Delay(1000);
                var result = await mon.UpdateAsync();

                if (result == null || result.Length == 0) continue;

                foreach(var r in result)
                {
                    Console.WriteLine($"{r.SensorIndex + 1:00}: [{(r.NewValue ? "O" : "C")}{(centralInfo.Zones[r.SensorIndex].ByPass ? "B" : " ")}{(centralInfo.Zones[r.SensorIndex].Trigger ? "T" : " ")}] {sensorNames[r.SensorIndex].Name}");
                }
            }
        }
    }

    private static async Task runContinuousMode(AMT8000 amt, int maxSensors)
    {
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
