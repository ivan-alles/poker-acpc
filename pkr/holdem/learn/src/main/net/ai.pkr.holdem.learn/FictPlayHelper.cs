/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy;
using ai.lib.utils;
using System.IO;
using ai.pkr.metastrategy.vis;
using ai.pkr.fictpl;

namespace ai.pkr.holdem.learn
{
    /// <summary>
    /// Creates an FP solver and allows to easily run it.
    /// </summary>
    public class FictPlayHelper
    {
        public delegate void ConfigureSolver(FictitiousPlay solver);

        public FictPlayHelper()
        {
            Epsilon = 0.001;
            BaseDir = "fp-temp";
            Solver = new FictitiousPlay();
        }

        public ConfigureSolver Configure
        {
            set;
            get;
        }

        public FictitiousPlay Solver
        {
            private set;
            get;
        }

        public bool VisualizeTrees
        {
            set;
            get;
        }

        public double Epsilon
        {
            set;
            get;
        }

        public string BaseDir
        {
            set;
            get;
        }

        /// <summary>
        /// Runs FictitiousPlay with the specified parameters. 
        /// Some parameters are set by default (e.g. verbosity), the caller has a chance to overwrite them
        /// using Configure delegate.
        /// </summary>
        public StrategyTree[] Solve(ActionTree at, ChanceTree ct)
        {
            int playersCount = 2;

            DirectoryExt.Delete(BaseDir);
            Directory.CreateDirectory(BaseDir);

            string inputDir = Path.Combine(BaseDir, "input");
            Directory.CreateDirectory(inputDir);
            string traceDir = Path.Combine(BaseDir, "trace");

            string chanceTreeFile = Path.Combine(inputDir, "ct.dat");
            string actionTreeFile = Path.Combine(inputDir, "at.dat");

            ct.Write(chanceTreeFile);
            at.Write(actionTreeFile);

            if (VisualizeTrees)
            {
                VisActionTree.Show(at, actionTreeFile + ".gv");
                VisChanceTree.Show(ct, chanceTreeFile + ".gv");
            }

            Solver.ChanceTreeFile = chanceTreeFile;
            Solver.EqualCa = false;
            Solver.ActionTreeFile = actionTreeFile;
            Solver.OutputPath = BaseDir;
            Solver.SnapshotsCount = 2;
            Solver.Epsilon = Epsilon;
            Solver.ThreadsCount = 6;
            Solver.IsVerbose = true;
            Solver.IterationVerbosity = 10000;
            Solver.MaxIterationCount = 1000000000;

            if (Configure != null)
            {
                Configure(Solver);
            }
            Solver.Solve();

            StrategyTree[] eqStrategies = new StrategyTree[playersCount];

            for (int p = 0; p < playersCount; ++p)
            {
                string fileName = Solver.CurrentSnapshotInfo.StrategyFile[p];
                eqStrategies[p] = StrategyTree.Read<StrategyTree>(fileName);
                if (VisualizeTrees)
                {
                    VisStrategyTree.Show(eqStrategies[p], fileName + ".gv");
                }
            }

            return eqStrategies;
        }
    }
}
