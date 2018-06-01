using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator.AODV;
using ConstellationSimulator.Network.Message;
using NUnit.Framework;

namespace ConstellationSimulator.Test.Unit.AODV
{
    [TestFixture]
    class RrepAckTests
    {
        [Test]
        public void Constructor_ValuesInitialized()
        {
            var uut = new Rrepack();

            Assert.AreEqual(MessageType.Rrepack, uut.Type);
            Assert.AreEqual(2, uut.Size);
        }

        [Test]
        public void CopyConstructor_ValuesInitialized()
        {
            var rrepack = new Rrepack();
            var uut = new Rrepack(rrepack);

            Assert.AreEqual(rrepack.Type, uut.Type);
            Assert.AreEqual(rrepack.Size, uut.Size);
        }
    }
}
