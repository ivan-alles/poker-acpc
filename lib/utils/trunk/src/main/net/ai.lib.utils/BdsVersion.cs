/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;

namespace ai.lib.utils
{
    /// <summary>
    /// Version information about an assembly, binary or xml data file, etc.
    /// Can be binary or XML serialized.
    /// </summary>
    [Serializable]
    public class BdsVersion
    {
        /// <summary>
        /// Is used to replace a version. 
        /// </summary>
        /// <param name="version">Contains actual version. Either modify it or 
        /// replace with a new one.</param>
        public delegate void ReplaceDelegate(ref BdsVersion version);

        public BdsVersion()
        {
            ScmInfo = "";
            BuildInfo = "";
            Description = "";
			UserDescription = "";
        }

        public BdsVersion(BdsVersion v)
        {
            CopyFrom(v);
        }

        public BdsVersion(Assembly assembly): this()
        {
            Version v = assembly.GetName().Version;
            Major = v.Major;
            Minor = v.Minor;
            Revision = v.Revision;
            Build = v.Build;

            IList<CustomAttributeData> attrs = CustomAttributeData.GetCustomAttributes(assembly);
            foreach (CustomAttributeData attr in attrs)
            {
                try
                {
                    string attrString = attr.ToString();

                    if (attrString.StartsWith("[System.Reflection.AssemblyDescriptionAttribute"))
                    {
                        Description = attr.ConstructorArguments[0].Value.ToString();
                    }
                    else if (attrString.StartsWith("[System.Reflection.AssemblyInformationalVersionAttribute"))
                    {
                        ScmInfo = attr.ConstructorArguments[0].Value.ToString();
                    }
                    else if (attrString.StartsWith("[System.Reflection.AssemblyConfigurationAttribute"))
                    {
                        BuildInfo = attr.ConstructorArguments[0].Value.ToString();
                    }
                }
                catch
                {
                    // Just ignore all exceptions (something may be wrong on Mono)
                }
            }
        }

        public BdsVersion(int major, int minor, int revision)
            : this()
        {
            Major = major;
            Minor = minor;
            Revision = revision;
        }

        public void CopyFrom(BdsVersion v)
        {
            Major = v.Major;
            Minor = v.Minor;
            Revision = v.Revision;
            Build = v.Build;
            ScmInfo = v.ScmInfo;
            BuildInfo = v.BuildInfo;
            Description = v.Description;
            UserDescription = v.UserDescription;
        }

        /// <summary>
        /// Major version (API of different versions are not compatible).
        /// </summary>
        public int Major
        {
            set;
            get;
        }

        /// <summary>
        /// Minor version (API of different versions are backwards compatible).
        /// </summary>
        public int Minor
        {
            set;
            get;
        }

        /// <summary>
        /// Revision (part of the version). Different revisions have the same API, 
        /// but the sources are different (e.g. a bugfix, a typo fix, etc.).
        /// </summary>
        public int Revision
        {
            set;
            get;
        }

        /// <summary>
        /// Build number (part of the version). Automatically assigned at every build.
        /// </summary>
        public int Build
        {
            set;
            get;
        }

        /// <summary>
        /// Information about source control management (path, revision).
        /// </summary>
        public string ScmInfo
        {
            set;
            get;
        }

        /// <summary>
        /// Extended information about how the file was built, for example 
        /// time, machine, configuration (Debug/Release), platform (x86), POM version, etc.
		/// For data files can include important configuration or other parameters, for instance, 
		/// number of monte-carlo iterations, etc.
        /// </summary>
        public string BuildInfo
        {
            set;
            get;
        }

        /// <summary>
        /// A brief description of the product and version, for example:
        /// <para>executable file: "Library of statistical algorithms"</para>
        /// <para>data file: "Strategy build from 11M logs".</para>
        /// </summary>
        public string Description
        {
            set;
            get;
        }
		
		/// <summary>
        /// Similar to Description with the guarantee that this field will never be changed by the tools.
		/// It can be set only by the user on an existing file. This allows to add a post-built description,
		/// for example a comment about how good was this instance of the file in some tests, etc.
        /// </summary>
        public string UserDescription
        {
            set;
            get;
        }

        public void Write(BinaryWriter writer)
        {
            /*
            Format history:
            0: format, Major, Minor, Revision, Build, ScmInfo
            1: format, Major, Minor, Revision, Build, ScmInfo, BuildInfo
            2: format, Major, Minor, Revision, Build, ScmInfo, BuildInfo, Description
            3: format, length, Major, Minor, Revision, Build, ScmInfo, BuildInfo, Description
			4: format, length, Major, Minor, Revision, Build, ScmInfo, BuildInfo, Description, UserDescription
			5: format, length, Major, Minor, Revision, Build, ScmInfo, BuildInfo, Description, UserDescription, CRC32
            */
            int format = 5;
            MemoryStream s = new MemoryStream();
            BinaryWriter dataWriter = new BinaryWriter(s);
            dataWriter.Write(Major);
            dataWriter.Write(Minor);
            dataWriter.Write(Revision);
            dataWriter.Write(Build);
            dataWriter.Write(ScmInfo);
            dataWriter.Write(BuildInfo);
            dataWriter.Write(Description);
			dataWriter.Write(UserDescription);

            byte[] data = s.ToArray();
            uint crc32 = Crc32.Compute(data);
            int length = data.Length;
            writer.Write(format);
            writer.Write(length);
            writer.Write(data);
            writer.Write(crc32);
        }

        public void Read(BinaryReader reader)
        {
            int format = reader.ReadInt32();
            if (format < 3)
            {
                Read_0_2(reader, format);
                return;
            }
            int length = reader.ReadInt32();
            if(length > MAX_SERIALIZED_LENGTH)
            {
                throw new ApplicationException(
                    String.Format("Length {0} exceeds maximum {1}", length, MAX_SERIALIZED_LENGTH));
            }
            byte[] data = reader.ReadBytes(length);
            MemoryStream s = new MemoryStream(data);
            UInt32 actualCrc32 = Crc32.Compute(data);
            using(BinaryReader dataReader = new BinaryReader(s))
            {
                Major = dataReader.ReadInt32();
                Minor = dataReader.ReadInt32();
                Revision = dataReader.ReadInt32();
                Build = dataReader.ReadInt32();
                ScmInfo = dataReader.ReadString();
                BuildInfo = dataReader.ReadString();
                Description = dataReader.ReadString();
				if(format >= 4)
				{
					UserDescription = dataReader.ReadString();
				}
            }
            if (format >= 5)
            {
                uint crc32 = reader.ReadUInt32();
                if (crc32 != actualCrc32)
                {
                    throw new ApplicationException(
                        string.Format("CRC32 mismatch, expected: {0:X}, actual: {1:X}",
                        crc32, actualCrc32));
                }
            }

        }

        /// <summary>
        /// Replaces BDS version in a data file (the version must be at the
        /// beginning of the file).
        /// </summary>
        public static void ReplaceInDataFile(string fileName, ReplaceDelegate replace)
        {
            string tempFile1 = fileName + ".repl";
            string tempFile2 = fileName + ".backup";

            using(BinaryReader r = new BinaryReader(File.OpenRead(fileName)))
            {
                using(BinaryWriter w = new BinaryWriter(File.Open(tempFile1, FileMode.Create, FileAccess.Write)))
                {
                    BdsVersion currentVersion = new BdsVersion();
                    currentVersion.Read(r);
                    replace(ref currentVersion);
                    currentVersion.Write(w);
                    byte[] buffer = new byte[1024 * 100];
                    for (; ; )
                    {
                        int actRead = r.Read(buffer, 0, buffer.Length);
                        w.Write(buffer, 0, actRead);
                        if (actRead != buffer.Length)
                        {
                            break;
                        }
                    }
                }
            }
            // Now tempFile1 contains the copy of original file with replaced
            // version. Now rename the files and delete the old file.
            File.Move(fileName, tempFile2);
            File.Move(tempFile1, fileName);
            File.Delete(tempFile2);
        }

        /// <summary>
        /// Reads old formats 0..2.
        /// </summary>
        private void Read_0_2(BinaryReader br, int format)
        {
            Major = br.ReadInt32();
            Minor = br.ReadInt32();
            Revision = br.ReadInt32();
            Build = br.ReadInt32();
            ScmInfo = br.ReadString();
            if (format >= 1)
            {
                BuildInfo = br.ReadString();
            }
            if (format >= 2)
            {
                Description = br.ReadString();
            }
        }

        /// <summary>
        /// Returns version information as string, 
        /// format "Major.Minor.Revision.Build; BuildInfo; ScmInfo; Description; UserDescription"
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3};{4};{5};{6};{7}", Major, Minor, Revision, Build, 
                BuildInfo != null ? " " + BuildInfo : "",
                ScmInfo != null ? " " + ScmInfo : "",
                Description != null ? " " + Description : "",
				UserDescription != null ? " " + UserDescription : "");
        }

        #region Equality

        /// <summary>
        /// Equality comparison, compares all fields.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            BdsVersion v = (BdsVersion)obj;
            return Equals(v);
        }

        /// <summary>
        /// Equality comparison, compares all fields.
        /// </summary>
        public bool Equals(BdsVersion v)
        {
            return Major == v.Major &&
                   Minor == v.Minor &&
                   Revision == v.Revision &&
                   Build == v.Build &&
                   ScmInfo == v.ScmInfo &&
                   BuildInfo == v.BuildInfo &&
                   Description == v.Description &&
				   UserDescription == v.UserDescription;
        }

        public override int GetHashCode()
        {
            return Major ^ Minor ^ Revision ^ Build;
        }

        /// <summary>
        /// Compares Major, Minor, Revision and Build.
        /// </summary>
        public bool IsEqualVersion(BdsVersion v)
        {
            return Major == v.Major &&
                   Minor == v.Minor &&
                   Revision == v.Revision &&
                   Build == v.Build;
        }

        #endregion

        #region Private fields

        private const int MAX_SERIALIZED_LENGTH = 2000;

        #endregion
    }
}
