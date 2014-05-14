//-----------------------------------------------------------------------
// <copyright file="FullRepoTests.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

/**
 * Unit Tests for CmisSync.
 *
 * To use them, first create a JSON file containing the credentials/parameters to your CMIS server(s)
 * Put it in TestLibrary/test-servers.json and use this format:
[
    [
        "unittest1",
        "/mylocalpath",
        "/myremotepath",
        "http://example.com/p8cmis/resources/Service",
        "myuser",
        "mypassword",
        "repository987080"
    ],
    [
        "unittest2",
        "/mylocalpath",
        "/myremotepath",
        "http://example.org:8080/Nemaki/cmis",
        "myuser",
        "mypassword",
        "repo3"
    ]
]
 */

namespace TestLibrary.IntegrationTests
{
    using System;
    using System.Net;

    using CmisSync.Lib;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Sync;

    using log4net;

    using Moq;

    using NUnit.Framework;

    // Default timeout per test is 15 minutes
    [TestFixture, Timeout(900000)]
    public class FullRepoTests
    {
        private RepoInfo repoInfo;

        [TestFixtureSetUp]
        public void ClassInit()
        {
            // Disable HTTPS Verification
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            // Reanable HTTPS Verification
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [SetUp]
        public void Init()
        {
            var config = ITUtils.GetConfig();
            this.repoInfo = new RepoInfo {
                AuthenticationType = AuthenticationType.BASIC,
                LocalPath = config[1].ToString(),
                RemotePath = config[2].ToString(),
                Address = new XmlUri(new Uri(config[3].ToString())),
                User = config[4].ToString(),
                RepositoryId = config[6].ToString()
            };
            this.repoInfo.SetPassword(config[5].ToString());
        }

        [Test, Category("Slow")]
        public void FullRepoTest()
        {
            var activityListener = new Mock<IActivityListener>();
            var queue = new SingleStepEventQueue(new SyncEventManager());
            var repo = new CmisRepoMock(this.repoInfo, activityListener.Object, queue);
            repo.Initialize();  
            
            queue.Run();
        }

        private class CmisRepoMock : CmisRepo {
            public CmisRepoMock(RepoInfo repoInfo, IActivityListener activityListener, SingleStepEventQueue queue) : base(repoInfo, activityListener, true, queue)
            {
            }
        }
    }
}
