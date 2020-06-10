/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.lib.utils.commandline;
using System.IO;
using System.Globalization;
using System.Reflection;

namespace ai.lib.utils.version
{
    static class Program
    {
        static CommandLine _cmdLine = new CommandLine();
        static int Main(string[] args)
        {
            if (!Parser.ParseArgumentsWithUsage(args, _cmdLine))
            {
                return -1;
            }

            foreach (string fileName in _cmdLine.Files)
            {
                string ext = Path.GetExtension(fileName);
                ext = ext.ToLower(CultureInfo.InvariantCulture);
                BdsVersion ver;
                if (ext == ".exe" || ext == ".dll")
                {
                    ver = ProcessAssembly(fileName);
                }
                else
                {
                    ver = ProcessBinary(fileName);
                }

                if (_cmdLine.PrintFileNames)
                {
                    Console.Write("{0}: ", fileName);
                }
                Console.Write("{0}", ver.ToString());

                if (_cmdLine.SetUserDescr != null)
                {
                    ver.UserDescription = _cmdLine.SetUserDescr;
                    Replace(fileName, ver);
                    Console.Write(". User description -> {0}", _cmdLine.SetUserDescr);
                }
                Console.WriteLine();
            }
            return 0;
        }

        private static BdsVersion ProcessBinary(string fileName)
        {
            BdsVersion ver = new BdsVersion();
            using (BinaryReader r = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                ver.Read(r);
            }
            return ver;
        }

        private static BdsVersion ProcessAssembly(string fileName)
        {
            // Use LoadFrom() instead of ReflectionOnlyLoadFrom(), because
            // the latter requires all dependencies to be loaded manually.
            Assembly assembly = Assembly.LoadFrom(fileName);
            BdsVersion ver = new BdsVersion(assembly);
            return ver;
        }

        private static void Replace(string fileName, BdsVersion newVersion)
        {
            string ext = Path.GetExtension(fileName).ToLower(CultureInfo.InvariantCulture);
            if (ext == ".exe" || ext == ".dll" || ext == ".xml")
            {
                throw new ApplicationException(string.Format("Expected data file, was {0}", ext));
            }
            BdsVersion.ReplaceInDataFile(fileName,
                (ref BdsVersion curVersion) => curVersion = newVersion);
        }
    }
}
