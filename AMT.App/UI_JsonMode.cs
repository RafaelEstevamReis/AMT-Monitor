namespace AMT.App;

using Simple.AMT;
using System.Threading.Tasks;

internal class UI_JsonMode
{
    public static async Task<object> StartUIAsync(string ip, int port, string passwd, Simple.BotUtils.Startup.Arguments argParser)
    {
        AMT8000 amt = new AMT8000(new Simple.AMT.AMTModels.ConnectionInfo()
        {
            IP = ip,
            Port = port,
            Password = passwd,
        });
        await amt.ConnectAsync();

        if (argParser.Has("--contral-info"))
        {
            var centralInfo = await amt.GetCentralStatusAsync();
            centralInfo.Data = null;
            centralInfo.Header = null;

            return centralInfo;
        }
        if (argParser.Has("--zone-names"))
        {
            var names = await amt.GetZonesNamesAsync();
            return names;
        }
        if (argParser.Has("--zone-types"))
        {
            var types = await amt.GetZonesTypesAsync();
            return types;
        }
        if (argParser.Has("--sensors"))
        {
            var oppened = await amt.GetOpenedZonesAsync();
            return oppened;
        }
        if (argParser.Has("--users"))
        {
            var users = await amt.GetUserNamesAsync();
            return users;
        }
        if (argParser.Has("--connections"))
        {
            var cnn = await amt.GetConnectionsAsync();
            cnn.Data = null;
            cnn.Header = null;
            return cnn;
        }
        return new { };
    }
}
