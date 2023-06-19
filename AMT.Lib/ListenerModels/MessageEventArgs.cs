using System;

namespace Simple.AMT.ListenerModels
{
    public class MessageEventArgs : EventArgs
    {
        public enum MessageType
        {
            CentralInformation = 0x94,
            MacReceived = 0xC4,
            DateTimeRequest = 0x80,
            Event = 0xB0,
            EventWithPhoto = 0xB5,
            UNKOWN = 0xC5,
        }

        public string Message { get; set; }
        public MessageType Type { get; set; }
        public CentralInformation CentralReference { get; set; }

        public override string ToString()
            => Message;
    }
}
