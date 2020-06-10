/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.pkr.stdpoker;
using System.IO;
using ai.lib.utils;
using System.Xml.Serialization;

namespace ai.pkr.bots.neytiri
{
    [Serializable]
    public class ActionTreeNode
    {
        #region Properties
        public int Id;
        public Ak ActionKind = Ak.b;
        public Buckets OppBuckets;
        public double[] PreflopValues;
        public GameState State;

        [XmlIgnore]
        public double Value;
        [XmlIgnore]
        public double StrategyFactor;

        public List<ActionTreeNode> Children = new List<ActionTreeNode>();
        #endregion 

        public ActionTreeNode FindChildByAction(Ak actionKind)
        {
            for (int c = 0; c < Children.Count; ++c)
            {
                if (Children[c].ActionKind == actionKind)
                {
                    return Children[c];
                }
            }
            return null;
        }

        public override string ToString()
        {
            return String.Format("{0} P{1}", ActionKind, State.Pot);
        }
    }
}
