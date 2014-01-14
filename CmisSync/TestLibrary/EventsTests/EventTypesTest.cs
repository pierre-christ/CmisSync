using log4net;
using log4net.Config;

using System;
using System.IO;
namespace TestLibrary.EventsTests
{
    using NUnit.Framework;
    using CmisSync.Lib;
    using CmisSync.Lib.Events;

    [TestFixture]
    public class EventTypesTest
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EventTypesTest));

        [TestFixtureSetUp]
        public void ClassInit()
        {
            log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());
        }


        [Test, Category("Fast")]
        public void FSEventTest() {
            ISyncEvent e = new FSEvent(WatcherChangeTypes.Created, "test");
            Assert.AreEqual("FSEvent with type \"Created\" on path \"test\"",e.ToString());
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void FSEventPreventNullTest() {
            ISyncEvent e = new FSEvent(WatcherChangeTypes.Created,null);
        }
    }
}
