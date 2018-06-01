using System.Collections.Generic;
using ConstellationSimulator.Network.Message;

namespace ConstellationSimulator.AODV
{
    /// <summary>
    /// The RERR-message. Fields are extracted from the RFC.
    /// </summary>
    internal class Rerr : IMessagePayload
    {
        public Rerr(List<UnreachableDestination> unreachableDestinations)
        {
            UnreachableDestinations = unreachableDestinations;
        }

        public Rerr(Rerr rerr)
        {
            N = rerr.N;
            UnreachableDestinations = new List<UnreachableDestination>();

            //Deep copy of list content
            foreach (var ud in rerr.UnreachableDestinations)
            {
                UnreachableDestinations.Add(new UnreachableDestination(ud));
            }
        }

        /// <summary>
        /// Size of message. Is 12 bytes if only one destination, but adds 8 bytes for each additional unreachable destination enclosed (section 5.3)
        /// </summary>
        public int Size => 12 + DestinationCount * 8;

        /// <summary>
        /// The type of the message, sat to RERR.
        /// </summary>
        public MessageType Type => MessageType.Rerr;

        /// <summary>
        /// No delete flag; set when a node has performed a local repair of a link, and upstream nodes should not delete the route.
        /// </summary>
        public bool N { get; set; }

        /// <summary>
        /// The number of unreachable destinations included in the message; MUST be at least 1.
        /// </summary>
        public int DestinationCount => UnreachableDestinations.Count;

        /// <summary>
        /// List of unreachable destinations. Contains address and destination sequence number for each.
        /// Is a list representation of these two fields in the original message specification.
        /// </summary>
        public List<UnreachableDestination> UnreachableDestinations { get; }
    }
}
