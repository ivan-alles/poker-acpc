/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metabots;
using ai.pkr.metagame;

namespace ai.pkr.acpc
{
    /// <summary>
    /// Converter for APCP 2011 server protocol. Now support for FL only.
    /// </summary>
    public class Acpc11ServerMessageConverter : IAcpcServerMessageConverter
    {
        public Acpc11ServerMessageConverter()
        {
            LineTerminator = "\r\n";
        }

        #region IAcpcServerMessageConverter Members

        /// <summary>
        /// Line terminator, default "\r\n".
        /// </summary>
        public string LineTerminator
        {
            private set;
            get;
        }

        public IPlayer Player
        {
            get;
            set;
        }

        public string PlayerName
        {
            set;
            get;
        }

        public const string OPPONENT_NAME = "?Opp";

        public string OnServerMessage(string textMessage)
        {
            if (textMessage.StartsWith("#") || textMessage.StartsWith(";"))
            {
                // Ignore comments
                return null;
            }

            ServerMessage msg = new ServerMessage(textMessage);
            int ourPosition = 1 - msg.AcpcPosition;

            int nextActor = (msg.Betting[msg.Round].Length) % 2;
            if (msg.Round > 0)
            {
                // reversed blinds
                nextActor = 1 - nextActor;
            }

            GameRecord gr = new GameRecord();
            // For now fill only fields that are necessary for Patience.

            gr.Id = msg.HandNumber;
            for (int p = 0; p < 2; ++p)
            {
                gr.Players.Add(new GameRecord.Player(OPPONENT_NAME, 0, p == 0 ? 0.5 : 1.0, 0));
            }

            gr.Players[ourPosition].Name = PlayerName;

            if (msg.Betting[0].Length == 0)
            {
                // Empty action string for round 0 - game started.
                GameCount++;
                Player.OnGameBegin(gr.ToGameString());
            }

            // Add actions to the game record after the call to OnGameBegin().
            ParseActionsForHe(msg, gr);

            if (nextActor != ourPosition)
            {
                // If we are not acting, do not update the bot.
                return null;
            }

            if (msg.IsGameOver())
            {
                return null;
            }

            PokerAction response = Player.OnActionRequired(gr.ToGameString());

            string responseText = textMessage + ":" + response.Kind.ToString();

            return responseText;
        }

        void ParseActionsForHe(ServerMessage msg, GameRecord gr)
        {
            string[] privateCards = msg.GetPrivateCards();
            // Reverse positions for our format
            for (int p = 1; p >= 0; --p)
            {
                string cards = privateCards[p];
                if (cards == "")
                {
                    cards = "? ?";
                }
                else
                {
                    cards = CardsFromAcpc(cards);
                }
                PokerAction a = PokerAction.d(1 - p, cards);
                gr.Actions.Add(a);
            }

            for (int r = 0; r <= msg.Round; ++r)
            {
                if (r > 0)
                {
                    PokerAction a = PokerAction.d(CardsFromAcpc(msg.Cards[r]));
                    gr.Actions.Add(a);
                }
                for (int move = 0; move < msg.Betting[r].Length; ++move)
                {
                    PokerAction a = null;
                    int position = r == 0 ? move % 2 : 1 - (move % 2);
                    switch (msg.Betting[r][move])
                    {
                        case 'r':
                            a = PokerAction.r(position, r < 2 ? 1 : 2);
                            break;
                        case 'c':
                            a = PokerAction.c(position);
                            break;
                        case 'f':
                            a = PokerAction.f(position);
                            break;
                    }
                    gr.Actions.Add(a);
                }
            }
        }

        string CardsFromAcpc(string cards)
        {
            StringBuilder result = new StringBuilder();
            for (int pos = 0; pos < cards.Length; pos += 2)
            {
                result.Append(cards.Substring(pos, 2));
                result.Append(' ');
            }
            return result.ToString().Trim();
        }

        public string HandshakeMessage
        {
            get { return "VERSION:2.0.0"; }
        }

        public int GameCount
        {
            get;
            private set;
        }

        #endregion

        class ServerMessage
        {

            public ServerMessage(string message)
            {
                string[] parts = message.Split(new char[] { ':' });
                if (parts.Length != 5)
                {
                    ReportWrongFormatError("Wrong number of message parts", message);
                }
                Header = parts[0];
                if (!int.TryParse(parts[1], out AcpcPosition))
                {
                    ReportWrongFormatError("Position is not a number", message);
                }
                HandNumber = parts[2];
                Betting = parts[3].Split(new char[] { '/' });
                Cards = parts[4].Split(new char[] { '/' });
            }

            private void ReportWrongFormatError(string comment, string message)
            {
                string text = string.Format("{0} in server message '{1}'", comment, message);
                throw new ApplicationException(text);
            }

            public int Round
            {
                get { return Cards.Length - 1; }
            }

            public String Header;

            /// <summary>
            /// Tells the client their position relative to the dealer button. A value of 0 indicates
            /// that for the current hand, the client is the first player after the button 
            /// (the small blind in ring games, or the big blind in reverse-blind heads-up games.)
            /// </summary>
            public int AcpcPosition;

            public String HandNumber;

            /// <summary>
            /// Player actions for each round.
            /// </summary>
            public String[] Betting;

            /// <summary>
            /// Cards for each round.
            /// </summary>
            public String[] Cards;

            public string[] GetPrivateCards()
            {
                string[] privateCards = Cards[0].Split(new char[] { '|' });
                return privateCards;
            }

            public bool IsShowDown()
            {
                string[] privateCards = GetPrivateCards();
                return !string.IsNullOrEmpty(privateCards[0]) && !string.IsNullOrEmpty(privateCards[1]);
            }

            public bool IsGameOver()
            {
                if (IsShowDown())
                {
                    return true;
                }
                return Betting[Betting.Length - 1].EndsWith("f");
            }

        }
    }
}
