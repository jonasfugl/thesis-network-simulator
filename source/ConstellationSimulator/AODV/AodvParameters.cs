using System;
using ConstellationSimulator.Helpers;

namespace ConstellationSimulator.AODV
{
    /// <summary>
    /// Containts the constants defined in the RFC, necessary to calculate delays, timeout periods and expiration times.
    /// </summary>
    public class AodvParameters
    {
        internal AodvParameters(int activeRouteTimeoutValue, int secondsPerTickvalue, int satsPerOrbit, int numberOfOrbits, bool crossSeam)
        {
            ActiveRouteTimeout = activeRouteTimeoutValue;
            NodeTraversalTime = secondsPerTickvalue; //It takes minimum this "time" for a node to process the data, due to the design of the simulator
            NetDiameter = (satsPerOrbit - satsPerOrbit % 2) / 2 + 
                          (crossSeam
                ? (numberOfOrbits - numberOfOrbits % 2) / 2
                : numberOfOrbits - 1);
            RerrRateLimit = 10;
            RreqRetries = 2;
            RreqRateLimit = 10;
            TimeoutBuffer = 0;
            TtlStart = 1;
            TtlIncrement = 2;
            TtlThreshold = 7;
            TtlValue = 0;
            HelloInterval = secondsPerTickvalue;
            LocalAddTtl = 2;
            AllowedHelloLoss = 2;
            MinRepairTtl = 000;

            MaxRepairTtl = 0.3 * NetDiameter;
            NetTraversalTime = 2 * NodeTraversalTime * NetDiameter;
            BlacklistTimeout = RreqRetries * NetTraversalTime;
            DeletePeriod = 5 * Math.Max(ActiveRouteTimeout, HelloInterval); //K=5
            MyRouteTimeout = 2 * ActiveRouteTimeout;
            NextHopWait = NodeTraversalTime + 10;
            PathDiscoveryTime = 2 * NetTraversalTime;
        }


        /// <summary>
        /// RFC3561 value: 3000 milliseconds
        /// </summary>
        public int ActiveRouteTimeout { get; }

        /// <summary>
        /// RFC3561 value: 2
        /// </summary>
        public int AllowedHelloLoss { get; }

        /// <summary>
        /// This period should be set to the upper bound of the time it takes to perform the allowed number of route
        /// request retry attempts as described in section 6.3. See also section 6.8.
        /// RFC3561 value: RREQ_RETRIES * NET_TRAVERSAL_TIME.
        /// </summary>
        public double BlacklistTimeout { get; }

        /// <summary>
        /// DELETE_PERIOD is intended to provide an upper bound on the time for which an upstream node A can have a neighbor B as an
        /// active next hop for destination D, while B has invalidated the route to D.
        /// RFC3561 value: DELETE_PERIOD = K * max (ACTIVE_ROUTE_TIMEOUT, HELLO_INTERVAL), where (K = 5 is recommended).
        /// </summary>
        public int DeletePeriod { get; }

        /// <summary>
        /// RFC3561 value: 1000 milliseconds
        /// </summary>
        public int HelloInterval { get; }

        /// <summary>
        /// RFC3561 value: 2
        /// </summary>
        public int LocalAddTtl { get; }

        /// <summary>
        /// RFC3561 value: 0.3 * NET_DIAMETER
        /// </summary>
        public double MaxRepairTtl { get; }

        /// <summary>
        /// RFC3561 value: //TODO:
        /// </summary>
        public int MinRepairTtl { get; }

        /// <summary>
        /// RFC3561 value: 2 * ACTIVE_ROUTE_TIMEOUT
        /// </summary>
        public int MyRouteTimeout { get; }

        /// <summary>
        /// NET_DIAMETER measures the maximum possible number of hops between two nodes in the network.
        /// RFC3561 value: 35
        /// </summary>
        public int NetDiameter { get; }

        /// <summary>
        /// RFC3561 value: 2* NODE_TRAVERSAL_TIME * NET_DIAMETER
        /// </summary>
        public double NetTraversalTime { get; }

        /// <summary>
        /// RFC3561 value: NODE_TRAVERSAL_TIME + 10
        /// </summary>
        public double NextHopWait { get; }

        /// <summary>
        /// NODE_TRAVERSAL_TIME is a conservative estimate of the average one hop traversal time for
        /// packets and should include queuing delays, interrupt processing times and transfer times.
        /// RFC3561 value: 40 milliseconds
        /// </summary>
        public double NodeTraversalTime { get; } //Seconds. Equal to SecondsPerTick in Simulator

        /// <summary>
        /// RFC3561 value: 2 * NET_TRAVERSAL_TIME
        /// </summary>
        public double PathDiscoveryTime { get; } //Seconds

        /// <summary>
        /// RFC3561 value: 10
        /// </summary>
        public int RerrRateLimit { get; }

        /// <summary>
        /// RFC3561 value: 2 * NODE_TRAVERSAL_TIME * (TTL_VALUE + TIMEOUT_BUFFER)
        /// </summary>
        public double RingTraversalTime(int ttlValue)
        {
            return 2 * NodeTraversalTime * (ttlValue + TimeoutBuffer);
        }

        /// <summary>
        /// RFC3561 value: 2
        /// </summary>
        public int RreqRetries { get; }

        /// <summary>
        /// A node SHOULD NOT originate more than RREQ_RATELIMIT RREQ messages per second.
        /// RFC3561 value: 10
        /// </summary>
        public int RreqRateLimit { get; }

        /// <summary>
        /// Its purpose is to provide a buffer for the timeout so that if the RREP is delayed due to congestion,
        /// a timeout is less likely to occur while the RREP is still en route back to the source.To omit this buffer,
        /// set TIMEOUT_BUFFER = 0.
        /// Specified value = 2.
        /// </summary>
        public int TimeoutBuffer { get; }

        /// <summary>
        /// RFC3561 value: 1
        /// </summary>
        public int TtlStart { get; }

        /// <summary>
        /// RFC3561 value: 2
        /// </summary>
        public int TtlIncrement { get; }

        /// <summary>
        /// RFC3561 value: 7
        /// </summary>
        public int TtlThreshold { get; }

        /// <summary>
        /// TTL_VALUE is the value of the TTL field in the IP header while the expanding
        /// ring search is being performed.This is described further in section 6.4.
        /// </summary>
        public int TtlValue { get; }

        /// <summary>
        /// RFC3561 value: (current time + 2 * NET_TRAVERSAL_TIME - 2 * HopCount * NODE_TRAVERSAL_TIME).
        /// </summary>
        public DateTime MinimalLifetime(int hopCount)
        {
            return TimeHelper.GetInstance().GetFutureTime(2 * NetTraversalTime - 2 * hopCount * NodeTraversalTime);
        }
    }
}
