/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;

namespace ai.lib.algorithms.graphvis_gen
{
    /// <summary>
    /// This is a parser of graphviz documentation summary table save as a csv file
    /// and generator of properties for VisTree class.
    /// see: http://www.graphviz.org/content/attrs
    /// </summary>
    class Program
    {
        class Row
        {
            public string Name;
            public string UsedBy;
            public string Type;
            public string Default;
            public string Minimum;
            public string Notes;

            public static Row Parse(string line)
            {
                //"Damping ","G","double","0.99","0.0","neato only"
                Regex reRow = new Regex("^([^,]*),([^,]*),([^,]*),([^,]*),([^,]*),(.*)$");
                Match m = reRow.Match(line);

                if (m.Groups.Count != 7)
                {
                    throw new ArgumentException();
                }

                Row r = new Row();
                r.Name = StripField(m.Groups[1].Value);
                r.UsedBy = StripField(m.Groups[2].Value);
                r.Type = StripField(m.Groups[3].Value);
                r.Default = StripField(m.Groups[4].Value);
                r.Minimum = StripField(m.Groups[5].Value);
                r.Notes = StripField(m.Groups[6].Value);
                return r;
            }

            static string StripField(string field)
            {
                if (field == "")
                {
                    return field;
                }
                if (field[0] == '"' && field[field.Length - 1] == '"')
                {
                    field = field.Substring(1, field.Length - 2);
                }
                field = field.Trim();
                field = field.Replace("\"\"", "\"");
                return field;
            }
        }

        static void Generate(string usedBy, Row r, TextWriter tw)
        {
            if (!r.UsedBy.Contains(usedBy))
                return;
            string type = "string";
            if (r.Type == "bool") type = "bool";
            else if (r.Type == "double") type = "double";
            else if (r.Type == "int") type = "int";

            tw.WriteLine("///<summary>");
            tw.WriteLine("///<para>Type: {0}.</para>", r.Type);
            if (!string.IsNullOrEmpty(r.Default)) tw.WriteLine("///<para>Default: {0}</para>", r.Default);
            if (!string.IsNullOrEmpty(r.Minimum)) tw.WriteLine("///<para>Minimum: {0}</para>", r.Minimum);
            if (!string.IsNullOrEmpty(r.Notes)) tw.WriteLine("///<para>Notes: {0}</para>", r.Notes);
            tw.WriteLine("///</summary>");
            tw.WriteLine("public {0} {1}", type, r.Name);
            tw.WriteLine("{");
            tw.WriteLine("set {{ Map[\"{0}\"] = value; }}", r.Name);
            tw.WriteLine("get {{ return ({0})GetValue(\"{1}\"); }}", type, r.Name);
            tw.WriteLine("}");
            tw.WriteLine();
        }

        static void Generate(string usedBy, List<Row> rows, TextWriter tw)
        {
            foreach (Row r in rows)
            {
                Generate(usedBy, r, tw);
            }
        }

        static void Main(string[] args)
        {
            List<Row> rows = new List<Row>();
            using (StreamReader tr = new StreamReader("..\\..\\graphviz.csv"))
            {
                tr.ReadLine(); // Skip header
                while (!tr.EndOfStream)
                {
                    string line = tr.ReadLine();
                    //Debug.WriteLine(line);
                    Row r = Row.Parse(line);
                    rows.Add(r);
                }
            }
            using (StreamWriter tw = new StreamWriter("..\\..\\result.cs"))
            {
                tw.WriteLine("class Code {");
                tw.WriteLine("// ----------- Graph attributes ------------");
                Generate("G", rows, tw);
                tw.WriteLine("// ----------- Cluster attributes ------------");
                Generate("C", rows, tw);
                tw.WriteLine("// ----------- Node attributes ------------");
                Generate("N", rows, tw);
                tw.WriteLine("// ----------- Edge attributes ------------");
                Generate("E", rows, tw);
                tw.WriteLine("}");
            }

        }
    }
}
