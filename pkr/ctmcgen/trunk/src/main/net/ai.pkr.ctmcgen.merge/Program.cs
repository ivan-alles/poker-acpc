/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace ai.pkr.ctmcgen.merge
{
    class Program
    {
        static CommandLineParams _cmdLine = new CommandLineParams();
        static private Regex _reIncludeFiles;
        static CtMcGen.Tree _targetTree;

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

            _reIncludeFiles = new Regex(_cmdLine.IncludeFiles, RegexOptions.Compiled);

            if (string.IsNullOrEmpty(_cmdLine.Output))
            {
                Console.WriteLine("Output file name is missing");
                return 1;
            }
            _targetTree = new CtMcGen.Tree();
            if (File.Exists(_cmdLine.Output))
            {
                _targetTree.Read(_cmdLine.Output);
            }


            DateTime startTime = DateTime.Now;

            foreach (string path in _cmdLine.InputPaths)
            {
                ProcessPath(path);
            }

            double time = (DateTime.Now - startTime).TotalSeconds;

            _targetTree.Write(_cmdLine.Output);
            

            Console.WriteLine("TOTAL: samples: {0:#,#}, time: {1:0.0} s, {2:#,#} sm/s",
                _targetTree.SamplesCount, time, _targetTree.SamplesCount / time);

            long leavesCount = _targetTree.CalculateLeavesCount();
            Console.WriteLine("Target file: leaves: {0:#,#}, samples: {1:#,#}, av. samples: {2:#,#}, path: {3}",
                              leavesCount, _targetTree.SamplesCount,
                              _targetTree.SamplesCount / (ulong)leavesCount,
                              _cmdLine.Output);

            return 0;
        }

        /// <summary>
        /// Processes directory or file recursively.
        /// </summary>
        public static void ProcessPath(string path)
        {
            string absPath = path;
            if (!Path.IsPathRooted(path))
            {
                absPath = Path.Combine(Directory.GetCurrentDirectory(), path);
            }

            FileAttributes attr = File.GetAttributes(absPath);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                ProcessDir(absPath);
            else
                ProcessFile(absPath);
        }

        /// <summary>
        /// Processes a directory recursively.
        /// </summary>
        public static void ProcessDir(string dir)
        {
            string[] files = Directory.GetFiles(dir);
            foreach (string fileName in files)
            {
                ProcessFile(fileName);
            }
            string[] dirs = Directory.GetDirectories(dir);
            foreach (string childDir in dirs)
            {
                ProcessPath(childDir);
            }
        }

        /// <summary>
        /// Processes a file.
        /// </summary>
        public static  void ProcessFile(string file)
        {
            if (!_reIncludeFiles.IsMatch(file))
            {
                Console.WriteLine("Skip file: {0}", file);
                return;
            }
            Console.Write("File: {0}", file);
            UInt64 curSamplesCount = _targetTree.SamplesCount;
            _targetTree.Read(file);
            Console.WriteLine("   samples: {0:#,#}", _targetTree.SamplesCount - curSamplesCount);
        }
    }
}
