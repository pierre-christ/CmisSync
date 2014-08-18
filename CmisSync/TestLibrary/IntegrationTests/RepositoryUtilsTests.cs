//-----------------------------------------------------------------------
// <copyright file="RepositoryUtilsTests.cs" company="GRAU DATA AG">
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
 * Unit Tests for Repository Utils.
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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Cmis.UiUtils;
    using CmisSync.Lib.Config;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Data.Impl;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    // Default timeout per test is 15 minutes
    [TestFixture, Timeout(900000)]
    public class RepositoryUtilsTests : IsTestWithConfiguredLog4Net
    {
        private readonly string cmisSyncDir = ConfigManager.CurrentConfig.GetFoldersPath();

        /// <summary>
        /// Waits until checkStop is true or waiting duration is reached.
        /// </summary>
        /// <returns>
        /// True if checkStop is true, otherwise waits for pollInterval miliseconds and checks again until the wait threshold is reached.
        /// </returns>
        /// <param name='checkStop'>
        /// Checks if the condition, which is waited for is <c>true</c>.
        /// </param>
        /// <param name='wait'>
        /// Waiting threshold. If this is reached, <c>false</c> will be returned.
        /// </param>
        /// <param name='pollInterval'>
        /// Sleep duration between two condition validations by calling checkStop.
        /// </param>
        public static bool WaitUntilDone(Func<bool> checkStop, int wait = 300000, int pollInterval = 1000)
        {
            while (wait > 0)
            {
                System.Threading.Thread.Sleep(pollInterval);
                wait -= pollInterval;
                if (checkStop()) {
                    return true;
                }

                Console.WriteLine(string.Format("Retry Wait in {0}ms", pollInterval));
            }

            Console.WriteLine("Wait was not successful");
            return false;
        }

        [TestFixtureSetUp]
        public void ClassInit()
        {
            // Disable HTTPS Verification
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            try {
                File.Delete(ConfigManager.CurrentConfig.GetLogFilePath());
            } catch (IOException) {
            }
        }

        [TearDown]
        public void TearDown()
        {
            foreach (string file in Directory.GetFiles(this.cmisSyncDir)) {
                if (file.EndsWith(".cmissync"))
                {
                    File.Delete(file);
                }
            }

            // Reanable HTTPS Verification
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [Test, TestCaseSource(typeof(ITUtils), "TestServers"), Category("Slow"), Timeout(20000)]
        public void GetRepositories(
            string canonical_name,
            string localPath,
            string remoteFolderPath,
            string url,
            string user,
            string password,
            string repositoryId)
        {
            ServerCredentials credentials = new ServerCredentials()
            {
                Address = new Uri(url),
                UserName = user,
                Password = password
            };

            Dictionary<string, string> repos = CmisUtils.GetRepositories(credentials);

            foreach (KeyValuePair<string, string> pair in repos)
            {
                Assert.That(string.IsNullOrEmpty(pair.Key), Is.False);
                Assert.That(string.IsNullOrEmpty(pair.Value), Is.False);
            }

            Assert.NotNull(repos);
        }

        [Test, TestCaseSource(typeof(ITUtils), "TestServersFuzzy"), Category("Slow"), Timeout(60000)]
        public void GetRepositoriesFuzzy(string url, string user, string password)
        {
            ServerCredentials credentials = new ServerCredentials()
            {
                Address = new Uri(url),
                UserName = user,
                Password = password
            };
            Tuple<CmisServer, Exception> server = CmisUtils.GetRepositoriesFuzzy(credentials);
            Assert.NotNull(server.Item1);
        }
    }
}