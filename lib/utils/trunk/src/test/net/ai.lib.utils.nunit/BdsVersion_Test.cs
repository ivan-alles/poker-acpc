/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using System.IO;

namespace ai.lib.utils.nunit
{
    /// <summary>
    /// Unit tests for BdsVersion. 
    /// </summary>
    [TestFixture]
    public class BdsVersion_Test
    {
        #region Tests

        [Test]
        public void Test_FromAssembly()
        {
            Assembly assembly = typeof (BdsVersion).Assembly;
            BdsVersion ver = new BdsVersion(assembly);
            Assert.AreEqual(assembly.GetName().Version.Major, ver.Major);
            Assert.AreEqual(assembly.GetName().Version.Minor, ver.Minor);
            Assert.AreEqual(assembly.GetName().Version.Revision, ver.Revision);
            Assert.AreEqual(assembly.GetName().Version.Build, ver.Build);

            Assert.IsTrue(ver.Description.StartsWith("Various utilities"));
            Assert.IsTrue(ver.BuildInfo.Contains("POM:"));
            Assert.IsTrue(ver.ScmInfo.Contains("utils/"));
        }

        [Test]
        public void Test_BinarySerialization()
        {
            Assembly assembly = typeof(BdsVersion).Assembly;
            BdsVersion v1 = new BdsVersion(assembly);

            MemoryStream s = new MemoryStream();
            BinaryWriter w = new BinaryWriter(s);
            v1.Write(w);
            BdsVersion v2 = new BdsVersion();
            s.Seek(0, SeekOrigin.Begin);
            using (BinaryReader r = new BinaryReader(s))
            {
                v2.Read(r);
            }
            Assert.AreEqual(v1, v2);
        }

        [Test]
        public void Test_Replace()
        {
            BdsVersion ver1 = new BdsVersion 
            {
                Major = 10, Minor = 11, Revision = 12, Build = 13,
                BuildInfo = "build info 1", 
                ScmInfo = "scm info 1", 
                Description = "description 1",
                UserDescription = "user description 1"
            };

            BdsVersion ver2 = new BdsVersion
            {
                Major = 20,
                Minor = 21,
                Revision = 22,
                Build = 23,
                BuildInfo = "build info 2",
                ScmInfo = "scm info 2",
                Description = "description 2",
                UserDescription = "user description 2"
            };

            TestDataFile file1 = new TestDataFile {Version = ver1, Data = "data" };
            string fileName = Path.Combine(_outDir, "test-file.dat");
            file1.Write(fileName);
            BdsVersion.ReplaceInDataFile(fileName, (ref BdsVersion v) => v = ver2);
            TestDataFile file2 = new TestDataFile();
            file2.Read(fileName);
            Assert.AreNotEqual(file1.Version, file2.Version);
            Assert.AreEqual(ver2, file2.Version);
            Assert.AreEqual(file1.Data, file2.Data);

        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        class TestDataFile
        {
            public BdsVersion Version;
            public string Data;

            public void Write(string fileName)
            {
                using (BinaryWriter w = new BinaryWriter(File.Open(fileName, FileMode.Create, FileAccess.Write)))
                {
                    Version.Write(w);
                    w.Write(Data);
                }
            }

            public void Read(string fileName)
            {
                using (BinaryReader r = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read)))
                {
                    Version = new BdsVersion();
                    Version.Read(r);
                    Data = r.ReadString();
                }
            }
        }

        string _outDir = UTHelperPrivate.MakeAndGetTestOutputDir("BdsVersion_Test");

        #endregion
    }
}
