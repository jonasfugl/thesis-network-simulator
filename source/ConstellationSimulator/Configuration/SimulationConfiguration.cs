using System;
using System.IO;
using ConstellationSimulator.AODV;
using Newtonsoft.Json;

namespace ConstellationSimulator.Configuration
{
    public class SimulationConfiguration
    {
        public void Initialize()
        {
            AodvConfiguration = new AodvParameters(AODV_ActiveRouteTimeout, SecondsPerTick, NumberOfSatellitesPerOrbit, NumberOfOrbits, CrossSeamCommunicationEnabled);
            if (MessageTtlValue == 0)
            {
                MessageTtlValue = AodvConfiguration.NetDiameter;
            }
        }

        public void ExportConfiguration(string path)
        {
            File.WriteAllText(path + "/configuration.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public string SimulationName { get; set; }
        public string SimulationGroup { get; set; }
        public string OutputPath { get; set; }

        //Constellation configuration
        public int NumberOfOrbits { get; set; }
        public int NumberOfSatellitesPerOrbit { get; set; }
        public int SatelliteAltitude { get; set; }
        public bool CrossSeamCommunicationEnabled { get; set; }

        //Constellation properties
        public int TotalNumberOfSatellites => NumberOfOrbits * NumberOfSatellitesPerOrbit;
        public int MinimumOrbitNumber => 0;
        public int MaximumOrbitNumber => NumberOfOrbits -1;
        public double AngleBetweenOrbits => 180/Convert.ToDouble(NumberOfOrbits);
        public double AngleBetweenSatellites => 360/Convert.ToDouble(NumberOfSatellitesPerOrbit);
        public double SatelliteSpeedKilometerPerHour => Math.Pow((398600 /(SimulationConstants.EarthRadiusKilometer + Convert.ToDouble(SatelliteAltitude))), 0.5) * 3600;
        public double SatelliteOrbitPeriodSeconds => (2 * Math.PI * Math.Pow((SimulationConstants.EarthRadiusKilometer + Convert.ToDouble(SatelliteAltitude)), 1.5)) / Math.Pow(398600, 0.5);

        //AODV configuration
        public AodvParameters AodvConfiguration { get; private set; }
        public bool AODV_UseGratuitousRREPs { get; set; }
        public bool AODV_UseRREPACKS { get; set; }
        public bool AODV_DestinationRespondOnlyFlag { get; set; }
        public bool AODV_UseHelloMessages { get; set; }
        public int AODV_ActiveRouteTimeout { get; set; }

        //Message configuration
        public int NewDataMessageProbability { get; set; } = 25;  //In percentage (0-100)
        public int DataMessageMinSizeBytes { get; set; } = 1024;
        public int DataMessageMaxSizeBytes { get; set; } = 512000;
        public int MessageTtlValue { get; set; } = 0;

        //Simulator properties
        public int TotalSimulationTicks => (TicksPerPositionChange * NumberOfSatellitesPerOrbit) * NumberOfPeriodsToSimulate; //How many ticks per position change * how many changes per orbit = total number of ticks per orbit , this * number of orbits = total ticks
        public int TicksPerPositionChange => SecondsPerPositionChange / SecondsPerTick; //Number of ticks before updating positions
        public int SecondsPerPositionChange => Convert.ToInt32(SatelliteOrbitPeriodSeconds / NumberOfSatellitesPerOrbit); //Number of seconds the constellation should stay in same place, before updating positions        


        //Simulation configuration
        public int NumberOfPeriodsToSimulate { get; set; } = 5;
        public int SecondsPerTick { get; set; } = 5;
        public bool EnableSatelliteLogging { get; set; } = false;
        public bool EnableDumpRoutingTables { get; set; } = false;
        public int LinkLayerReceptionProbability { get; set; } = 100;
        public int NumberOfSimulationIterations { get; set; } = 5;
        public bool ExportSnapshots { get; set; } = false;
        public string NetworkLayer { get; set; } = "AODV";
        public int LinkBufferSize { get; set; } = 50;
    }

    public enum NeighbourDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public static class SimulationConstants
    {
        public const int PacketProcessingDelay = 10; //Milliseconds

        public const double EarthRadiusKilometer = 6378.137; //http://nssdc.gsfc.nasa.gov/planetary/factsheet/earthfact.html

        public const string BroadcastAddress = "broadcast"; //Substitute for IP-broadcast address
        public const int MessageHeaderSize = IPv4HeaderSize;

        private const int IPv4HeaderSize = 20; //IPv4 header without options
        private const int IPv6HeaderSize = 40; //IPv6 header without extension headers
        private const int SixLowPanHeaderSize = 2; //Example compression taken from http://www.ti.com/lit/wp/swry013/swry013.pdf page 8
    }
}
