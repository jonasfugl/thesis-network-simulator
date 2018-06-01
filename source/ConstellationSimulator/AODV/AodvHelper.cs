using System;
using System.Collections.Generic;
using System.Linq;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Helpers;
using ConstellationSimulator.Network.Message;

namespace ConstellationSimulator.AODV
{
    internal class AodvHelper
    {
        public AodvHelper(string localAddr, SimulationConfiguration conf, Logger logger)
        {
            _localAddress = localAddr;
            _logger = logger;
            RoutingTable = new AodvRoutingTable(_localAddress, logger, conf);
            _conf = conf;
            _time = TimeHelper.GetInstance();
            _aodvParameters = _conf.AodvConfiguration;
        }

        /// <summary>
        /// Determines if a RREQ attempt exists for this destination/originator pair.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="originator"></param>
        /// <returns></returns>
        public bool RreqAttemptExist(string destination, string originator)
        {
            var rreqAttempt = _bufferedRreqAttempts.Find(attempt => attempt.DestinationAddress == destination && attempt.OriginatorAddress == originator);
            return rreqAttempt != null;
        }

        /// <summary>
        /// Retrieves a RREQ attempt for the supplied destination and originator, if it exists.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="originator"></param>
        /// <returns>The found RREQ attempt or null if none exists.</returns>
        public BufferedRreqAttempt GetBufferedRreqAttempt(string destination, string originator)
        {
            return _bufferedRreqAttempts.Find(attempt => attempt.DestinationAddress == destination && attempt.OriginatorAddress == originator);
        }

        /// <summary>
        /// Buffers a sent RREQ.
        /// </summary>
        /// <param name="rreqAttempt">The RREQ attempt to buffer.</param>
        public void AddBufferedRreqAttempt(BufferedRreqAttempt rreqAttempt)
        {
            _bufferedRreqAttempts.Add(rreqAttempt);
        }

        /// <summary>
        /// Removes a buffered RREQ for the supplied destination, if the RREQ originated from this node.
        /// </summary>
        /// <param name="destination">Name of destination</param>
        public void RemoveLocalBufferedRreq(string destination)
        {
            _bufferedRreqAttempts.RemoveAll(attempt => attempt.DestinationAddress == destination && attempt.OriginatorAddress == _localAddress);
        }

        /// <summary>
        /// Generates a list of new RREQs to send, since the original has timed out without response, but retries are still allowed.
        /// </summary>
        /// <returns>A list of Messages to send, containing RREQs.</returns>
        public List<Message> UpdateBufferedRreqAttempts()
        {
            var rreqRetries = new List<Message>();

            //Buffered RREQ attempts also contains RREQs buffered from other destinations when received. Only update if originator=me.
            //RREQs not from me will not be updated, and therefore removed when they expire.
            var myAttempts = _bufferedRreqAttempts.FindAll(rreq => rreq.OriginatorAddress == _localAddress);
            foreach (var rreqAttempt in myAttempts)
            {
                //If RREQ has timed out without a response (expired)
                if (_time.CurrentTime > rreqAttempt.ExpirationTime)
                {
                    //We still have some attempts left to try and discover a route. Detals per section 6.4
                    rreqAttempt.AttemptNo++; //Increase attempt number
                    rreqAttempt.ExpirationTime = _time.GetFutureTime(Math.Pow(2, rreqAttempt.AttemptNo) * _aodvParameters.NetTraversalTime); //Binary exponentiel backoff
                    rreqAttempt.Rreq.RouteRequestId = RoutingTable.NextRouteRequestId; //Update id on each new RREQ

                    var msg = new Message(new Rreq(rreqAttempt.Rreq), SimulationConstants.BroadcastAddress, _localAddress, _conf.MessageTtlValue);
                    msg.Ttl = _aodvParameters.TtlStart + (rreqAttempt.AttemptNo * _aodvParameters.TtlIncrement); //Section 6.4
                    if (msg.Ttl > _aodvParameters.TtlThreshold)
                    {
                        rreqAttempt.RetryNo++;   
                        msg.Ttl = _aodvParameters.NetDiameter;
                    }

                    //Routing table entry waiting for a RREP SHOULD NOT be expunged before (current_time + 2 * NET_TRAVERSAL_TIME).
                    //Entries does not exists for initial rreqs, since they are trying to discover a route.
                    //From section 6.4 ending.
                    var waitingEntry = RoutingTable.GetEntry(rreqAttempt.DestinationAddress);
                    if (waitingEntry != null)
                    {
                        RoutingTable.UpdateExpirationTime(rreqAttempt.DestinationAddress, _time.GetFutureTime(2 * _aodvParameters.NetTraversalTime));
                    }

                    var index = _bufferedRreqAttempts.FindIndex(attempt =>
                        attempt.RouteRequestId == rreqAttempt.RouteRequestId &&
                        attempt.OriginatorAddress == rreqAttempt.OriginatorAddress);

                    _bufferedRreqAttempts[index] = rreqAttempt; //Replace current attempt with new one
                    rreqRetries.Add(msg);
                }
            }

            return rreqRetries;
        }

        /// <summary>
        /// Removes buffered RREQ attempts, when the retry limit has been reached. Also purges RREQs received from other sources, when they expire.
        /// </summary>
        /// <returns>A list of destination addresses, that are deemed unreachable, due to the RREQ retry being maxed out without results.</returns>
        public List<string> RemoveExpiredBufferedRreqAttempts()
        {
            var unreachableDestinations = new List<string>();

            foreach (var attempt in _bufferedRreqAttempts.ToList()) //Go through all buffered RREQs; also those received from neighbours
            {
                //If RREQ has expired (timed out without a response), and I have retried the maximum allowed times
                if (attempt.OriginatorAddress == _localAddress && attempt.RetryNo >= _aodvParameters.RreqRetries)
                {
                    _bufferedRreqAttempts.Remove(attempt); //Destination is unreachable, since we have maxed out our retries without discovering a route. Remove it from attemp-list.
                    unreachableDestinations.Add(attempt.DestinationAddress); //Mark destination as unreachable
                }
                //if not sent by me, purge when expired
                else if (attempt.OriginatorAddress != _localAddress && _time.CurrentTime >= attempt.ExpirationTime)
                {
                    _bufferedRreqAttempts.Remove(attempt);
                }
            }

            return unreachableDestinations; //Notify upper layer of unreachable destination, and drop the parked packets there.
        }

        /// <summary>
        /// Generates a new Message, containing a RREQ for the wanted destination.
        /// Implemented according to Section 6.3: Generating Route Requests (RREQ) in the RFC.
        ///
        /// Method also handles buffering of RREQ, to avoid sending a duplicate in next iteration.
        /// </summary>
        /// <param name="destination">String representation of the destination address.</param>
        /// <returns>A Message with the RREQ enclosed. Includes destination address set to broadcast, and TTL adjusted according to the RFC.</returns>
        public Message GenerateInitialRreqMessage(string destination)
        {
            //If RREQ has been sent for this destination by me, and has not yet timed out, dont send a new one
            if (RreqAttemptExist(destination, _localAddress) && _time.CurrentTime < GetBufferedRreqAttempt(destination, _localAddress).ExpirationTime)
            {
                 _logger.WriteLine("RREQ has been sent for this destination, and has not yet timed out, so dont send a new one.");
                return null;
            }

            //Else, no RREQ for this destination waiting for response, send a new one
            RoutingTable.IncrementSequenceNumber(); //Must be incremented before sending a RREQ, per section 6.1 and 6.3

            var knownEntry = RoutingTable.GetEntry(destination); //Get stale entry if it exists

            var rreq = new Rreq
            {
                G = _conf.AODV_UseGratuitousRREPs,
                D = _conf.AODV_DestinationRespondOnlyFlag,
                U = knownEntry == null, //True if no current DSN is known, per section 6.3
                HopCount = 0,
                RouteRequestId = RoutingTable.NextRouteRequestId,
                DestinationAddress = destination,
                DestinationSequenceNumber = knownEntry?.DestinationSequenceNumber ?? 0, //Known DSN and U=false, or unknown DSN=0 and U=true
                OriginatorAddress = _localAddress,
                OriginatorSequenceNumber = RoutingTable.SequenceNumber
            };

            //Any stale routing table entry waiting for a RREP (because of the newly generated RREQ) should not be expunged before current_time + 2 * NET_TRAVERSAL_TIME
            if (knownEntry != null)
            {
                knownEntry.ExpirationTime = _time.GetFutureTime(2 * _aodvParameters.NetTraversalTime);
            }

            //Create Message to send RREQ in, and adjust RREQ
            var msg = new Message(rreq, SimulationConstants.BroadcastAddress, _localAddress, _conf.MessageTtlValue);
            msg.Ttl = knownEntry?.HopCount + _aodvParameters.TtlIncrement ?? _aodvParameters.TtlStart; //Per section 6.4, use last known hopcount as initial TTL + TTL_INCREMENT

            //Buffer RREQ ID and Originator address, pr. section 6.3 (taken from rreq)
            var rreqAttempt = new BufferedRreqAttempt(0, rreq, _conf.AodvConfiguration.PathDiscoveryTime);
            rreqAttempt.ExpirationTime = _time.GetFutureTime(_aodvParameters.PathDiscoveryTime);
            AddBufferedRreqAttempt(rreqAttempt);

            return msg;
        }


        public readonly AodvRoutingTable RoutingTable;
        private readonly AodvParameters _aodvParameters;
        private readonly string _localAddress;
        private readonly TimeHelper _time;
        private readonly Logger _logger;
        private readonly SimulationConfiguration _conf;
        private readonly List<BufferedRreqAttempt> _bufferedRreqAttempts = new List<BufferedRreqAttempt>();
    }
}
