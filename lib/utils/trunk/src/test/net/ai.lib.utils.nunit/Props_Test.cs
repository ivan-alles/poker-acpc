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
    /// Unit tests for class Props. 
    /// </summary>
    [TestFixture]
    public class Props_Test
    {
        #region Tests

        [Test]
        public void Test_Set_Get()
        {
            Props v = new Props();

            v.Set("var1", "value1");
            v.Set("var2", "value2");
            v.Set("var4", null);
            Assert.AreEqual("value1", v.Get("var1"));
            Assert.AreEqual("value2", v.Get("var2"));
            Assert.IsNull(v.Get("var3"));
            Assert.IsNull(v.Get("var4"));
        }

        [Test]
        public void Test_Expand()
        {
            Props v = new Props();
            
            v.Set("var1", "value1");
            v.Set("var2", "value2");
            string text = "${var1}-bla-bla-${var2}-tra-la-la-${var1}-ha-${var444}-ha-${var2}";
            string subst = v.Expand(text);
            Assert.AreEqual("value1-bla-bla-value2-tra-la-la-value1-ha-${var444}-ha-value2", subst);

            text = "${var1}-${0}-${var2}-${1}";
            subst = v.Expand(text, "param1", "param2");
            Assert.AreEqual("value1-param1-value2-param2", subst);

        }

        [Test]
        public void Test_Expand_Recursive()
        {
            Props v = new Props();

            v.Set("var1", "111-${var2}-222");
            v.Set("var2", "value2");
            string text = "ttt-${var1}-TTT";
            string subst = v.Expand(text);
            Assert.AreEqual("ttt-111-value2-222-TTT", subst);
        }

        [Test]
        public void Test_UpdateEnvironmentVariables()
        {
            string var1 = "AI_LIB_UTILS_NUNIT_VARS_VAR1";
            string var2 = "AI_LIB_UTILS_NUNIT_VARS_VAR2";

            // First we have to clear the variables, otherwise the test fails
            // if started multiple times from nunit.
            Environment.SetEnvironmentVariable(var1, "", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable(var2, "", EnvironmentVariableTarget.Process);

            Props v = new Props();
            v.UpdateEnvironmentVariables();

            Assert.IsNull(v.Get("env." + var1));
            Assert.IsNull(v.Get("env." + var2));

            Environment.SetEnvironmentVariable(var1, "value1", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable(var2, "value2", EnvironmentVariableTarget.Process);

            v.UpdateEnvironmentVariables();
            Assert.AreEqual("value1", v.Get("env." + var1));
            Assert.AreEqual("value2", v.Get("env." + var2));
        }

        [Test]
        public void Test_SetStandardVariables()
        {

            Props v = new Props();
            v.SetStandardProps();

            Assert.IsNotNull(v.Get("ai.Root"));
            Assert.IsNotNull(v.Get("ai.InitCD"));

            Assert.AreEqual(new string(Path.DirectorySeparatorChar, 1), v.Get("ai.DirSep"));
        }

        [Test]
        public void Test_XmlDeserialize()
        {
            string testResourcesPath = UTHelperPrivate.GetTestResourcesPath();
            string fileName = Path.Combine(testResourcesPath, "Props_Test-prop1.xml");
            // Merged with global variables
            Props p =
                XmlSerializerExt.Deserialize<Props>(fileName);

            Assert.AreEqual("pvalue1", p.Get("pvar1"));
            Assert.AreEqual("pvalue2", p.Get("pvar2"));
            Assert.IsTrue(AreDirPathsEqual(testResourcesPath, p.Get("MyDirName")));
            Assert.AreEqual(fileName, p.Get("MyFileName"));
            Assert.IsTrue(p.XmlMergeWithGlobal);
            Assert.IsNotNull(p.Get("ai.Root"));

            // Not merged with global variables
            p = XmlSerializerExt.Deserialize<Props>(Path.Combine(testResourcesPath, "Props_Test-prop2.xml"));
            Assert.AreEqual("pvalue1", p.Get("pvar1"));
            Assert.AreEqual("pvalue2", p.Get("pvar2"));
            Assert.IsFalse(p.XmlMergeWithGlobal);
            Assert.IsNull(p.Get("ai.Root"));
        }

        /// <summary>
        /// Compares 2 paths to directories, ignoring the trailing directory separator char.
        /// </summary>
        /// Developer notes: I would have added this to DirectoryExt, but comparing paths is not such an easy task.
        /// There are a lot of issues to consider (case-sensitivity, paths like c:\dir\..\ and c:\ that are actually the same,
        /// etc, so it's a huge working package actuall. Here is a useful link about this:
        /// http://stackoverflow.com/questions/2281531/how-can-i-compare-directory-paths-in-c. There is also a link to an
        /// open-source library doing such comparison.
        bool AreDirPathsEqual(string path1, string path2)
        {
            if(path1.Length > 0 && path1[path1.Length - 1] != Path.DirectorySeparatorChar)
            {
                path1 += Path.DirectorySeparatorChar;
            }
            if (path2.Length > 0 && path2[path2.Length - 1] != Path.DirectorySeparatorChar)
            {
                path2 += Path.DirectorySeparatorChar;
            }
            return path1 == path2;
        }


        [Test]
        public void Test_XmlSerialize()
        {
            // Just print xml to console, no checks for now.
            Props p = new Props();
            p.Set("p1", "val1");
            XmlSerializerExt.Serialize(p, Console.Out);
            p.XmlMergeWithGlobal = false;
            Console.WriteLine();
            XmlSerializerExt.Serialize(p, Console.Out);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        


        #endregion
    }
}
