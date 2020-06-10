/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ai.lib.utils;
using System.Reflection;

namespace ai.lib.algorithms.tree
{
    /// <summary>
    /// For a given UFTree builds an index for direct access to all children of a node.
    /// </summary>
    /// Developer notes:
    /// This class supports the number of nodes up to int.MaxValue. This should be enough
    /// for the most use-cases. Indexing of huge trees with size > 2G will be problematic anyway, 
    /// because the index requires 2*NodesCount elements,  so it will need at least 
    /// sizeof(Int64) * 2 * 2G = 32 G memory.
    public class UFTreeChildrenIndex
    {
        #region Public API


        /// <summary>
        /// Create instance for memory access.
        /// </summary>
        public UFTreeChildrenIndex(UFTree ufTree)
        {
            CreateIndex(ufTree);
        }

        ///<summary>Create instance for file acesss.</summary>
        ///<param name="forceCreation">If true - always create a new index file, otherwise create only if no file exists.</param>
        public UFTreeChildrenIndex(UFTree ufTree, string fileName, bool forceCreation)
        {
            if (forceCreation || !File.Exists(fileName))
            {
                CreateIndex(ufTree);
                using(BinaryWriter bw = new BinaryWriter(File.Open(fileName, FileMode.Create, FileAccess.Write)))
                {
                    BdsVersion v = new BdsVersion(Assembly.GetExecutingAssembly());
                    v.Description = "UFTreeChildrenIndex for : '" + ufTree.Version.Description + "'";
                    v.Write(bw);
                    bw.Write(SERIALIZATION_FORMAT_VERSION);
                    bw.Write(ufTree.NodesCount);
                    for(long i = 0; i < _childrenBeginIdx.Length; ++i)
                    {
                        bw.Write(_childrenBeginIdx[i]);
                    }
                    for (long i = 0; i < _childrenIdx.Length; ++i)
                    {
                        bw.Write(_childrenIdx[i]);
                    }
                }
            }
            _childrenBeginIdxReader =
                new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));
            BdsVersion v1 = new BdsVersion();
            v1.Read(_childrenBeginIdxReader);
            int serFmtVer = _childrenBeginIdxReader.ReadInt32();
            if(serFmtVer > SERIALIZATION_FORMAT_VERSION)
            {
                throw new ApplicationException(
                    string.Format("Unsupported serialization format '{0}', max supported: '{1}'", serFmtVer,
                    SERIALIZATION_FORMAT_VERSION));
            }
            long nodesCount = _childrenBeginIdxReader.ReadInt64();
            _childrenBeginIdxFilePos = _childrenBeginIdxReader.BaseStream.Position;
            _childrenIdxFilePos = _childrenBeginIdxFilePos + (nodesCount + 1)*4;
            _childrenIdxReader = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));

            _childrenBeginIdx = _childrenIdx = null;
        }

        /// <summary>
        /// Creates new instance and indexes the tree (requires 2*N steps).
        /// </summary>
        /// <param name="ufTree"></param>
        private void CreateIndex(UFTree ufTree)
        {
            if (ufTree.NodesCount > (Int64)int.MaxValue - 1)
            {
                throw new ArgumentOutOfRangeException(String.Format("Flat tree size {0} is too large.", ufTree.NodesCount));
            }
            _childrenBeginIdx = new int[ufTree.NodesCount+1];
            _childrenIdx = new int[ufTree.NodesCount];

            WalkUFTreePP<UFTree, Context> walk = new WalkUFTreePP<UFTree, Context>();
            walk.OnNodeBegin = OnNodeBegin1;
            walk.OnNodeEnd = OnNodeEnd1;
            walk.Walk(ufTree);

            int beginIdx = 0;
            for(int i = 0; i < _childrenBeginIdx.Length; ++i)
            {
                int tmp = _childrenBeginIdx[i];
                _childrenBeginIdx[i] = beginIdx;
                beginIdx += tmp;
            }

            walk.OnNodeBegin = OnNodeBegin2;
            walk.OnNodeEnd = null;
            walk.Walk(ufTree);
        }

        /// <summary>
        /// Returns number of children.
        /// </summary>
        public int GetChildrenCount(long nodeId)
        {
            int childrenBeginIdx, childrenCount;
            GetChildrenBeginIdxAndCount(nodeId, out childrenBeginIdx, out childrenCount);
            return childrenCount;
        }

        /// <summary>
        /// Returns number of children and index of the first child for GetChildIdx().
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public void GetChildrenBeginIdxAndCount(long nodeId, out int childrenBeginIdx, out int childrenCount)
        {
            if (_childrenBeginIdx != null)
            {
                childrenBeginIdx = _childrenBeginIdx[(int) nodeId];
                childrenCount = _childrenBeginIdx[(int) nodeId + 1] - childrenBeginIdx;
            }
            else
            {
                _childrenBeginIdxReader.BaseStream.Seek(_childrenBeginIdxFilePos + nodeId*4, SeekOrigin.Begin);
                childrenBeginIdx = _childrenBeginIdxReader.ReadInt32();
                int next = _childrenBeginIdxReader.ReadInt32();
                childrenCount = next - childrenBeginIdx;
            }
        }

        /// <summary>
        /// Indexes of children. See also GetChildrenBeginIdx().
        /// </summary>
        public int GetChildIdx(int i)
        {
            if (_childrenIdx != null)
            {
                return _childrenIdx[i];
            }
            else
            {
                _childrenIdxReader.BaseStream.Seek(_childrenIdxFilePos + i * 4, SeekOrigin.Begin);
                int idx = _childrenIdxReader.ReadInt32();
                return idx;
            }
        }

        #endregion

        #region Implementation

 

        void OnNodeBegin1(UFTree tree, Context[] stack, int depth)
        {
            stack[depth].ChildCount = 0;
        }

        void OnNodeEnd1(UFTree tree, Context[] stack, int depth)
        {
            if (depth > 0)
            {
                stack[depth-1].ChildCount++;
            }
            _childrenBeginIdx[stack[depth].NodeIdx] = stack[depth].ChildCount;
        }

        void OnNodeBegin2(UFTree tree, Context[] stack, int depth)
        {
            stack[depth].ChildCount = 0;
            if (depth > 0)
            {
                int begin = _childrenBeginIdx[stack[depth - 1].NodeIdx];
                _childrenIdx[begin + stack[depth - 1].ChildCount] = (int)stack[depth].NodeIdx;
                stack[depth - 1].ChildCount++;
            }
        }

        class Context : WalkUFTreePPContext
        {
            public int ChildCount;
        }

        private int SERIALIZATION_FORMAT_VERSION = 1;

        private int[] _childrenBeginIdx;
        private int[] _childrenIdx;

        private BinaryReader _childrenBeginIdxReader;
        private BinaryReader _childrenIdxReader;
        private long _childrenBeginIdxFilePos;
        private long _childrenIdxFilePos;


        #endregion
    }
}
