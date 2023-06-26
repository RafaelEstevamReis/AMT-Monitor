namespace AMT.App;

using Simple.AMT;
using System;
using System.Threading.Tasks;

internal static class UI_InteractiveMode
{
    public static async Task StartUIAsync(string ip, int port, string passwd)
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
        while (true)
        {
            Console.Clear();
            var centralInfo = await amt.GetCentralStatusAsync();
            var sensorNames = await amt.GetZonesNamesAsync();
            //var sensorTypes = await amt.GetZoneTypesAsync();
            //var sensorCfg = await amt.GetSensorConfigurationAsync();
            //var events = await amt.GetEventsAsync();

            // Central Data
            Console.WriteLine($"Central Information");
            Console.WriteLine($" DateTime: {centralInfo.CentralDateTime:g}  Firmware: {centralInfo.Firmware}");
            Console.WriteLine($" ETH: {centralInfo.IsEth}  WiFi: {centralInfo.IsWifi}  GPRS: {centralInfo.IsGprs} ");
            Console.WriteLine($" Battery: {centralInfo.BatteryLevel}  Problems: {centralInfo.HasProblems}  Siren: {centralInfo.HasSiren}");
            Console.WriteLine($" All Closed:  {centralInfo.AllZonesClosed}  Any Bypassed: {centralInfo.AnyZoneByPassed}  Any Triggered: {centralInfo.AnyZoneTriggered} ");

            Console.SetCursorPosition(1, 8);
            Console.WriteLine("Statuses: [O] Openned | [C] Closed | [B] Bypass | [T] Triggered");
            Console.WriteLine("Sensors:");
            int offset = 0;
            for (int c = 0; c < 4; c++)
            {
                for (int r = 0; r < 16; r++)
                {
                    Console.SetCursorPosition(1 + 30 * c, 10 + r);
                    Console.Write($"{r + 1:00}: [{(centralInfo.Zones[offset].Open ? "O" : "C")}{(centralInfo.Zones[offset].ByPass ? "B" : " ")}{(centralInfo.Zones[offset].Trigger ? "T" : " ")}] {sensorNames[offset].Name.PadRight(16)} ");
                    offset++;
                }
            }

            int countdown = 60;
            string progress = "\\-/|";
            while (countdown-- > 0)
            {
                await Task.Delay(1000);
                //Console.Clear();
                var oppenedZones = await amt.GetOpenedZonesAsync();

                offset = 0;
                for (int c = 0; c < 4; c++)
                {
                    for (int r = 0; r < 16; r++)
                    {
                        Console.SetCursorPosition(6 + 30 * c, 10 + r);
                        Console.Write(oppenedZones[offset] ? "O" : "C");
                        offset++;
                    }
                }
                Console.SetCursorPosition(1, 27);
                Console.Write(progress[countdown % 4]);

                //for (int i = 0; i < 16; i++)
                //{
                //    Console.Write($"[{i + 1:00}] {sensorNames[i].Name.PadRight(16)} {(oppenedZones[i] ? "OPEN " : "CLOSE")}");
                //}
            }
        }
    }
}
