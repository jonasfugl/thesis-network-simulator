namespace ConstellationSimulator.Network.Message
{
    /// <summary>
    /// Interface that all message payloads have to implement.
    /// </summary>
    internal interface IMessagePayload
    {
        /// <summary>
        /// The size of the payload.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// The type of the payload.
        /// </summary>
        MessageType Type { get; }
    }
}
