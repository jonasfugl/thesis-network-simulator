namespace ConstellationSimulator.AODV
{
    internal class UnreachableDestination
    {
        public UnreachableDestination(string destinationAddress, int sequenceNumber)
        {
            UnreachableDestinationAddress = destinationAddress;
            UnreachableDestinationSequenceNumber = sequenceNumber;
        }

        public UnreachableDestination(UnreachableDestination ud)
        {
            UnreachableDestinationAddress = ud.UnreachableDestinationAddress;
            UnreachableDestinationSequenceNumber = ud.UnreachableDestinationSequenceNumber;
        }

        /// <summary>
        /// The address of the destination that has become unreachable due to a link break.
        /// </summary>
        public string UnreachableDestinationAddress { get; }

        /// <summary>
        /// The sequence number in the route table entry for the destination listed in the previous Unreachable Destination Address field.
        /// </summary>
        public int UnreachableDestinationSequenceNumber { get; }
    }
}
