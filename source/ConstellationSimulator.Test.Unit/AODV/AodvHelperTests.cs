using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator.AODV;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Helpers;
using ConstellationSimulator.Statistics;
using NUnit.Framework;

namespace ConstellationSimulator.Test.Unit.AODV
{
    [TestFixture]
    class AodvHelperTests
    {
        private AodvHelper _uut;

        private string destination = "destination";
        private const string LocalAddr = "local";


        [SetUp]
        public void SetUp()
        {
            var conf = new SimulationConfiguration();
            conf.Initialize();
            conf.EnableSatelliteLogging = false;

            var results = new StatisticsCollector();
            results.NewSimulation();

            var logger = new Logger(string.Empty, conf);

            _uut = new AodvHelper(LocalAddr, conf, logger);
        }

    }
}
