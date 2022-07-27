using System.Net.Sockets;

namespace AMT.Lib
{
    public class Listener
    {
        private readonly int port;
        private TcpListener listener;

        public ListenerModels.CentralInformation CentralInformation { get; init; }
        public event EventHandler<ListenerModels.EventInformation> OnEvent;
        public event EventHandler<ListenerModels.MessageEventArgs> OnMessage;

        public Listener(int port)
        {
            this.port = port;

            CentralInformation = new();
        }

        public async Task StartAsync()
        {
            listener = new TcpListener(System.Net.IPAddress.Any, port);
            listener.Start();

            while (true)
            {
                if (!listener.Pending())
                {
                    await Task.Delay(100);
                    continue;
                }
                var client = await listener.AcceptTcpClientAsync();
                _ = processaClientAsync(client);
            }
        }

        private async Task processaClientAsync(TcpClient client)
        {
            using var stream = client.GetStream();
            byte[] buffer = new byte[512];

            while (true)
            {
                if (client.Available < 2)
                {
                    await Task.Delay(50);
                    continue;
                }

                // read len
                var pktLen = stream.ReadByte();
                await Task.Delay(50);


                if (pktLen == 0)
                {
                    /* ?? */
                    continue;
                }
                if (pktLen == 0xf7)
                {
                    /* ?? */
                    // heart beat
                    await sendAckAsync(stream);
                }
                if (pktLen > 127) { /* ?? */ }

                pktLen++; // Include CheckSum
                var len = await stream.ReadAsync(buffer, 0, pktLen); // +CHK

                showHex($"{DateTime.Now:T} L{len} ", buffer, len);

                if (len == 0) { }
                if (len != pktLen)
                {

                }

                bool ack;
                switch (buffer[0])
                {
                    case 0x94: // IDENT (len==7)
                        ack = processIdent(buffer, len);
                        OnMessage?.Invoke(this, new ListenerModels.MessageEventArgs()
                        {
                            Message = $"Central information Received [{CentralInformation.Connection}] Id: {CentralInformation.AccountId} PartilMac: {CentralInformation.PartialMacAddressString}",
                            Type = ListenerModels.MessageEventArgs.MessageType.CentralInformation
                        });
                        break;
                    case 0xC4: // MAC
                        ack = processMac(buffer, len);
                        OnMessage?.Invoke(this, new ListenerModels.MessageEventArgs()
                        {
                            Message = "Central MAC Received",
                            Type = ListenerModels.MessageEventArgs.MessageType.MacReceived
                        });
                        break;

                    case 0x80: // Date/Time
                        ack = await processDateTimeAsync(stream, buffer, len);
                        OnMessage?.Invoke(this, new ListenerModels.MessageEventArgs()
                        {
                            Message = "Central asked current Date/Time",
                            Type = ListenerModels.MessageEventArgs.MessageType.DateTimeRequest,
                        });
                        break;

                    case 0xB0: // Event
                        ack = processEvent(buffer, len, photo: false);
                        OnMessage?.Invoke(this, new ListenerModels.MessageEventArgs()
                        {
                            Message = "Central Event",
                            Type = ListenerModels.MessageEventArgs.MessageType.Event
                        });
                        break;
                    case 0xB5: // Event
                        ack = processEvent(buffer, len, photo: true);
                        OnMessage?.Invoke(this, new ListenerModels.MessageEventArgs()
                        {
                            Message = "Central Photo Event",
                            Type = ListenerModels.MessageEventArgs.MessageType.EventWithPhoto
                        });
                        break;

                    default:
                        showHex($"{DateTime.Now:T} [UNKOW] L{len} ", buffer, len);
                        OnMessage?.Invoke(this, new ListenerModels.MessageEventArgs()
                        {
                            Message = "UNKOWN MESSAGE " + buffer[0],
                            Type = ListenerModels.MessageEventArgs.MessageType.UNKOWN
                        });
                        ack = true;
                        break;
                }

                // send ACK
                if (ack) await sendAckAsync(stream);
            }


        }

        private bool processIdent(byte[] buffer, int len)
        {
            CentralInformation.Connection = (ListenerModels.CentralInformation.ConnectionType)buffer[1];
            CentralInformation.AccountId = fromBinary(buffer[2], buffer[3]);
            CentralInformation.PartialMacAddress = new byte[] { buffer[4], buffer[5], buffer[6] };

            return true;
        }
        private bool processMac(byte[] buffer, int len)
        {
            return true;
        }
        private async Task<bool> processDateTimeAsync(NetworkStream stream, byte[] buffer, int len)
        {
            await sendDateTime_TZBR(stream);
            return false;
        }

        private bool processEvent(byte[] buffer, int len, bool photo)
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

            OnEvent?.Invoke(this, eventInfo);

            return true;
        }


        private async Task sendAckAsync(NetworkStream stream)
        {
            await stream.WriteAsync(new byte[] { 0xfe });
        }
        private async Task sendDateTime_TZBR(NetworkStream stream)
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
            for (int i = 0; i < len; i++)
            {
                //if (buffer[i] >= 32 && buffer[i] < 128) Console.Write((char)buffer[i]);
                //else
                Console.Write($"[{buffer[i]:X2}]");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// binary converter
        /// </summary>
        static byte bcd(byte number)
        {
            if (number > 99) throw new InvalidOperationException("number must be smaller than 100");

            // shit nibbles
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
                { } // multiplica
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
