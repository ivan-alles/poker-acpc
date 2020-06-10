/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Text;
using System.Reflection;

namespace ai.lib.utils
{
    /// <summary>
    /// Helper for run-time compilation.
    /// </summary>
    public static class RuntimeCompile
    {
        public static Assembly Compile(string code, CompilerParameters cp)
        {
            CodeDomProvider cdp = CodeDomProvider.CreateProvider("CSharp"); 
            CompilerResults cr = cdp.CompileAssemblyFromSource(cp, code.ToString());
            if (cr.Errors.HasErrors)
            {
                StringBuilder error = new StringBuilder();
                error.Append("Error Compiling Expression: ");
                foreach (CompilerError err in cr.Errors)
                {
                    error.AppendFormat("{0}\n", err.ErrorText);
                }
                throw new Exception("Error Compiling Expression: " + error.ToString());
            }
            return cr.CompiledAssembly;
        }
    }

}
