using ConstellationSimulator.Network.Message;

namespace ConstellationSimulator.AODV
{
    internal class Rrepack : IMessagePayload
    {
        public Rrepack() {}

        public Rrepack(Rrepack ack) {}

        /// <summary>
        /// Size of the RREPACK. Size specified in RFC.
        /// </summary>
        public int Size => 2;

        /// <summary>
        /// The message type. Set to RREPACK.
        /// </summary>
        public MessageType Type => MessageType.Rrepack;
    }
}
