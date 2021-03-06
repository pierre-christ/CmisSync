﻿//-----------------------------------------------------------------------
// <copyright file="EventManagerInitializer.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib
{
    using System;

    using CmisSync.Lib.Accumulator;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Producer.ContentChange;
    using CmisSync.Lib.Producer.Crawler;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.SelectiveIgnore;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Enums;

    using log4net;

    /// <summary>
    /// Successful login handler. It handles the SuccessfulLoginEvent and registers
    /// the necessary handlers and registers the root folder to the MetaDataStorage.
    /// </summary>
    public class EventManagerInitializer : ReportingSyncEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EventManagerInitializer));

        private ContentChangeEventAccumulator ccaccumulator;
        private RepoInfo repoInfo;
        private IMetaDataStorage storage;
        private IFileTransmissionStorage fileTransmissionStorage;
        private ContentChanges contentChanges;
        private RemoteObjectFetcher remoteFetcher;
        private DescendantsCrawler crawler;
        private SyncMechanism mechanism;
        private IFileSystemInfoFactory fileSystemFactory;
        private IgnoreAlreadyHandledContentChangeEventsFilter alreadyHandledFilter;
        private RemoteObjectMovedOrRenamedAccumulator romaccumulator;
        private IFilterAggregator filter;
        private ActivityListenerAggregator activityListener;
        private IIgnoredEntitiesStorage ignoredStorage;
        private SelectiveIgnoreEventTransformer transformer;
        private SelectiveIgnoreFilter selectiveIgnoreFilter;
        private IgnoreFlagChangeDetection ignoreChangeDetector;
  
        /// <summary>
        /// Initializes a new instance of the <see cref="EventManagerInitializer"/> class.
        /// </summary>
        /// <param name='queue'>The SyncEventQueue.</param>
        /// <param name='storage'>Storage for Metadata.</param>
        /// <param name='repoInfo'>Repo info.</param>
        /// <param name="filter">Filter aggregation.</param>
        /// <param name='activityListner'>Listener for Sync activities.</param>
        /// <param name='fsFactory'>File system factory.</param>
        /// <exception cref='ArgumentNullException'>
        /// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
        /// </exception>
        public EventManagerInitializer(
            ISyncEventQueue queue,
            IMetaDataStorage storage,
            IFileTransmissionStorage fileTransmissionStorage,
            IIgnoredEntitiesStorage ignoredStorage,
            RepoInfo repoInfo,
            IFilterAggregator filter,
            ActivityListenerAggregator activityListener,
            IFileSystemInfoFactory fsFactory = null) : base(queue)
        {
            if (storage == null) {
                throw new ArgumentNullException("storage null");
            }

            if (fileTransmissionStorage == null)
            {
                throw new ArgumentNullException("fileTransmissionStorage null");
            }

            if (repoInfo == null)
            {
                throw new ArgumentNullException("Repoinfo null");
            }

            if (filter == null) {
                throw new ArgumentNullException("Filter null");
            }

            if (activityListener == null) {
                throw new ArgumentNullException("Given activityListener is null");
            }

            if (ignoredStorage == null) {
                throw new ArgumentNullException("Given storage for ignored entries is null");
            }

            if (fsFactory == null) {
                this.fileSystemFactory = new FileSystemInfoFactory();
            } else {
                this.fileSystemFactory = fsFactory;
            }

            this.filter = filter;
            this.repoInfo = repoInfo;
            this.storage = storage;
            this.ignoredStorage = ignoredStorage;
            this.fileTransmissionStorage = fileTransmissionStorage;
            this.activityListener = activityListener;
        }

        /// <summary>
        /// Handle the specified e if it is a SuccessfulLoginEvent
        /// </summary>
        /// <param name='e'>
        /// The event.
        /// </param>
        /// <returns>
        /// true if handled.
        /// </returns>
        public override bool Handle(ISyncEvent e) {
            if (e is SuccessfulLoginEvent) {
                var successfulLoginEvent = e as SuccessfulLoginEvent;
                var session = successfulLoginEvent.Session;

                var remoteRoot = successfulLoginEvent.Session.GetObjectByPath(this.repoInfo.RemotePath) as IFolder;

                // Remove former added instances from event Queue.EventManager
                if (this.ccaccumulator != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.ccaccumulator);
                }

                if (this.contentChanges != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.contentChanges);
                }

                if (this.alreadyHandledFilter != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.alreadyHandledFilter);
                }

                if (this.selectiveIgnoreFilter != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.selectiveIgnoreFilter);
                }

                if (this.transformer != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.transformer);
                }

                if (this.ignoreChangeDetector != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.ignoreChangeDetector);
                }

                if (session.AreChangeEventsSupported() &&
                    (this.repoInfo.SupportedFeatures == null || this.repoInfo.SupportedFeatures.GetContentChangesSupport != false)) {
                    Logger.Info("Session supports content changes");

                    // Add Accumulator
                    this.ccaccumulator = new ContentChangeEventAccumulator(session, this.Queue);
                    this.Queue.EventManager.AddEventHandler(this.ccaccumulator);

                    // Add Content Change sync algorithm
                    this.contentChanges = new ContentChanges(session, this.storage, this.Queue);
                    this.Queue.EventManager.AddEventHandler(this.contentChanges);

                    // Add Filter of already handled change events
                    this.alreadyHandledFilter = new IgnoreAlreadyHandledContentChangeEventsFilter(this.storage, session);
                    this.Queue.EventManager.AddEventHandler(this.alreadyHandledFilter);
                }

                if (session.SupportsSelectiveIgnore()) {
                    // Transforms events of ignored folders
                    this.transformer = new SelectiveIgnoreEventTransformer(this.ignoredStorage, this.Queue);
                    this.Queue.EventManager.AddEventHandler(this.transformer);

                    // Filters events of ignored folders
                    this.selectiveIgnoreFilter = new SelectiveIgnoreFilter(this.ignoredStorage);
                    this.Queue.EventManager.AddEventHandler(this.selectiveIgnoreFilter);

                    // Detection if any ignored object has changed its state
                    this.ignoreChangeDetector = new IgnoreFlagChangeDetection(this.ignoredStorage, new PathMatcher.PathMatcher(this.repoInfo.LocalPath, this.repoInfo.RemotePath), this.Queue);
                    this.Queue.EventManager.AddEventHandler(this.ignoreChangeDetector);
                }

                // Add remote object fetcher
                if (this.remoteFetcher != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.remoteFetcher);
                }

                this.remoteFetcher = new RemoteObjectFetcher(session, this.storage);
                this.Queue.EventManager.AddEventHandler(this.remoteFetcher);

                // Add crawler
                if (this.crawler != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.crawler);
                }

                this.crawler = new DescendantsCrawler(this.Queue, remoteRoot, this.fileSystemFactory.CreateDirectoryInfo(this.repoInfo.LocalPath), this.storage, this.filter, this.activityListener, this.ignoredStorage);
                this.Queue.EventManager.AddEventHandler(this.crawler);

                // Add remote object moved accumulator
                if (this.romaccumulator != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.romaccumulator);
                }

                this.romaccumulator = new RemoteObjectMovedOrRenamedAccumulator(this.Queue, this.storage, this.fileSystemFactory);
                this.Queue.EventManager.AddEventHandler(this.romaccumulator);

                // Add sync mechanism
                if (this.mechanism != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.mechanism);
                }

                var localDetection = new LocalSituationDetection();
                var remoteDetection = new RemoteSituationDetection();

                this.mechanism = new SyncMechanism(localDetection, remoteDetection, this.Queue, session, this.storage, this.fileTransmissionStorage, this.activityListener, this.filter);
                this.Queue.EventManager.AddEventHandler(this.mechanism);

                var localRootFolder = this.fileSystemFactory.CreateDirectoryInfo(this.repoInfo.LocalPath);
                Guid rootFolderGuid;
                if (!Guid.TryParse(localRootFolder.GetExtendedAttribute(MappedObject.ExtendedAttributeKey), out rootFolderGuid)) {
                    try {
                        rootFolderGuid = Guid.NewGuid();
                        localRootFolder.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, rootFolderGuid.ToString(), false);
                    } catch (ExtendedAttributeException ex) {
                        Logger.Warn("Problem on setting Guid of the root path", ex);
                        rootFolderGuid = Guid.Empty;
                    }
                }

                var rootFolder = new MappedObject("/", remoteRoot.Id, MappedObjectType.Folder, null, remoteRoot.ChangeToken) {
                    LastRemoteWriteTimeUtc = remoteRoot.LastModificationDate,
                    Guid = rootFolderGuid
                };

                Logger.Debug("Saving Root Folder to DataBase");
                this.storage.SaveMappedObject(rootFolder);

                // Sync up everything that changed
                // since we've been offline
                // start full crawl sync on beginning
                this.Queue.AddEvent(new StartNextSyncEvent(true));
                return true;
            }

            return false;
        }
    }
}