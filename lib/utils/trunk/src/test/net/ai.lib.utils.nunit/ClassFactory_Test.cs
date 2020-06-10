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
    /// Unit tests for ClassFactory. 
    /// </summary>
    [TestFixture]
    public class ClassFactory_Test
    {
        #region Tests

        [Test]
        public void Test_CreateByAssemblyName()
        {
            string typeName = "ai.lib.utils.nunit.ClassFactory_Test+TestClass1,ai.lib.utils.nunit";
            ClassFactoryParams p = new ClassFactoryParams {TypeName = typeName };
            TestClass1 tc = ClassFactory.CreateInstance<TestClass1>(p);
            Assert.IsNotNull(tc);
        }

        [Test]
        public void Test_CreateByAssemblyFile()
        {
            string assemblyFile = Path.GetFileName(CodeBase.Get(Assembly.GetExecutingAssembly()));
            string typeName = "ai.lib.utils.nunit.ClassFactory_Test+TestClass1";
            ClassFactoryParams p = new ClassFactoryParams { AssemblyFile = assemblyFile, TypeName = typeName };
            TestClass1 tc = ClassFactory.CreateInstance<TestClass1>(p);
            Assert.IsNotNull(tc);
        }


        [Test]
        public void Test_CreateByAssemblyName_BaseClass()
        {
            string typeName = "ai.lib.utils.nunit.ClassFactory_Test+TestClass1,ai.lib.utils.nunit";
            ClassFactoryParams p = new ClassFactoryParams {TypeName = typeName };
            TestClass1 tc = (TestClass1)ClassFactory.CreateInstance<TestClassBase>(p);
            Assert.IsNotNull(tc);
        }

        [Test]
        public void Test_CreateByAssemblyFile_BaseClass()
        {
            string assemblyFile = Path.GetFileName(CodeBase.Get(Assembly.GetExecutingAssembly()));
            string typeName = "ai.lib.utils.nunit.ClassFactory_Test+TestClass1";
            ClassFactoryParams p = new ClassFactoryParams { AssemblyFile = assemblyFile, TypeName = typeName };
            TestClass1 tc = (TestClass1)ClassFactory.CreateInstance<TestClassBase>(p);
            Assert.IsNotNull(tc);
        }

        [Test]
        public void Test_CreateByAssemblyName_Params()
        {
            string typeName = "ai.lib.utils.nunit.ClassFactory_Test+TestClass2,ai.lib.utils.nunit";
            ClassFactoryParams p = new ClassFactoryParams
            {
                TypeName = typeName,
                Arguments = new object[] { 113, "bla-bla" }
            };
            TestClass2 tc = ClassFactory.CreateInstance<TestClass2>(p);
            Assert.IsNotNull(tc);
            Assert.AreEqual(113, tc.P1);
            Assert.AreEqual("bla-bla", tc.P2);
        }

        [Test]
        public void Test_CreateByAssemblyFile_Params()
        {
            string assemblyFile = Path.GetFileName(CodeBase.Get(Assembly.GetExecutingAssembly()));
            string typeName = "ai.lib.utils.nunit.ClassFactory_Test+TestClass2";
            ClassFactoryParams p = new ClassFactoryParams
                                       {
                                           AssemblyFile = assemblyFile, 
                                           TypeName = typeName,
                                           Arguments = new object[]{113, "bla-bla"}
                                       };
            TestClass2 tc = ClassFactory.CreateInstance<TestClass2>(p);
            Assert.IsNotNull(tc);
            Assert.AreEqual(113, tc.P1);
            Assert.AreEqual("bla-bla", tc.P2);
        }

        [Test]
        public void Test_CreateByAssemblyFileAndAssemblyName_AnotherAssembly()
        {
            string typeName = "ai.lib.utils.testlib.DerivedTestClass1, ai.lib.utils.testlib";
            ClassFactoryParams p = new ClassFactoryParams { AssemblyFile = "", TypeName = typeName };
            TestClassBase tc = ClassFactory.CreateInstance<TestClassBase>(p);
            Assert.IsNotNull(tc);
        }

        [Test]
        public void Test_CreateByAssemblyFile_NonReferencedAssembly()
        {
            string assemblyFile = "ai.lib.utils.testlib.dll";
            string assemblyName = Path.GetFileNameWithoutExtension(assemblyFile);

            // First do not use assembly name in the type name
            string typeName = "ai.lib.utils.testlib.DerivedTestClass1";
            ClassFactoryParams p = new ClassFactoryParams { AssemblyFile = assemblyFile, TypeName = typeName };
            TestClassBase tc = ClassFactory.CreateInstance<TestClassBase>(p);
            Assert.IsNotNull(tc);
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        public class TestClassBase
        {
        }

        public class TestClass1 : TestClassBase
        {
        }

        public class TestClass2 : TestClassBase
        {
            public TestClass2(int p1, string p2)
            {
                P1 = p1;
                P2 = p2;
            }

            public int P1;
            public string P2;

        }

        #endregion
    }
}
