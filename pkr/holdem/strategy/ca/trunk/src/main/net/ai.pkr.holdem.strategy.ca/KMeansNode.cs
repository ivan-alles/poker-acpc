/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ai.lib.algorithms.tree;
using ai.lib.algorithms.la;
using ai.lib.algorithms;

namespace ai.pkr.holdem.strategy.ca
{
    /// <summary>
    /// A node to build a tree for bucketizing by k-means of some value.
    /// </summary>
    public class KMeansNode : IClusterNode
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
                return 1;
            }
        }

        public void Write(BinaryWriter w)
        {
            w.Write(Center.Length);
            for (int d = 0; d < Center.Length; ++d)
            {
                w.Write(Center[d]);
            }
            for (int d = 0; d < Center.Length; ++d)
            {
                w.Write(ValueMin[d]);
            }
            for (int d = 0; d < Center.Length; ++d)
            {
                w.Write(ValueBounds[d]);
            } 
            w.Write(ChildrenCount);
            for (int i = 0; i < ChildrenCount; ++i)
            {
                Children[i].Write(w);
            }
        }

        public void Read(BinaryReader r, int serializationFormatVersion)
        {
            int dim = r.ReadInt32();
            Center = new double[dim];
            for (int d = 0; d < Center.Length; ++d)
            {
                Center[d] = r.ReadDouble();
            } 
            ValueMin = new double[dim];
            ValueBounds = new double[dim].Fill(1);
            if(serializationFormatVersion > 0)
            {
                for (int d = 0; d < Center.Length; ++d)
                {
                    ValueMin[d] = r.ReadDouble();
                }
                for (int d = 0; d < Center.Length; ++d)
                {
                    ValueBounds[d] = r.ReadDouble();
                } 
            }
            int childCount = r.ReadInt32();
            Children = childCount == 0 ? null : new KMeansNode[childCount];
            for (int i = 0; i < ChildrenCount; ++i)
            {
                Children[i] = new KMeansNode();
                Children[i].Read(r, serializationFormatVersion);
            }
        }

        public string ToGvString()
        {
            return ToString("\\n");
        }

        #endregion

        #region Public API

        public KMeansNode()
        {
        }

        public KMeansNode(int dim, int childCount)
        {
            Center = new double[dim];
            ValueMin = new double[dim];
            ValueBounds = new double[dim].Fill(1);
            AllocateChildren(childCount);
        }

        public void AllocateChildren(int childCount)
        {
            if (childCount != 0)
            {
                Children = new KMeansNode[childCount];
            }
        }

        /// <summary>
        /// K-means center.
        /// </summary>
        public double[] Center
        {
            private set;
            get;
        }

        /// <summary>
        /// Minimal value seen during generation.
        /// </summary>
        public double[] ValueMin
        {
            private set;
            get;
        }

        /// <summary>
        /// Bounds (max-min) of the values seen during generation.
        /// </summary>
        public double[] ValueBounds
        {
            private set;
            get;
        }

        /// <summary>
        /// Children. Is null for leaves.
        /// </summary>
        public KMeansNode[] Children
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns index of the child closest to the point.
        /// </summary>
        public int FindClosestChild(double [] point, bool normalize)
        {
            double[] normalizedPoint = point.ShallowCopy();
            if (normalize)
            {
                VectorS.NormalizeByDiff(normalizedPoint, ValueMin, ValueBounds);
            }

            double minDist = double.MaxValue;
            int best = -1;
            for (int c = 0; c < Children.Length; ++c)
            {
                double dist = VectorS.SquaredDistance(Children[c].Center, normalizedPoint);
                if (dist < minDist)
                {
                    minDist = dist;
                    best = c;
                }
            }
            return best;
        }


        public override string ToString()
        {
            return ToString("");
        }

        #endregion

        #region Implementation

        public string ToString(string separator)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("min: ");
            for (int d = 0; d < ValueMin.Length; ++d)
            {
                sb.AppendFormat("{0:0.000} ", ValueMin[d]);
            }
            sb.Append(separator);
            sb.Append("b: ");
            for (int d = 0; d < ValueBounds.Length; ++d)
            {
                sb.AppendFormat("{0:0.000} ", ValueBounds[d]);
            }
            sb.Append(separator);
            sb.Append("c: ");
            for (int d = 0; d < Center.Length; ++d)
            {
                sb.AppendFormat("{0:0.000} ", Center[d]);
            }
            return sb.ToString();
        }

        #endregion

    }
}
