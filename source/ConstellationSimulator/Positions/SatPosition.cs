namespace ConstellationSimulator.Positions
{
    internal class SatPosition
    {
        public SatPosition(int orbitNo, int satNo)
        {
            OrbitNumber = orbitNo;
            SatNumber = satNo;
        }

        public int OrbitNumber { get; }
        public int SatNumber { get; }
    }
}
