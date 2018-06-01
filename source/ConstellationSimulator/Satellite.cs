using System.Collections.Generic;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Helpers;
using ConstellationSimulator.Network.Message;
using ConstellationSimulator.Positions;
using ConstellationSimulator.Statistics;

namespace ConstellationSimulator
{
    internal class Satellite
    {
        public Satellite(string localAddress, Coordinate coordinate, SatPosition satPosition, SatPosition logical, ref List<List<Satellite>> constellation, ref SimulationConfiguration conf, StatisticsCollector results)
        {
            LocalAddress = localAddress;
            Location = coordinate;
            SatPosition = satPosition;
            _conf = conf;
            LogicalPosition = logical;
            _statistics = results;

            _logger = new Logger(_conf.OutputPath + "/" + LocalAddress + ".log", conf);
            _logger.WriteLine(LocalAddress + " starting up...");

            _network = new Network.Network(ref constellation, ref conf, this, _logger, _statistics);
        }

        /// <summary>
        /// Run once every iteration of the simulation. Functionality herein is the application layer in the network stack.
        /// </summary>
        public void Run()
        {
            //Routing table maintenance once every tick
            _network.PerformNetworkMaintenance();

            //Dequeue and handle incoming messages
            while (_network.IncomingMessageCount > 0)
            {
                var incomingPacket = _network.GetIncomingMessage();
                if (incomingPacket != null)
                {
                    _statistics.AddDataMessageReceived(incomingPacket);
                    _logger.WriteLine("Data packet received from " + incomingPacket.SourceAddress);
                }                
            }
            
            //Generate and send new data packet
            SendNewDataPacket();
        }

        /// <summary>
        /// Generates a new data packet for transfer, according to the set up probabilities.
        /// Receipient is determined by random.
        /// </summary>
        private void SendNewDataPacket()
        {
            var generateData = RandomHelper.GenerateData(_conf.NewDataMessageProbability);

            if (generateData)
            {
                //Generat random receiver in constellation (random orbit, random sat), ensure destination is not me
                var orbitNo = RandomHelper.GetRandomNumber(_conf.MinimumOrbitNumber, _conf.NumberOfOrbits);
                var satNo = RandomHelper.GetRandomNumber(0, _conf.NumberOfSatellitesPerOrbit);
                
                if (orbitNo == SatPosition.OrbitNumber && satNo == SatPosition.SatNumber)
                {
                    satNo = satNo + 1 % _conf.NumberOfSatellitesPerOrbit; //Dont try to send a message to myself
                }

                var destination = "R" + orbitNo + "S" + satNo;

                //Generate new data message, with random size between min/max values
                var payloadSize = RandomHelper.GetRandomNumber(_conf.DataMessageMinSizeBytes, _conf.DataMessageMaxSizeBytes + 1);
                var payload = new DataMsg(payloadSize);
                var outgoingPacket = new Message(payload, destination, LocalAddress, _conf.MessageTtlValue);

                _logger.WriteLine("Generated " + payloadSize + " bytes of data. Sending to " + destination);
                _statistics.AddDataMessageSent(outgoingPacket);
                _network.SendMessage(outgoingPacket);
            }
        }

        /// <summary>
        /// Address of this satellite.
        /// </summary>
        public string LocalAddress { get; }

        /// <summary>
        /// The location of this satellite in longitude, latitude and altitude.
        /// </summary>
        public Coordinate Location { get; set; }

        /// <summary>
        /// The position of this satellite, within the constellation. This is static throughout the entire simulation.
        /// </summary>
        public SatPosition SatPosition { get; set; }

        /// <summary>
        /// The logical location of the satellite. This is incremented at each iteration, to move the satellites forward in their orbit.
        /// </summary>
        public SatPosition LogicalPosition { get; set; }

        /// <summary>
        /// Places a message in the link-layer input buffer. This is the interface, through which other satellites send messages to this satellite (workaround for not making Network member public).
        /// </summary>
        /// <param name="msg"></param>
        public void ReceiveNewMessage(Message msg)
        {
            _network.ReceiveNewMessage(msg);
        }

        /// <summary>
        /// Exports the routing table for this satellite.
        /// </summary>
        /// <param name="path"></param>
        public void ExportRoutingTable(string path)
        {
            _network.ExportRoutingTable(path); //Print routing table to file, just for keeps
        }

        private readonly StatisticsCollector _statistics;
        private readonly SimulationConfiguration _conf;
        private readonly Network.Network _network;
        private readonly Logger _logger;
    }
}

//TODO: Receiving RREQ with U flag set. Not mentioned in spec how this is handled.
//TODO: Enable dumping of packets. Maybe store them in memory, and write them at the end instead.