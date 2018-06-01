using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator.AODV;
using ConstellationSimulator.Network;
using ConstellationSimulator.Network.Message;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ConstellationSimulator.Test.Unit
{
    [TestFixture]
    class LinkLayerResultTests
    {
        [Test]
        public void Constructor_ValuesInitialized()
        {
            var type = LinkLayerResultType.Success;
            var uut = new LinkLayerResult(type);

            Assert.AreEqual(type, uut.Result);
        }

        [Test]
        public void Constructor_Explicit_ValuesInitialized()
        {
            var nb = "neighbourName";
            var msg = new Message(new DataMsg(2), "destination", "source", 1);

            var uut = new LinkLayerResult(msg, nb);

            Assert.AreEqual(nb, uut.MissingNeighbour);
            Assert.AreEqual(msg, uut.DroppedMessage);
            Assert.AreEqual(LinkLayerResultType.NextHopNotFound, uut.Result);
        }
    }
}
