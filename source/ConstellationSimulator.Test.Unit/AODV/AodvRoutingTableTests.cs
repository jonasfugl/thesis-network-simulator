using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator.AODV;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Helpers;
using ConstellationSimulator.Network;
using ConstellationSimulator.Statistics;
using NUnit.Framework;

namespace ConstellationSimulator.Test.Unit.AODV
{
    [TestFixture]
    class AodvRoutingTableTests
    {
        private AodvRoutingTable _uut;
        private string localAddress = "address";
        private string destination = "destination";

        [SetUp]
        public void SetUp()
        {
            var conf = new SimulationConfiguration();
            conf.Initialize();
            conf.EnableSatelliteLogging = false;

            var results = new StatisticsCollector();
            results.NewSimulation();

            var logger = new Logger(string.Empty, conf);

            _uut = new AodvRoutingTable(localAddress, logger, conf);
        }

        [Test]
        public void SequenceNumber_InitializedToZero()
        {
            Assert.AreEqual(0, _uut.SequenceNumber);
        }

        [Test]
        public void IncrementSequenceNumber_NumberIncremented()
        {
            _uut.IncrementSequenceNumber();

            Assert.AreEqual(1, _uut.SequenceNumber);
        }

        [Test]
        public void NextRouteRequestId_IdIncremented()
        {
            Assert.AreEqual(1, _uut.NextRouteRequestId);
            Assert.AreEqual(2, _uut.NextRouteRequestId);
        }

        [Test]
        public void RouteExists_NoRoutes()
        {
            var res = _uut.RouteExists(string.Empty);

            Assert.IsFalse(res);
        }

        [Test]
        public void AddEntry_EntryAdded()
        {
            var entry = new AodvTableEntry();
            entry.DestinationAddress = "destination";

            _uut.AddEntry(entry);

            var res = _uut.RouteExists("destination");
            Assert.IsTrue(res);
        }

        [Test]
        public void GetEntry_EntryDontExist()
        {
            Assert.IsNull(_uut.GetEntry("ThisAddressIsWrong"));
        }

        [Test]
        public void AddPrecursor_PrecursorAdded()
        {
            var entry = new AodvTableEntry();
            entry.DestinationAddress = "destination";
            _uut.AddEntry(entry);

            _uut.AddPrecursorToEntry("destination", "precursor");

            var precursorEntry = _uut.GetEntry("destination");

            Assert.True(precursorEntry.Precursors.Contains("precursor"));
        }

        [Test]
        public void UpdateExpirationTime_TimeUpdated()
        {
            var lifetime = DateTime.MaxValue;

            var entry = new AodvTableEntry();
            entry.DestinationAddress = destination;
            _uut.AddEntry(entry);

            _uut.UpdateExpirationTime(destination, lifetime);

            var lifetimeEntry = _uut.GetEntry(destination);
            Assert.AreEqual(lifetime, lifetimeEntry.ExpirationTime);
        }

        [Test]
        public void SetRouteActive_RouteActivated()
        {
            var entry = new AodvTableEntry();
            entry.DestinationAddress = destination;
            entry.Valid = false;
            _uut.AddEntry(entry);

            _uut.SetRouteActive(destination);

            var route = _uut.GetEntry(destination);
            Assert.True(route.Valid);
        }

        [Test]
        public void ActiveRouteExists_RouteExist()
        {
            var entry = new AodvTableEntry();
            entry.DestinationAddress = destination;
            entry.Valid = true;
            _uut.AddEntry(entry);

            Assert.IsTrue(_uut.ActiveRouteExists(destination));
        }

        [Test]
        public void ActiveRouteExists_RouteIsInvalid()
        {
            var entry = new AodvTableEntry();
            entry.DestinationAddress = destination;
            entry.Valid = false;
            _uut.AddEntry(entry);

            Assert.IsFalse(_uut.ActiveRouteExists(destination));
        }

        [Test]
        public void ActiveRouteExists_RouteDontExist()
        {
            var entry = new AodvTableEntry();
            entry.DestinationAddress = destination;
            entry.Valid = false;
            _uut.AddEntry(entry);

            Assert.IsFalse(_uut.ActiveRouteExists("unknownDestination"));
        }

        [Test]
        public void InvalidateEntry_EntryInvalidated()
        {
            var entry = new AodvTableEntry();
            entry.DestinationAddress = destination;
            entry.Valid = true;
            _uut.AddEntry(entry);

            Assert.IsTrue(_uut.ActiveRouteExists(destination));

            _uut.InvalidateEntry(destination);

            Assert.IsFalse(_uut.ActiveRouteExists(destination));
        }

        [Test]
        public void UpdateDestinationSequenceNumber_DsnUpdated()
        {
            var entry = new AodvTableEntry();
            entry.DestinationAddress = destination;
            entry.Valid = true;
            _uut.AddEntry(entry);

            _uut.UpdateDestinationSequenceNumber(destination, 1337);

            var route = _uut.GetEntry(destination);

            Assert.AreEqual(1337, route.DestinationSequenceNumber);
        }

        [Test]
        public void UpdateEntry_EntryUpdated()
        {
            var entry = new AodvTableEntry();
            entry.DestinationAddress = destination;
            entry.Valid = true;

            _uut.AddEntry(entry);

            entry.DestinationAddress = destination;
            entry.DestinationSequenceNumber = 1337;
            entry.ValidDestinationSequenceNumberFlag = true;
            entry.HopCount = 10;
            entry.Valid = true;
            entry.NextHop = destination;
            entry.ExpirationTime = DateTime.MaxValue;

            _uut.UpdateEntry(entry);

            var updEntry = _uut.GetEntry(destination);

            Assert.AreEqual(entry.DestinationAddress, updEntry.DestinationAddress);
            Assert.AreEqual(entry.DestinationSequenceNumber, updEntry.DestinationSequenceNumber);
            Assert.AreEqual(entry.ValidDestinationSequenceNumberFlag, updEntry.ValidDestinationSequenceNumberFlag);
            Assert.AreEqual(entry.HopCount, updEntry.HopCount);
            Assert.AreEqual(entry.Valid, updEntry.Valid);
            Assert.AreEqual(entry.NextHop, updEntry.NextHop);
            Assert.AreEqual(DateTime.MaxValue, updEntry.ExpirationTime);
        }
    }
}
