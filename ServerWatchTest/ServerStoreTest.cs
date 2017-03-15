using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerWatch;
using System.Threading.Tasks;

namespace ServerWatchTest
{
    [TestClass]
    public class ServerStoreTest
    {
        private string onlineQueryableServer = "45.32.194.114:27015";
        private string nonexistentServer = "0.0.0.0:27015";

        [TestMethod]
        public void TestAddServer()
        {
            var model = new ServerStore();

            Assert.AreEqual(0, model.Servers.Count);

            model.AddServer(onlineQueryableServer);

            Assert.AreEqual(1, model.Servers.Count);
        }

        [TestMethod]
        public void TestRemoveServer()
        {
            var model = new ServerStore();
            model.AddServer(onlineQueryableServer);
            Assert.AreEqual(1, model.Servers.Count);

            model.RemoveServer(onlineQueryableServer);
            Assert.AreEqual(0, model.Servers.Count);
        }

        [TestMethod]
        public async Task TestQueryServer()
        {
            var model = new ServerStore();

            // Query a valid server, verify we got results.
            model.AddServer(onlineQueryableServer);
            await model.QueryServers();

            Assert.AreEqual(1, model.Results.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(System.Net.Sockets.SocketException))]
        public async Task TestInvalidQuery()
        {
            var model = new ServerStore();

            // Query an invalid address.

            model.AddServer(nonexistentServer);
            await model.QueryServers();

            Assert.AreNotEqual(2, model.Results.Count);
        }

        
    }
}
