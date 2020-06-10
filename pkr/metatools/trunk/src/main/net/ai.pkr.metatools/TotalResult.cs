/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using System.IO;
using ai.lib.utils;

namespace ai.pkr.metatools
{
    /// <summary>
    /// Calculates total game result in multiple games, possibly in multiple sessions 
    /// with different players.
    /// </summary>
    public class TotalResult: IGameLogReport
    {
        #region IGameLogReport implementation

        public void ShowHelp(TextWriter tw)
        {
            tw.WriteLine("Calculates total game result in multiple games, possibly in multiple sessions with different players.");
            tw.WriteLine("Parameters: none.");
        }

        public string Name
        {
            set;
            get;
        }

        public void Configure(Props pm)
        {
        }

        public void Update(GameRecord gameRecord)
        {
            if (!gameRecord.IsGameOver)
                return;
            GamesCount++;
            for (int pos = 0; pos < gameRecord.Players.Count; ++pos)
            {
                GameRecord.Player grPlayer = gameRecord.Players[pos];
                Player myPlayer = FindOrCreatePlayer(grPlayer.Name);
                myPlayer.Update(pos, 1, grPlayer.Result);
            }
        }

        public void Print(TextWriter tw)
        {
            tw.WriteLine("{0}, {1} games", Name, GamesCount);

            // Group results by players, because usually it is necessary to see the result of 
            // one bot under test in all positions and total as one piece of data.

            foreach (Player player in _players.Values)
            {
                tw.WriteLine("Player: {0}", player.Name);
                for (int pos = 0; pos < player.Result.Length; ++pos)
                {
                    if (player.GamesCount[pos] > 0)
                    {
                        tw.WriteLine("Pos {0,2}: {1,9:#.0} b, {2,8:#.00} mb/g, {3,8} games",
                                     pos, player.Result[pos], player.Rate(pos), player.GamesCount[pos]);
                    }
                    else
                    {
                        tw.WriteLine("{0,2}: N/A", player.Name);
                    }
                }
                tw.WriteLine("Total : {1,9:#.0} b, {2,8:#.00} mb/g, {3,8} games",
                    "dummy", player.ResultTotal(), player.RateTotal(), player.GamesCountTotal());
            }
        }

        #endregion

        public class Player
        {
            public string Name
            {
                set;
                get;
            }

            /// <summary>
            /// Number of games by position this player took part in.
            /// </summary>
            public int[] GamesCount
            {
                get { return _gamesCount;  }
            }

            /// <summary>
            /// Total number of games this player took part in.
            /// </summary>
            public int GamesCountTotal()
            {
                int r = 0;
                for (int i = 0; i < _gamesCount.Length; ++i)
                    r += _gamesCount[i];
                return r;
            }

            /// <summary>
            /// Result by position in b.
            /// </summary>
            public double[] Result
            {
                get { return _result; }
            }

            /// <summary>
            /// Total result in b.
            /// </summary>
            public double ResultTotal()
            {
                double r = 0;
                for (int i = 0; i < _result.Length; ++i)
                    r += _result[i];
                return r;                    
            }

            /// <summary>
            /// Win rate by position, in mb/g.
            /// </summary>
            public double Rate(int pos)
            {
                int gamesCount = GamesCount[pos];
                if (gamesCount == 0)
                    return 0;
                return 1000 * Result[pos] / gamesCount;
            }

            /// <summary>
            /// Win rate, in mb/g.
            /// </summary>
            public double RateTotal()
            {
                int gamesCount = GamesCountTotal();
                if (gamesCount == 0)
                    return 0;
                return 1000 * ResultTotal() / gamesCount;
            }
            #region Internal

            internal void Update(Player player)
            {
                SetArraySizes(player._result.Length);
                for(int p = 0; p < player._result.Length; ++p)
                {
                    _gamesCount[p] += player._gamesCount[p];
                    _result[p] += player._result[p];
                }
            }

            internal void Update(int pos, int gamesCount, double result)
            {
                SetArraySizes(pos);
                _gamesCount[pos] += gamesCount;
                _result[pos] += result;
            }

            #endregion

            #region Private

            void SetArraySizes(int pos)
            {
                if (pos >= _result.Length)
                {
                    Array.Resize(ref _result, pos + 1);
                    Array.Resize(ref _gamesCount, pos + 1);
                }
            }

            private double[] _result = new double[0];
            private int[] _gamesCount = new int[0];

            #endregion
        }

        /// <summary>
        /// A descriptive name, will be printed.
        /// </summary>


        public int GamesCount
        {
            set;
            get;
        }

        public Dictionary<string, Player> Players
        {
            get
            {
                return _players;
            }
        }

        public void UpdateByTotalResult(TotalResult totalResult)
        {
            GamesCount += totalResult.GamesCount;
            foreach (Player player in totalResult._players.Values)
            {
                Player myPlayer = FindOrCreatePlayer(player.Name);
                myPlayer.Update(player);
            }
        }

        #region Private

        private Player FindOrCreatePlayer(string name)
        {
            Player player;
            if (!_players.TryGetValue(name, out player))
            {
                player = new Player { Name = name };
                _players[player.Name] = player;
            }
            return player;
        }

        #endregion

        #region Data members

        private Dictionary<string, Player> _players = new Dictionary<string, Player>();

        #endregion

    }
}
