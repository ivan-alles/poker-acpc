/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using ai.lib.utils;

namespace ai.lib.algorithms.sort
{
    /// <summary>
    /// External merge sorting.
    /// Sorts one or many big files containing elements. In order to use this class you have to implement
    /// IElement interface and than call Sort() or Merge(). 
    /// </summary>
    public class ExternalMergeSort<T>
    {
        /// <summary>
        /// Operations with elements to be sorted. You have to implement this class.
        /// All functions must be written as though they were static and reentrant,
        /// because there is no guarantee in which context and sequence they will be called.
        /// </summary>
        public interface IElement: IComparer<T>
        {
            /// <summary>
            /// Writes one element to a binary writer.
            /// </summary>
            /// <param name="element">The element.</param>
            void Write(T element, BinaryWriter w);

            /// <summary>
            /// Read one element from a file. It must either return a valid record or
            /// throw an exception. After reading out the last record the file position must be 
            /// at the end of the file (i.e. no tail containing some data is allowed in the file).
            /// </summary>
            T Read(BinaryReader r);
        }

        /// <summary>
        /// If true, prints messages to console.
        /// </summary>
        public bool IsVerbose
        {
            set; get;
        }

        /// <summary>
        /// Sorts files in the input path.
        /// </summary>
        /// <param name="inputPath">Either file name or directory name. In the latter case the directory will be processed recursively
        /// and all files will be merged together in sorted order.</param>
        /// <param name="tempDir">Directory for temporary files.</param>
        /// <param name="inMemSortCount">Number of elements to sort in memory. Must be found impirically. Usually the more memory is 
        /// used the faster the sort. At the moment there is a .NET limitation of 2G of bytes.</param>
        /// <param name="element">IElement.</param>
        /// <param name="resultFile">Full name of the file to store the result.</param>
        public void Sort(string inputPath, string tempDir, Int64 inMemSortCount, IElement element, string resultFile)
        {
            if(inMemSortCount > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    String.Format("Memory sorting size > {0} is not supported yet", int.MaxValue));
            }

            DirectoryExt.Delete(tempDir);
            Directory.CreateDirectory(tempDir);

            DateTime start = DateTime.Now;

            T[] arr = new T[inMemSortCount];
            _chunkCount = 0;
            Int64 elCount = SortChuncksPath(inputPath, tempDir, arr, element);

            double time = (DateTime.Now - start).TotalSeconds;

            if(IsVerbose)
            {
                Console.WriteLine("Sorted {0:#,#} chuncks, {1:#,#} elements, {2:#,#} el/s",
                    _chunkCount, elCount, elCount / time);
            }

            Merge(tempDir, element, resultFile);
        }

        /// <summary>
        /// For the case you have some sorted files that have to be merged together in sorted order, 
        /// this function does the job.
        /// </summary>
        /// <param name="sortedDir">Directory with sorted files.</param>
        /// <param name="element">IElement</param>
        /// <param name="resultFile">Full name of the result file.</param>
        public void Merge(string sortedDir, IElement element, string resultFile)
        {
            DateTime start = DateTime.Now;

            string[] files = Directory.GetFiles(sortedDir);
            List<MergeFile> mergeFiles = new List<MergeFile>(files.Length);
            for (int i = 0; i < files.Length; ++i)
            {
                MergeFile mf = new MergeFile(files[i], element);
                if(mf.MoveNext())
                {
                    // Non-empty file.
                    mergeFiles.Add(mf);
                }
            }
            Int64 elCount = 0;

            using (BinaryWriter writer = new BinaryWriter(new FileStream(resultFile, FileMode.Create, FileAccess.Write)))
            {
                while (mergeFiles.Count > 0)
                {
                    MergeFile lowest = mergeFiles[0];
                    for (int i = 1; i < mergeFiles.Count; ++i)
                    {
                        int compareResult = element.Compare(lowest.CurElement, mergeFiles[i].CurElement);
                        if (compareResult > 0)
                        {
                            // lowest > current
                            lowest = mergeFiles[i];
                        }
                    }
                    element.Write(lowest.CurElement, writer);
                    elCount++;
                    if (!lowest.MoveNext())
                    {
                        // File is exhausted.
                        lowest.Dispose();
                        mergeFiles.Remove(lowest);
                    }
                }
            }

            double time = (DateTime.Now - start).TotalSeconds;

            if (IsVerbose)
            {
                Console.WriteLine("Merged {0:#,#} chuncks, {1:#,#} elements, {2:#,#} el/s",
                    files.Length, elCount, elCount / time);
            }

        }

        class MergeFile : IDisposable
        {
            private readonly BinaryReader _reader;
            private Int64 _fileLength;
            private IElement _element;

            public MergeFile(string fileName, IElement element)
            {
                 FileInfo f = new FileInfo(fileName);
                _fileLength = f.Length;
                _element = element;
                _reader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read));
            }

            /// <summary>
            /// Current element.
            ///  </summary>
            /// Dev. notes: use a field instead of property, hopefully it avoids copying data on
            /// access and saves time.
            public T CurElement;

            public void Dispose()
            {
                _reader.Close();
            }

            public bool MoveNext()
            {
                if (_reader.BaseStream.Position >= _fileLength)
                {
                    return false;
                }
                CurElement = _element.Read(_reader);
                return true;
            }
        }

        /// <summary>
        /// Decides if input path is a directory or file and proceeds as necessary.
        /// </summary>
        Int64 SortChuncksPath(string inputPath, string tempDir, T[] arr, IElement element)
        {
            FileAttributes attr = File.GetAttributes(inputPath);
            Int64 elCount = 0;
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                elCount = SortChuncksDir(inputPath, tempDir, arr, element);
            }
            else
            {
                elCount = SortChuncksFile(inputPath, tempDir, arr, element);
            }
            return elCount;
        }

        /// <summary>
        /// Traverses a directory recursively and calls SortChuncksFile for each file.
        /// </summary>
        private Int64 SortChuncksDir(string dirName, string tempDir, T[] arr, IElement element)
        {
            string[] files = Directory.GetFiles(dirName);

            Int64 elCount = 0;

            foreach (string fileName in files)
            {
                elCount += SortChuncksFile(fileName, tempDir, arr, element);
            }
            string[] dirs = Directory.GetDirectories(dirName);
            foreach (string childDir in dirs)
            {
                elCount += SortChuncksPath(childDir, tempDir, arr, element);
            }
            return elCount;
        }

        Int64 SortChuncksFile(string fileName, string tempDir, T[] arr, IElement element)
        {
            FileInfo f = new FileInfo(fileName);
            Int64 totalElCount = 0;
            Int64 elCount = 0;
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader r = new BinaryReader(fs))
                {
                    for (; fs.Position < f.Length; )
                    {
                        arr[elCount++] = element.Read(r);
                        if(elCount == arr.Length)
                        {
                            SortAndWriteArray(tempDir, arr, elCount, element);
                            totalElCount += elCount;
                            elCount = 0;
                        }
                    }
                }
            } 
            if(elCount > 0)
            {
                SortAndWriteArray(tempDir, arr, elCount, element);
            }
            totalElCount += elCount;
            return totalElCount;
        }

        void SortAndWriteArray(string tempDir, T[] arr, Int64 actualSize, IElement element)
        {
            Array.Sort<T>(arr, 0, (int)actualSize, element);
            string fileName = Path.Combine(tempDir, String.Format("{0:D6}.dat", _chunkCount));
            _chunkCount++;
            WriteArray(fileName, arr, actualSize, element);
        }

        void WriteArray(string fileName, T [] array, Int64 count, IElement element)
        {
            using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write)))
            {
                for(Int64 i = 0; i < count; ++i)
                {
                    element.Write(array[i], writer);
                }
            }
        }

        private int _chunkCount;
    }
}
