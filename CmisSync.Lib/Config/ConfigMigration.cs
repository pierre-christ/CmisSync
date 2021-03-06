//-----------------------------------------------------------------------
// <copyright file="ConfigMigration.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Migrate config.xml from past versions.
    /// </summary>
    public static class ConfigMigration
    {
        /// <summary>
        /// Migrate from the config.xml format of CmisSync 0.3.9 to the current format, if necessary.
        /// </summary>
        public static void Migrate()
        {
            // If file does not exist yet, no need for migration.
            if (!File.Exists(ConfigManager.CurrentConfigFile))
            {
                return;
            }

            // Replace uppercase notification boolean to lower case
            ReplaceCaseSensitiveNotification();

            // Replace XML root element from <sparkleshare> to <CmisSync>
            ReplaceXMLRootElement();
            CheckForDoublicatedLog4NetElement();
            ReplaceTrunkByChunk();
            MigrateIgnoredPatterns();
            MigrateHiddenReposPatterns();
        }

        private static void CheckForDoublicatedLog4NetElement()
        {
            XmlElement log4net = ConfigManager.CurrentConfig.GetLog4NetConfig();
            if (log4net.ChildNodes.Item(0).Name.Equals("log4net"))
            {
                ConfigManager.CurrentConfig.SetLog4NetConfig(log4net.ChildNodes.Item(0));
                ConfigManager.CurrentConfig.Save();
            }
        }

        private static void ReplaceTrunkByChunk()
        {
            var fileContents = System.IO.File.ReadAllText(ConfigManager.CurrentConfigFile);
            if (fileContents.Contains("<trunkSize>") || fileContents.Contains("</trunkSize>"))
            {
                fileContents = fileContents.Replace("<trunkSize>", "<chunkSize>");
                fileContents = fileContents.Replace("</trunkSize>", "</chunkSize>");
                System.IO.File.WriteAllText(ConfigManager.CurrentConfigFile, fileContents);
                System.Console.Out.WriteLine("Migrated old trunkSize to chunkSize");
            }
        }

        /// <summary>
        /// Replace XML root element name from sparkleshare to CmisSync
        /// </summary>
        private static void ReplaceXMLRootElement()
        {
            try
            {
                // If log4net element is found, it means that the root element is already correct.
                XmlElement element = ConfigManager.CurrentConfig.GetLog4NetConfig();
                if (element != null)
                {
                    return;
                }
            }
            catch (Exception)
            {
                // Replace root XML element from <sparkleshare> to <CmisSync>
                var fileContents = System.IO.File.ReadAllText(ConfigManager.CurrentConfigFile);
                fileContents = fileContents.Replace("<sparkleshare>", "<CmisSync>");
                fileContents = fileContents.Replace("</sparkleshare>", "</CmisSync>");

                System.IO.File.WriteAllText(ConfigManager.CurrentConfigFile, fileContents);
            }
        }

        /// <summary>
        /// Replaces True by true in the notification to make it possible to deserialize
        /// Xml Config to C# Objects
        /// </summary>
        private static void ReplaceCaseSensitiveNotification()
        {
            var fileContents = System.IO.File.ReadAllText(ConfigManager.CurrentConfigFile);
            if (fileContents.Contains("<notifications>True</notifications>"))
            {
                fileContents = fileContents.Replace("<notifications>True</notifications>", "<notifications>true</notifications>");
                System.IO.File.WriteAllText(ConfigManager.CurrentConfigFile, fileContents);
                System.Console.Out.WriteLine("Migrated old upper case notification to lower case");
            }
        }

        private static void MigrateIgnoredPatterns()
        {
            if(ConfigManager.CurrentConfig.Version < 1.0)
            {
                Config conf = ConfigManager.CurrentConfig;
                conf.Version = 1.0;
                conf.IgnoreFileNames = Config.CreateInitialListOfGloballyIgnoredFileNames();
                conf.IgnoreFolderNames = Config.CreateInitialListOfGloballyIgnoredFolderNames();
                conf.Save();
            }
        }

        private static void MigrateHiddenReposPatterns()
        {
            if(ConfigManager.CurrentConfig.Version < 1.1)
            {
                Config conf = ConfigManager.CurrentConfig;
                conf.Version = 1.1;
                conf.HiddenRepoNames = Config.CreateInitialListOfGloballyHiddenRepoNames();
                conf.Save();
            }
        }
    }
}
