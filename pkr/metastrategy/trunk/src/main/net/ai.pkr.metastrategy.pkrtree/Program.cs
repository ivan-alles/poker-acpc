/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ai.lib.utils.commandline;
using System.Threading;
using ai.pkr.metagame;
using ai.lib.utils;
using ai.lib.algorithms.tree;
using System.Xml;
using System.Diagnostics;
using ai.lib.algorithms;
using ai.pkr.metastrategy.algorithms;
using ai.pkr.metastrategy.vis;

namespace ai.pkr.metastrategy.pkrtree
{
    static class Program
    {
        static CommandLineParams _cmdLine = new CommandLineParams();
        static string _inputFormat;
        static string _outputFormat;
        static GameDefinition _gd;

        static int Main(string[] args)
        {
            if (!Parser.ParseArgumentsWithUsage(args, _cmdLine))
            {
                return 1;
            }

            if (_cmdLine.DebuggerLaunch)
            {
                Debugger.Launch();
            }

            if (_cmdLine.Input == "")
            {
                if (!File.Exists(_cmdLine.GameDef))
                {
                    Console.Error.WriteLine("Game definition file '{0}' doesn't exist", _cmdLine.GameDef);
                    return 1;
                }
                _gd = XmlSerializerExt.Deserialize<GameDefinition>(_cmdLine.GameDef);
            }

            _inputFormat = Path.GetExtension(_cmdLine.Input).ToLower();
            _outputFormat = Path.GetExtension(_cmdLine.Output).ToLower();


            if (_outputFormat != ".gv" && _outputFormat != ".xml" && _outputFormat != ".dat" && _outputFormat != ".txt")
            {
                Console.Error.WriteLine("Unknown output format '{0}'", _outputFormat);
                return 1;
            }

            if (_cmdLine.TreeKind == "action")
            {
                if (!ProcessActionTree())
                {
                    return 1;
                }
            }
            else if (_cmdLine.TreeKind == "chance" || _cmdLine.TreeKind == "chance-player")
            {
                if (!ProcessChanceTree())
                {
                    return 1;
                }
            }
            else if (_cmdLine.TreeKind == "strategy")
            {
                if (!ProcessStrategyTree())
                {
                    return 1;
                }
            }
            else
            {
                Console.Error.WriteLine("Unknown tree kind '{0}'", _cmdLine.TreeKind);
                return 1;
            }

            return 0;
        }

        private static bool ProcessActionTree()
        {
            ActionTree tree;

            if (_cmdLine.Input != "")
            {
                tree = UFTree.Read<ActionTree>(_cmdLine.Input);
            }
            else
            {
                tree = CreateActionTreeByGameDef.Create(_gd);
            }

            if (_outputFormat == ".gv")
            {
                using (TextWriter w = new StreamWriter(File.Open(_cmdLine.Output, FileMode.Create)))
                {
                    VisActionTree vis = new VisActionTree { Output = w };
                    if (_cmdLine.ClearExpr) vis.ShowExpr.Clear();
                    vis.ShowExprFromString(_cmdLine.ShowExpr);
                    vis.PruneIfExt = (t, n, s, d) => s[d].Round > _cmdLine.MaxRound;
                    vis.MatchPath = _cmdLine.MatchPath;
                    vis.Show(tree);
                }
            }
            else if (_outputFormat == ".dat")
            {
                tree.Write(_cmdLine.Output);
            }
            else
            {
                Console.Error.WriteLine("Unsupported output format '{0}' for tree kind '{1}'", _outputFormat, _cmdLine.TreeKind);
                return false;
            }
            return true;
        }

        private static bool ProcessChanceTree()
        {
            ChanceTree tree;

            if (_cmdLine.Input != "")
            {
                if (_inputFormat == ".dat")
                {
                    tree = UFTree.Read<ChanceTree>(_cmdLine.Input);
                }
                else if (_inputFormat == ".txt")
                {
                    tree = DumpChanceTree.FromTxt(_cmdLine.Input);
                }
                else
                {
                    Console.Error.WriteLine("Unsupported input format '{0}' for tree kind '{1}'", _inputFormat, _cmdLine.TreeKind);
                    return false;
                }
            }
            else
            {
                tree = CreateChanceTreeByGameDef.Create(_gd);
                if (_cmdLine.TreeKind == "chance-player")
                {
                    ChanceTree pt = ExtractPlayerChanceTree.ExtractS(tree, _cmdLine.Position);
                    tree = pt;
                }
            }

            if (_outputFormat == ".gv")
            {
                using (TextWriter w = new StreamWriter(File.Open(_cmdLine.Output, FileMode.Create)))
                {
                    VisChanceTree vis = new VisChanceTree { Output = w };
                    if (_gd != null) vis.CardNames = _gd.DeckDescr.CardNames;
                    if (_cmdLine.ClearExpr) vis.ShowExpr.Clear();
                    vis.ShowExprFromString(_cmdLine.ShowExpr);
                    vis.PruneIfExt = (t, n, s, d) => s[d].Round > _cmdLine.MaxRound;
                    vis.MatchPath = _cmdLine.MatchPath;
                    vis.Show(tree);
                }
            }
            else if (_outputFormat == ".dat")
            {
                tree.Write(_cmdLine.Output);
            }
            else if (_outputFormat == ".txt")
            {
                DumpChanceTree.ToTxt(tree, _cmdLine.Output);
            }
            else
            {
                Console.Error.WriteLine("Unsupported output format '{0}' for tree kind '{1}'", _outputFormat, _cmdLine.TreeKind);
                return false;
            }
            return true;
        }

        private static bool ProcessStrategyTree()
        {
            StrategyTree tree;

            if (_cmdLine.Input != "")
            {
                if (_inputFormat == ".dat")
                {
                    tree = UFTree.Read<StrategyTree>(_cmdLine.Input);
                }
                else if (_inputFormat == ".txt")
                {
                    tree = DumpStrategyTree.FromTxt(_cmdLine.Input);
                }
                else
                {
                    Console.Error.WriteLine("Unsupported input format '{0}' for tree kind '{1}'", _inputFormat, _cmdLine.TreeKind);
                    return false;
                }
            }
            else
            {
                ActionTree at = CreateActionTreeByGameDef.Create(_gd);
                ChanceTree ct = CreateChanceTreeByGameDef.Create(_gd);
                ChanceTree pct = ExtractPlayerChanceTree.ExtractS(ct, _cmdLine.Position);
                tree = CreateStrategyTreeByChanceAndActionTrees.CreateS(pct, at);
            }

            if (_outputFormat == ".gv")
            {
                using (TextWriter w = new StreamWriter(File.Open(_cmdLine.Output, FileMode.Create)))
                {
                    VisStrategyTree vis = new VisStrategyTree { Output = w };
                    if (_gd != null) vis.CardNames = _gd.DeckDescr.CardNames;
                    if (_cmdLine.ClearExpr) vis.ShowExpr.Clear();
                    vis.ShowExprFromString(_cmdLine.ShowExpr);
                    vis.PruneIfExt = (t, n, s, d) => s[d].Round > _cmdLine.MaxRound;
                    vis.MatchPath = _cmdLine.MatchPath;
                    vis.Show(tree, _cmdLine.Root);
                }
            }
            else if (_outputFormat == ".xml")
            {
                using (TextWriter w = new StreamWriter(File.Open(_cmdLine.Output, FileMode.Create)))
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Encoding = Encoding.ASCII;
                    settings.Indent = true;
                    using (XmlWriter xmlWriter = XmlWriter.Create(w, settings))
                    {
                        StrategyTreeToXml xmlizer = new StrategyTreeToXml {Output = xmlWriter};
                        if (_cmdLine.ClearExpr) xmlizer.ShowExpr.Clear();
                        xmlizer.ShowExprFromString(_cmdLine.ShowExpr);
                        xmlizer.Convert(tree);
                    }
                }
            }
            else if (_outputFormat == ".dat")
            {
                tree.Write(_cmdLine.Output);
            }
            else if (_outputFormat == ".txt")
            {
                DumpStrategyTree.ToTxt(tree, _cmdLine.Output);
            }
            else
            {
                Console.Error.WriteLine("Unsupported ouput format '{0}' for tree kind '{1}'", _outputFormat, _cmdLine.TreeKind);
                return false;
            }
            return true;
        }
    }
}
