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
    class DataMsgTests
    {
        [Test]
        public void Constructor_ValuesInitialized()
        {
            int size = 10;
            var uut = new DataMsg(size);

            Assert.AreEqual(size, uut.Size);
            Assert.AreEqual(MessageType.Data, uut.Type);
        }

        [Test]
        public void CopyConstructor_ValuesInitialized()
        {
            int size = 10;
            var dmsg = new DataMsg(size);

            var uut = new DataMsg(dmsg);

            Assert.AreEqual(dmsg.Type, uut.Type);
            Assert.AreEqual(dmsg.Size, uut.Size);
        }
    }
}
