using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ConstellationSimulator.Statistics
{
    internal static class ResultOutputter
    {
        /// <summary>
        /// Writes the results to the output file.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        public static void DumpResults(List<SimulationResult> results, string path, string filename)
        {
            var iterations = Convert.ToDouble(results.Count);

            var totalPacketsSentNetwork = results.Sum(item => item.TotalPacketsSentNetwork) / iterations;
            var totalPacketsReceivedNetwork = results.Sum(item => item.TotalPacketsReceivedNetwork) / iterations;
            var totalPacketsDroppedNetwork = results.Sum(item => item.TotalPacketsDroppedNetwork) / iterations;

            var totalBytesSentNetwork = results.Sum(item => item.TotalBytesSentNetwork) / iterations;
            var totalBytesReceivedNetwork = results.Sum(item => item.TotalBytesReceivedNetwork) / iterations;
            var totalBytesDroppedNetwork = results.Sum(item => item.TotalBytesDroppedNetwork) / iterations;

            var totalHeaderBytesSent = results.Sum(item => item.TotalHeaderBytesSent) / iterations;
            var totalHeaderBytesReceived = results.Sum(item => item.TotalHeaderBytesReceived) / iterations;
            var totalHeaderBytesDropped = results.Sum(item => item.TotalHeaderBytesDropped) / iterations;

            var totalDataPacketsSent = results.Sum(item => item.TotalDataPacketsSent) / iterations;
            var totalDataPacketsReceived = results.Sum(item => item.TotalDataPacketsReceived) / iterations;
            var totalDataPacketsDropped = results.Sum(item => item.TotalDataPacketsDropped) / iterations;

            var totalDataBytesSent = results.Sum(item => item.TotalDataBytesSent) / iterations;
            var totalDataBytesReceived = results.Sum(item => item.TotalDataBytesReceived) / iterations;
            var totalDataBytesDropped = results.Sum(item => item.TotalDataBytesDropped) / iterations;

            var totalRoutingPacketsSent = results.Sum(item => item.TotalRoutingPacketsSent) / iterations;
            var totalRoutingPacketsReceived = results.Sum(item => item.TotalRoutingPacketsReceived) / iterations;
            var totalRoutingPacketsDropped = results.Sum(item => item.TotalRoutingPacketsDropped) / iterations;

            var totalRoutingBytesSent = results.Sum(item => item.TotalRoutingBytesSent) / iterations;
            var totalRoutingBytesReceived = results.Sum(item => item.TotalRoutingBytesReceived) / iterations;
            var totalRoutingBytesDropped = results.Sum(item => item.TotalRoutingBytesDropped) / iterations;

            var totalRoutingRequestsGenerated = results.Sum(item => item.TotalRoutingRequestsGenerated) / iterations;

            var packetDeliveryRate = results.Sum(item => item.PacketDeliveryRate) / iterations;
            var averageHopCount = results.Sum(item => item.AverageNumberOfHops) / iterations;
            var averageEndToEndDelay = results.Sum(item => item.AverageEndToEndDelay) / iterations;

            var dataDroppedDestinationUnreachable = results.Sum(item => item.DataPacketsDroppedDestinationUnrechable) / iterations;
            var dataDroppedNoActiveForwardRoute = results.Sum(item => item.DataPacketsDroppedNoActiveForwardingRoute) / iterations;
            var dataDroppedNextHopUnavailable = results.Sum(item => item.DataPacketsDroppedNextHopUnavailable) / iterations;
            var dataDroppedLinkLayer = results.Sum(item => item.DataPacketsDroppedLinkLayer) / iterations;
            var dataDroppedTtlExpired = results.Sum(item => item.DataPacketsDroppedTtlExpired) / iterations;

            var totalDataDropped = dataDroppedDestinationUnreachable + dataDroppedNextHopUnavailable + dataDroppedNoActiveForwardRoute + dataDroppedTtlExpired + dataDroppedLinkLayer;

            var dataDroppedDestinationUnreachablePercentage = (dataDroppedDestinationUnreachable / totalDataDropped) * 100;
            var dataDroppedNoActiveForwardRoutePercentage = (dataDroppedNoActiveForwardRoute / totalDataDropped) * 100;
            var dataDroppedNextHopUnavailablePercentage = (dataDroppedNextHopUnavailable / totalDataDropped) * 100;
            var dataDroppedLinkLayerPercentage = (dataDroppedLinkLayer / totalDataDropped) * 100;
            var dataDroppedTtlExpiredPercentage = (dataDroppedTtlExpired / totalDataDropped) * 100;

            var ttlExpiredRreq = results.Sum(item => item.TtlExpiredRreq) / iterations;
            var ttlExpiredRrep = results.Sum(item => item.TtlExpiredRrep) / iterations;
            var ttlExpiredRerr = results.Sum(item => item.TtlExpiredRerr) / iterations;
            var ttlExpiredRrepack = results.Sum(item => item.TtlExpiredRrepAck) / iterations;

            //Average execution time
            var averageTicks = results.Average(sim => sim.GetSimulationDuration().Ticks);
            var longAverageTicks = Convert.ToInt64(averageTicks);
            var averageTime = new TimeSpan(longAverageTicks);



            var file = new StreamWriter(path + "/" + filename, false);


            var unprocessedData = totalDataPacketsSent - totalDataPacketsDropped - totalDataPacketsReceived;

            file.WriteLine("Simulation output:" +
                         "\nTotal packets sent in network: " + totalPacketsSentNetwork +
                         "\nTotal packets received in network: " + totalPacketsReceivedNetwork +
                         "\nTotal packets dropped in network: " + totalPacketsDroppedNetwork +
                         "\n" +
                         "\nTotal bytes sent: " + totalBytesSentNetwork +
                         "\nTotal bytes received: " + totalBytesReceivedNetwork +
                         "\nTotal bytes dropped: " + totalBytesDroppedNetwork +
                         "\n" +
                         "\nTotal header bytes sent: " + totalHeaderBytesSent +
                         "\nTotal header bytes received: " + totalHeaderBytesReceived +
                         "\nTotal header bytes dropped: " + totalHeaderBytesDropped +
                         "\n");

            file.WriteLine("Data-packet statistics:" +
                         "\nData packets sent: " + totalDataPacketsSent +
                         "\nData packets received: " + totalDataPacketsReceived +
                         "\n---Data packets waiting for processing on termination:" + unprocessedData +
                         "\nData packets dropped: " + totalDataPacketsDropped + " - " + (Convert.ToDouble(totalDataPacketsDropped) / Convert.ToDouble(totalDataPacketsSent)) * 100 +
                         "\nPacket Delivery Rate: " + packetDeliveryRate + "%" +
                         "\n---Data dropped because of destination unreachable: " + dataDroppedDestinationUnreachable + "(" + dataDroppedDestinationUnreachablePercentage + "%)" +
                         "\n---Data dropped because of next-hop unavailable: " + dataDroppedNextHopUnavailable + "(" + dataDroppedNextHopUnavailablePercentage + "%)" +
                         "\n---Data dropped because of no active forwarding route: " + dataDroppedNoActiveForwardRoute + "(" + dataDroppedNoActiveForwardRoutePercentage + "%)" +
                         "\n---Data dropped because of expired TTL: " + dataDroppedTtlExpired + "(" + dataDroppedTtlExpiredPercentage + "%)" +
                         "\n---Data dropped because of link-loss: " + dataDroppedLinkLayer + "(" + dataDroppedLinkLayerPercentage + "%)" +
                         "\n---Total: " + totalDataDropped +
                         "\nGoodput packets: " + (Convert.ToDouble(totalDataPacketsReceived) / Convert.ToDouble(totalPacketsReceivedNetwork)) * 100 + "%" +
                         "\nGoodput bytes: " + (Convert.ToDouble(totalDataBytesReceived) / Convert.ToDouble(totalBytesReceivedNetwork)) * 100 + "%" +
                         "\n" +
                         "\nTotal data bytes sent: " + totalDataBytesSent +
                         "\nTotal data bytes received: " + totalDataBytesReceived +
                         "\nTotal data bytes dropped: " + totalDataBytesDropped);

            file.WriteLine(
                        "\nTotal routing packets sent: " + totalRoutingPacketsSent +
                        "\nTotal routing packets received: " + totalRoutingPacketsReceived +
                        "\nTotal routing packets dropped: " + totalRoutingPacketsDropped +
                        "\n" +
                        "\nTotal routing bytes sent: " + totalRoutingBytesSent +
                        "\nTotal routing bytes received: " + totalRoutingBytesReceived +
                        "\nTotal routing bytes dropped: " + totalRoutingBytesDropped);

            file.WriteLine(
                        "\nRREQs dropped because of expired TTL: " + ttlExpiredRreq +
                        "\nRREPs dropped because of expired TTL: " + ttlExpiredRrep +
                        "\nRRERs dropped because of expired TTL: " + ttlExpiredRerr +
                        "\nRREPACKs dropped because of expired TTL: " + ttlExpiredRrepack +
                        "\n" +

                        "\nTotal RREQs (routing requests) generated: " + totalRoutingRequestsGenerated +
                        "\n" +
                        "\nAverage end-to-end delay: " + averageEndToEndDelay +
                        "\nAverage hop-count: " + averageHopCount);

            file.WriteLine("\n\nAverage run-time: {0:hh\\:mm\\:ss}", averageTime);

            file.Close();

            var nfi = new CultureInfo("da-DK", false).NumberFormat;
            nfi.NumberDecimalSeparator = ".";
            var tempDelayList = new List<double>();

            foreach (var result in results)
            {
                tempDelayList.AddRange(result.AllEndToEndDelaysList);
            }

            File.WriteAllLines(
                path + "/all-end-to-end-delays.txt",
                tempDelayList.Select(d => d.ToString(nfi)));



            var tempHopCountList = new List<int>();

            foreach (var result in results)
            {
                tempHopCountList.AddRange(result.AllHopCountsList);
            }

            File.WriteAllLines(
                path + "/all-end-to-end-hop-counts.txt",
                tempHopCountList.Select(d => d.ToString(nfi)));
        }
    }
}
