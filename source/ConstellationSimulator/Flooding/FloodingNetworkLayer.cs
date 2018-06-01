using System;
using System.Collections.Generic;
using System.IO;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Helpers;
using ConstellationSimulator.Network;
using ConstellationSimulator.Network.Message;
using ConstellationSimulator.Statistics;

namespace ConstellationSimulator.Flooding
{
    class FloodingNetworkLayer : INetworkLayer
    {
        private readonly int _bufferPeriodSeconds = 90;
        private readonly Dictionary<string, bool> _bufferedMessages = new Dictionary<string, bool>();
        private readonly Queue<KeyValuePair<DateTime, string>> _bufferedMessageTimestamps = new Queue<KeyValuePair<DateTime, string>>();

        public FloodingNetworkLayer(string localAddress, SimulationConfiguration configuration, Logger logger, LinkLayer link, StatisticsCollector results)
        {
            _localAddress = localAddress;
            _conf = configuration;
            _logger = logger;
            _linkLayer = link;
            _statistics = results;
            _time = TimeHelper.GetInstance();
        }

        /// <summary>
        /// Performs mainteance for the flooding network layer. This consists of removing received messages from the buffer.
        /// </summary>
        public void PerformMaintenance()
        {
            while (_bufferedMessageTimestamps.Count > 0 && _time.CurrentTime > _bufferedMessageTimestamps.Peek().Key)
            {
                _bufferedMessages.Remove(_bufferedMessageTimestamps.Peek().Value);
                _bufferedMessageTimestamps.Dequeue();
            }  
        }

        /// <summary>
        /// Exports the routing table, if required by the configuration.
        /// You should know, that when using flooding, we dont keep a routing table.
        ///
        /// But you asked for it. So we are exporting it. You're the boss. Cool beans. Get it?
        /// </summary>
        /// <param name="path"></param>
        public void ExportRoutingTable(string path)
        {
            var routingtable =
                "________________1¶¶¶¶¶¶¶¶¶¶¶1________________\r\n_____________¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶1____________\r\n__________¶¶¶¶118¶¶8¶¶¶¶¶¶¶¶¶¶¶¶¶¶___________\r\n_______8¶¶¶¶888¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶8________\r\n______¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶______\r\n____8¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶____\r\n___¶¶¶¶¶¶¶¶¶¶¶8¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶8¶¶¶___\r\n__8¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶8¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶8¶¶¶¶__\r\n__¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶88881__¶¶¶¶¶¶¶__\r\n_¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶81___________¶¶¶¶¶¶__\r\n1¶¶¶¶¶111____________________________8¶¶¶¶¶¶_\r\n¶¶¶¶¶1___________________________1___1¶¶¶¶¶¶_\r\n¶¶¶¶¶8111___________________________11¶¶¶¶¶¶_\r\n1¶¶¶¶88111__________________________111¶¶¶¶¶_\r\n_¶¶¶¶1881________________________11111_¶¶¶¶¶_\r\n_¶¶¶¶18811_____________________¶¶¶¶¶¶8_1¶¶¶¶1\r\n_¶¶¶¶118¶¶¶¶81______________8¶¶¶¶¶¶¶¶¶_1¶¶¶8¶\r\n_8¶¶881¶¶¶¶¶¶¶¶¶1_________1¶¶¶¶811__1¶811¶¶1¶\r\n¶¶¶¶118¶1__18¶¶¶¶¶8118818¶¶¶88¶111¶888¶11¶¶18\r\n¶8¶¶11¶¶11¶¶111¶¶¶¶¶1___1¶¶¶¶1_1¶__¶¶8888¶¶8_\r\n_1¶¶11¶¶¶¶¶_8¶8_8¶8¶¶____8188__¶¶__¶1__18881_\r\n__8¶88111¶¶_8¶8__1__11___1___111_181___18811_\r\n__11881___181111____11_________________1881__\r\n__118¶81_____________8_________________1881__\r\n___18¶¶1__________1111_____1_11______1_188___\r\n___88¶¶8________88____________8¶81____188____\r\n______1¶1_____8¶888_11____18¶¶118¶8888888____\r\n_______¶¶8881¶¶818¶¶¶¶¶8_1¶¶¶8118¶¶¶¶8888____\r\n_______¶¶¶¶¶¶¶¶881188¶¶¶¶¶¶818¶¶¶¶¶__1888____\r\n_______¶888¶18¶¶¶¶¶8888¶¶¶8¶¶¶¶8_11__88¶_____\r\n______1¶¶8181_118¶¶¶¶¶¶¶¶¶¶¶¶8111___8¶¶______\r\n¶¶¶¶¶¶¶¶¶¶88¶8_1_18¶¶¶¶¶¶¶8881_____1¶8_______\r\n88888¶¶¶¶¶¶¶¶¶8_11118881111_______8¶¶________\r\n88118¶__¶¶8¶8888_1_____________18¶¶_¶¶_______\r\n88888¶___¶¶8818¶¶11_1_______11¶¶¶8__¶¶¶______\r\n¶81¶¶¶____1¶¶¶88¶¶¶88888188¶¶¶¶81__¶¶¶¶¶_____\r\n88¶¶¶8______¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶811__1¶¶¶¶¶¶1____\r\n8¶¶8_________11_1¶¶¶¶¶¶¶888811__¶¶¶¶8¶¶¶¶1___\r\n¶¶8_______¶¶¶¶¶_____11118881__1¶¶¶¶¶¶¶¶¶¶¶___\r\n¶1_______¶¶¶¶¶¶______________¶¶¶¶¶¶¶¶¶8¶¶¶8__\r\n¶_______¶¶¶¶¶¶¶____________1¶¶¶¶¶¶¶8¶¶¶88¶¶8_\r\n_______1¶¶¶¶8_____________¶¶¶¶¶8¶¶¶888888¶¶¶8\r\n_______¶¶¶¶_____________1¶¶¶¶¶88¶¶¶¶¶88¶¶¶88¶\r\n______¶¶¶¶¶____________¶¶¶¶8888¶8¶88881¶8¶888\r\n_____¶¶¶¶¶8__________8¶¶88¶18818¶88118¶88888¶";
            var file = path + "/" + _localAddress + "-routing-table.txt";

            File.WriteAllText(file, routingtable);
        }

        /// <summary>
        /// Gets an incoming message from the link-layer. If message is addressed to this node, it is returned to the application. Otherwise, it is rebroadcasted if not rececived before.
        /// </summary>
        /// <returns></returns>
        public Message GetIncomingMessage()
        {
            //Get new message from link-layer
            var msg = _linkLayer.GetIncomingMessage();
            if (msg == null)
            {
                return null; //No message available in the queue
            }

            //Check if I have already processed this message
            var key = msg.SourceAddress + msg.DestinationAddress + msg.PayloadSize; //Generate somewhat unique key (1/512000 chance that this happens)

            //If message has been received earlier, dont forward it again. We are not savages.
            if (_bufferedMessages.ContainsKey(key))
            {
                return null;
            }

            //I have not seen it before, add it to the list
            _bufferedMessages.Add(key, true);
            //_bufferedMessageTimestamps.Add(new KeyValuePair<DateTime, string>(TimeHelper.GetInstance().GetFutureTime(_bufferPeriodSeconds), key));
            _bufferedMessageTimestamps.Enqueue(new KeyValuePair<DateTime, string>(TimeHelper.GetInstance().GetFutureTime(_bufferPeriodSeconds), key));

            //If I'm the destination, packet was delivered successfully. Pass to application-layer and return.
            if (msg.DestinationAddress == _localAddress)
            {
                return msg;
            }
            
            //Else if message is DATA and I'm NOT the destination, it is clearly meant to be forwarded
            SendDataMessage(msg); //ONWARDS #livinglavidaloca

            return null;
        }

        /// <summary>
        /// Passes a data message to the link layer for broadcasting.
        /// </summary>
        /// <param name="outgoingMessage">The message to broadcast.</param>
        public void SendDataMessage(Message outgoingMessage)
        {
            _linkLayer.SendMessage(outgoingMessage, SimulationConstants.BroadcastAddress);
        }

        private readonly string _localAddress;
        private readonly SimulationConfiguration _conf;
        private readonly Logger _logger;
        private readonly LinkLayer _linkLayer;
        private readonly StatisticsCollector _statistics;
        private readonly TimeHelper _time;
    }
}
