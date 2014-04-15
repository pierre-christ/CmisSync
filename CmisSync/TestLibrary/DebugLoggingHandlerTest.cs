using log4net;
using log4net.Config;

using System;
using System.IO;
namespace TestLibrary
{
    using NUnit.Framework;
    using CmisSync.Lib;
    using CmisSync.Lib.Events;

    [TestFixture]
    public class DebugLoggingHandlerTest
    {

        [TestFixtureSetUp]
        public void ClassInit()
        {
            log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());
        }


        [Test, Category("Fast")]
        public void ToStringTest() {
            var handler = new DebugLoggingHandler();
            Assert.AreEqual("CmisSync.Lib.Events.DebugLoggingHandler with Priority 100000", handler.ToString());
        }
        
        [Test, Category("Fast")]
        public void PriorityTest() {
            var handler = new DebugLoggingHandler();
            Assert.AreEqual(100000, handler.Priority);
            
        }
        
    }
}
