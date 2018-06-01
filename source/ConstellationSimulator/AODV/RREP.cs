using System;
using ConstellationSimulator.Network.Message;

namespace ConstellationSimulator.AODV
{
    internal class Rrep : IMessagePayload
    {
        public Rrep()
        {
            R = false;
            A = false;
            PrefixSize = 0;
            HopCount = 0;
        }

        public Rrep(Rrep rrep)
        {
            R = rrep.R;
            A = rrep.A;
            PrefixSize = rrep.PrefixSize;
            HopCount = rrep.HopCount;
            DestinationAddress = rrep.DestinationAddress;
            DestinationSequenceNumber = rrep.DestinationSequenceNumber;
            OriginatorAddress = rrep.OriginatorAddress;
            Lifetime = rrep.Lifetime;
        }

        /// <summary>
        /// The size of the RREP in bytes. Size specified in RFC.
        /// </summary>
        public int Size => 20;

        /// <summary>
        /// Type of the message. Set to RREP.
        /// </summary>
        public MessageType Type => MessageType.Rrep;

        /// <summary>
        /// Repair flag; used for multicast
        /// </summary>
        public bool R { get; set; }

        /// <summary>
        /// Acknowledgment required; see sections 5.4 and 6.7.
        /// </summary>
        public bool A { get; set; }

        /// <summary>
        /// If nonzero, the 5-bit Prefix Size specifies that the indicated next hop may be used for any nodes with the same routing prefix(as defined by the Prefix Size) as the requested destination.
        /// </summary>
        public int PrefixSize { get; set; }

        /// <summary>
        /// The number of hops from the Originator IP Address to the Destination IP Address.For multicast route requests this indicates the number of hops to the multicast tree member sending the RREP.
        /// </summary>
        public int HopCount { get; set; }

        /// <summary>
        /// The address of the destination for which a route is supplied.
        /// </summary>
        public string DestinationAddress { get; set; }

        /// <summary>
        /// The destination sequence number associated to the route.
        /// </summary>
        public int DestinationSequenceNumber { get; set; }

        /// <summary>
        /// The address of the node which originated the RREQ for which the route is supplied.
        /// </summary>
        public string OriginatorAddress { get; set; }

        /// <summary>
        /// The time in seconds for which nodes receiving the RREP consider the route to be valid.
        /// </summary>
        public DateTime Lifetime { get; set; }
    }
}
