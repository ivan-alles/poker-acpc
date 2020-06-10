/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ai.pkr.bots.neytiri
{
    public class ActionTree
    {
        public ActionTree()
        {
        }

        //public ActionTree(Bucketizer bucketizer)
        //{
        //    Version = new BdsVersion(Assembly.GetExecutingAssembly());
        //    //GameDef = gameDef;
        //    Bucketizer = bucketizer;
        //    Positions = new ActionTreeNode[GameDef.MinPlayers];
        //    for (int p = 0; p < Positions.Length; ++p)
        //    {
        //        Positions[p] = new ActionTreeNode();
        //        Positions[p].State = new GameState(gameDef, gameDef.MinPlayers);
        //    }
        //}

        //public void Build()
        //{
        //    for (int p = 0; p < Positions.Length; ++p)
        //    {
        //        ActionTreeBuilder actionTreeBuilder = new ActionTreeBuilder();
        //        actionTreeBuilder.Build(this, p);
        //    }
        //}

        //public BdsVersion Version
        //{
        //    set;
        //    get;
        //}

        public ActionTreeNode[] Positions
        {
            set;
            get;
        }

        //public GameDefinition GameDef
        //{
        //    set;
        //    get;
        //}

        //public Bucketizer Bucketizer
        //{
        //    set;
        //    get;
        //}

        //public static bool TreeGetChild(ActionTree tree, ActionTreeNode n, ref int i, out ActionTreeNode child)
        //{
        //    if (i < n.Children.Count)
        //    {
        //        child = n.Children[i++];
        //        return true;
        //    }
        //    child = null;
        //    return false;
        //}
    }
}
