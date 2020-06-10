/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using System.Drawing;
using ZedGraph;
using System.IO;
using System.Drawing.Drawing2D;

namespace ai.pkr.metatools.pkrchart
{
    /// <summary>
    /// Creates a chart from card log(s).
    /// </summary>
    class LogChart
    {
        internal void CreateChart(string [] paths, string includeFiles, ZedGraphControl zedGraph, GraphPane pane, double colorCoeff, string [] curveDescriptorsS)
        {
            ParseCurveDescriptors(curveDescriptorsS);
            _pane = pane;
            _paths = paths;
            _colorCoeff = colorCoeff;

            _zedGraph = zedGraph;

            _isLogInProgress = GetIsLogInProgress();

            _logParser.IncludeFiles = includeFiles;

            _logParser.OnError += new GameLogParser.OnErrorHandler(logParser_OnError);
            _logParser.OnGameRecord += new GameLogParser.OnGameRecordHandler(logParser_OnGameRecord);

            foreach (string path in _paths)
            {
                _logParser.ParsePath(path);
            }
            FinalUpdate();
        }

        internal void ShowCurveFitting()
        {
            foreach (CurveDescriptor curve in _curves)
            {
                ShowCurveFitting(curve);
            }
        }

        private void ParseCurveDescriptors(string[] curveDescriptorsS)
        {
            if (curveDescriptorsS.Length == 0)
            {
                _autocreateCurveDescriptors = true;
                return;
            }
            _autocreateCurveDescriptors = false;
            foreach (string cdS in curveDescriptorsS)
            {
                string [] parts = cdS.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1 || parts.Length > 2)
                {
                    throw new ApplicationException(string.Format("Wrong curve descriptor '{0}'", cdS));
                }
                CurveDescriptor cd = new CurveDescriptor();
                cd.Player = parts[0];
                cd.Position = parts.Length < 2 ? -1 : int.Parse(parts[1]);
                _curveDescriptors.Add(cd, cd);
            }
        }

        internal bool IsLogInProgress
        {
            get { return _isLogInProgress; }
        }

        private bool GetIsLogInProgress()
        {
            return _paths.Length == 1 && IsFileOpened(_paths[0]);
        }

        private bool IsFileOpened(string path)
        {
            bool result = false;
            if (File.Exists(path))
            {
                // This is an existing file and not a directory.
                try
                {
                    using(FileStream s = File.Open(path, FileMode.Open, FileAccess.Read))
                    {
                        s.Close();
                    }
                }
                catch (IOException)
                {
                    // File is used by another app.
                    result = true;
                }
            }
            return result;
        }

        internal void OnTimer()
        {
            if (!_isLogInProgress)
                return;

            _logParser.ParseFile(_logParser.CurrentFile, _logParser.LastFilePosition);

            _isLogInProgress = GetIsLogInProgress();
        }

        private string GetPaneTitle()
        {
            if (_paths.Length > 1)
                return "Multiple paths";

            string path = _paths[0];
            string fileName = Path.GetFileName(path);
            string dirName = Path.GetDirectoryName(path);
            if (fileName.Length > 0)
            {
                // Keep file name unchanged.
                if (dirName.Length > 20)
                    dirName = dirName.Substring(0, 20) + "...";
            }
            else
            {
                if (dirName.Length > 30)
                    dirName = dirName.Substring(0, 15) + "..." + dirName.Substring(dirName.Length - 10, 10);
            }
            if (dirName.Length > 0)
                dirName += Path.DirectorySeparatorChar;
            return dirName + fileName + (_isLogInProgress ? " *" : "");
        }

        /// <summary>
        /// Update the final results (they may have been skipped due to the current sample step).
        /// </summary>
        void FinalUpdate()
        {
            foreach (CurveDescriptor curveDescriptor in _curveDescriptors.Values)
            {
                double total;
                if (!_totalResult.TryGetValue(curveDescriptor, out total))
                {
                    total = 0;
                }
                UpdateChart(curveDescriptor, total);
            }
        }

        /// <summary>
        /// Does a curve fitting (actually by a straight line) as described in "Neural Networks for Pattern Recognition" by C.M. Bishop, page 9.
        /// </summary>
        /// <param name="cd"></param>
        private void ShowCurveFitting(CurveDescriptor cd)
        {
            PointPairList ppl = cd.Points;
            // Create a set of linear equations Aw = b, where
            // a[k,j] = sum(n = 1..N : x[n] ^ (k + j)
            // b[k] =   sum(n = 1..N : x[n] ^ k * y[n]
            // Actually we need only the 2nd equation, because we have an additional condition
            // (and therefore 1 degree of freedom less): the straight goes througth the point (x0, 0).
            // Therefore solve the following system:
            // a[1,0]*w[0] + a[1,1]*w[1] = b[1]
            // w[0] + x[0]*w[1] = 0
            
            int N = ppl.Count;
            double[] b = new double[2];
            double[,] a = new double[2,N];
            for(int k = 1; k <= 1; ++k)
            {
                for(int n = 0; n < N; ++n)
                {
                    b[k] += Math.Pow(ppl[n].X, k)*ppl[n].Y;
                }
                for(int j = 0; j < N; ++j)
                {
                    for(int n = 0; n < N; ++n)
                    {
                        a[k, j] += Math.Pow(ppl[n].X, k+j);
                    }
                }
            }

            // w[0] = -x[0]*w[1]
            // -a[1,0]*x[0]*w[1] + a[1,1]*w[1] = b[1]
            // w[1] = b[1] / (a[1,1] - a[1,0]*x[0])

            
            double[] w = new double[2];
            w[1] = b[1]/(a[1, 1] - a[1, 0]*ppl[0].X);
            w[0] = -ppl[0].X*w[1];

            PointPairList linePpl = new PointPairList();
            for (int n = 0; n < N; ++n)
            {
                double x = ppl[n].X;
                double y = w[0] + x*w[1];
                linePpl.Add(x, y);
            }
            LineItem curve1 = _pane.AddCurve(cd.Curve.Label.Text + " c-fitting" , linePpl, cd.Curve.Color, SymbolType.None);
        }

        void logParser_OnGameRecord(GameLogParser source, GameRecord gameRecord)
        {
            if (!gameRecord.IsGameOver)
                return;

            _samplesCount++;
            if (_samplesCount >= _sampleLimit)
            {
                _sampleStep *= 2;
                _sampleLimit *= 2;
            }

            CurveDescriptor cdKey = new CurveDescriptor();
            for (int pos = 0; pos < gameRecord.Players.Count; ++pos)
            {
                GameRecord.Player player = gameRecord.Players[pos];

                cdKey.Player = player.Name;
                cdKey.Position = pos;

                CurveDescriptor curveDescriptor;

                if (!_curveDescriptors.TryGetValue(cdKey, out curveDescriptor))
                {
                    if (_autocreateCurveDescriptors)
                    {
                        curveDescriptor = new CurveDescriptor { Player = player.Name, Position = -1 };
                        _curveDescriptors.Add(curveDescriptor, curveDescriptor);
                    }
                    else
                    {
                        // No curve for this player and position
                        continue;
                    }
                }


                double total;
                if (!_totalResult.TryGetValue(curveDescriptor, out total))
                {
                    total = 0;
                }
                total += player.Result;
                _totalResult[curveDescriptor] = total;

                curveDescriptor.SamplesCount++;

                // Add to the chart only samples from players from game record,
                // not from all from _totalResult, in order to see where a player
                // stops playing.

                if ((curveDescriptor.SamplesCount % _sampleStep) != 0)
                {
                    continue;
                }

                UpdateChart(curveDescriptor, total);
            }
        }

        private void UpdateChart(CurveDescriptor curveDescriptor, double total)
        {
            if (!_curves.Contains(curveDescriptor))
            {
                curveDescriptor.Points = new PointPairList();
                Color col = _colors[(_nextColor++)%_colors.Length];
                col = Color.FromArgb((int)(col.R * _colorCoeff), (int)(col.G * _colorCoeff), (int)(col.B * _colorCoeff));
                string label = string.Format("{0}{1}", curveDescriptor.Player, curveDescriptor.Position == -1 ? "" : ","+curveDescriptor.Position.ToString());
                curveDescriptor.Curve = _pane.AddCurve(label, curveDescriptor.Points, col, SymbolType.None);
                _curves.Add(curveDescriptor);
                // Add initial 0-value.
                curveDescriptor.Points.Add(_samplesCount - 1, 0);
            }
            curveDescriptor.Points.Add(_samplesCount, total);
        }

        void logParser_OnError(GameLogParser source, string error)
        {
            throw new ApplicationException(source.GetDefaultErrorText(error));
        }

        #region Data members    

        /// <summary>
        /// Compares name, position == -1 equals to anything.
        /// </summary>
        class CurveDescriptorKeyComparer: IEqualityComparer<CurveDescriptor>
        {
            #region IEqualityComparer<CurveDescriptor> Members

            public bool Equals(CurveDescriptor x, CurveDescriptor y)
            {
                if (x.Player != y.Player)
                {
                    return false;
                }
                if (x.Position == -1 || y.Position == -1)
                {
                    return true;
                }
                return x.Position == y.Position;
            }

            public int GetHashCode(CurveDescriptor obj)
            {
                // Ignore position.
                return obj.Player.GetHashCode();
            }

            #endregion
        }

        /// <summary>
        /// Compares both name and position.
        /// </summary>
        class CurveDescriptorComparer : IEqualityComparer<CurveDescriptor>
        {
            public bool Equals(CurveDescriptor x, CurveDescriptor y)
            {
                return x.Player == y.Player && x.Position == y.Position;                
            }

            public int GetHashCode(CurveDescriptor obj)
            {
                return obj.Player.GetHashCode() ^ obj.Position;
            }
        }

        class CurveDescriptor
        {
            /// <summary>
            /// Player name.
            /// </summary>
            public string Player;

            /// <summary>
            /// Player position, -1 - all positions.
            /// </summary>
            public int Position;

            public int SamplesCount = 0;

            public LineItem Curve;

            public PointPairList Points;
        }


        Dictionary<CurveDescriptor, CurveDescriptor> _curveDescriptors = new Dictionary<CurveDescriptor, CurveDescriptor>(new CurveDescriptorKeyComparer());
        bool _autocreateCurveDescriptors = true;

        private HashSet<CurveDescriptor> _curves = new HashSet<CurveDescriptor>(new CurveDescriptorComparer());
        private Dictionary<CurveDescriptor, double> _totalResult = new Dictionary<CurveDescriptor, double>(new CurveDescriptorComparer());

        private readonly Color[] _colors = new Color[]
                                      {
                                          Color.Red, Color.Green, Color.Blue, Color.Black, Color.Brown,
                                          Color.Cyan, Color.Gray, Color.Yellow, Color.Orange, Color.Olive
                                      };

        private string[] _paths;

        private int _nextColor = 0;

        private int _samplesCount = 0;
        private int _sampleStep = 1;
        private int _sampleLimit = 512;
        GameLogParser _logParser = new GameLogParser();

        private ZedGraphControl _zedGraph;
        private GraphPane _pane;

        bool _isLogInProgress = false;

        private double _colorCoeff;

        #endregion
    }
}
