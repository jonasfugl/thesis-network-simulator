namespace ConstellationSimulator.Network.Message
{
    internal class DataMsg : IMessagePayload
    {
        public DataMsg(int size)
        {
            Size = size;
        }

        public DataMsg(DataMsg data)
        {
            Size = data.Size;
        }

        /// <summary>
        /// The type of this message. Set to DATA.
        /// </summary>
        public MessageType Type => MessageType.Data;

        /// <summary>
        /// The size of this data-message.
        /// </summary>
        public int Size { get; }
    }
}
