using System;
using System.Collections.Generic;
using ConstellationSimulator.AODV;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Flooding;
using ConstellationSimulator.Helpers;
using ConstellationSimulator.Statistics;

namespace ConstellationSimulator.Network
{
    internal class Network
    {
        public Network(ref List<List<Satellite>> constellation, ref SimulationConfiguration conf, Satellite thisSatellite, Logger logger, StatisticsCollector results)
        {
            _linkLayer = new LinkLayer(conf, constellation, thisSatellite, logger, results);

            if (conf.NetworkLayer == "AODV")
            {
                _networkLayer = new AodvNetworkLayer(thisSatellite.LocalAddress, conf, logger, _linkLayer, results);
            }
            else if (conf.NetworkLayer.Equals("Flooding", StringComparison.OrdinalIgnoreCase))
            {
                _networkLayer = new FloodingNetworkLayer(thisSatellite.LocalAddress, conf, logger, _linkLayer, results);
            }
        }

        /// <summary>
        /// Send data message to other node
        /// </summary>
        /// <param name="msg">Message to transmit to other node.</param>
        public void SendMessage(Message.Message msg)
        {
            _networkLayer.SendDataMessage(msg);
        }

        /// <summary>
        /// Receive message placed in queue from another node
        /// </summary>
        /// <returns>Incoming message if available, or null if no new messages.</returns>
        public Message.Message GetIncomingMessage()
        {
            var msg = _networkLayer.GetIncomingMessage(); //Get new message from link-layer. Might be null, if no data-message available in queue
            return msg;
        }

        /// <summary>
        /// Place a message in this nodes queue (called from another satellite)
        /// </summary>
        /// <param name="msg">Message that other satellite wants to send to this satellite.</param>
        public void ReceiveNewMessage(Message.Message msg)
        {
            _linkLayer.ReceiveMessage(msg);
        }

        /// <summary>
        /// Performs maintenance for this network layer. Must be called once pr. iteration, to ensure retries and cleanup of buffered data.
        /// </summary>
        public void PerformNetworkMaintenance()
        {
            _networkLayer.PerformMaintenance();
        }

        /// <summary>
        /// Exports the routing table to the folder specified by path, if enabled in the configuration.
        /// </summary>
        /// <param name="path">The folder in which the routing table is exported.</param>
        public void ExportRoutingTable(string path)
        {
            _networkLayer.ExportRoutingTable(path);
        }

        /// <summary>
        /// The number of unhandled incoming messages placed in the link-layer input buffer.
        /// </summary>
        public int IncomingMessageCount => _linkLayer.IncomingMessageCount;

        private readonly INetworkLayer _networkLayer;
        private readonly LinkLayer _linkLayer;
    }
}
