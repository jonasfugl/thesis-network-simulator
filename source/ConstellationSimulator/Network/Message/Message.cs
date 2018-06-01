using System;
using ConstellationSimulator.AODV;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Positions;

namespace ConstellationSimulator.Network.Message
{
    /// <summary>
    /// Message class, used to transmit payloads from one node to another. 
    /// </summary>
    internal class Message
    {
        public Message(IMessagePayload payload, string destination, string source, int ttl)
        {
            Payload = payload;
            DestinationAddress = destination;
            SourceAddress = source;
            PreviousHop = source; //When a new Message is created, the previous hop must be the source of the packet.
            Ttl = ttl;

            HopCount = 0;
            TotalPropagationDelay = 0;
        }

        public Message(Message msg) //Copy constructor
        {
            DestinationAddress = msg.DestinationAddress;
            PreviousHop = msg.PreviousHop;
            SourceAddress = msg.SourceAddress;
            Ttl = msg.Ttl;
            HopCount = msg.HopCount;

            switch (msg.Type)
            {
                case MessageType.Rreq:
                    var rreq = (Rreq) msg.Payload;
                    Payload = new Rreq(rreq);
                    break;
                case MessageType.Rrep:
                    var rrep = (Rrep)msg.Payload;
                    Payload = new Rrep(rrep);
                    break;
                case MessageType.Rerr:
                    var rerr = (Rerr)msg.Payload;
                    Payload = new Rerr(rerr);
                    break;
                case MessageType.Rrepack:
                    var rrepack = (Rrepack)msg.Payload;
                    Payload = new Rrepack(rrepack);
                    break;
                case MessageType.Data:
                    var data = (DataMsg) msg.Payload;
                    Payload = new DataMsg(data);
                    break;
                case MessageType.Hello:
                    var hello = (Hello)msg.Payload;
                    Payload = new Hello(hello);
                    break;
            }

            TotalPropagationDelay = msg.TotalPropagationDelay;
        }
        
        /// <summary>
        /// Address of satellite for whom this message is intended (destination), and whom are receiving this message.
        /// </summary>
        public string DestinationAddress { get; set; }

        /// <summary>
        /// Address of satellite which was the last to forward this message. Equal to SourceAddress on first transfer.
        /// </summary>
        public string PreviousHop { get; set; }

        /// <summary>
        /// The originator/source/creator of the message in the first place.
        /// </summary>
        public string SourceAddress { get; set; }

        /// <summary>
        /// The TTL value of the message
        /// </summary>
        public int Ttl { get; set; }

        /// <summary>
        /// The size of the payload being carried.
        /// </summary>
        public int PayloadSize => Payload.Size;

        /// <summary>
        /// The message type, extracted from the payload.
        /// </summary>
        public MessageType Type => Payload.Type;

        /// <summary>
        /// The actual payload being sent.
        /// </summary>
        public IMessagePayload Payload { get; set; }

        /// <summary>
        /// The size of the message including header and payload.
        /// </summary>
        public int Size => SimulationConstants.MessageHeaderSize + PayloadSize;

        /// <summary>
        /// Decrements the TTL value. An exception is thrown, if the value falls below 0.
        /// </summary>
        public void DecrementTtl()
        {
            Ttl = Ttl - 1;
            HopCount++;

            if (Ttl == -1)
            {
                throw new ArgumentException("TTL reached -1, which should not be possible.");
            }
        }

        /*
         * Fields used for measuring data about the packet for the statistic results.
         */

        /// <summary>
        /// The amount of hops this message has traversed.
        /// </summary>
        public int HopCount { get; private set; }

        /// <summary>
        /// The total time, that this message has been in transit so far. Calculated in milliseconds.
        /// </summary>
        public double TotalEndToEndDelay => TotalPropagationDelay + TotalProcessingDelay;

        /// <summary>
        /// The total time the message has been processed at nodes along the route.
        /// </summary>
        public double TotalProcessingDelay => HopCount * SimulationConstants.PacketProcessingDelay;

        /// <summary>
        /// The total time the message has spent traversing the radio medium.
        /// </summary>
        public double TotalPropagationDelay { get; private set; }

        /// <summary>
        /// Adds to the total propagation delay, by calulating the time it takes to travel from source to destination.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public void AddPropagationDelay(Coordinate source, Coordinate destination)
        {
            TotalPropagationDelay = TotalPropagationDelay + CoordinateHelper.CalculatePropagationDelayInMilliseconds(source, destination);
        }
    }
}
