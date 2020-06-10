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
using System.Globalization;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Converts a chance tree to a plain text line-based format and back. 
    /// This can be used to convert data between different binary format versons.
    /// </summary>
    public static unsafe class DumpChanceTree
    {
        public const int SERIALIZATION_FORMAT = 1;

        public static void ToTxt(ChanceTree t, TextWriter w)
        {
            int roundsCount = t.CalculateRoundsCount();
            if(t.PlayersCount != 2)
            {
                throw new ApplicationException("Only 2 players are supported");
            }
            w.WriteLine("SeralizationFormat {0}", SERIALIZATION_FORMAT);
            XmlWriterSettings s = new XmlWriterSettings { Indent = false, NewLineChars = "" };
            w.Write("Version ");
            XmlSerializerExt.Serialize(t.Version, w, s);
            w.WriteLine();
            w.WriteLine("NodesCount {0}", t.NodesCount);
            w.WriteLine("RoundsCount {0}", roundsCount);
            double [] potShare = new double[t.PlayersCount];
            for (Int64 n = 0; n < t.NodesCount; ++n)
            {
                w.WriteLine("Id {0}", n);
                byte depth = t.GetDepth(n);
                w.WriteLine("D {0}", depth);
                w.WriteLine("P {0}", t.Nodes[n].Position);
                w.WriteLine("C {0}", t.Nodes[n].Card);
                w.WriteLine("Pr {0}", TextDumpHelper.DoubleToBinString(t.Nodes[n].Probab));
                if (depth == roundsCount * t.PlayersCount)
                {
                    UInt16 [] activePlayerMasks = ActivePlayers.Get(t.PlayersCount, 2, t.PlayersCount);
                    foreach (UInt16 ap in activePlayerMasks)
                    {
                        t.Nodes[n].GetPotShare(ap, potShare);
                        w.Write("Ps {0:X}", ap);
                        foreach (double ps in potShare)
                        {
                            w.Write(" {0}",TextDumpHelper.DoubleToBinString(ps));
                        }
                        w.WriteLine();
                    }
                }
            }
        }

        public static void ToTxt(ChanceTree t, string fileName)
        {
            using (TextWriter w = new StreamWriter(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Write)))
            {
                ToTxt(t, w);
            }
        }


        public static ChanceTree FromTxt(TextReader r)
        {
            int ln = 0;
            int serializationFormat = int.Parse(TextDumpHelper.ReadTag(r, ref ln, "SeralizationFormat"));
            if (serializationFormat > SERIALIZATION_FORMAT)
            {
                throw new ApplicationException(String.Format("Line {0}: serialization format {1} is not supported by this version, max supported {2}",
                    ln, serializationFormat, SERIALIZATION_FORMAT));
            }
            string value;
            value = TextDumpHelper.ReadTag(r, ref ln, "Version");

            StringReader sr = new StringReader(value);
            BdsVersion v;
            XmlSerializerExt.Deserialize(out v, sr);
            Int64 nodesCount = Int64.Parse(TextDumpHelper.ReadTag(r, ref ln, "NodesCount"));
            int roundsCount = int.Parse(TextDumpHelper.ReadTag(r, ref ln, "RoundsCount"));
            ChanceTree t = new ChanceTree(nodesCount);
            t.SetNodesMemory(0); // Clear memory to ensure zeros at unused fields
            t.Version.CopyFrom(v);

            char [] separators = new char[]{' ', '\t'};
            for (Int64 n = 0; n < nodesCount; ++n)
            {
                Int64 id = Int64.Parse(TextDumpHelper.ReadTag(r, ref ln, "Id"));
                if (id != n)
                {
                    throw new ApplicationException(String.Format("Line {0}: wrong node id '{1}', expected '{2}'", ln, id, n));
                }
                byte depth = byte.Parse(TextDumpHelper.ReadTag(r, ref ln, "D"));
                t.SetDepth(n, depth);
                t.Nodes[n].Position = int.Parse(TextDumpHelper.ReadTag(r, ref ln, "P"));
                t.Nodes[n].Card = int.Parse(TextDumpHelper.ReadTag(r, ref ln, "C"));
                t.Nodes[n].Probab = TextDumpHelper.BinStringToDouble(TextDumpHelper.ReadTag(r, ref ln, "Pr"));
                if (depth == t.PlayersCount * roundsCount)
                {
                    double[] potShare = new double[t.PlayersCount];
                    UInt16[] activePlayerMasks = ActivePlayers.Get(t.PlayersCount, 2, t.PlayersCount);
                    for (int a = 0; a < activePlayerMasks.Length; ++a)
                    {
                        value = TextDumpHelper.ReadTag(r, ref ln, "Ps");
                        string[] parts = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != t.PlayersCount + 1)
                        {
                            throw new ApplicationException(
                                String.Format("Line {0}: wrong number of values: '{1}', expected '{2}'", ln,
                                              parts.Length, t.PlayersCount + 1));
                        }
                        UInt16 activePlayers = UInt16.Parse(parts[0], NumberStyles.AllowHexSpecifier);
                        for (int i = 1; i < parts.Length; ++i)
                        {
                            potShare[i - 1] = TextDumpHelper.BinStringToDouble(parts[i]);
                        }
                        t.Nodes[n].SetPotShare(activePlayers, potShare);
                    }
                }
            }
            return t;
        }

        public static ChanceTree FromTxt(string fileName)
        {
            using (TextReader r = new StreamReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                return FromTxt(r);
            }
        }
    }
}
