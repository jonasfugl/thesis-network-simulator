using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstellationSimulator.Positions;

namespace ConstellationSimulator.Test.Unit
{
    [TestFixture]
    public class CoordinateTests
    {
        private ConstellationSimulator.Positions.Coordinate _uut;
        private double validLat = 45;
        private double validLon = 45;
        private double validAlt = 100;

        private double invalidLat = -91;
        private double invalidLon = -181;
        private double invalidAlt = -1;

        [Test]
        public void ValidLatLonAlt_ValuesSatCorrectly()
        {
            _uut = new Coordinate(validLat, validLon, validAlt);

            Assert.AreEqual(validLat, _uut.Latitude);
            Assert.AreEqual(validLon, _uut.Longitude);
            Assert.AreEqual(validAlt, _uut.Altitude);
        }

        [Test]
        public void InvalidLat_ValueSatToZero()
        {
            _uut = new Coordinate(invalidLat, validLon, validAlt);
            Assert.AreEqual(0, _uut.Latitude);
        }

        [Test]
        public void InvalidLon_ValueSatToZero()
        {
            _uut = new Coordinate(validLat, invalidLon, validAlt);
            Assert.AreEqual(0, _uut.Longitude);
        }

        [Test]
        public void InvalidAlt_ValueSatToZero()
        {
            _uut = new Coordinate(validLat, validLon, invalidAlt);
            Assert.AreEqual(0, _uut.Altitude);
        }
    }
}
