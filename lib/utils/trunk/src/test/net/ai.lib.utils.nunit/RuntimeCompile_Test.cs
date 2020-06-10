/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;

namespace ai.lib.utils.nunit
{
    /// <summary>
    /// Unit tests for ... 
    /// </summary>
    [TestFixture]
    public class RuntimeCompile_Test
    {
        #region Tests

        public int Field1 = 14;
        public double Prop1
        {
            get { return 3.14; }
        }

        public int[] FieldArr1 = new int[] {3, 6, 9};

        [Test]
        public void Test_Evaluator()
        {
            CompilerParameters cp = new CompilerParameters();
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;
            cp.ReferencedAssemblies.Add("system.dll");
            cp.ReferencedAssemblies.Add(Path.GetFileName(CodeBase.Get(Assembly.GetExecutingAssembly())));

            string code = "using System; using ai.lib.utils.nunit;";
            code += "public class _Evaluator {\n";
            code += "   public  object Evaluate(RuntimeCompile_Test param) {\n";
            code += "  return param.Field1*3;\n";
            code += "  }}";
            Assembly asm = RuntimeCompile.Compile(code, cp);

            Object evaluator = asm.CreateInstance("_Evaluator");
            MethodInfo mi = evaluator.GetType().GetMethod("Evaluate");

            object result = mi.Invoke(evaluator, new object[] { this });
            Assert.AreEqual(Field1*3, (int) result);

            code = "using System; using ai.lib.utils.nunit;";
            code += "public class _Evaluator {\n";
            code += "   public  object Evaluate(RuntimeCompile_Test param) {\n";
            code += "  return param.FieldArr1[2] + 5;\n";
            code += "  }}";
            asm = RuntimeCompile.Compile(code, cp);

            evaluator = asm.CreateInstance("_Evaluator");
            mi = evaluator.GetType().GetMethod("Evaluate");

            result = mi.Invoke(evaluator, new object[] { this });
            Assert.AreEqual(FieldArr1[2] + 5, (int)result);
        }


        #endregion

        #region Benchmarks
        #endregion

        #region Implementation
        #endregion
    }
}
