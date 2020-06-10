/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace ai.lib.algorithms.tree
{
    /// <summary>
    /// Flat serializable tree using unmanaged memory. Keeps a tree structure in a plain array with a 
    /// very small overhead of 1 byte per node. The elements are stored in preorder. 
    /// This allows very fast pre- and post-order traversal. For other traversal types an index can be built.
    /// <para>This class is designed to keep huge trees (>1G nodes) with fast access and compact representation both in memory and on disk.
    /// This imposes a couple of tough design decisions. To overcome .NET limitation of 2G for one managed 
    /// allocation it is necessary to use unmanaged memory.  As it is impossible to have pointers to generic types, 
    /// this tree does not use generic parameters. Also it is only possible to store structs with unmanaged fields in this tree.
    /// This should be ok, because creating billions of managed objects will be too much overhead anyway.
    /// </para>
    /// <para>The recomended usage of this class is to create a derived class like this:</para>
    /// <para>unsafe class  MyTree : UFTree</para>
    /// <para>{</para>
    /// <para>  /// Create a consturtor allocating the base class. </para>
    /// <para>  public TestTree(Int64 nodesCount): base(nodesCount, Marshal.SizeOf(typeof(MyNode)))</para>
    /// <para>  {</para>
    /// <para>    // We can clear the unmanaged memory.</para>
    /// <para>    UnmanagedMemory.SetMemory(_nodesPtr.Ptr, _nodesByteSize, 0);</para>
    /// <para>    _nodes = (TestNode*)_nodesPtr.Ptr.ToPointer();</para>
    /// <para>  }</para>
    /// <para>  /// Access to user-defined nodes. </para>
    /// <para>  public MyNode* Nodes</para>
    /// <para>  {</para>
    /// <para>    get { return _nodes; }</para>
    /// <para>  }</para>
    /// <para>  private MyNode * _nodes;</para>
    /// <para>}</para>
    /// </summary>
    /// Todo: for FDA - store node byte size instead of all nodes size.
    public unsafe class UFTree: IDisposable
    {
        #region Public API

        public BdsVersion Version
        {
            get;
            private set;
        }

        public Int64 NodesCount
        {
            get { return _nodesCount; }
        }

        /// <summary>
        /// Returns true if from-disk access is used.
        /// </summary>
        public bool IsFDA
        {
            get { return _fdaReader != null;  }
        }

        public byte GetDepth(Int64 nodeIdx)
        {
            // Do only an assertion to get the max. performance
            Debug.Assert(nodeIdx < _nodesCount, "Index out of range");
            if (_depths != null)
            {
                return _depths[nodeIdx];
            }
            else
            {
                _fdaReader.BaseStream.Seek(this._depthFilePos + nodeIdx, SeekOrigin.Begin);
                byte depth = _fdaReader.ReadByte();
                return depth;
            }
        }

        public void GetNode(Int64 nodeIdx, void * node)
        {
            // Do only an assertion to get the max. performance
            Debug.Assert(nodeIdx < _nodesCount, "Index out of range");
            if (_nodesPtr != null)
            {
                byte* p = ((byte*)_nodesPtr) + nodeIdx*_nodeByteSize;
                byte* n = (byte*)node;
                for(int i = 0; i < _nodeByteSize; ++i)
                {
                    n[i] = p[i];
                }
            }
            else
            {
                _fdaReader.BaseStream.Seek(this._nodesFilePos + nodeIdx * _nodeByteSize, SeekOrigin.Begin);
                UnmanagedMemory.Read(_fdaReader, new IntPtr(node), _nodeByteSize);
            }
        }

        public void SetDepth(Int64 nodeIdx, byte depth)
        {
            // Check index, this should not influence the performance much because this is
            // done during intitalization.
            if (nodeIdx >= _nodesCount)
            {
                throw new ApplicationException(String.Format("Node index out of range: was {0}, node count {1}", nodeIdx, _nodesCount));
            }
            _depths[nodeIdx] = depth;
        }

        public void Write(BinaryWriter w)
        {
            // Write version first to allow standard tools work.
            Version.Write(w);
            w.Write(SERIALIZATION_FORMAT_VERSION);
            w.Write(_nodesByteSize);
            w.Write(_nodesCount);
            UnmanagedMemory.Write(w, _depthPtr, _nodesCount);
            UnmanagedMemory.Write(w, _nodesPtr, _nodesByteSize);
            WriteUserData(w);
        }

        public void Write(string fileName)
        {
            using (BinaryWriter bw = new BinaryWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write)))
            {
                Write(bw);
            }
        }

        /// <summary>
        /// Binary deserialization from a reader to memory.
        /// </summary>
        public static T Read<T>(BinaryReader r) where T : UFTree, new()
        {
            T newTree = new T();
            newTree.ReadInternal(r);
            return newTree;
        }

        /// <summary>
        /// Binary deserialization from file to memory.
        /// </summary>
        public static T Read<T>(string fileName) where T : UFTree, new()
        {
            T tree;
            using (BinaryReader br = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                tree = UFTree.Read<T>(br);
            }
            return tree;
        }

        /// <summary>
        /// Binary deserialization from file for from-disk access.
        /// </summary>
        public static T ReadFDA<T>(string fileName) where T : UFTree, new()
        {
            T tree = new T();
            BinaryReader br = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));
            tree.ReadInternalFDA(br);
            return tree;
        }

        /// <summary>
        /// Set memory of depths array to a given value.
        /// </summary>
        public void SetDepthsMemory(byte value)
        {
            UnmanagedMemory.SetMemory(_depthPtr.Ptr, _nodesCount, 0);
        }


        /// <summary>
        /// Set memory of nodes array to a given value.
        /// </summary>
        public void SetNodesMemory(byte value)
        {
            UnmanagedMemory.SetMemory(_nodesPtr.Ptr, _nodesByteSize, 0);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_depthPtr != null)
            {
                _depthPtr.Dispose();
                _depthPtr = null;
            }
            if (_nodesPtr != null)
            {
                _nodesPtr.Dispose();
                _nodesPtr = null;
            }
            if(_fdaReader != null)
            {
                _fdaReader.Close();
                _fdaReader = null;
            }
        }

        #endregion

        #region Protected API 

        /// <summary>
        /// For deserialization.
        /// </summary>
        protected UFTree()
        {
        }

        /// <summary>
        /// Allocates memory for the nodes and depths arrays. The memory is not initialized.
        /// </summary>
        protected UFTree(Int64 nodesCount, Int32 nodeByteSize)
        {
            Version = new BdsVersion();
            _nodeByteSize = nodeByteSize;
            Allocate(nodesCount, nodeByteSize);
        }

        /// <summary>
        /// Overwrite to serialize user data. The stream pointer is set to the position where the user data
        /// should start.
        /// <para>The base class writes its data first, and then calles this function to let
        /// the derived classes to write their data at the end of the file. This convention makes the classes
        /// binary-compatible. A base class can load the data of the derived class withou extra coding. 
        /// A derived class may handle the data of the base class by using for example default data.</para>
        /// <para>This convention covers many use cases often seen in practice. The chain of derived classes 
        /// should follow this convention too.
        /// thi</para>
        /// </summary>
        protected virtual void WriteUserData(BinaryWriter w)
        {
        }

        /// <summary>
        /// Deserializes user data. The stream pointer is set to the position where the user data
        /// should start. See also WriteUserData().
        /// </summary>
        protected virtual void ReadUserData(BinaryReader r)
        {
        }

        /// <summary>
        /// Is called by the base class after deserialization (see Read()). 
        /// Override to initialize your class.
        /// </summary>
        protected virtual void AfterRead()
        {
        }

        protected SmartPtr _depthPtr;
        protected byte* _depths;

        protected SmartPtr _nodesPtr;
        /// <summary>
        /// Byte size of the nodes array. Allows inheritors to clear memory, etc.
        /// </summary>
        protected Int64 _nodesByteSize;
        protected int _nodeByteSize;

        private BinaryReader _fdaReader;
        private long _depthFilePos;
        private long _nodesFilePos;

        #endregion

        #region Implementation

        private const int SERIALIZATION_FORMAT_VERSION = 1;

        private void Allocate(long nodesCount, int nodeByteSize)
        {
            _nodesCount = nodesCount;
            _nodesByteSize = nodesCount * nodeByteSize;
            _depthPtr = UnmanagedMemory.AllocHGlobalExSmartPtr(NodesCount);
            _depths = (byte*)_depthPtr;
            _nodesPtr = UnmanagedMemory.AllocHGlobalExSmartPtr(_nodesByteSize);
        }

        private void ReadInternal(BinaryReader r)
        {
            Version = new BdsVersion();
            Version.Read(r);
            int serFmtVer = r.ReadInt32();
            _nodesByteSize = r.ReadInt64();
            _nodesCount = r.ReadInt64();
            _nodeByteSize = (int)(_nodesByteSize/_nodesCount);
            Allocate(_nodesCount, _nodeByteSize);
            UnmanagedMemory.Read(r, _depthPtr, _nodesCount);
            UnmanagedMemory.Read(r, _nodesPtr, _nodesByteSize);
            ReadUserData(r);
            AfterRead();
        }

        private void ReadInternalFDA(BinaryReader r)
        {
            Version = new BdsVersion();
            Version.Read(r);
            int serFmtVer = r.ReadInt32();
            _nodesByteSize = r.ReadInt64();
            _nodesCount = r.ReadInt64();
            _nodeByteSize = (int)(_nodesByteSize / _nodesCount);
            _fdaReader = r;
            _depthFilePos = r.BaseStream.Position;
            _nodesFilePos = _depthFilePos + _nodesCount;
            AfterRead();
        }

        private Int64 _nodesCount;

        #endregion
    }
}
