/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZedGraph;
using System.Drawing;
using System.IO;

namespace ai.pkr.metatools.pkrchart
{
    class PaneWrapper
    {
        internal void CreatePane(string [] paths, ZedGraphControl zedGraph)
        {
            _paths = paths;
            _zedGraph = zedGraph;
            _pane = new GraphPane(new RectangleF(
                zedGraph.ClientRectangle.X,
                zedGraph.ClientRectangle.Y,
                zedGraph.ClientRectangle.Width,
                zedGraph.ClientRectangle.Height),
                GetTitle(),
                "Games", "Result (b)");


            _pane.XAxis.Scale.MinAuto = true;
            _pane.XAxis.Scale.MaxAuto = true;
            _pane.XAxis.Scale.MagAuto = false;
            _pane.XAxis.Scale.Mag = 0;
            _pane.XAxis.Scale.Format = "###,###";

            _pane.YAxis.Scale.MinAuto = true;
            _pane.YAxis.Scale.MaxAuto = true;
            _pane.YAxis.Scale.MagAuto = false;
            _pane.YAxis.Scale.Mag = 0;
            _pane.YAxis.Scale.Format = "###,###";

            zedGraph.MasterPane[0] = _pane;

            zedGraph.AxisChange();
            _zedGraph.Refresh();
        }

        internal GraphPane Pane
        {
            get { return _pane; }
        }

        internal string GetTitle()
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
            return dirName + fileName;
        }

        #region Data members

        private string[] _paths;
        private GraphPane _pane;

        private ZedGraphControl _zedGraph;

        #endregion
    }
}
