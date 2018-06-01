namespace ConstellationSimulator.Network
{
    /// <summary>
    /// Used as a result-type by the link layer, to avoid the use of exceptions, since these take longer time to process.
    /// </summary>
    internal class LinkLayerResult
    {
        public LinkLayerResult(LinkLayerResultType result)
        {
            Result = result;
        }
        public LinkLayerResult(Message.Message droppedMessage, string missingNeighbour)
        {
            DroppedMessage = droppedMessage;
            MissingNeighbour = missingNeighbour;
            Result = LinkLayerResultType.NextHopNotFound;
        }

        /// <summary>
        /// The result of the link-layer operation.
        /// </summary>
        public LinkLayerResultType Result { get; }

        /// <summary>
        /// The message that could not be delivered.
        /// </summary>
        public Message.Message DroppedMessage { get; }

        /// <summary>
        /// The missing neighbour, that caused the failure.
        /// </summary>
        public string MissingNeighbour { get; }
    }

    /// <summary>
    /// The available types of results from the link-layer.
    /// </summary>
    internal enum LinkLayerResultType
    {
        Success,
        TtlExpired,
        NextHopNotFound,
    }
}
