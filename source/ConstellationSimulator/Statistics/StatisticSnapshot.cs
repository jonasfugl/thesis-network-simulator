namespace ConstellationSimulator.Statistics
{
    public class StatisticSnapshot
    {
        /// <summary>
        /// Amount of minutes into the simulation, this snapshot represents.
        /// </summary>
        public string MinutesIntoSim { get; set; }

        /// <summary>
        /// The PDR for the entire simulation, at this point in time.
        /// Calculated as: Total delivered DATA packets / Total sent DATA packets
        /// </summary>
        public double DataPacketDeliveryRate { get; set; }

        /// <summary>
        /// The current goodput in the simulation for packets.
        /// </summary>
        public double Goodput { get; set; }

        /// <summary>
        /// The amount of RREQs that have been generated in this snapshot.
        /// </summary>
        public uint GeneratedRreqs { get; set; }
    }
}
