using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.AMT
{
    /// <summary>
    /// Connect-Id IP Monitoring class
    /// </summary>
    public class Listener
    {
        private static long lastCnnId = 0;
        private readonly int port;
        private TcpListener listener;

        //public ListenerModels.CentralInformation CentralInformation { get; private set; }
        public event EventHandler<ListenerModels.EventInformation> OnEvent;
        public event EventHandler<ListenerModels.MessageEventArgs> OnMessage;
        public event EventHandler<Exception> OnClientException;
        public event EventHandler<Exception> OnError;

        public bool Debug_PrintHex { get; set; }

        public Listener(int port)
        {
            this.port = port;
            Debug_PrintHex = true;
            //CentralInformation = new();
        }

        bool running = false;
        public async Task StartAsync()
        {
            running = true;
            listener = new TcpListener(System.Net.IPAddress.Any, port);
            listener.Start();
            while (running)
            {
                if (!listener.Pending())
                {
                    await Task.Delay(100);
                    continue;
                }
                var client = await listener.AcceptTcpClientAsync();
                _ = processClientAsync(client);
            }
            listener.Stop();
        }
        public void Stop()
        {
            running = false;
        }

        private async Task processClientAsync(TcpClient client)
        {
            var id = Interlocked.Increment(ref lastCnnId);
            try
            {
                await _processClientAsync(client, id);
            }
            catch (Exception ex)
            {
                OnClientException?.Invoke(this, ex);
            }
            finally
            {
                if (client != null)
                {
                    if (client.Connected) client.Close();
                    client.Dispose();
                }
            }
        }
        private async Task _processClientAsync(TcpClient client, long id)
        {
            var centralInformation = new ListenerModels.CentralInformation();
            using var stream = client.GetStream();
            
            byte[] buffer = new byte[512];

            DateTime lastReceive = DateTime.Now;

            while (client.Connected)
            {
                if (!running) break;

                if (client.Available < 2)
                {
                    await Task.Delay(50);

                    // A ping (0x94-central information) is received every 2 minutes
                    // If nothing arrive after 5 minutes, the connection was lost/dropped
                    if ((DateTime.Now - lastReceive).TotalMinutes > 4) client.Close();

                    continue;
                }
                lastReceive = DateTime.Now;

                try
                {
                    await receivePacketAsync(stream, buffer, centralInformation, id);
                }
                catch (Exception ex)
                {
                    OnClientException?.Invoke(this, ex);
                }
            }
        }
        private async Task receivePacketAsync(NetworkStream stream, byte[] buffer, ListenerModels.CentralInformation centralInformation, long cnnId)
        {
            // read len
            var pktLen = stream.ReadByte();
            await Task.Delay(50);

            if (pktLen == 0)
            {
                /* ?? */
                return;
            }
            if (pktLen == 0xf7)
            {
                /* ?? */
                // heart beat
                await sendAckAsync(stream);
                if (Debug_PrintHex) Console.WriteLine($"{DateTime.Now:T} [CnnId:{cnnId}] 0xF7 HeartBeat");
                return;


            }
            if (pktLen > 127) { /* ?? */ }

            pktLen++; // Include CheckSum
            var len = await stream.ReadAsync(buffer, 0, pktLen); // +CHK

            if (len == 0) { }
            if (len != pktLen)
            { }

            if (Debug_PrintHex) showHex($"{DateTime.Now:T} [CnnId:{cnnId}] L{len} P{pktLen} ", buffer, len);

            bool ack;
            switch (buffer[0])
            {
                case 0xF7: // ?
                    ack = true;
                    break;
                case 0x94: // IDENT (len==7)
                    ack = processIdent(centralInformation, buffer, len);
                    sendMessage(centralInformation, new ListenerModels.MessageEventArgs()
                    {
                        Message = $"Central information Received [{centralInformation.Connection}] AccId: {centralInformation.AccountId} PartilMac: {centralInformation.PartialMacAddressString}",
                        Type = ListenerModels.MessageEventArgs.MessageType.CentralInformation
                    });

                    if (centralInformation.MacAddress == null)
                    {
                        // request MAC
                        stream.Write(new byte[] { 0x01, 0xc4, 0x3a });
                    }

                    break;
                case 0xC4: // MAC (len==8)
                    ack = processMac(centralInformation, buffer, len);
                    sendMessage(centralInformation, new ListenerModels.MessageEventArgs()
                    {
                        Message = "Central MAC Received",
                        Type = ListenerModels.MessageEventArgs.MessageType.MacReceived
                    });
                    break;

                case 0x80: // Date/Time
                    ack = await processDateTimeAsync(stream, buffer, len);
                    sendMessage(centralInformation, new ListenerModels.MessageEventArgs()
                    {
                        Message = "Central asked current Date/Time",
                        Type = ListenerModels.MessageEventArgs.MessageType.DateTimeRequest,
                    });
                    break;

                case 0xB0: // Event
                    ack = processEvent(centralInformation, buffer, len, photo: false);
                    sendMessage(centralInformation, new ListenerModels.MessageEventArgs()
                    {
                        Message = "Central Event",
                        Type = ListenerModels.MessageEventArgs.MessageType.Event
                    });
                    break;
                case 0xB5: // Event
                    ack = processEvent(centralInformation, buffer, len, photo: true);
                    sendMessage(centralInformation, new ListenerModels.MessageEventArgs()
                    {
                        Message = "Central Photo Event",
                        Type = ListenerModels.MessageEventArgs.MessageType.EventWithPhoto
                    });
                    break;

                default:
                    if (Debug_PrintHex) showHex($"{DateTime.Now:T} [UNKOW] L{len}P{pktLen} ", buffer, len);
                    sendMessage(centralInformation, new ListenerModels.MessageEventArgs()
                    {
                        Message = $"UNKOWN MESSAGE {buffer[0]} L{len}P{pktLen} {buildHexString(buffer, len)}",
                        Type = ListenerModels.MessageEventArgs.MessageType.UNKOWN
                    });
                    ack = true;
                    break;
            }

            // send ACK
            if (ack) await sendAckAsync(stream);

        }
        private bool processIdent(ListenerModels.CentralInformation centralInformation,byte[] buffer, int len)
        {
            centralInformation.Connection = (ListenerModels.CentralInformation.ConnectionType)buffer[1];
            centralInformation.AccountId = fromBinary(buffer[2], buffer[3]);
            centralInformation.PartialMacAddress = new byte[] { buffer[4], buffer[5], buffer[6] };

            return true;
        }
        private bool processMac(ListenerModels.CentralInformation centralInformation,byte[] buffer, int len)
        {
            centralInformation.MacAddress = new byte[6];
            Buffer.BlockCopy(buffer, 1, centralInformation.MacAddress, 0, 6);
            return true;
        }
        private static async Task<bool> processDateTimeAsync(NetworkStream stream, byte[] buffer, int len)
        {
            await sendDateTime_TZBR(stream);
            return false;
        }

        private bool processEvent(ListenerModels.CentralInformation centralInformation, byte[] buffer, int len, bool photo)
        {
            if (photo && len != 21)
            {
            }
            if (!photo && len != 18)
            {
            }

            var eventInfo = new ListenerModels.EventInformation
            {
                Channel = (ListenerModels.EventInformation.ChannelType)buffer[1],
                ContactId = decode(buffer[2], buffer[3], buffer[4], buffer[5]),
                MessageType = decode(buffer[6], buffer[7]), // 18: ContactId
                Qualifier = buffer[8],
                Code = decode(buffer[9], buffer[10], buffer[11]),
                Partition = decode(buffer[12], buffer[13]),
                Zone = decode(buffer[14], buffer[15], buffer[16])
            };
            // checksum[16]
            if (photo)
            {
                eventInfo.PhotoIndex = (short)(buffer[17] * 256 + buffer[18]);
                eventInfo.PhotoCount = buffer[19];
            }

            sendEvent(centralInformation, eventInfo);

            return true;
        }

        // AsyncVoid, do not wait
        private async void sendMessage(ListenerModels.CentralInformation central, ListenerModels.MessageEventArgs messageEventArgs)
        {
            if (OnMessage == null) return;

            try
            {
                OnMessage?.Invoke(central, messageEventArgs);
            }
            catch(Exception ex) { OnError?.Invoke(this, ex); }
        }
        // AsyncVoid, do not wait
        private async void sendEvent(ListenerModels.CentralInformation central, ListenerModels.EventInformation eventInfoArgs)
        {
            if (OnEvent == null) return;

            try
            {
                OnEvent?.Invoke(central, eventInfoArgs);
            }
            catch (Exception ex) { OnError?.Invoke(this, ex); }
        }

        private static async Task sendAckAsync(NetworkStream stream)
        {
            await stream.WriteAsync(new byte[] { 0xfe });
        }
        private static async Task sendDateTime_TZBR(NetworkStream stream)
        {
            DateTime now = DateTime.UtcNow.AddHours(-3);

            byte year = (byte)(now.Year - 2000);
            byte month = (byte)now.Month;
            byte day = (byte)now.Day;

            byte dow = (byte)now.DayOfWeek;

            byte hour = (byte)now.Hour;
            byte minute = (byte)now.Minute;
            byte second = (byte)now.Second;

            var data = new byte[] {
                0x80,
                bcd(year),bcd( month), bcd(day),
                dow,
                bcd(hour) ,bcd(minute), bcd(second)
            };

            await stream.WriteAsync(data);
        }

        public static void showHex(string prePend, byte[] buffer, int len)
        {
            Console.Write(prePend);
            showHex(buffer, len);
        }
        private static void showHex(byte[] buffer, int len)
        {
            Console.WriteLine(buildHexString(buffer, len));
        }
        private static string buildHexString(byte[] buffer, int len)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                sb.Append($"[{buffer[i]:X2}]");
            }
            return sb.ToString();
        }

        /// <summary>
        /// binary converter
        /// </summary>
        static byte bcd(byte number)
        {
            if (number > 99) throw new InvalidOperationException("number must be smaller than 100");

            // shift nibbles
            return (byte)(((number / 10) << 4) + (number % 10));
        }
        static int fromBinary(params byte[] bytes)
        {
            int n = 0;
            int p = 1;

            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                n += (bytes[i] >> 4) * 10 * p;
                n += (bytes[i] & 0x04) * p;
                p *= 100;
            }

            return n;
        }
        static int decode(params byte[] bytes)
        {
            int numero = 0;
            int posicao = 1;
            foreach (var digito in bytes.Reverse())
            {
                if (digito == 0x0a) // zero
                { } // multiply
                else if (digito >= 0x01 && digito <= 0x09)
                {
                    numero += posicao * digito;
                }
                else
                {
                    throw new InvalidOperationException("Invalid contact id");
                }

                posicao *= 10;
            }
            return numero;
        }
    }
}
