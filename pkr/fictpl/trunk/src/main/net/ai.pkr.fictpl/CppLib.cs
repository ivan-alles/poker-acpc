/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ai.lib.utils;
using System.Reflection;
using System.IO;
using ChanceValueT = System.Single;

namespace ai.pkr.fictpl
{
    public unsafe class CppLib
    {
        [DllImport("ai.pkr.fictpl.cpplib.dll")]
        public static extern void IncrementGameValue(double * pGameValues, UInt32 gameValuesCount,
            ChanceValueT* pChanceFactors, UInt32* pChanceMasks, UInt32 chanceMaskIdx);

        [DllImport("ai.pkr.fictpl.cpplib.dll")]
        public static extern void IncrementGameValueNoMasks(double* pGameValues, UInt32 gameValuesCount,
            ChanceValueT* pChanceFactors);


        public static void Init()
        {
            string platform = System.IntPtr.Size == 8 ? "win64" : "win32";
            string codeBase = CodeBase.Get(Assembly.GetExecutingAssembly());
            string dllDir = Path.Combine(Path.GetDirectoryName(codeBase),  platform);

            string dllName = "ai.pkr.fictpl.cpplib.dll";

            string dllPath = Path.Combine(dllDir, dllName);

            if (!System.IO.File.Exists(dllPath))
            {
                // In case we are in development folder (debug or release) try to load from bin.   
                dllDir = Props.Global.Expand("${bds.BinDir}") + platform;
                dllPath = Path.Combine(dllDir, dllName);
                if (!System.IO.File.Exists(dllPath))
                {
                    throw new ApplicationException(string.Format("Cannot load {0}", dllPath));
                }
            }
            string envPath = Environment.GetEnvironmentVariable("PATH");
            string envPathL = envPath.ToLower() + ";";
            if (envPathL.IndexOf(dllDir.ToLower() + ";") < 0)
            {
                Environment.SetEnvironmentVariable("PATH", dllDir + ";" + envPath, EnvironmentVariableTarget.Process);
            }
        } 


    }
}
