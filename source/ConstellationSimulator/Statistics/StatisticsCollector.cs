using System;
using System.Collections.Generic;
using ConstellationSimulator.Network.Message;

namespace ConstellationSimulator.Statistics
{
    internal class StatisticsCollector
    {
        public StatisticsCollector()
        {
            _results = new List<SimulationResult>();
        }

        public void NewSimulation()
        {
            _curIndex++;
            _results.Add(new SimulationResult());
        }

        #region Link-layer statistics
        public void LinkLayerAddSentMessage(Message msg)
        {
            _results[_curIndex].LinkLayer_AddSentMessage(msg.Size, msg.PayloadSize);
        }

        public void LinkLayerAddReceivedMessage(Message msg)
        {
            _results[_curIndex].LinkLayer_AddReceivedMessage(msg.Size, msg.PayloadSize);
        }

        public void LinkLayerAddDroppedMessage(Message msg)
        {
            _results[_curIndex].LinkLayer_AddDroppedMessage(msg.Size, msg.PayloadSize, msg.Type == MessageType.Data);
        }

        public void LinkLayerTtlExpired(Message msg)
        {
            switch (msg.Type)
            {
                case MessageType.Rreq:
                    _results[_curIndex].TtlExpiredRreqMsg(msg.PayloadSize);
                    break;
                case MessageType.Rrep:
                    _results[_curIndex].TtlExpiredRrepMsg(msg.PayloadSize);
                    break;
                case MessageType.Rerr:
                    _results[_curIndex].TtlExpiredRerrMsg(msg.PayloadSize);
                    break;
                case MessageType.Rrepack:
                    _results[_curIndex].TtlExpiredRrrepackMsg(msg.PayloadSize);
                    break;
                case MessageType.Hello:
                    _results[_curIndex].TtlExpiredHelloMsg(msg.PayloadSize);
                    break;
                case MessageType.Data:
                    _results[_curIndex].TtlExpiredDataMsg(msg.PayloadSize);
                    break;
            }
        }

        #endregion

        #region Data statistics

        public void AddDataMessageSent(Message msg)
        {
            if (msg.Type != MessageType.Data)
            {
                throw new ArgumentException("Sorry, DATA only.");
            }

            _results[_curIndex].AddDataMessageSent(msg.PayloadSize);
        }

        public void AddDataMessageReceived(Message msg)
        {
            if (msg.Type != MessageType.Data)
            {
                throw new ArgumentException("Sorry AODV, DATA only.");
            }

            _results[_curIndex].AddDataMessageReceived(msg.HopCount, msg.TotalPropagationDelay, msg.TotalProcessingDelay, msg.TotalEndToEndDelay, msg.PayloadSize);
        }

        public void AddDataMessageDroppedDestinationUnreachable(Message msg)
        {
            _results[_curIndex].DataDropped_DestinationUnreachable(msg.PayloadSize);
        }

        public void AddDataMessageDroppedNoActiveForwardingRoute(Message msg)
        {
            _results[_curIndex].DataDropped_NoActiveForwardingRoute(msg.PayloadSize);
        }

        public void AddDataMessageDroppedNextHopUnavailable(Message msg)
        {
            _results[_curIndex].DataDropped_NextHopUnavailable(msg.PayloadSize);
        }
        #endregion
        
        #region AODV Statistics
        public void AddAodvMessageSent(Message msg)
        {
            if (msg.Type == MessageType.Data)
            {
                throw new ArgumentException("Sorry, AODV only.");
            }

            _results[_curIndex].AddAodvMessageSent(msg.PayloadSize);
        }

        public void AddAodvMessageReceived(Message msg)
        {
            if (msg.Type == MessageType.Data)
            {
                throw new ArgumentException("Sorry, AODV only.");
            }

            _results[_curIndex].AddAodvMessageReceived(msg.PayloadSize);
        }

        public void AodvDropped_NoActiveForwardingRoute(Message msg)
        {
            if (msg.Type == MessageType.Data)
            {
                throw new ArgumentException("Sorry, AODV only.");
            }

            _results[_curIndex].AodvDropped_NoActiveForwardingRoute(msg.PayloadSize);
        }

        public void AodvDropped_NextHopUnavailable(Message msg)
        {
            if (msg.Type == MessageType.Data)
            {
                throw new ArgumentException("Sorry, AODV only.");
            }

            _results[_curIndex].AodvDropped_NextHopUnavailable(msg.PayloadSize);
        }

        public void RoutingRequestGenerated()
        {
            _results[_curIndex].RoutingRequestGenerated();
        }

        #endregion

        #region Simulation timing
        public void SetStartTime()
        {
            _results[_curIndex].SetStartTime();
        }

        public void SetStopTime()
        {
            _results[_curIndex].SetStopTime();
        }
        #endregion

        public List<SimulationResult> GetResults()
        {
            return _results;
        }

        public SimulationResult GetCurrentResults()
        {
            return _results[_curIndex];
        }

        private int _curIndex = -1;
        private readonly List<SimulationResult> _results;
    }
}
