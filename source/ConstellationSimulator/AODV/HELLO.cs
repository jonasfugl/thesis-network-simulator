using System;
using ConstellationSimulator.Helpers;
using ConstellationSimulator.Network.Message;

namespace ConstellationSimulator.AODV
{
    /// <summary>
    /// The HELLO-message. Fields are extracted from the RFC.
    /// </summary>
    internal class Hello : IMessagePayload
    {
        public Hello(string destination, int dsn, int lifetime)
        {
            DestinationAddress = destination;
            DestinationSequenceNumber = dsn;
            Lifetime = TimeHelper.GetInstance().GetFutureTime(lifetime);
        }

        public Hello(Hello msg)
        {
            DestinationAddress = msg.DestinationAddress;
            DestinationSequenceNumber = msg.DestinationSequenceNumber;
            Lifetime = msg.Lifetime;
        }

        /// <summary>
        /// The type of the message, sat to HELLO.
        /// </summary>
        public MessageType Type => MessageType.Hello;

        /// <summary>
        /// The size in bytes of the HELLO message. HELLOs are normally sent as a RREP, so equal size to them.
        /// </summary>
        public int Size => 20;

        /// <summary>
        /// The destination address of the HELLO. This is the name of the node broadcasting the HELLO.
        /// </summary>
        public string DestinationAddress { get; }
        
        /// <summary>
        /// The current Sequence Number from the routing table of the node broadcasting the HELLO.
        /// </summary>
        public int DestinationSequenceNumber { get; }

        /// <summary>
        /// Amount of hops to the destination. Are incremented at destination.
        /// </summary>
        public int HopCount => 0;

        /// <summary>
        /// The time of which this HELLO message indicates that the node broadcasting it is alive.
        /// </summary>
        public DateTime Lifetime { get; }
    }
}
