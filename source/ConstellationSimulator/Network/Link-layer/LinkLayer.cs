using System;
using System.Collections.Generic;
using System.Linq;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Helpers;
using ConstellationSimulator.Positions;
using ConstellationSimulator.Statistics;

namespace ConstellationSimulator.Network
{
    internal class LinkLayer
    {
        public LinkLayer(SimulationConfiguration conf, List<List<Satellite>> constellation, Satellite thisSatellite, Logger logger, StatisticsCollector results)
        {
            _conf = conf;
            _constellation = constellation;
            _thisSatellite = thisSatellite;
            _logger = logger;
            _statistics = results;

            _lastNeighbours = new Dictionary<string, Satellite>();
        }

        /// <summary>
        /// Get incoming message, sent from another satellite.
        /// </summary>
        public Message.Message GetIncomingMessage()
        {
            if (_inputQueue.Count > 0)
            {
                var msg = _inputQueue.Dequeue();
                return msg;
            }

            return null;
        }

        /// <summary>
        /// Place a new message in the incoming-queue of this satellite.
        /// </summary>
        public void ReceiveMessage(Message.Message incomingMessage)
        {
            //Receive message with some probability
            if (RandomHelper.ReceiveData(_conf.LinkLayerReceptionProbability) && _inputQueue.Count < _conf.LinkBufferSize)
            {
                _statistics.LinkLayerAddReceivedMessage(incomingMessage);
                _inputQueue.Enqueue(new Message.Message(incomingMessage));
            }
            else
            {
                _statistics.LinkLayerAddDroppedMessage(incomingMessage);
            }
        }

        /// <summary>
        /// Send a message to another satellite.
        /// </summary>
        public LinkLayerResult SendMessage(Message.Message outgoingMessage, string nextHop)
        {
            var msg = new Message.Message(outgoingMessage); //Create copy of message

            //Dont forward messages with TTL == 0, pr. https://tools.ietf.org/html/rfc1812#section-4.2.2.9 / https://networkengineering.stackexchange.com/questions/10929/when-is-an-ipv4-ttl-decremented-and-what-is-the-drop-condition
            if (msg.Ttl <= 0)
            {
                _statistics.LinkLayerTtlExpired(msg);
                _logger.WriteLine("TTL reached 0 for " + msg.Type + " packet to " + msg.DestinationAddress + ", dropping packet.");
                return new LinkLayerResult(LinkLayerResultType.TtlExpired);
            }

            msg.PreviousHop = _thisSatellite.LocalAddress;
            msg.DecrementTtl(); //Decrement TTL on egress part of router

            var neighbours = GetAvailableLinks();
            var availableLinks = string.Join(", ", from item in neighbours.Values select item.LocalAddress);

            //If broadcast to all neighbours
            if (msg.DestinationAddress == SimulationConstants.BroadcastAddress || nextHop == SimulationConstants.BroadcastAddress)
            {
                _logger.WriteLine("Sending " + msg.Type + " broadcast packet. Destination: " + msg.DestinationAddress + ", Source: " + msg.SourceAddress + " Previous-hop: " + msg.PreviousHop + ", Next-hops: " + availableLinks);

                foreach (var link in neighbours)
                {
                    msg.AddPropagationDelay(_thisSatellite.Location, link.Value.Location);
                    link.Value.ReceiveNewMessage(new Message.Message(msg));
                }
            }
            else //Unicast towards specific neighbour (if available)
            {
                //If neighbour is not found, throw exception
                if (!neighbours.ContainsKey(nextHop))
                {
                    _logger.WriteLine("ERROR - LINK NOT FOUND. NextHop: " + nextHop + ", Available links: " + availableLinks);
                    return new LinkLayerResult(new Message.Message(msg), nextHop);
                }

                _logger.WriteLine("Sending packet. Type: " + msg.Type + ", Destination: " + msg.DestinationAddress + ", Source: " + msg.SourceAddress + " Previous-hop: " + msg.PreviousHop + ", Next-hop: " + nextHop + ", Available links: " + availableLinks);
                msg.AddPropagationDelay(_thisSatellite.Location, neighbours[nextHop].Location);
                neighbours[nextHop].ReceiveNewMessage(new Message.Message(msg));
            }

            _statistics.LinkLayerAddSentMessage(msg);
            return new LinkLayerResult(LinkLayerResultType.Success);
        }

        /// <summary>
        /// Number of messages remaining in link-layer incoming buffer.
        /// </summary>
        public int IncomingMessageCount => _inputQueue.Count;
        
        private Dictionary<string, Satellite> GetAvailableLinks()
        {
            if (_thisSatellite.LogicalPosition.SatNumber == _lastSatellitePosition)
            {
                return _lastNeighbours; //No problem on first run, since last position is initialized to -1
            }

            var neighbours = new Dictionary<string, Satellite>();

            _lastNeighbours.Clear();//Clear saved last neighbours
            _lastSatellitePosition = _thisSatellite.LogicalPosition.SatNumber;

            foreach (NeighbourDirection dir in Enum.GetValues(typeof(NeighbourDirection)))
            {
                var nb = FindNeighbour(dir);
                if (nb != null) //Neighbour will be null for cross-seam links, if cross-seam communication is disabled. null should not be added to the list (and throws exceptions, if we try, so there's that)
                {
                    neighbours.Add(nb.LocalAddress, nb);
                    _lastNeighbours.Add(nb.LocalAddress, nb);
                }
            }

            return neighbours;
        }


        /// <summary>
        /// Finds the available neighbours, depending on the logical location of this satellite, in the provided direction.
        /// </summary>
        /// <param name="direction">The direction for which the neighbour is searched for. Can be Up, Down, Left and Right.</param>
        /// <returns>The Satellite neighbour in the specified direction.</returns>
        private Satellite FindNeighbour(NeighbourDirection direction)
        {
            Satellite neighbour = null;

            if (direction == NeighbourDirection.Up)
            {
                neighbour = _constellation[_thisSatellite.SatPosition.OrbitNumber][(_thisSatellite.SatPosition.SatNumber + 1) % _conf.NumberOfSatellitesPerOrbit];
            }
            if (direction == NeighbourDirection.Down)
            {
                neighbour = _constellation[_thisSatellite.SatPosition.OrbitNumber][(_thisSatellite.SatPosition.SatNumber + _conf.NumberOfSatellitesPerOrbit - 1) % _conf.NumberOfSatellitesPerOrbit];
            }

            var hemisphere = CoordinateHelper.CalcHemisphere(_thisSatellite.Location);

            if (direction == NeighbourDirection.Right && _conf.NumberOfOrbits > 1)
            {
                var neighbourOrbitRight = 0;
                var rightCrossSeamOrbit = 0;

                if (hemisphere == CoordinateHelper.Hemisphere.NorthEast || hemisphere == CoordinateHelper.Hemisphere.SouthEast) //If this satellite is in the eastern hemisphere
                {
                    neighbourOrbitRight = (_thisSatellite.SatPosition.OrbitNumber + 1) % _conf.NumberOfOrbits; //Calculate number of next orbit
                    rightCrossSeamOrbit = _conf.MinimumOrbitNumber;

                    if (neighbourOrbitRight > 0) //If orbit to my right is NOT across the seam
                    {
                        neighbour = _constellation[neighbourOrbitRight][_thisSatellite.SatPosition.SatNumber]; //My right neighbour is my initial neighbour
                    }
                }
                else if (hemisphere == CoordinateHelper.Hemisphere.NorthWest || hemisphere == CoordinateHelper.Hemisphere.SouthWest) //If this satellite is in the western hemisphere (mirrored constellation)
                {
                    neighbourOrbitRight = (_thisSatellite.SatPosition.OrbitNumber + (_conf.NumberOfOrbits - 1)) % _conf.NumberOfOrbits; //Calculate number of next orbit
                    rightCrossSeamOrbit = _conf.MaximumOrbitNumber;

                    if (neighbourOrbitRight < _conf.MaximumOrbitNumber) //If orbit to my right is NOT across the seam
                    {
                        neighbour = _constellation[neighbourOrbitRight][_thisSatellite.SatPosition.SatNumber];
                    }
                }

                //If my orbit is the last one, i.e. right neighbour is crossing the seam AND cross-seam communication is enabled (otherwise this satellite has no right neighbour right now)
                if (neighbourOrbitRight == rightCrossSeamOrbit && _conf.CrossSeamCommunicationEnabled)
                {
                    var ticks = _thisSatellite.LogicalPosition.SatNumber - _thisSatellite.SatPosition.SatNumber;
                    if (ticks < 0) //How many ticks since original position
                    {
                        ticks = _conf.NumberOfSatellitesPerOrbit - Math.Abs(_thisSatellite.LogicalPosition.SatNumber - _thisSatellite.SatPosition.SatNumber); //10
                    }

                    //Original neighbour position
                    var initNb = (_conf.NumberOfSatellitesPerOrbit - _thisSatellite.SatPosition.SatNumber - (_conf.NumberOfSatellitesPerOrbit / 2)) % _conf.NumberOfSatellitesPerOrbit;
                    if (initNb < 0)
                        initNb = _conf.NumberOfSatellitesPerOrbit - Math.Abs(initNb);

                    //Current neighbour
                    var newNb = initNb - (ticks * 2) % _conf.NumberOfSatellitesPerOrbit; //Each "tick" shifts cross-seam neighbours by 2
                    if (newNb < 0)
                        newNb = _conf.NumberOfSatellitesPerOrbit - Math.Abs(newNb);

                    neighbour = _constellation[neighbourOrbitRight][newNb];
                }
            }
            if (direction == NeighbourDirection.Left && _conf.NumberOfOrbits > 1)
            {
                var neighbourOrbitLeft = 0;
                var leftCrossSeamOrbit = 0;

                if (hemisphere == CoordinateHelper.Hemisphere.NorthEast || hemisphere == CoordinateHelper.Hemisphere.SouthEast) //If this satellite is in the eastern hemisphere
                {
                    neighbourOrbitLeft = (_thisSatellite.SatPosition.OrbitNumber + _conf.NumberOfOrbits - 1) % _conf.NumberOfOrbits; //Calculate number of next orbit
                    leftCrossSeamOrbit = _conf.MaximumOrbitNumber;

                    if (neighbourOrbitLeft != leftCrossSeamOrbit) //If orbit to my right is NOT across the seam
                    {
                        neighbour = _constellation[neighbourOrbitLeft][_thisSatellite.SatPosition.SatNumber]; //My right neighbour is my initial neighbour
                    }
                }
                else if (hemisphere == CoordinateHelper.Hemisphere.NorthWest || hemisphere == CoordinateHelper.Hemisphere.SouthWest) //If this satellite is in the western hemisphere (mirrored constellation)
                {
                    neighbourOrbitLeft = (_thisSatellite.SatPosition.OrbitNumber + 1) % _conf.NumberOfOrbits; //Calculate number of next orbit
                    leftCrossSeamOrbit = _conf.MinimumOrbitNumber;

                    if (neighbourOrbitLeft != leftCrossSeamOrbit) //If orbit to my right is NOT across the seam
                    {
                        neighbour = _constellation[neighbourOrbitLeft][_thisSatellite.SatPosition.SatNumber];
                    }
                }

                //If my orbit is the last one, i.e. right neighbour is crossing the seam AND cross-seam communication is enabled (otherwise this satellite has no right neighbour right now)
                if (neighbourOrbitLeft == leftCrossSeamOrbit && _conf.CrossSeamCommunicationEnabled)
                {
                    var ticks = _thisSatellite.LogicalPosition.SatNumber - _thisSatellite.SatPosition.SatNumber;
                    if (ticks < 0) //How many ticks since original position
                    {
                        ticks = _conf.NumberOfSatellitesPerOrbit - Math.Abs(_thisSatellite.LogicalPosition.SatNumber - _thisSatellite.SatPosition.SatNumber);
                    }

                    //Original neighbour position
                    var initNb = (_conf.NumberOfSatellitesPerOrbit - _thisSatellite.SatPosition.SatNumber - (_conf.NumberOfSatellitesPerOrbit / 2)) % _conf.NumberOfSatellitesPerOrbit;
                    if (initNb < 0)
                        initNb = _conf.NumberOfSatellitesPerOrbit - Math.Abs(initNb);

                    //Current neighbour
                    var newNb = initNb - (ticks * 2) % _conf.NumberOfSatellitesPerOrbit;
                    if (newNb < 0)
                        newNb = _conf.NumberOfSatellitesPerOrbit - Math.Abs(newNb);

                    neighbour = _constellation[neighbourOrbitLeft][newNb];
                }
            }

            return neighbour;
        }

        private int _lastSatellitePosition = -1;
        private readonly Dictionary<string, Satellite> _lastNeighbours;

        private readonly StatisticsCollector _statistics;
        private readonly Satellite _thisSatellite;
        private readonly Logger _logger;
        private readonly SimulationConfiguration _conf;
        private readonly List<List<Satellite>> _constellation;
        private readonly Queue<Message.Message> _inputQueue = new Queue<Message.Message>();
    }
}