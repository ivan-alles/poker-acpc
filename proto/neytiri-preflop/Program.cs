using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ai.pkr.bots.neytiri;
using System.IO;
using System.Diagnostics;

namespace neytiri_preflop
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlSerializer s = new XmlSerializer(typeof (ActionTree));
            ActionTree at;
            using (TextReader tr = new StreamReader(File.Open(args[0], FileMode.Open, FileAccess.Read)))
            {
                at = (ActionTree) s.Deserialize(tr);
            }

            string[][] moves = new string[2][];
            for (int neyPos = 0; neyPos < 2; ++neyPos)
            {
                moves[neyPos] = new string[169];
                for (int pock = 0; pock < 169; ++pock)
                {
                    moves[neyPos][pock] = "";
                    Process(moves[neyPos], pock, at.Positions[1 - neyPos], neyPos, 0);
                }
            }

            for (int pock = 0; pock < 169; ++pock)
            {
                Console.WriteLine("{0,-20}{1}", moves[0][pock], moves[1][pock]);
            }

        }

        static void Process(string [] moves, int pocket, ActionTreeNode node, int neyPos, int curPos)
        {
            if (node.ActionKind == Ak.f || node.Children[0].ActionKind == Ak.s)
            {
                // Round 1 reached.
                moves[pocket] += ".";
                return; 
            }
            if (node.ActionKind == Ak.f || node.ActionKind == Ak.c || node.ActionKind == Ak.r || (node.ActionKind == Ak.d /*&& neyPos == 0 */ && curPos == 0))
            {
                if (curPos == neyPos)
                {
                    int best = -1;
                    double bestVal = double.MinValue;
                    for (int c = 0; c < node.Children.Count; ++c)
                    {
                        Debug.Assert(node.Children[c].ActionKind == Ak.f || node.Children[c].ActionKind == Ak.c || node.Children[c].ActionKind == Ak.r);
                        if (node.Children[c].PreflopValues[pocket] > bestVal)
                        {
                            best = c;
                            bestVal = node.Children[c].PreflopValues[pocket];
                        }
                    }
                    moves[pocket] += node.Children[best].ActionKind.ToString() + "(";
                    Process(moves, pocket, node.Children[best], neyPos, (curPos + 1) % 2);
                    moves[pocket] += ")";
                }
                else
                {
                    for (int c = 0; c < node.Children.Count; ++c)
                    {
                        Debug.Assert(node.Children[c].ActionKind == Ak.f || node.Children[c].ActionKind == Ak.c || node.Children[c].ActionKind == Ak.r);
                        moves[pocket] += node.Children[c].ActionKind.ToString();
                        Process(moves, pocket, node.Children[c], neyPos, (curPos + 1) % 2);
                    }
                }
            }
            else
            {
                for (int c = 0; c < node.Children.Count; ++c)
                {
                    Process(moves, pocket, node.Children[c], neyPos, (curPos + 1) % 2);
                }
            }
        }
    }

}
