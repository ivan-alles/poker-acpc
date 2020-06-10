/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Reflection;

namespace ai.lib.utils.nunit
{
    /// <summary>
    /// Unit tests for class Profile. 
    /// </summary>
    [TestFixture]
    public class Profile_Test
    {
        #region Tests

        [Test]
        public void Test_LoadFromXml()
        {
            string testResourcesPath = UTHelperPrivate.GetTestResourcesPath();

            Profile p =
                XmlSerializerExt.Deserialize<Profile>(Path.Combine(testResourcesPath, "Profile_Test-profile1.xml"));

            Assert.AreEqual("pvalue1", p.Properties.Get("pvar1"));
            Assert.AreEqual("pvalue2", p.Properties.Get("pvar2"));
            Assert.IsTrue(p.Properties.XmlMergeWithGlobal);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        
        #endregion
    }
}
