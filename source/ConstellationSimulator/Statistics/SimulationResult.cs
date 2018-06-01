using System;
using System.Collections.Generic;
using ConstellationSimulator.Configuration;

namespace ConstellationSimulator.Statistics
{
    public class SimulationResult
    {
        /*
         * Methods called in Link-layer. Updating global statistics.
         */
        public void LinkLayer_AddSentMessage(int size, int payloadSize)
        {
            TotalPacketsSentNetwork++;
            TotalBytesSentNetwork += Convert.ToInt64(size);
            _totalPayloadBytesSentNetwork += Convert.ToInt64(payloadSize);
        }
        
        public void LinkLayer_AddReceivedMessage(int size, int payloadSize)
        {
            TotalPacketsReceivedNetwork++;
            TotalBytesReceivedNetwork += Convert.ToInt64(size);
            _totalPayloadBytesReceivedNetwork += Convert.ToInt64(payloadSize);
        }

        public void LinkLayer_AddDroppedMessage(int size, int payloadSize, bool dataMsg)
        {
            if (dataMsg)
            {
                TotalDataPacketsDropped++;
                DataPacketsDroppedLinkLayer++;
                TotalDataBytesDropped += Convert.ToInt64(payloadSize);
            }
            else
            {
                TotalRoutingPacketsDropped++;
                TotalRoutingBytesDropped += Convert.ToInt64(payloadSize);
            }
        }

        public void TtlExpiredDataMsg(int payloadSize)
        {
            DataPacketsDroppedTtlExpired++;
            TotalDataPacketsDropped++;
            TotalDataBytesDropped += Convert.ToInt64(payloadSize);
        }

        public void TtlExpiredRreqMsg(int payloadSize)
        {
            TtlExpiredRoutingMessage(payloadSize);
            TtlExpiredRreq++;
        }

        public void TtlExpiredRrepMsg(int payloadSize)
        {
            TtlExpiredRoutingMessage(payloadSize);
            TtlExpiredRrep++;
        }

        public void TtlExpiredRerrMsg(int payloadSize)
        {
            TtlExpiredRoutingMessage(payloadSize);
            TtlExpiredRerr++;
        }

        public void TtlExpiredRrrepackMsg(int payloadSize)
        {
            TtlExpiredRoutingMessage(payloadSize);
            TtlExpiredRrepAck++;
        }

        public void TtlExpiredHelloMsg(int payloadSize)
        {
            TtlExpiredRoutingMessage(payloadSize);
            _ttlExpiredHello++;
        }

        private void TtlExpiredRoutingMessage(int payloadSize)
        {
            TotalRoutingPacketsDropped++;
            TotalRoutingBytesDropped += Convert.ToInt64(payloadSize);
        }


        /*
         * Methods updating DATA statistics
         */

        public void AddDataMessageSent(int payloadSize)
        {
            TotalDataPacketsSent++;
            TotalDataBytesSent += Convert.ToInt64(payloadSize);
        }

        public void AddDataMessageReceived(int hopCount, double propDelay, double procDelay, double endToEndDelay, int payloadSize)
        {
            //End to end characteristics, measured only for SUCCESSFULLY delivered messages
            _totalNumberOfHops += Convert.ToInt64(hopCount);
            _totalPropagationDelay += Convert.ToInt64(propDelay);
            _totalProcessingDelay += Convert.ToInt64(procDelay);

            AllEndToEndDelaysList.Add(endToEndDelay);
            AllHopCountsList.Add(hopCount);

            TotalDataPacketsReceived++;
            TotalDataBytesReceived += Convert.ToInt64(payloadSize);
        }

        public void DataDropped_DestinationUnreachable(int payloadSize)
        {
            DataPacketsDroppedDestinationUnrechable++;
            TotalDataPacketsDropped++;
            TotalDataBytesDropped += Convert.ToInt64(payloadSize);
        }

        public void DataDropped_NoActiveForwardingRoute(int payloadSize)
        {
            DataPacketsDroppedNoActiveForwardingRoute++;
            TotalDataPacketsDropped++;
            TotalDataBytesDropped += Convert.ToInt64(payloadSize);
        }

        public void DataDropped_NextHopUnavailable(int payloadSize)
        {
            DataPacketsDroppedNextHopUnavailable++;
            TotalDataPacketsDropped++;
            TotalDataBytesDropped += Convert.ToInt64(payloadSize);
        }



        /*
         * Methods updating AODV statistics
         */
        public void AddAodvMessageSent(int payloadSize)
        {
            TotalRoutingPacketsSent++;
            TotalRoutingBytesSent += Convert.ToInt64(payloadSize);
        }

        public void AddAodvMessageReceived(int payloadSize)
        {
            TotalRoutingPacketsReceived++;
            TotalRoutingBytesReceived += Convert.ToInt64(payloadSize);
        }

        public void AodvDropped_NoActiveForwardingRoute(int payloadSize)
        {
            _aodvDroppedNoActiveForwardingRoute++;
            TotalRoutingPacketsDropped++;
            TotalRoutingBytesDropped += Convert.ToInt64(payloadSize);
        }

        public void AodvDropped_NextHopUnavailable(int payloadSize)
        {
            _aodvDroppedNextHopUnavailable++;
            TotalRoutingPacketsDropped++;
            TotalRoutingBytesDropped += Convert.ToInt64(payloadSize);
        }

        public void RoutingRequestGenerated()
        {
            TotalRoutingRequestsGenerated++;
        }


        /*
         * Timing
         */

        public void SetStartTime()
        {
            _startTime = DateTime.Now;
        }

        public void SetStopTime()
        {
            _stopTime = DateTime.Now;
        }

        public TimeSpan GetSimulationDuration()
        {
            return _stopTime - _startTime;
        }


        /*
         * Global statistics. Total number of packets/bytes sent, received and lost
         */

        public long TotalPacketsSentNetwork { get; private set; }
        public long TotalPacketsReceivedNetwork { get; private set; }
        public long TotalPacketsDroppedNetwork => TotalDataPacketsDropped + TotalRoutingPacketsDropped;

        public long TotalBytesSentNetwork { get; private set; }
        public long TotalBytesReceivedNetwork { get; private set; }
        public long TotalBytesDroppedNetwork => TotalDataBytesDropped + TotalRoutingBytesDropped + TotalHeaderBytesDropped;

        public long TotalHeaderBytesSent => TotalDataPacketsSent * SimulationConstants.MessageHeaderSize;
        public long TotalHeaderBytesReceived => TotalPacketsReceivedNetwork * SimulationConstants.MessageHeaderSize;
        public long TotalHeaderBytesDropped => TotalPacketsDroppedNetwork * SimulationConstants.MessageHeaderSize;

        /*
         * Total number of data packets/bytes sent, received and lost
         */

        public long TotalDataPacketsSent { get; private set; }
        public long TotalDataPacketsReceived { get; private set; }
        public long TotalDataPacketsDropped { get; private set; }

        public long TotalDataBytesSent { get; private set; }
        public long TotalDataBytesReceived { get; private set; }
        public long TotalDataBytesDropped { get; private set; }


        /*
         * Total number of AODV packets/bytes sent, received and lost
         */

        public long TotalRoutingPacketsSent { get; private set; }
        public long TotalRoutingPacketsReceived { get; private set; }
        public long TotalRoutingPacketsDropped { get; private set; }

        public long TotalRoutingBytesSent { get; private set; }
        public long TotalRoutingBytesReceived { get; private set; }
        public long TotalRoutingBytesDropped { get; private set; }

        public long TotalRoutingRequestsGenerated { get; private set; }



        /*
         * Network metrics
         */
        public double PacketDeliveryRate => (Convert.ToDouble(TotalDataPacketsReceived) / Convert.ToDouble(TotalDataPacketsSent)) * 100;
        public double AverageNumberOfHops => Convert.ToDouble(_totalNumberOfHops) / Convert.ToDouble(TotalDataPacketsReceived);
        public double AverageEndToEndDelay => (_totalPropagationDelay + _totalProcessingDelay) / TotalDataPacketsReceived;


        
        /*
         * Reasons for dropping a data packet
         */
        public uint DataPacketsDroppedDestinationUnrechable { get; private set; }
        public uint DataPacketsDroppedNoActiveForwardingRoute { get; private set; }
        public uint DataPacketsDroppedNextHopUnavailable { get; private set; }
        public uint DataPacketsDroppedLinkLayer { get; private set; }
        public uint DataPacketsDroppedTtlExpired { get; private set; }



        private long _aodvDroppedNoActiveForwardingRoute;
        private long _aodvDroppedNextHopUnavailable;

        public long TtlExpiredRreq { get; private set; }
        public long TtlExpiredRrep { get; private set; }
        public long TtlExpiredRerr { get; private set; }
        public long TtlExpiredRrepAck { get; private set; }

        private long _ttlExpiredHello;

        private long _totalPayloadBytesSentNetwork;
        private long _totalPayloadBytesReceivedNetwork;

        public List<double> AllEndToEndDelaysList { get; } = new List<double>();
        public List<int> AllHopCountsList { get; } = new List<int>();
        private double _totalPropagationDelay;
        private double _totalProcessingDelay;
        private long _totalNumberOfHops;

        private DateTime _startTime;
        private DateTime _stopTime;
    }
}
