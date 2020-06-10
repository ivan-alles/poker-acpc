/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ai.lib.utils;
using System.Xml;
using ai.lib.algorithms.convert;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Converts a strategy tree to a plain text line-based format and back. 
    /// This can be used to convert data between different binary format versons.
    /// </summary>
    public static unsafe class DumpStrategyTree 
    {
        public const int SERIALIZATION_FORMAT = 1;

        public static void ToTxt(StrategyTree t, TextWriter w)
        {
            w.WriteLine("SeralizationFormat {0}", SERIALIZATION_FORMAT);
            XmlWriterSettings s = new XmlWriterSettings { Indent = false, NewLineChars = ""};
            w.Write("Version ");
            XmlSerializerExt.Serialize(t.Version, w, s);
            w.WriteLine();
            w.WriteLine("NodesCount {0}", t.NodesCount);
            for (Int64 n = 0; n < t.NodesCount; ++n)
            {
                w.WriteLine("Id {0}", n);
                w.WriteLine("D {0}", t.GetDepth(n));
                w.WriteLine("P {0}", t.Nodes[n].Position);
                if (t.Nodes[n].IsDealerAction)
                {
                    w.WriteLine("C {0}", t.Nodes[n].Card);
                }
                else
                {
                    w.WriteLine("A {0}", TextDumpHelper.DoubleToBinString(t.Nodes[n].Amount));
                    w.WriteLine("Pr {0}", TextDumpHelper.DoubleToBinString(t.Nodes[n].Probab));
                }
            }
        }

        public static void ToTxt(StrategyTree t, string fileName)
        {
            using (TextWriter w = new StreamWriter(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Write)))
            {
                ToTxt(t, w);
            }
        }


        public static StrategyTree FromTxt(TextReader r)
        {
            int ln = 0;
            int serializationFormat = int.Parse(TextDumpHelper.ReadTag(r, ref ln, "SeralizationFormat"));
            if(serializationFormat > SERIALIZATION_FORMAT)
            {
                throw new ApplicationException(String.Format("Line {0}: serialization format {1} is not supported by this version, max supported {2}", 
                    ln, serializationFormat, SERIALIZATION_FORMAT));
            }
            string tag, value;
            value = TextDumpHelper.ReadTag(r, ref ln, "Version");

            StringReader sr = new StringReader(value);
            BdsVersion v;
            XmlSerializerExt.Deserialize(out v, sr);
            Int64 nodesCount = Int64.Parse(TextDumpHelper.ReadTag(r, ref ln, "NodesCount"));
            StrategyTree t = new StrategyTree(nodesCount);
            t.SetNodesMemory(0); // Clear memory to ensure zeros at probability for dealer.
            t.Version.CopyFrom(v);
            for (Int64 n = 0; n < nodesCount; ++n)
            {
                Int64 id = Int64.Parse(TextDumpHelper.ReadTag(r, ref ln, "Id"));
                if (id != n)
                {
                    throw new ApplicationException(String.Format("Line {0}: wrong node id '{1}', expected '{2}'", ln, id, n));
                }
                t.SetDepth(n, byte.Parse(TextDumpHelper.ReadTag(r, ref ln, "D")));
                t.Nodes[n].Position = int.Parse(TextDumpHelper.ReadTag(r, ref ln, "P"));
                TextDumpHelper.Split(TextDumpHelper.ReadLine(r, ref ln), out tag, out value);
                if (tag == "C")
                {
                    t.Nodes[n].IsDealerAction = true;
                    t.Nodes[n].Card = int.Parse(value);
                }
                else if (tag == "A")
                {
                    t.Nodes[n].IsDealerAction = false;
                    t.Nodes[n].Amount = TextDumpHelper.BinStringToDouble(value);
                    t.Nodes[n].Probab = TextDumpHelper.BinStringToDouble(TextDumpHelper.ReadTag(r, ref ln, "Pr"));
                }
                else
                {
                    throw new ApplicationException(String.Format("Line {0}: wrong tag '{1}', 'C' or 'A'", ln, tag));
                }
            }
            return t;
        }

        public static StrategyTree FromTxt(string fileName)
        {
            using (TextReader r = new StreamReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                return FromTxt(r);
            }
        }
    }
}
