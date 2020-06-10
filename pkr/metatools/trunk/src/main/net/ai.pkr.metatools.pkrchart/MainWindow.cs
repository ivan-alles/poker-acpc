/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Soap;
using System.IO;
using ZedGraph;
using ai.pkr.metagame;
using ai.lib.algorithms;
using System.Drawing.Drawing2D;

namespace ai.pkr.metatools.pkrchart
{
    public partial class MainWindow : Form
    {
        private const int _refreshRate = 5000;

        public MainWindow()
        {
            InitializeComponent();
            zedGraph.Size = Size;
            zedGraph.IsShowPointValues = true;
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Interval = 100;
            _timer.Enabled = true;
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            if (_firstTime)
            {
                _firstTime = false;
                _timer.Enabled = false;
                Cursor cursor = Cursor;
                Cursor = Cursors.WaitCursor;
                CreateChart();
                Cursor = cursor;
                _timer.Interval = _refreshRate;
                _timer.Enabled = true;
            }
            else
            {
                bool isLogInProgress = false;
                foreach (LogChart lc in _logCharts)
                {
                    lc.OnTimer();
                    isLogInProgress = isLogInProgress | lc.IsLogInProgress;
                }
                
                SetTitle(isLogInProgress);
                zedGraph.AxisChange();
                zedGraph.Refresh();
            }
        }

        private void SetTitle(bool isLogInProgress)
        {
            _paneWrapper.Pane.Title.Text = _paneWrapper.GetTitle() + (isLogInProgress ? "*" : "");
        }

        private void CreateChart()
        {
            int chartsCount = _cmdLine.Compare ? _cmdLine.InputPaths.Length : 1;
            _logCharts = new LogChart[chartsCount].Fill(i => new LogChart());

            _paths = _cmdLine.InputPaths;
            // Convert to absolute path to avoid problems if CD changes.
            for (int i = 0; i < _paths.Length; ++i)
            {
                if (!Path.IsPathRooted(_paths[i]))
                {
                    _paths[i] = Path.Combine(Directory.GetCurrentDirectory(), _paths[i]);
                }
            }
            _paneWrapper.CreatePane(_paths, zedGraph);

            bool isLogInProgress = false;
            for (int i = 0; i < _logCharts.Length; ++i)
            {
                LogChart lc = _logCharts[i];
                string [] paths = _paths;
                if(_cmdLine.Compare)
                {
                    paths = new string[] {_paths[i]};
                }
                lc.CreateChart(paths, _cmdLine.IncludeFiles, zedGraph, _paneWrapper.Pane, _colorCoeffs[i % _colorCoeffs.Length], _cmdLine.CurveDescriptors);
                isLogInProgress = isLogInProgress | lc.IsLogInProgress;
                if(_cmdLine.ShowCurveFitting)
                {
                    lc.ShowCurveFitting();
                }
            }
            SetTitle(isLogInProgress);

            zedGraph.AxisChange();
            zedGraph.Refresh();
        }

        internal void SetCommandLine(CommandLine cmdLine)
        {
            _cmdLine = cmdLine;
        }

        bool _firstTime = true;
        private LogChart[] _logCharts;
        private PaneWrapper _paneWrapper = new PaneWrapper();

        private static readonly double[] _colorCoeffs = new double[]
                                                              {1, 0.8, 0.6, 0.4, 0.2};
        private CommandLine _cmdLine;
        private string[] _paths;
    }
}
