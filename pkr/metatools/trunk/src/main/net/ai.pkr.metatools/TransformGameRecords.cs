/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;

namespace ai.pkr.metatools
{
    /// <summary>
    /// Transforms game records. Processes game re
    /// <para>Renaming players:</para>
    /// <para>Renames are done before other usages of player names. 
    /// Order: eq, neq. Only one rename per name can be done.</para>
    /// </summary>
    public class TransformGameRecords
    {
        public TransformGameRecords()
        {
            NormalizeStakes = -1;
        }

        /// <summary>
        /// Set all games to game over.
        /// </summary>
        public bool FinalizeGames
        {
            set;
            get;
        }

        /// <summary>
        /// If truc, renumerate games starting from GameCount.
        /// </summary>
        public bool RenumerateGames
        {
            set;
            get;
        }

        /// <summary>
        /// Game count, is incremented with call to Trasform() that returns true.
        /// </summary>
        public int GameCount
        {
            set;
            get;
        }

        /// <summary>
        /// Replace cards of opponents by '?' (private and public).
        /// </summary>
        public bool HideOpponentCards
        {
            set;
            get;
        }
        
        /// <summary>
        /// Makes sure the cards are one space separated and trimmed.
        /// </summary>
        public bool NormalizeCards
        {
            set;
            get;
        }

        /// <summary>
        /// Set stacks to 0.
        /// </summary>
        public bool ResetStacks
        {
            set;
            get;
        }

        /// <summary>
        /// Set game results to 0.
        /// </summary>
        public bool ResetResults
        {
            set;
            get;
        }

        /// <summary>
        /// Normalize stakes with the given norm. Specify 0 to auto-detect, a negative number to skip normalizing.
        /// </summary>
        public double NormalizeStakes
        {
            set;
            get;
        }

        /// <summary>
        /// Renames a player to RenameEqNewName if his name equals to RenameEqName.
        /// </summary>
        public string RenameEqName
        {
            set;
            get;
        }

        /// <summary>
        /// Renames a player to RenameEqNewName if his name equals to RenameEqName.
        /// </summary>
        public string RenameEqNewName
        {
            set;
            get;
        }

        /// <summary>
        /// Renames a player RenameNeqNewName if his name does NOT equal to RenameNeqName.
        /// </summary>
        public string RenameNeqName
        {
            set;
            get;
        }

        /// <summary>
        /// Renames a player RenameNeqNewName if his name does NOT equal to RenameNeqName.
        /// </summary>
        public string RenameNeqNewName
        {
            set;
            get;
        }

        /// <summary>
        /// Hero name.
        /// </summary>
        public string HeroName
        {
            set;
            get;
        }

        /// <summary>
        /// Remove games without hero moves.
        /// </summary>
        public bool RemoveNoHeroMoves
        {
            set;
            get;
        }

        /// <summary>
        /// Remove finished games without showdown. Is done before finalizing.
        /// </summary>
        public bool RemoveNoShowdown
        {
            set;
            get;
        }

        /// <summary>
        /// Transforms the game record.
        /// </summary>
        /// <returns>False is this game record must be skipped (e.g. by RemoveNoShowdown). The game record may be nevertheless partially transformed.</returns>
        public bool Transform(GameRecord gameRecord)
        {
            RenamePlayers(gameRecord);

            // Now we know the hero
            int heroPos = GetHeroPos(gameRecord);
            if (RemoveNoHeroMoves)
            {
                bool heroMoved = false;
                foreach (PokerAction pa in gameRecord.Actions)
                {
                    if (pa.IsPlayerAction(heroPos))
                    {
                        heroMoved = true;
                        break;
                    }
                }
                if (!heroMoved)
                {
                    return false;
                }
            }

            GameState gs = new GameState(gameRecord, null);
            if (gs.IsGameOver)
            {
                if (RemoveNoShowdown && !gs.IsShowdownRequired)
                {
                    return false;
                }
            }

            if (HideOpponentCards)
            {
                DoHideOpponentCards(gameRecord, heroPos);
            }
            if (NormalizeCards)
            {
                DoNormalizeCards(gameRecord);
            }
            if (ResetResults)
            {
                foreach (GameRecord.Player p in gameRecord.Players)
                {
                    p.Result = 0;
                }
            }
            if (ResetStacks)
            {
                foreach (GameRecord.Player p in gameRecord.Players)
                {
                    p.Stack = 0;
                }
            }
            if (NormalizeStakes >= 0)
            {
                double factor = NormalizeStakes;
                if (NormalizeStakes == 0 && gameRecord.Players.Count >= 2)
                {
                    factor = gameRecord.Players[1].Blind;
                }
                gameRecord.NormalizeStakes(factor);
            }
            // Do renaming at the end in case we removed some games.
            if (RenumerateGames)
            {
                gameRecord.Id = GameCount.ToString();
                GameCount++;
            }

            if (FinalizeGames)
            {
                gameRecord.IsGameOver = true;
            }
            return true;
        }

        private void RenamePlayers(GameRecord gameRecord)
        {
            foreach (GameRecord.Player pl in gameRecord.Players)
            {
                if (!string.IsNullOrEmpty(RenameEqName))
                {
                    if (pl.Name == RenameEqName)
                    {
                        pl.Name = RenameEqNewName;
                        continue;
                    }
                }
                if (!string.IsNullOrEmpty(RenameNeqName))
                {
                    if (pl.Name != RenameNeqName)
                    {
                        pl.Name = RenameNeqNewName;
                        continue;
                    }
                }
            }
        }

        private void DoHideOpponentCards(GameRecord gameRecord, int heroPos)
        {
            foreach (PokerAction pa in gameRecord.Actions)
            {
                if (pa.Kind == Ak.d && pa.Position != -1 && pa.Position != heroPos)
                {
                    string[] cards = pa.Cards.Split(new char[] { ' ' },
                                                    StringSplitOptions.RemoveEmptyEntries);
                    pa.Cards = "";
                    for (int c = 0; c < cards.Length; ++c)
                    {
                        pa.Cards += '?';
                        if (c != cards.Length - 1)
                        {
                            pa.Cards += ' ';
                        }
                    }
                }
            }
        }

        private int GetHeroPos(GameRecord gameRecord)
        {
            int heroPos = -1;
            for (int p = 0; p < gameRecord.Players.Count; ++p)
            {
                if (gameRecord.Players[p].Name == HeroName)
                {
                    heroPos = p;
                    break;
                }
            }
            return heroPos;
        }

        private void DoNormalizeCards(GameRecord gameRecord)
        {
            foreach (PokerAction pa in gameRecord.Actions)
            {
                if (pa.Kind == Ak.d)
                {
                    string[] cards = pa.Cards.Split(new char[] { ' ' },
                                                    StringSplitOptions.RemoveEmptyEntries);
                    pa.Cards = "";
                    for (int c = 0; c < cards.Length; ++c)
                    {
                        pa.Cards += cards[c];
                        if (c != cards.Length - 1)
                        {
                            pa.Cards += ' ';
                        }
                    }
                }
            }
        }

    }
}
