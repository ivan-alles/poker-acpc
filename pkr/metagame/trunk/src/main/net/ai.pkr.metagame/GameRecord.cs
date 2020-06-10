/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System.Collections.Generic;
using System.Text;
using System;
using System.Globalization;

namespace ai.pkr.metagame
{
    /// <summary>
    /// Information about a game. The format of data allows to save/restore 
    /// every strategy-relevant game state information without knowing it's definition.
    /// <para>
    /// Can be represented as textual string (game string) with a compact and expressive format
    /// to log the game. Equivalent game records convert to equal game strings.
    /// </para>
    /// <para>
    /// Game log is string-oriented and can contain comments, meta-data and game records.
    /// </para>
    ///<remarks>
    /// <para>
    /// Game log format is designed for the following use-cases:</para>
    /// <para>- server writes log</para>
    /// <para>- server replays games dealing same cards</para>
    /// <para>- a player imitates another player</para>
    /// <para>- show a chart of a game.</para>
    /// <para>- anaylyze statistics and strategy</para>
    /// <para>- format to send game information from server to player.</para>
    /// 
    /// <para>Example:</para>
    /// 
    /// <para>"164; Phil{100 5 0} Gus{200 10 0} ...  Mat{156 0 0}; 0d{Ac Ah} 1d{Kh Kc} ... Nd{Xx Xx} 0r5 1c 2f ... Nf s{2c 3h 5c} ....;"
    /// </para>
    /// <para>Format:</para>
    ///  <para>All numbers are doubles with optional floating point, max 4 digits after the point are allowed.
    /// If space separator (" ") is required, number of spaces must be >= 1.
    /// # is a placeholder for player index, is an integer >= 0.
    /// </para>
    /// <para>
    /// game_log: [log_string "end-of-line"] [game-log]</para>
    /// <para>
    /// log_string: comment_string | meta_data_string | game_string</para>
    /// <para>
    /// comment_string: a string starting from '#' (no spaces before are allowed), will be ignored by parsers.</para>
    /// 
    /// <para>meta_data_string: a string starting from &gt; (no spaces before are allowed), will raise a 
    /// corresponding event in parser. This is up to the user to interperet the meta-data.
    /// </para>
    /// <para>
    /// game_string: [game_id] ";" player_list ";" [" " action_list ] terminator</para>
    /// <para>
    /// terminator: ";" | "."</para>
    /// <para>     "." means that the game is over, otherwise ";" is used.</para>
    /// <para>
    /// game_id: is an optional string without whitespace and ';' identifying the game.</para>
    /// <para>   It is recommended not to not it from # or &gt;. If it is necessary, a space separator at 
    ///          the beginning of the game string should be used.</para>
    /// <para>
    /// player_list: player_info  [" " player_info]</para>
    /// <para>
    /// player_info: player_name "{" stack_before_game " " blind " " player_result "}"</para>
    /// <para>
    /// player_name: is a string "[^ {]+".</para>
    /// <para>
    /// stack_before_game: stack size before the game (no blinds and actions are made).</para>
    /// <para>
    /// blind: blind posted, remains constant through the game.</para>
    /// <para>
    /// player_result: becomes known after gameover, amount won or lost. If there is rake, it can be taken 
    /// into account by reducing pot share.</para>
    /// <para>
    /// action_list: action [" " action]</para>
    /// <para>
    /// action: #r abs_amount | #c [abs_amount] | #f | #d card_list | #p card_list | s card_list</para>
    /// <para>#r: is a raise, amount is required</para>
    /// <para>#c: is a call, amount can be optionally specified to cover the use-case in online games
    ///        when a player disconnects and goes all-in with less than his stack.</para>
    /// <para>#f: is a fold.</para>
    /// <para>#d: deal private cards. If the cards are unknown, ? is used, the cards will be revealed at the showdown.</para>
    /// <para>#p: deal public cards for a player.</para>
    /// <para>s: deal shared cards.</para>
    /// <para></para>
    /// <para>
    /// card_list: {} | { card [" " card }</para>
    /// <para>
    /// card: a string "[^?]+" . "?" is reserved for an unknown card.</para>
    /// <para> 
    /// Note that every new round starts when all the players have made their "stopping" moves (calls or folds) and 
    /// the first actor in each round is the dealer (#d, #p or s).</para>  
    /// </remarks>
    /// </summary>
    [Serializable]
    public class GameRecord
    {
        #region Types
        /// <summary>
        /// Player's data. 
        /// Position of the player is defined by his index in Players list.
        /// </summary>
        [Serializable]
        public class Player
        {
            public Player()
            {
            }

            public Player(string name, double stack, double blind, double result)
            {
                Name = name;
                Stack = stack;
                Blind = blind;
                Result = result;
            }

            public string Name
            {
                set;
                get;
            }

            /// <summary>Stack at the beginning of the game.</summary>
            public double Stack
            {
                set;
                get;
            }
            public double Blind
            {
                set;
                get;
            }

            /// <summary>
            /// Amount won or lost. StackAfterGame = Stack + Result.
            /// <remarks>Result does not always corellate with the pot size. For instance,
            /// if one player wins everything, Result can differ from the pot size in case 
            /// somebody is folded.
            /// </remarks>
            /// </summary>
            public double Result
            {
                set;
                get;
            }
        }

        #endregion

        #region Constructors

        public GameRecord()
        {
        }

        public GameRecord(string gameString)
        {
            string error;
            if(!GameStringParser.Parse(gameString, out error, this))
            {
                throw new ApplicationException("Cannot create GameRecord: " + error);
            }
        }

        #endregion

        #region Properties

        public string Id { get; set; }

        /// <summary>Is true if game is finished. </summary>
        public bool IsGameOver { get; set; }

        public List<Player> Players
        {
            get { return _players; }
        }

        public List<PokerAction> Actions
        {
            get { return _actions; }
        }

        #endregion

        #region Methods

        public static GameRecord Parse(string gameString)
        {
            string error;
            GameRecord gr = new GameRecord();
            return GameStringParser.Parse(gameString, out error, gr) ? gr : null;
        }

        public int FindPositionByName(string name)
        {
            for (int p = 0; p < Players.Count; ++p)
            {
                if (Players[p].Name == name)
                    return p;
            }
            return int.MinValue;
        }

        public static GameRecord Parse(string gameString, out string error)
        {
            GameRecord gr = new GameRecord();
            return GameStringParser.Parse(gameString, out error, gr) ? gr : null;
        }

        public override string ToString()
        {
            return ToGameString();
        }

        /// <summary>
        /// Convert to game string. It is guaranteed that the output is normalized, 
        /// that means that:
        /// - white spaces are minimized - 1 space but no more between all parts of the string,
        /// including separators ; and }: "11; P1{0 0 0} P2{0 0 0}; 0d{Ac Ad} 1d{7c 2d};"
        /// - no whitespace at the beginning and after the terminator.
        /// </summary>
        /// Developer notes:
        /// - Normalizing of output makes it easy to write test code that compares
        /// game string with an expected one.
        public string ToGameString()
        {
            StringBuilder sb = new StringBuilder(512);

            sb.Append(Id);
            sb.Append("; ");

            for (int p = 0; p < _players.Count; ++p)
            {
                if(p > 0)
                    sb.Append(' ');
                
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}{{{1:0.####} {2:0.####} {3:0.####}}}", 
                    _players[p].Name, _players[p].Stack, _players[p].Blind, _players[p].Result);
            }
            sb.Append(';');
            
            for (int a = 0; a < _actions.Count; ++a)
            {
                sb.Append(' ');
                sb.Append(_actions[a].ToGameString());
            }
            sb.Append(IsGameOver ? '.' : ';');
            return sb.ToString();
        }

        public void NormalizeStakes(double b0)
        {
            foreach (Player player in Players)
            {
                player.Blind /= b0;
                player.Stack /= b0;
                player.Result /= b0;
            }
            foreach (PokerAction pa in Actions)
            {
                pa.Amount /= b0;
            }
        }

        #endregion

        #region Implementation

        private List<Player> _players = new List<Player>(10);
        private List<PokerAction> _actions = new List<PokerAction>(150);

        #endregion

    }
}