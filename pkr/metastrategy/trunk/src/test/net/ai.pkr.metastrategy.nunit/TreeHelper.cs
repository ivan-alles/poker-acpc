/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metastrategy;
using ai.pkr.metagame;
using ai.pkr.metastrategy.algorithms;

namespace ai.pkr.metastrategy.nunit
{
    class TreeHelper
    {
        public static StrategyTree CreateStrategyTree(GameDefinition gd, int pos)
        {
            ChanceTree ct = CreateChanceTreeByGameDef.Create(gd);
            ChanceTree pct = ExtractPlayerChanceTree.ExtractS(ct, 0);
            ActionTree at = CreateActionTreeByGameDef.Create(gd);
            StrategyTree st = CreateStrategyTreeByChanceAndActionTrees.CreateS(pct, at);
            return st;
        }
    }
}
