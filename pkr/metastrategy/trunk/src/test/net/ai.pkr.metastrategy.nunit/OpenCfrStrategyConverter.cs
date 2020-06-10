/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using ai.lib.algorithms.tree;
using ai.pkr.metagame;
using ai.lib.utils;
using System.Diagnostics;
using ai.pkr.metastrategy.algorithms;

namespace ai.pkr.metastrategy.nunit
{
    /// <summary>
    /// Converts test strategy files in Open CFR format to StrategyTree.
    /// Can be used for non-abstracted and abstracted games. In the latter case the abstaction of
    /// the OCFR file must correspond to the abstraction used in the strategy.
    /// </summary>
    internal unsafe class OpenCfrStrategyConverter
    {
        public int HeroPosition
        {
            set;
            get;
        }

        public string SourceFile
        {
            set;
            get;
        }

        public GameDefinition GameDef
        {
            set;
            get;
        }

        public StrategyTree Strategy
        {
            set;
            get;
        }

        /// <summary>
        /// Allows to test bucketizer. The bucketizer must be initialized for the full game.
        /// </summary>
        public IChanceAbstraction ChanceAbstraction
        {
            set;
            get;
        }


        private void LoadStrategy()
        {
            Regex reProbabTriple = new Regex(@"^([^\s]+)\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)", RegexOptions.Compiled);

            using (StreamReader reader = new StreamReader(SourceFile))
            {
                // For debugging
                int lineCount = 0; 
                for (; ; )
                {
                    string line = reader.ReadLine();
                    lineCount++;
                    // Console.WriteLine(line);
                    if (line.StartsWith("#"))
                        continue;

                    Match m = reProbabTriple.Match(line);
                    string actionString = m.Groups[1].Value;
                    
                    double probabR = double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                    double probabC = double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);
                    double probabF = double.Parse(m.Groups[4].Value, CultureInfo.InvariantCulture);

                    int round;
                    List<PokerAction> actions = ParseActionString(actionString, out round);

                    GameState gs = new GameState(GameDef);
                    foreach(PokerAction a in actions)
                    {
                        gs.UpdateByAction(a, GameDef);
                    }

                    List<Ak> allowedActions = gs.GetAllowedActions(GameDef);

                    if (allowedActions.Contains(Ak.r))
                    {
                        actions.Add(PokerAction.r(HeroPosition, GetRaiseAmount(round)));
                        UpdateProbability(actions.ToArray(), probabR);
                        actions.RemoveAt(actions.Count - 1);
                    }

                    if (allowedActions.Contains(Ak.c))
                    {
                        actions.Add(PokerAction.c(HeroPosition));
                        UpdateProbability(actions.ToArray(), probabC);
                        actions.RemoveAt(actions.Count - 1);
                    }

                    if (allowedActions.Contains(Ak.f))
                    {
                        actions.Add(PokerAction.f(HeroPosition));
                        UpdateProbability(actions.ToArray(), probabF);
                    }

                    if (reader.EndOfStream)
                        break;
                }
            }
        }

        private void UpdateProbability(PokerAction[] actions, double probab)
        {
            StringBuilder strategicString = new StringBuilder();
            GameState gs = new GameState(GameDef);
            int raiseCount = 0;
            for(int a = 1; a < actions.Length; ++a)
            {
                PokerAction pa = actions[a];
                if (pa.Kind == Ak.d)
                {
                    // Skip private deals to opponents
                    if (pa.Position > 0) 
                    {
                        continue;
                    }
                    gs.UpdateByAction(pa, GameDef);
                    strategicString.Append('0');
                    strategicString.Append('d');
                    strategicString.Append(pa.Cards);
                    raiseCount = 0;
                }
                else
                {
                    double curInPot = gs.Players[pa.Position].InPot;
                    gs.UpdateByAction(pa, GameDef);
                    strategicString.Append(pa.Position.ToString());
                    if(pa.Kind == Ak.r)
                    {
                        raiseCount++;
                    }
                    strategicString.Append('p');
                    double newInPot = gs.Players[pa.Position].InPot;
                    double amount = newInPot - curInPot;
                    strategicString.Append(amount.ToString());
                }
                strategicString.Append(' ');
            }
            int node = (int)Strategy.FindNode(strategicString.ToString(), null);
            // Just set the value.
            // It is obviously correct for non-abstract games.
            // For abstract games the cards corresponding to the same abstract cards will have the same
            // probability anyway provided that the OCFR strategy uses the same abstraction.
            Strategy.Nodes[node].Probab = probab;
        }

        double GetRaiseAmount(int round)
        {
            return round == 0 ? 1 : 2;
        }

        /// <summary>
        /// Parses Open CFG format to PokerAction list. Converts cards to indexes for both abstracted
        /// and non-abstracted games.
        /// </summary>
        List<PokerAction> ParseActionString(string key, out int round)
        {
            List<PokerAction> actions = new List<PokerAction>();
            actions.Add(new PokerAction());
            string[] parts = key.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
            if(parts.Length != 2)
            {
                throw new ApplicationException("Wrong format");
            }
            string pocket = parts[0].Substring(0, 1);
            string board = parts[0].Length == 1 ? null : parts[0].Substring(1, 1);

            // Add two deals, as this is required to update game state.
            // One deal will be ignored later in the conversion to a strategic string.
            actions.Add(PokerAction.d(0, GameDef.DeckDescr.GetIndex(pocket).ToString()));
            actions.Add(PokerAction.d(1, GameDef.DeckDescr.GetIndex(pocket).ToString()));

            round = 0;
            int pos = 0;
            foreach(char actChar in parts[1].Substring(1))
            {
                switch(actChar)
                {
                    case 'r':
                        actions.Add(PokerAction.r(pos, GetRaiseAmount(round)));
                        pos = 1 - pos;
                        break;
                    case 'c':
                        actions.Add(PokerAction.c(pos));
                        pos = 1 - pos;
                        break;
                    case '/':
                        actions.Add(PokerAction.d(GameDef.DeckDescr.GetIndex(board).ToString()));
                        round++;
                        pos = 0;
                        break;
                    default:
                        throw new ApplicationException("Wrong format");
                }
            }
            if (ChanceAbstraction != null)
            {
                int[] hand = new int[2];
                round = -1;
                for(int i = 0; i < actions.Count; ++i)
                {
                    PokerAction a = actions[i];
                    if(a.Kind == Ak.d && a.Position <= 0)
                    {
                        ++round;
                        hand[round] = int.Parse(a.Cards);
                    }
                    if (a.IsDealerAction() && a.Cards != "")
                    {
                        int bucket = ChanceAbstraction.GetAbstractCard(hand, round+1);
                        a.Cards = bucket.ToString();
                    }
                }
            }

            return actions;
        }


        public void Convert()
        {
            LoadStrategy();
            ConvertCondToAbs.Convert(Strategy, HeroPosition);
        }
    }
}
