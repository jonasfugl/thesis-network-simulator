using System;
using System.Collections.Generic;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Helpers;

namespace ConstellationSimulator.AODV
{
    internal class AodvTableEntry
    {
        /// <summary>
        /// Address of the destination for this routing entry.
        /// </summary>
        public string DestinationAddress { get; set; }

        /// <summary>
        /// Destination Sequence Number for this route to the destination.
        /// </summary>
        public int DestinationSequenceNumber { get; set; }

        /// <summary>
        /// Flag to determine if the sequence number is valid or not. Determines if the route is "active" and can be used.
        /// </summary>
        public bool ValidDestinationSequenceNumberFlag { get; set; } 

        /// <summary>
        /// The amount of thops to the destination, when following this route.
        /// </summary>
        public int HopCount { get; set; }

        /// <summary>
        /// Boolean flag to indicate if the route is valid or not.
        /// </summary>
        public bool Valid { get; set; }

        /// <summary>
        /// Address of the next hop neighbour for this route.
        /// </summary>
        public string NextHop { get; set; }

        /// <summary>
        /// List of precursors that may be forwarding packets on this route- List of neighbours likely to use it as a next-hop towards a destination.
        /// RREP has to be sent to a node in the precurser list. If you ask for a RREP, you will send me data, when trying to get to the destination.
        /// Therefore, you are a precurser to this destination (nodes on the forward path, BEFORE me)
        /// </summary>
        public List<string> Precursors = new List<string>();

        /// <summary>
        /// Time at which route expires (set when route is created as "timehelper.currenttime + lifetime") or is deleted, if already expired
        /// </summary>
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// Invalidates the routing entry.
        /// </summary>
        /// <param name="conf">The configuration used for this simulation.</param>
        public void Invalidate(SimulationConfiguration conf)
        {
            Valid = false;
            ExpirationTime = TimeHelper.GetInstance().GetFutureTime(conf.AodvConfiguration.DeletePeriod);
        }

        /// <summary>
        /// Sets the routing entry to active. Opposite actions of Invalidate().
        /// </summary>
        /// <param name="conf"></param>
        public void SetActive(SimulationConfiguration conf)
        {
            Valid = true;
            ExpirationTime = TimeHelper.GetInstance().GetFutureTime(conf.AodvConfiguration.ActiveRouteTimeout);
        }
    }
}
