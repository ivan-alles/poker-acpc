/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System.IO;
namespace ai.pkr.holdem.strategy.ca
{
    public interface IClusterNode
    {
        /// <summary>
        /// Returns the number of children (same as Children.Count). For leaves returns 0.
        /// </summary>
        int ChildrenCount { get; }

        /// <summary>
        /// Access to children.
        /// </summary>
        IClusterNode GetChild(int idx);

        /// <summary>
        /// Write the node and all children to the writer.
        /// </summary>
        void Write(BinaryWriter w);

        /// <summary>
        /// Reads the node and all children from the reader.
        /// </summary>
        void Read(BinaryReader r, int serializationFormatVersion);

        int SerializationFormatVersion
        {
            get;
        }

        string ToGvString();

    }
}