using System;
using System.Collections.Generic;
using System.Linq;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Helpers;
using ConstellationSimulator.Network;
using ConstellationSimulator.Network.Message;
using ConstellationSimulator.Statistics;

namespace ConstellationSimulator.AODV
{
    internal class AodvNetworkLayer : INetworkLayer
    {
        public AodvNetworkLayer(string localAddress, SimulationConfiguration configuration, Logger logger, LinkLayer link, StatisticsCollector results)
        {
            _aodvHelper = new AodvHelper(localAddress, configuration, logger);
            _localAddress = localAddress;
            _logger = logger;
            _time = TimeHelper.GetInstance();
            _linkLayer = link;
            _statistics = results;
            _conf = configuration;
            _aodvParameters = _conf.AodvConfiguration;
        }

        /// <summary>
        /// Retrieves and processes a new message from the link-layer.
        /// </summary>
        /// <returns>Returns the Message, if it is a DATA message and if it was addressed to this node.</returns>
        public Message GetIncomingMessage()
        {
            //Get new message from link-layer
            var msg = _linkLayer.GetIncomingMessage();
            if (msg == null)
            {
                return null; //No message available in the queue
            }
            
            //If message is DATA and I'm the destination, packet was delivered successfully. Pass to application-layer and return.
            if (msg.Type == MessageType.Data && msg.DestinationAddress == _localAddress)
            {
                return msg;
            }
            //Else if message is DATA and I'm NOT the destination, it is clearly meant to be forwarded
            if (msg.Type == MessageType.Data && msg.DestinationAddress != _localAddress)
            {
                //If I dont have an active route for the destination, I cant forward the message
                if (!_aodvHelper.RoutingTable.ActiveRouteExists(msg.DestinationAddress))
                {
                    _statistics.AddDataMessageDroppedNoActiveForwardingRoute(msg);

                    //Section 6.11 (ii) - RERR
                    var dsn = _aodvHelper.RoutingTable.GetEntry(msg.DestinationAddress)?.DestinationSequenceNumber ?? 0;
                    var unreachableDestinations = new List<UnreachableDestination> {new UnreachableDestination(msg.DestinationAddress, dsn)};

                    //Notify previous hop of link-break
                    var rerrMsg = new Message(new Rerr(unreachableDestinations), msg.PreviousHop, _localAddress, _conf.MessageTtlValue); //normally sent to previous hop
                    SendAodvMessage(rerrMsg, msg.PreviousHop);
                    return null;
                }

                SendDataMessage(msg); //Route exists, forward message along route
            }
            //Message is not DATA (so routing messages). Keep in network layer, for update of the routing table and handling of routing protocol messages (and forward them afterwards, if necessary)
            else
            {
                _statistics.AddAodvMessageReceived(msg);
                HandleIncomingAodvMessage(msg); //Update routing information if required by incoming information. Creates and sends any outgoing messages, as a result of the incoming message

                //Routing-table has maybe been updated with new routes, retry parked messages
                if (_parkedMessages.Count > 0)
                {
                    RetryParkedDataMessages();
                }
            }

            return null;
        }

        /// <summary>
        /// Sends a new DATA message. Looks up the next-hop in the routing table, and passes message to link-layer.
        /// </summary>
        /// <param name="msg"></param>
        public void SendDataMessage(Message msg)
        {
            var outgoingMessage = new Message(msg);
            if (outgoingMessage.Type != MessageType.Data)
            {
                throw new ArgumentException("Sorry mate, DATA-messages only.");
            }
            
            LinkLayerResult result = null;

            //If this is a broadcast-packet, no table-lookup is necessary (and will fail)
            if (outgoingMessage.DestinationAddress == SimulationConstants.BroadcastAddress) //RREQs will always fall into this one, even those processed by me, since destination=broadcast
            {
                result = _linkLayer.SendMessage(outgoingMessage, SimulationConstants.BroadcastAddress); //Broadcasts cannot miss a neighbour
            }

            //If an active route valid for forwarding exists, find next-hop and send message
            if (_aodvHelper.RoutingTable.ActiveRouteExists(outgoingMessage.DestinationAddress))
            {
                //Find route to destination, to acquire next-hop
                var destinationEntry = _aodvHelper.RoutingTable.GetEntry(outgoingMessage.DestinationAddress);

                //Update lifetime of active route to destination when it's used per section 6.2 ending.
                _aodvHelper.RoutingTable.SetRouteActive(destinationEntry.DestinationAddress);

                //Update lifetime of route to next-hop per section 6.2 ending.
                var nextHopRoute = _aodvHelper.RoutingTable.GetEntry(destinationEntry.NextHop);
                if (nextHopRoute != null)
                {
                    _aodvHelper.RoutingTable.SetRouteActive(nextHopRoute.DestinationAddress);
                }

                //Update lifetime of route to originator of data (if it was not sent by me) per section 6.2 ending.
                if (outgoingMessage.SourceAddress != _localAddress)
                {
                    var sourceRoute = _aodvHelper.RoutingTable.GetEntry(outgoingMessage.SourceAddress);
                    if (sourceRoute != null)
                    {
                        _aodvHelper.RoutingTable.SetRouteActive(sourceRoute.DestinationAddress);
                    }
                }

                //Update lifetime of route to previous hop (if it was not sent by me) per section 6.2 ending.
                if (outgoingMessage.PreviousHop != _localAddress)
                {
                    var previousHopRoute = _aodvHelper.RoutingTable.GetEntry(outgoingMessage.PreviousHop);
                    if (previousHopRoute != null)
                    {
                        _aodvHelper.RoutingTable.SetRouteActive(previousHopRoute.DestinationAddress);
                    }
                }

                result = _linkLayer.SendMessage(outgoingMessage, destinationEntry.NextHop); //Send message along route towards destination. If it fails, routes updated above are invalidated, no worries.
            }
            //If active route does not exist, but a RREQ has been sent for this destination and are not expired yet
            else if (!_aodvHelper.RoutingTable.ActiveRouteExists(outgoingMessage.DestinationAddress) && _aodvHelper.RreqAttemptExist(outgoingMessage.DestinationAddress, _localAddress))
            {
                ParkMessage(new Message(outgoingMessage)); //Park message and wait for RREP
                return;
            }
            //If any kind of route does NOT exist, OR existing route is invalid/expired, send a new RREQ for the destination
            else if (!_aodvHelper.RoutingTable.RouteExists(outgoingMessage.DestinationAddress) || (!_aodvHelper.RoutingTable.GetEntry(outgoingMessage.DestinationAddress)?.Valid ?? true))
            {
                ParkMessage(new Message(outgoingMessage));
                _statistics.RoutingRequestGenerated();

                var rreqMsg = _aodvHelper.GenerateInitialRreqMessage(outgoingMessage.DestinationAddress);
                _statistics.AddAodvMessageSent(rreqMsg);
                result = _linkLayer.SendMessage(rreqMsg, SimulationConstants.BroadcastAddress); //No need to check for missing neighbour in result
            }

            if (result.Result == LinkLayerResultType.NextHopNotFound)
            {
                //This is a clear Section 6.11 (i): Link break for next hop of active route while transmiting data (hehe)
                //_statistics.AddMessageDropped(e.DroppedMessage);
                _statistics.AddDataMessageDroppedNextHopUnavailable(result.DroppedMessage);

                //Get a list of all unreachable destinations, because of this link-failure. This also updates and invalidates entries in routing tables.
                var unreachableDestinations = _aodvHelper.RoutingTable.HandleAndGetUnreachableDestinations(result.MissingNeighbour);
                var rerr = new Rerr(unreachableDestinations);

                //If any destinations were affected by this link-failure
                if (unreachableDestinations.Count > 0)
                {
                    //Get all nodes that use the now missing link as a part of their route
                    var precursors = _aodvHelper.RoutingTable.GetEntry(unreachableDestinations[0].UnreachableDestinationAddress).Precursors;

                    foreach (var precursor in precursors)
                    {
                        //Send RERR to precursors if any is available
                        var rerrMsg = new Message(new Rerr(rerr), precursors[0], _localAddress, _conf.MessageTtlValue);
                        SendAodvMessage(rerrMsg, precursor);
                    }
                }
            }
        }

        /// <summary>
        /// Sends an AODV message to the link-layer, and handles next-hop missing errors.
        /// </summary>
        /// <param name="msg">Message to send.</param>
        /// <param name="nextHop">Address of next-hop in the route towards the destination.</param>
        private void SendAodvMessage(Message msg, string nextHop)
        {
            var outgoingMessage = new Message(msg);

            if (outgoingMessage.Type == MessageType.Data)
            {
                throw new ArgumentException("Nope, only AODV messages allowed.");
            }

            _statistics.AddAodvMessageSent(msg);
            var result = _linkLayer.SendMessage(outgoingMessage, nextHop);
            if (result.Result == LinkLayerResultType.NextHopNotFound)
            {
                _statistics.AodvDropped_NextHopUnavailable(result.DroppedMessage);

                //Section 6.11 (ii) - RERR
                var dsn = _aodvHelper.RoutingTable.GetEntry(result.MissingNeighbour)?.DestinationSequenceNumber ??
                          0;
                var unreachableDestinations =
                    new List<UnreachableDestination> {new UnreachableDestination(result.MissingNeighbour, dsn)};

                //Notify neighbours of link-break
                var rerrMsg = new Message(new Rerr(unreachableDestinations),SimulationConstants.BroadcastAddress, _localAddress, _conf.MessageTtlValue); //normally sent to previous hop
                SendAodvMessage(rerrMsg, SimulationConstants.BroadcastAddress);
            }
        }

        /// <summary>
        /// Handles an incoming AODV message, by passing it to the appropriate message handler.
        /// </summary>
        /// <param name="msg">Incoming AODV message.</param>
        private void HandleIncomingAodvMessage(Message msg)
        {
            var incomingPacket = new Message(msg);

            _logger.WriteLine("Processing new incoming " + incomingPacket.Type + " AODV packet.");
            switch (incomingPacket.Type)
            {
                case MessageType.Rreq:
                    HandleRreq(incomingPacket);
                    break;
                case MessageType.Rrep:
                    HandleRrep(incomingPacket);
                    break;
                case MessageType.Rerr:
                    HandleRerr(incomingPacket);
                    break;
                case MessageType.Rrepack:
                    HandleRrepack(incomingPacket);
                    break;
                case MessageType.Hello:
                    HandleHello(incomingPacket);
                    break;
            }
        }

        /// <summary>
        /// Performs mainteance in the AODV network layer. It removes any expired RREQs, and calculates unreachable destinations. Any parked message meant for an unreachable destination
        /// are purged from the list.
        /// </summary>
        public void PerformMaintenance()
        {           
            //Find unreachable destinations (if any), and drop the packets for them
            var unreachableDestinations = _aodvHelper.RemoveExpiredBufferedRreqAttempts(); //Remove expired RREQs in buffer

            foreach (var parkedMessage in _parkedMessages.ToList())
            {
                //If parked message is intended for a now unreachable destination
                if (unreachableDestinations.Contains(parkedMessage.DestinationAddress))
                {
                    _statistics.AddDataMessageDroppedDestinationUnreachable(parkedMessage);
                    _parkedMessages.Remove(parkedMessage);
                }
            }

            //Update RREQs and resend timed-out (but not expired)
            foreach (var retry in _aodvHelper.UpdateBufferedRreqAttempts())
            {
                SendAodvMessage(retry, SimulationConstants.BroadcastAddress);
            }

            _aodvHelper.RoutingTable.InvalidateOrRemoveExpiredRoutes(); //Invalidate routing entries with expired lifetime

            //Broadcast HELLO-messages per section 6.9, if enabled (not required in RFC)
            if (_conf.AODV_UseHelloMessages)
            {
                var hello = new Hello(_localAddress, _aodvHelper.RoutingTable.SequenceNumber, _aodvParameters.AllowedHelloLoss * _aodvParameters.HelloInterval);

                var helloMsg = new Message(hello, SimulationConstants.BroadcastAddress, _localAddress, 1); //Section 6.9 says TTL=1, because it should only survive 1 hop.
                SendAodvMessage(helloMsg, SimulationConstants.BroadcastAddress);
            }
        }

        /// <summary>
        /// Exports the routing table to the supplied path.
        /// </summary>
        /// <param name="path">Folder into which the routing table should be exported.</param>
        public void ExportRoutingTable(string path)
        {
            _aodvHelper.RoutingTable.Print(path);
        }

        /// <summary>
        /// Parks a DATA message for later retry, if the routing table does not include an entry for the destination of the Message.
        /// </summary>
        /// <param name="msg">Message to park.</param>
        private void ParkMessage(Message msg)
        {
            if (msg.Type != MessageType.Data)
            {
                //Message has end up here, because I dont have a route to it.
                //If data message, this is expected and fine. Just park it, and retry later.
                //RREQ should not end up here, since they are broadcast
                //RREP should not end up here, since they are forwarded along an reverse path, established by the RREQs. They can have expired, but shouldnt have.
                //RERR ends up here. If I try to send a 
                    //Error is (ii), where a route a previous hop assumes is active, no longer works for me. I try to send a RERR to the previous hop. I dont have a route to previous hop. RERR is parked and RREQ is sent.
                throw new NotImplementedException("AAAAAAAH!");
            }

            _parkedMessages.Add(new Message(msg));
        }

        /// <summary>
        /// Retries all parked messages, if a new route has been discovered.
        /// </summary>
        private void RetryParkedDataMessages()
        {
            _logger.WriteLine("Retrying " + _parkedMessages.Count + " DATA messages.");

            foreach (var message in _parkedMessages.ToList()) //make a copy of the list, since it may be changed below, and throwing an exception. Correct list is stil being modified.
            {
                if (_aodvHelper.RoutingTable.ActiveRouteExists(message.DestinationAddress))
                {
                    _logger.WriteLine("Retrying DATA to " + message.DestinationAddress + " from " + message.SourceAddress + ". Type: " + message.Type);

                    var result = _parkedMessages.Remove(message); //Remove it from the parked list, since SendMessage will add it again, if it fails
                    if (!result) { throw new NotImplementedException(); }

                    SendDataMessage(message); //Route exists, retry data message
                }
            }
        }

        /// <summary>
        /// Handles an incoming RREQ packet.
        /// </summary>
        /// <param name="incomingPacket"></param>
        private void HandleRreq(Message incomingPacket)
        {
            //Processing RREQ according to section 6.5
            var rreq = (Rreq)incomingPacket.Payload; //Cast message to right type

            //Check that received RREQ has not been received and processed before
            if (_aodvHelper.RreqAttemptExist(rreq.DestinationAddress, rreq.OriginatorAddress))
            {
                _logger.WriteLine("Incoming RREQ from " + rreq.OriginatorAddress + " discarded, since it has been processed before.");
                return; //Discard RREQ
            }

            //RREQ not received before, place in buffer with 0 as attempt number
            _aodvHelper.AddBufferedRreqAttempt(new BufferedRreqAttempt(0, rreq, _conf.AodvConfiguration.PathDiscoveryTime));

            //First, create/update route to previous hop. If route to previous hop exists, update lifetime
            if (_aodvHelper.RoutingTable.ActiveRouteExists(incomingPacket.PreviousHop))
            {
                var prevRoute = _aodvHelper.RoutingTable.GetEntry(incomingPacket.PreviousHop);
                prevRoute.Valid = true;
                prevRoute.HopCount = 1;
                prevRoute.NextHop = incomingPacket.PreviousHop;
                prevRoute.ExpirationTime = _time.GetFutureTime(_aodvParameters.ActiveRouteTimeout);
                
                _logger.WriteLine("RREQ handler: Updating route to previous hop: " + incomingPacket.PreviousHop + " for RREQ #" + rreq.RouteRequestId);
                _aodvHelper.RoutingTable.UpdateEntry(prevRoute);
            }
            else //Route did not exist, so create it
            {
                var prevHopEntry = new AodvTableEntry
                {
                    Valid = true,
                    DestinationAddress = incomingPacket.PreviousHop,
                    DestinationSequenceNumber = 0,
                    ValidDestinationSequenceNumberFlag = false, //(He's my neighbour, so even though I dont know his sequence number, I know that he's there)
                    HopCount = 1, //Only 1 hop to previous neighbour (duh)
                    NextHop = incomingPacket.PreviousHop,
                    ExpirationTime = _time.GetFutureTime(_aodvParameters.ActiveRouteTimeout)
                };

                _logger.WriteLine("RREQ handler: Adding route to previous hop: " + incomingPacket.PreviousHop + " for RREQ #" + rreq.RouteRequestId);
                _aodvHelper.RoutingTable.AddEntry(prevHopEntry);
            }

            //RREQ has not been seen before or was not sent by me, continue to process it.
            rreq.HopCount = rreq.HopCount + 1; //Increment HopCount in RREQ, since me receiving it, was one hop more

            //Create or update reverse route to RREQ Originator
            var oldEntry = _aodvHelper.RoutingTable.GetEntry(rreq.OriginatorAddress);
            var minimalLifetime = _aodvParameters.MinimalLifetime(rreq.HopCount);

            //No route to originator exists, create it
            if (oldEntry == null)
            {
                var originatorRoute = new AodvTableEntry
                {
                    DestinationAddress = rreq.OriginatorAddress,
                    DestinationSequenceNumber = rreq.OriginatorSequenceNumber,
                    ValidDestinationSequenceNumberFlag = true,
                    HopCount = rreq.HopCount,
                    Valid = true,
                    NextHop = incomingPacket.PreviousHop,
                    ExpirationTime = minimalLifetime
                };

                _logger.WriteLine("RREQ handler: Adding route to originator: " + rreq.OriginatorAddress + " for RREQ #" + rreq.RouteRequestId);
                _aodvHelper.RoutingTable.AddEntry(originatorRoute);

            }
            else //Route exists, update existing entry
            {
                var originatorRoute = new AodvTableEntry
                {
                    DestinationAddress = rreq.OriginatorAddress,
                    DestinationSequenceNumber = rreq.OriginatorSequenceNumber > oldEntry.DestinationSequenceNumber ? rreq.OriginatorSequenceNumber : oldEntry.DestinationSequenceNumber, //See section 6.5, action 1.
                    ValidDestinationSequenceNumberFlag = true,
                    HopCount = rreq.HopCount,
                    Valid = true,
                    NextHop = incomingPacket.PreviousHop,
                    ExpirationTime = oldEntry.ExpirationTime > minimalLifetime ? oldEntry.ExpirationTime : minimalLifetime
                };

                _logger.WriteLine("RREQ handler: Updating route to originator: " + rreq.OriginatorAddress + " for RREQ #" + rreq.RouteRequestId);
                _aodvHelper.RoutingTable.UpdateEntry(originatorRoute); //And now we have a route towards the originator that can be used. neat-o.
            }

            //If I am the destination specified in RREQ
            if (rreq.DestinationAddress == _localAddress) //Section 6.6 (i) and section 6.6.1
            {
                //Increment my sequence number, if DSN in RREQ is 1 larger per secton 6.6.1
                if (rreq.DestinationSequenceNumber == _aodvHelper.RoutingTable.SequenceNumber + 1)
                {
                    _aodvHelper.RoutingTable.IncrementSequenceNumber();
                }

                var rrep = new Rrep
                {
                    OriginatorAddress = rreq.OriginatorAddress,
                    DestinationAddress = rreq.DestinationAddress,
                    DestinationSequenceNumber = _aodvHelper.RoutingTable.SequenceNumber,
                    HopCount = 0,
                    Lifetime = _time.GetFutureTime(_aodvParameters.MyRouteTimeout) //Section 6.6.1, see also section 10.
                };

                var responseMsg = new Message(rrep, rreq.OriginatorAddress, _localAddress, _conf.MessageTtlValue);
                SendAodvMessage(responseMsg, incomingPacket.PreviousHop); //Send RREP to previous hop, which is where I received the RREQ from
            }
            //If I have an active route to destination, which upholds requirements in section 6.6 (ii), see section 6.6.2
            else if (_aodvHelper.RoutingTable.ActiveRouteExists(rreq.DestinationAddress) //If I have an active route
                && _aodvHelper.RoutingTable.GetEntry(rreq.DestinationAddress).ValidDestinationSequenceNumberFlag //and the route has a valid dsn
                && _aodvHelper.RoutingTable.GetEntry(rreq.DestinationAddress).DestinationSequenceNumber >= rreq.DestinationSequenceNumber //and route dsn is >= rreq.dsn
                && !rreq.D) //and rreq must NOT only be answered by destination
            {
                var forwardRoute = _aodvHelper.RoutingTable.GetEntry(rreq.DestinationAddress);
                var rrep = new Rrep
                {
                    OriginatorAddress = rreq.OriginatorAddress,
                    DestinationAddress = rreq.DestinationAddress,
                    DestinationSequenceNumber = forwardRoute.DestinationSequenceNumber,
                    HopCount = forwardRoute.HopCount,
                    Lifetime = _time.GetFutureTime((forwardRoute.ExpirationTime - _time.CurrentTime).TotalSeconds) //6.6.2, "subtract current time from expiration time in route table entry"
                };

                //Add previous hop of incoming message to precursor list of destination
                _aodvHelper.RoutingTable.AddPrecursorToEntry(rreq.DestinationAddress, incomingPacket.PreviousHop);

                //Add next-hop of forward route to precursor list of originator
                _aodvHelper.RoutingTable.AddPrecursorToEntry(rreq.OriginatorAddress, forwardRoute.NextHop);

                //Place RREP in outbox
                var responseMsg = new Message(rrep, rreq.OriginatorAddress, _localAddress, _conf.MessageTtlValue);
                SendAodvMessage(responseMsg, incomingPacket.PreviousHop); //Send RREP to previous hop, which is where I received the RREQ from

                //Send gratuitous RREP if required in RREQ, specified in section 6.6.3. Dont send it, if path to destination goes through previousHop
                if (rreq.G)
                {
                    var gratuitousRrep = new Rrep
                    {
                        HopCount = _aodvHelper.RoutingTable.GetEntry(rreq.OriginatorAddress).HopCount,
                        DestinationAddress = rreq.OriginatorAddress,
                        DestinationSequenceNumber = rreq.OriginatorSequenceNumber,
                        OriginatorAddress = rreq.DestinationAddress,
                        Lifetime = _time.GetFutureTime((_aodvHelper.RoutingTable.GetEntry(rreq.OriginatorAddress).ExpirationTime - _time.CurrentTime).TotalSeconds) //"Remaining lifetime of route towards RREQ originator, as known by intermediate node" (expiration time - current time = remaining time)
                    };

                    var gratuitousMsg = new Message(gratuitousRrep, rreq.DestinationAddress, _localAddress, _conf.MessageTtlValue);
                    SendAodvMessage(gratuitousMsg, forwardRoute.NextHop); //Gratuitous RREP sent forward
                }
            }
            //Else, I have no active route and am not the destination
            else
            {
                var outMsg = new Message(incomingPacket);

                rreq.DestinationSequenceNumber = Math.Max(rreq.DestinationSequenceNumber, _aodvHelper.RoutingTable.GetEntry(rreq.DestinationAddress)?.DestinationSequenceNumber ?? -1); //Set DSN to max of rreq.dsn or table[destination].dsn
                outMsg.Payload = new Rreq(rreq);
                outMsg.DestinationAddress = SimulationConstants.BroadcastAddress;

                SendAodvMessage(outMsg, SimulationConstants.BroadcastAddress);
            }
        }

        /// <summary>
        /// Handles an incoming RREP packet.
        /// </summary>
        /// <param name="incomingPacket"></param>
        private void HandleRrep(Message incomingPacket)
        {
            var rrep = (Rrep)incomingPacket.Payload;
            //var outgoingMessages = new Dictionary<string, Message>();

            //Search routing table for route to previous hop (where RREP was received from), and create it, if it does not exists (may now be used for forwarding)
            if (!_aodvHelper.RoutingTable.RouteExists(incomingPacket.PreviousHop))
            {
                var route = new AodvTableEntry
                {
                    Valid = true,
                    DestinationAddress = incomingPacket.PreviousHop,
                    DestinationSequenceNumber = 0,
                    HopCount = 1,
                    NextHop = incomingPacket.PreviousHop,
                    ValidDestinationSequenceNumberFlag = false,
                    ExpirationTime = _time.GetFutureTime(_aodvParameters.ActiveRouteTimeout),
                    Precursors = new List<string> { _aodvHelper.RoutingTable.GetEntry(rrep.OriginatorAddress)?.NextHop }
                };

                _logger.WriteLine("1Adding route entry from received RREP towards previous hop: " + incomingPacket.PreviousHop);
                _aodvHelper.RoutingTable.AddEntry(route);
            }
            else //Route does exist, update what we can infer from the fact that our neighbour forwarded a RREQ to us
            {
                var route = _aodvHelper.RoutingTable.GetEntry(incomingPacket.PreviousHop);
                route.Valid = true;
                route.HopCount = 1;
                route.NextHop = incomingPacket.PreviousHop;
                route.ExpirationTime = _time.GetFutureTime(_aodvParameters.ActiveRouteTimeout);

                _logger.WriteLine("2Updating existing route entry for previous hop from received RREP: " + incomingPacket.PreviousHop);
                _aodvHelper.RoutingTable.UpdateEntry(route);
            }

            //Increment HopCount, since me receiving the RREP, adds another hop to the path
            rrep.HopCount = rrep.HopCount + 1;

            //If route forward in RREP path (towards destination in RREP) does not exist, create it
            if (!_aodvHelper.RoutingTable.RouteExists(rrep.DestinationAddress))
            {
                var destinationRoute = new AodvTableEntry
                {
                    Valid = true,
                    DestinationAddress = rrep.DestinationAddress,
                    DestinationSequenceNumber = rrep.DestinationSequenceNumber,
                    HopCount = rrep.HopCount,
                    ValidDestinationSequenceNumberFlag = true,
                    NextHop = incomingPacket.PreviousHop,
                    ExpirationTime = rrep.Lifetime,
                    Precursors = new List<string>() //Precursor added below, per section 6.7 last paragraph
                }; 
                _logger.WriteLine("Creating route entry for destination from received RREP: " + rrep.DestinationAddress);
                _aodvHelper.RoutingTable.AddEntry(destinationRoute); //If I'm the originator in the RREP, I now have a route to the destination, yay!
            }
            else //Route towards destination in rrep exists
            {
                var entry = _aodvHelper.RoutingTable.GetEntry(rrep.DestinationAddress);

                var identicalDsn = rrep.DestinationSequenceNumber == entry.DestinationSequenceNumber;
                var update = false;

                if (!entry.ValidDestinationSequenceNumberFlag) // Section 6.7 (i)
                {
                    update = true;
                }
                else if (rrep.DestinationSequenceNumber > entry.DestinationSequenceNumber && entry.ValidDestinationSequenceNumberFlag) //Section 6.7 (ii)
                {
                    update = true;
                }
                else if (identicalDsn && !entry.Valid) // Section 6.7 (iii)
                {
                    update = true;
                }
                else if (identicalDsn && rrep.HopCount < entry.HopCount) //Section 6.7 (iv)
                {
                    update = true;
                }

                if (update)
                {
                    entry.Valid = true;
                    entry.ValidDestinationSequenceNumberFlag = true;
                    entry.NextHop = incomingPacket.PreviousHop;
                    entry.HopCount = rrep.HopCount;
                    entry.ExpirationTime = rrep.Lifetime; //Current time + rrep.lifetime
                    entry.DestinationSequenceNumber = rrep.DestinationSequenceNumber;
                    //Precursor added below, per section 6.7 last paragraph

                    _logger.WriteLine("3Updating existing route entry for destination from received RREP: " + rrep.DestinationAddress);
                    _aodvHelper.RoutingTable.UpdateEntry(entry);
                }
            }
            
            //Removes all buffered RREQs I have sent for this destination
            _aodvHelper.RemoveLocalBufferedRreq(rrep.DestinationAddress); 

            //If I'm not the originator (i.e. I am not the one who requested the route)
            if (rrep.OriginatorAddress != _localAddress)
            {
                //Verify that I have a route to the destination. Previous-hop has a routing entry that says I do, otherwise he wouldnt have forwarded the RREP to me.
                if (!_aodvHelper.RoutingTable.ActiveRouteExists(rrep.OriginatorAddress))
                {
                    _statistics.AodvDropped_NoActiveForwardingRoute(incomingPacket);

                    //Well, I dont have a route (even though I should - most likely due to a rrep sent back too late)

                    //Send RERR to previous hop
                    var ud = new List<UnreachableDestination>{new UnreachableDestination(rrep.OriginatorAddress, 0)};
                    var rerrMsg = new Message(new Rerr(ud), incomingPacket.PreviousHop, _localAddress, _conf.MessageTtlValue);
                    SendAodvMessage(rerrMsg, incomingPacket.PreviousHop);

                    return;
                }

                //Add next hop towards originator, to precursors of destination (6.7, last paragraph) 
                //Section 6.7, last paragraph
                //"When any node transmits a RREP, the precursor list for the corresponding DESTINATION node is updated, by adding to it, the next hop node
                //to which the RREP is forwarded." So, "the next hop node to which the RREP is forwarded" is next hop towards ORIGINATOR.
                _aodvHelper.RoutingTable.AddPrecursorToEntry(rrep.DestinationAddress, _aodvHelper.RoutingTable.GetEntry(rrep.OriginatorAddress).NextHop);

                //Update lifetime of reverse route towards originator
                var reverseRoute = _aodvHelper.RoutingTable.GetEntry(rrep.OriginatorAddress);
                var future = _time.GetFutureTime(_aodvParameters.ActiveRouteTimeout);
                reverseRoute.ExpirationTime = reverseRoute.ExpirationTime > future ? reverseRoute.ExpirationTime : future;

                //Forward RREP to node according to routing table towards the originator
                var outMsg = new Message(incomingPacket);
                outMsg.Payload = new Rrep(rrep);

                SendAodvMessage(outMsg, reverseRoute.NextHop);

                _logger.WriteLine("4Updating existing route entry for originator from received RREP: " + rrep.OriginatorAddress);
                _aodvHelper.RoutingTable.UpdateEntry(reverseRoute);
            }
        }

        /// <summary>
        /// Handles an incoming RERR packet.
        /// </summary>
        /// <param name="incomingPacket"></param>
        private void HandleRerr(Message incomingPacket)
        {
            //Section 6.11 (iii)
            var rerr = (Rerr) incomingPacket.Payload;
            var unreachableDestinations = new List<UnreachableDestination>();

            //For each of the received unreachable destinations
            foreach (var unreachableDestination in rerr.UnreachableDestinations)
            {
                var invalidRoute = _aodvHelper.RoutingTable.GetEntry(unreachableDestination.UnreachableDestinationAddress);

                //If I have an active route to that destination, and the route uses the transmitter of the RREP as the next-hop (then route is also invalid for me)
                if (invalidRoute != null && invalidRoute.Valid && invalidRoute.NextHop == incomingPacket.PreviousHop)
                {
                    //Create a list of destinations from the RERR, for which i have a routing entry, that uses the rerr.previoushop as next hop.
                    var ud = new UnreachableDestination(invalidRoute.DestinationAddress, invalidRoute.DestinationSequenceNumber);
                    unreachableDestinations.Add(ud);

                    //Perform invalidation actions from RFC
                    _aodvHelper.RoutingTable.UpdateDestinationSequenceNumber(invalidRoute.DestinationAddress, unreachableDestination.UnreachableDestinationSequenceNumber); //Section 6.11 (iii)
                    _aodvHelper.RoutingTable.InvalidateEntry(invalidRoute.DestinationAddress);
                }
            }

            
            //Find route with largest number of precursors
            var minCount = 0;
            var precursors = new List<string>();

            foreach (var ud in unreachableDestinations)
            {
                var route = _aodvHelper.RoutingTable.GetEntry(ud.UnreachableDestinationAddress);
                var precursorCount = route.Precursors.Count;
                if (precursorCount > minCount)
                {
                    minCount = precursorCount;
                    precursors = route.Precursors;
                }
            }

            if (precursors.Count > 0)
            {
                var msg = new Message(new Rerr(rerr), precursors[0], _localAddress, _conf.MessageTtlValue);
                SendAodvMessage(msg, precursors[0]);
            }
        }

        /// <summary>
        /// Handles an incoming RREPACK packet.
        /// </summary>
        /// <param name="incomingPacket"></param>
        private void HandleRrepack(Message incomingPacket)
        {
            _logger.WriteLine("Received RREPACK from " + incomingPacket.SourceAddress);
            throw new NotImplementedException(); //If RREP-ACK (not used, so scale this back. plz.)
        }

        /// <summary>
        /// Handles an incoming HELLO packet.
        /// </summary>
        /// <param name="incomingPacket"></param>
        private void HandleHello(Message incomingPacket)
        {
            var hello = (Hello) incomingPacket.Payload;

            var route = new AodvTableEntry
            {
                Valid = true,
                DestinationAddress = hello.DestinationAddress,
                DestinationSequenceNumber = hello.DestinationSequenceNumber,
                HopCount = 1,
                NextHop = incomingPacket.PreviousHop,
                ValidDestinationSequenceNumberFlag = true,
                ExpirationTime = new DateTime(hello.Lifetime.Ticks) //Creates an exact copy of the lifetime in DateTime format. 
            };

            //Ensure that we have an active route towards the neighbour
            if (!_aodvHelper.RoutingTable.RouteExists(hello.DestinationAddress))
            {
                _aodvHelper.RoutingTable.AddEntry(route);
            }
            else //Route does exist, update what we can infer from the fact that our neighbour send us a HELLO
            {
                _aodvHelper.RoutingTable.UpdateEntry(route);
            }

            _logger.WriteLine("Processed HELLO from " + hello.DestinationAddress);
        }

        private readonly AodvParameters _aodvParameters;
        private readonly List<Message> _parkedMessages = new List<Message>();
        private readonly StatisticsCollector _statistics;
        private readonly SimulationConfiguration _conf;
        private readonly string _localAddress;
        private readonly AodvHelper _aodvHelper;
        private readonly TimeHelper _time;
        private readonly Logger _logger;
        private readonly LinkLayer _linkLayer;
    }
}
