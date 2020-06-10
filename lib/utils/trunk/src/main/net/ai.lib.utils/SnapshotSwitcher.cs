/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ai.lib.utils
{
    /// <summary>
    /// Manages snapshots for tasks that store intermediate results on disk.
    /// It creates a directories for each snapshot under the base path. Directories are
    /// named 0, 1, ..., snapshotsCount - 1.
    /// When started, it finds the newest snapshot by comparing write date of files with the given name
    /// (keyFileName) in each snapshot directory.
    /// If snapshot is found, IsSnapshotAvailable is true and the user can load it.
    /// Otherwise, the current snapshot is set to 0 and the user can store the data in it.
    /// When the user has new data, it calls GoToNextSnapshot() to switch snapshots cyclically (0, 1, ..., max, 0, ...).
    /// </summary>
    public class SnapshotSwitcher
    {
        #region Public API

        /// <summary>
        /// Creates an instance, creates snapshot directories if not available and
        /// finds the newest snapshot if available.
        /// </summary>  
        public SnapshotSwitcher(string basePath, string keyFileName, int snapshotsCount)
        {
            BasePath = basePath;
            KeyFileName = keyFileName;
            SnapshotsCount = snapshotsCount;

            CreateDirectoryStructure();
            FindNewestSnapshot();
        }

        /// <summary>
        /// Base path where snapshot directories are is created.
        /// </summary>
        public string BasePath
        {
            protected set;
            get;
        }

        /// <summary>
        /// File name to find the newest snapshot.
        /// </summary>
        public string KeyFileName
        {
            protected set;
            get;
        }

        /// <summary>
        /// Number of snapshots.
        /// </summary>
        public int SnapshotsCount
        {
            protected set;
            get;
        }

        /// <summary>
        /// Checks if a key file exists in the current snapshot directory.
        /// </summary>
        public bool IsSnapshotAvailable
        {
            get
            {
                string keyFile = Path.Combine(CurrentSnapshotPath, KeyFileName);
                return File.Exists(keyFile);
            }
        }

        /// <summary>
        /// Path to the current snapshot.
        /// </summary>
        public string CurrentSnapshotPath
        {
            get;
            protected set;
        }

        /// <summary>
        /// Index of the current snapshot.
        /// </summary>
        public int CurrentSnapshotIndex
        {
            get;
            protected set;
        }

        /// <summary>
        /// Go to the next snapshot (0 -> 1 -> ... max -> 0 ...).
        /// </summary>
        public void GoToNextSnapshot()
        {
            CurrentSnapshotIndex = (CurrentSnapshotIndex + 1) % SnapshotsCount;
            CurrentSnapshotPath = GetSnapshotPath(CurrentSnapshotIndex);
        }

        /// <summary>
        /// Clears all snapshot directories recursively and sets current snapshot to 0.
        /// </summary>
        /// <param name="recreateStructure">If true, recreates directories again after clearing, 
        /// this allows to continue working with the object.</param>
        public void Clear(bool recreateStructure)
        {
            for (int i = 0; i < SnapshotsCount; ++i)
            {
                string path = GetSnapshotPath(i);
                Directory.Delete(path, true);
            }

            CurrentSnapshotIndex = -1;
            GoToNextSnapshot(); // Set to 0.
            if (recreateStructure)
            {
                CreateDirectoryStructure();
            }
        }

        #endregion

        #region Protected API

        protected void CreateDirectoryStructure()
        {
            for (int i = 0; i < SnapshotsCount; ++i)
            {
                string path = GetSnapshotPath(i);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        protected string GetSnapshotPath(int index)
        {
            string path = Path.Combine(BasePath, index.ToString());
            return path;
        }

        protected void FindNewestSnapshot()
        {
            DateTime newestTime = DateTime.MinValue;
            CurrentSnapshotPath = null;
            CurrentSnapshotIndex = -1;
            for (int i = 0; i < SnapshotsCount; ++i)
            {
                string path = GetSnapshotPath(i);
                string keyFile = Path.Combine(path, KeyFileName);
                if (File.Exists(keyFile))
                {
                    DateTime writeTime = File.GetLastWriteTime(keyFile);
                    if (writeTime > newestTime)
                    {
                        newestTime = writeTime;
                        CurrentSnapshotPath = path;
                        CurrentSnapshotIndex = i;
                    }
                }
            }
            if(CurrentSnapshotIndex == -1)
            {
                // This will set current snapshot to 0.
                GoToNextSnapshot();
            }
        }

        #endregion
    }
}
