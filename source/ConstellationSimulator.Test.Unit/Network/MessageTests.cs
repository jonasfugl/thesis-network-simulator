using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator.AODV;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Helpers;
using ConstellationSimulator.Network;
using ConstellationSimulator.Network.Message;
using NUnit.Framework;

namespace ConstellationSimulator.Test.Unit
{
    [TestFixture]
    class MessageTests
    {
        private string destination = "DestinationName";
        private string source = "SourceName";
        private int ttl = 20;
        
        private int payloadSize = 500;
        private Message _uut;

        [SetUp]
        public void SetUp()
        {
            _uut = new Message(new DataMsg(payloadSize), destination, source, ttl);
        }

        [Test]
        public void Constructor_ValuesSatCorrectly()
        {
            Assert.AreEqual(MessageType.Data, _uut.Type);
            Assert.AreEqual(payloadSize, _uut.PayloadSize);
            Assert.AreEqual(destination, _uut.DestinationAddress);
            Assert.AreEqual(source, _uut.SourceAddress);
            Assert.AreEqual(ttl, _uut.Ttl);

            Assert.AreEqual(source, _uut.PreviousHop);
            Assert.AreEqual(0, _uut.HopCount);
            Assert.AreEqual(0, _uut.TotalPropagationDelay);
        }

        [Test]
        public void DecrementTtl_TtlDecreased()
        {
            _uut.DecrementTtl();
            Assert.AreEqual(ttl-1, _uut.Ttl);
        }

        [Test]
        public void DecrementTtl_HopCountIncreased()
        {
            _uut.DecrementTtl();
            Assert.AreEqual(1, _uut.HopCount);
        }

        [Test]
        public void Size()
        {
            var expectedSize = SimulationConstants.MessageHeaderSize + payloadSize;
            Assert.AreEqual(expectedSize, _uut.Size);
        }

        [Test]
        public void TotalProcessingDelay()
        {
            var expected = _uut.HopCount * SimulationConstants.PacketProcessingDelay;
            Assert.AreEqual(expected, _uut.TotalProcessingDelay);
        }

        [Test]
        public void DecrementTtl_ThrowsException()
        {
            _uut.Ttl = 0;
            
            Assert.Throws<ArgumentException>( () => { _uut.DecrementTtl(); } );
        }


    }
}
