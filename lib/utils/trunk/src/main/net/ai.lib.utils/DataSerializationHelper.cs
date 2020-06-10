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
    /// Implements a common BDS functionality for data files (version, serialization format).
    /// <para>The developer has a free choice to set BDS version of the file. The intention is to let the user know what file it is and how it was created
    /// by using standard BDS tools. Usually the major, minor, etc. are copied from the assembly generating the file. The description can be set as well.</para>
    /// <para>Serialization format version is an integer describing data format. By convention, it grows with new versions.
    /// Usually newer versions of the data files cannot be read by older versions of the software, this can be checked automatically. 
    /// There are no fixed rules for other situations, it must be decided by the caller.</para>
    /// </summary>
    public static class DataSerializationHelper
    {
        /// <summary>
        /// Writes the standard BDS header.
        /// </summary>
        public static void WriteHeader(BinaryWriter w, BdsVersion bdsVersion, int formatVersion)
        {
            // Write version first to allow standard tools work.
            bdsVersion.Write(w);
            w.Write(formatVersion);
        }

        /// <summary>
        /// Reads the standard BDS header.
        /// </summary>
        public static void ReadHeader(BinaryReader r, out BdsVersion bdsVersion, out int formatVersion)
        {
            bdsVersion = new BdsVersion();
            bdsVersion.Read(r);
            formatVersion = r.ReadInt32();
        }

        /// <summary>
        /// Writes the standard BDS header and checks the format version.
        /// </summary>
        public static void ReadHeader(BinaryReader r, out BdsVersion bdsVersion, out int formatVersion, int lastSupportedFormatVersion)
        {
            ReadHeader(r, out bdsVersion, out formatVersion);
            if (formatVersion > lastSupportedFormatVersion)
            {
                throw new ApplicationException(
                    string.Format("Unsupported serialization format '{0}', max supported: '{1}'", formatVersion,
                                  lastSupportedFormatVersion));
            }
        }
    }
}
