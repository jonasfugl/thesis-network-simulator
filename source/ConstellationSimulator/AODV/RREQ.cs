using ConstellationSimulator.Network.Message;

namespace ConstellationSimulator.AODV
{
    internal class Rreq : IMessagePayload
    {
        public Rreq() {}

        public Rreq(Rreq rreq) //Copy constructor
        {
            J = rreq.J;
            R = rreq.R;
            G = rreq.G;
            D = rreq.D;
            U = rreq.U;
            HopCount = rreq.HopCount;
            RouteRequestId = rreq.RouteRequestId;
            DestinationAddress = rreq.DestinationAddress;
            DestinationSequenceNumber = rreq.DestinationSequenceNumber;
            OriginatorAddress = rreq.OriginatorAddress;
            OriginatorSequenceNumber = rreq.OriginatorSequenceNumber;
        }

        /// <summary>
        /// The size of the RREQ in bytes. Size specified in RFC.
        /// </summary>
        public int Size => 24;

        /// <summary>
        /// The type of the message, set to RREQ.
        /// </summary>
        public MessageType Type => MessageType.Rreq;

        /// <summary>
        /// Join flag, reserved for multicast.  
        /// </summary> 
        public bool J { get; set; }

        /// <summary>
        /// Repair-flag, reserved for multicast.
        /// </summary> 
        public bool R { get; set; }

        /// <summary>
        /// Gratuitous RREP flag; indicates whether a gratuitous RREP should be unicast to the node specified in the Destination IP Address field(see sections 6.3, 6.6.3).
        /// </summary> 
        public bool G { get; set; }

        /// <summary>
        /// Destination only flag; indicates only the destination may respond to this RREQ(see section 6.5).
        /// </summary> 
        public bool D { get; set; }

        /// <summary>
        /// Unknown destination sequence number flag; indicates the destination sequence number is unknown (see section 6.3).
        /// </summary> 
        public bool U { get; set; }

        /// <summary>
        /// The number of hops from the Originator IP Address to the node handling the request.
        /// </summary> 
        public int HopCount { get; set; }

        /// <summary>
        /// A sequence number uniquely identifying the particular RREQ when taken in conjunction with the originating node's IP address.
        /// </summary> 
        public int RouteRequestId { get; set; }

        /// <summary>
        /// The address of the destination for which a route is desired.
        /// </summary> 
        public string DestinationAddress { get; set; }

        /// <summary>
        /// The latest sequence number received in the past by the originator for any route towards the destination.
        /// </summary> 
        public int DestinationSequenceNumber { get; set; }

        /// <summary>
        /// The address of the node which originated the Route Request.
        /// </summary> 
        public string OriginatorAddress { get; set; }

        /// <summary>
        /// The current sequence number to be used in the route entry pointing towards the originator of the route request.
        /// </summary> 
        public int OriginatorSequenceNumber { get; set; }
    }
}
