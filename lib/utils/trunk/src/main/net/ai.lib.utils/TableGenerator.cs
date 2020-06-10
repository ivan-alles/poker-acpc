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
    /// A helper class to create table generators.
    /// Creates a class containing a constant array. The array is filled in PrintContent() 
    /// which you have to override.
    /// This library is a good place for this class because it can be used everywhere.
    /// </summary>
    public class TableGenerator
    {
        public string Namespace { set; get; }
        public string Comment { set; get; }
        public string Name { set; get; }
        public string Type { set; get; }
        public bool UseUnmanagedMemory { set; get; }

        public void Generate(string fileName)
        {
            using (TextWriter tw = new StreamWriter(fileName))
            {
                Generate(tw);
                tw.Close();
            }
        }

        public virtual void Generate(TextWriter wr)
        {
            string unsafeModifier = UseUnmanagedMemory ? "unsafe" : "";
            wr.WriteLine("using System;");

            if (UseUnmanagedMemory)
            {
                wr.WriteLine("using ai.lib.utils;");
            }
            wr.WriteLine("namespace {0}", Namespace);
            wr.WriteLine("{");
            wr.WriteLine("    {0}", Comment);
            wr.WriteLine("    public static {0} class {1}", unsafeModifier, Name);
            wr.WriteLine("    {");
            if (UseUnmanagedMemory)
            {
                PrintUnmanagedCode(wr);
            }
            wr.WriteLine("        {0} static readonly {1}[] {2} = ",
                UseUnmanagedMemory ? "private" : "public",
                Type,
                UseUnmanagedMemory ? "_mt" : "T"
            );
            wr.WriteLine("        {");
            PrintContent(wr);
            wr.WriteLine("        };");
            wr.WriteLine("    }");
            wr.WriteLine("}");
        }

        protected virtual void PrintUnmanagedCode(TextWriter wr)
        {
            wr.WriteLine("        public static readonly {0}* T;", Type);
            wr.WriteLine("        static {0}()", Name);
            wr.WriteLine("        {");
            wr.WriteLine("            _p = UnmanagedMemory.AllocHGlobalExSmartPtr(_mt.Length * sizeof({0}));", Type);
            wr.WriteLine("            T = ({0}*)_p;", Type);
            wr.WriteLine("            UnmanagedMemory.Copy(_mt, T);");
            wr.WriteLine("            _mt = null;");
            wr.WriteLine("        }");
            wr.WriteLine("        static readonly SmartPtr _p;");
        }

        protected virtual void PrintContent(TextWriter wr)
        {
        }
    }
}