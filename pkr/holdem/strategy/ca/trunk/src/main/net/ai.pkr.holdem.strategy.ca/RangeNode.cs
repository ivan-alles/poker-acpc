/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ai.lib.algorithms.tree;

namespace ai.pkr.holdem.strategy.ca
{
    /// <summary>
    /// A node to build a tree for bucketizing by range of some value.
    /// Each node corresponds to a range. A node stores the exclusive upper limit of the range.
    /// So, for nodes with upper limits 1, 3, 5, +inf the ranges are [-inf, 1), [1, 3), [3, 5), [5, +inf).
    /// </summary>
    /// Developer notes:
    /// 1. Use float for ranges, because the whole idea of bucketizing is based on approximations, and the precision
    /// of float must be enough.
    public class RangeNode : IClusterNode
    {
        #region IClusterNode Members

        public int ChildrenCount
        {
            get { return Children == null ? 0 : Children.Length; }
        }

        public IClusterNode GetChild(int idx)
        {
            return Children[idx];
        }

        public int SerializationFormatVersion
        {
            get
            {
                return 0;
            }
        }

        public void Write(BinaryWriter w)
        {
            w.Write(ChildrenCount);
            w.Write(UpperLimit);
            for (int i = 0; i < ChildrenCount; ++i)
            {
                Children[i].Write(w);
            }
        }

        public void Read(BinaryReader r, int serializationFormatVersion)
        {
            int childCount = r.ReadInt32();
            Children = childCount == 0 ? null : new RangeNode[childCount];
            UpperLimit = r.ReadSingle();
            for (int i = 0; i < ChildrenCount; ++i)
            {
                Children[i] = new RangeNode();
                Children[i].Read(r, serializationFormatVersion);
            }
        }

        public string ToGvString()
        {
            return String.Format("ul: {0:0.000}", UpperLimit);
        }

        #endregion

        #region Public API

        public RangeNode()
        {
        }

        public RangeNode(int childCount)
        {
            AllocateChildren(childCount);
        }

        public void AllocateChildren(int childCount)
        {
            if (childCount != 0)
            {
                Children = new RangeNode[childCount];
            }
        }

        /// <summary>
        /// Exclusive upper value of the range.
        /// </summary>
        public float UpperLimit
        {
            set;
            get;
        }

        /// <summary>
        /// Children. Is null for leaves.
        /// </summary>
        public RangeNode[] Children
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns index of the child satisfying the value.
        /// In case no value is found, returns Children.Length + 1.
        /// </summary>
        public int FindChildByValue(float value)
        {
            int c = 0;
            for (; c < Children.Length; ++c)
            {
                if (value < Children[c].UpperLimit)
                {
                    return c;
                }
            }
            throw new ApplicationException(string.Format("Cannot find children for value {0}", value));
        }


        public override string ToString()
        {
            return ToGvString();
        }

        #endregion

        #region Implementation

        #endregion

    }
}
