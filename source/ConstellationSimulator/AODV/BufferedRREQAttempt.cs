using System;
using ConstellationSimulator.Helpers;

namespace ConstellationSimulator.AODV
{
    internal class BufferedRreqAttempt
    {
        public BufferedRreqAttempt(int attemptNo, Rreq rreq, double expirationTime)
        {
            AttemptNo = attemptNo;
            RetryNo = 0;
            Rreq = new Rreq(rreq);
            ExpirationTime = TimeHelper.GetInstance().GetFutureTime(expirationTime);
        }

        /// <summary>
        /// The destination address located in the RREQ.
        /// </summary>
        public string DestinationAddress => Rreq.DestinationAddress;

        /// <summary>
        /// The originator address located in the RREQ.
        /// </summary>
        public string OriginatorAddress => Rreq.OriginatorAddress;

        /// <summary>
        /// The RREQ paylod being buffered.
        /// </summary>
        public Rreq Rreq { get; set; }

        /// <summary>
        /// The amount of times it has been attempted to establish a route to this destination.
        /// </summary>
        public int AttemptNo { get; set; }

        /// <summary>
        /// The number of times it has been attempted to establish a route to this destination, with the TTL value in the Message sat to NET_DIAMETER.
        /// </summary>
        public int RetryNo { get; set; }

        /// <summary>
        /// The ID of this Route Request
        /// </summary>
        public int RouteRequestId => Rreq.RouteRequestId;

        /// <summary>
        /// The time when this RREQ attempt expires, so a new retry can be made, or the destination marked as unreachable.
        /// </summary>
        public DateTime ExpirationTime { get; set; }
    }
}
