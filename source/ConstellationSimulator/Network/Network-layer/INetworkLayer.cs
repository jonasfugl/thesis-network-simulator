namespace ConstellationSimulator.Network
{
    /// <summary>
    /// Interface that all network layers must implement.
    /// </summary>
    internal interface INetworkLayer
    {
        /// <summary>
        /// Perform network/routing maintenance, once every simulation tick. Clear old routes, update lifetime etc.
        /// </summary>
        void PerformMaintenance();

        /// <summary>
        /// Print routing table to file.
        /// </summary>
        void ExportRoutingTable(string path);

        /// <summary>
        /// Retrieve incoming message for processing, if any is available.
        /// </summary>
        /// <returns></returns>
        Message.Message GetIncomingMessage();

        /// <summary>
        /// Send a Data Message to another Satellite. 
        /// </summary>
        /// <param name="outgoingMessage"></param>
        void SendDataMessage(Message.Message outgoingMessage);
    }
}
