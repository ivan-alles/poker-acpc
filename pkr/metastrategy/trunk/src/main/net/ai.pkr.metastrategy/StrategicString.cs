/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using ai.pkr.metagame;

namespace ai.pkr.metastrategy
{
    /// <summary>
    /// Operations with text representations of IStrategicAction. 
    /// Resembles in some ways the class GameRecord, but designed for use 
    /// cases for development of strategic algorithms.
    /// <para>As parsing of real game logs is not required, there are the following limitations:</para>
    /// <para>-Tabs are not supported.</para>
    /// <para>-Positions in range [0..9] only are supported, position must be present.</para>
    /// </summary>
    public static class StrategicString
    {
        #region Public API

        /// <summary>
        /// Default string representation of a dealer action.
        /// Intended to be reused by implementations of IDealerAction.
        /// </summary>
        public static string ToStrategicString(int position, int card, object parameters)
        {
            if (parameters is string[])
            {
                string[] cardNames = (string[])parameters;
                string cardName = card < 0 ? "" : cardNames[card];
                return String.Format("{0}d{1}", position == -1 ? "" : position.ToString(), cardName);
            }
            return String.Format("{0}d{1}",
                position == -1 ? "" : position.ToString(), card);
        }

        /// <summary>
        /// Default string representation of a player action.
        /// Intended to be reused by implementations of IPlayerAction.
        /// </summary>
        public static string ToStrategicString(int position, double amount, object parameters)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}p{1:0.#}", position, amount);
        }

        public static string ToStrategicString(IStrategicAction[] actions, object parameters)
        {
            StringBuilder sb = new StringBuilder();
            for(int a = 0; a < actions.Length; ++a)
            {
                sb.Append(actions[a].ToStrategicString(parameters));
                if (a < actions.Length - 1)
                {
                    sb.Append(' ');
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Parses a strategic string.
        /// </summary>
        /// <param name="strategicString"></param>
        /// <param name="parameters">
        /// <para>One of the following:</para>
        /// <para>- null</para>
        /// <para>- a DeckDescriptor, in this case the cards must be repesented by names.</para>
        /// </param>
        /// <returns></returns>
        public static List<IStrategicAction> FromStrategicString(string strategicString, object parameters)
        {
            List<IStrategicAction> list = new List<IStrategicAction>();
            string error;
            if (!ParseActions(strategicString, list, out error, parameters))
            {
                throw new ApplicationException(error);
            }
            return list;
        }

        #endregion

        #region Implementation

        class DealerAction : IDealerAction
        {
            public int Card
            {
                set;
                get;
            }

            public int Position
            {
                set;
                get;
            }

            public string ToStrategicString(object parameters)
            {
                return StrategicString.ToStrategicString(this.Position, this.Card, parameters);
            }
        }

        class PlayerAction : IPlayerAction
        {
            public double Amount
            {
                set;
                get;
            }

            public int Position
            {
                set;
                get;
            }

            public string ToStrategicString(object parameters)
            {
                return StrategicString.ToStrategicString(Position, Amount, parameters);
            }
        }

        #endregion

        #region String parsing

        static bool ParseActions(string gameString, List<IStrategicAction> actions, out string error, object parameters)
        {
            DeckDescriptor deckDescr = parameters as DeckDescriptor;
            error = "";
            int startPos = 0;
            char curChar;
            for (; startPos < gameString.Length; )
            {
                // Skip whitespace
                startPos = SkipWhiteSpace(gameString, startPos);
                if (startPos == gameString.Length)
                {
                    break;
                }
                curChar = gameString[startPos + 1];
                bool isDealer = false;
                switch(curChar)
                {
                    case 'd':
                        isDealer = true; break;
                    case 'p':
                        isDealer = false; break;
                    default:
                        return Error(out error, "Wrong action character: " + curChar.ToString());
                }
                if(!isDealer)
                {
                    PlayerAction parsedAction = new PlayerAction();
                    int pos;
                    if (!ParsePlayerPosition(ref error, gameString.Substring(startPos, 1), out pos))
                    {
                        return false;
                    }
                    parsedAction.Position = pos;
                    startPos+=2;
                    double amount;
                    if (!ReadAmount(gameString, ref startPos, ref error, out amount))
                        return false;
                    parsedAction.Amount = amount;
                    actions.Add(parsedAction);
                }
                else
                {
                    DealerAction parsedAction = new DealerAction();
                    int pos;
                    if (!ParsePlayerPosition(ref error, gameString.Substring(startPos, 1), out pos))
                    {
                        return false;
                    }
                    parsedAction.Position = pos;
                    startPos+=2;
                    if (!ReadCard(gameString, ref startPos, ref error, parsedAction, deckDescr))
                        return false;
                    actions.Add(parsedAction);
                }
            }
            return true;
        }

        private static bool ReadCard(string gameString, ref int curPos, ref string error, DealerAction parsedAction, DeckDescriptor deckDescr)
        {
            int card;
            int nextSpace = gameString.IndexOf(' ', curPos);
            int cardLength = nextSpace == -1 ? gameString.Length - curPos : nextSpace - curPos;
            string textCard = gameString.Substring(curPos, cardLength);
            if (deckDescr == null)
            {
                if (!int.TryParse(textCard, out card))
                {
                    return Error(out error, String.Format("Wrong card {0}", textCard));
                }
            }
            else
            {
                card = deckDescr.GetIndex(textCard);
            }
            parsedAction.Card = card;
            curPos += cardLength;
            return true;
        }

        private static bool ReadAmount(string gameString, ref int curPos, ref string error, out double amount)
        {
            amount = 0;
            int startPos = curPos;
            for (; curPos < gameString.Length; ++curPos)
            {
                switch (gameString[curPos])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                    case '.':
                        break;
                    default:
                        goto end;
                }
            }
        end:
            if (curPos - startPos == 0)
            {
                return Error(out error, String.Format("No amount found in {0}", gameString.Substring(startPos-2)));
            }
            string amountText = gameString.Substring(startPos, curPos - startPos);
            return ParseAmount(ref error, "amount", amountText, out amount);
        }

        private static int SkipWhiteSpace(string gameString, int curPos)
        {
            for (; curPos < gameString.Length; ++curPos)
            {
                switch (gameString[curPos])
                {
                    case ' ':
                        continue;
                    default:
                        return curPos;
                }
            }
            return curPos;
        }

        static bool ParseAmount(ref string error, string name, string text, out double value)
        {
            value = 0;
            if (text.Length == 0)
            {
                return Error(out error, String.Format("Empty number"));
            }
            try
            {
                // This is much faster as parsing with the .Net functions.
                bool dotFound = false;
                bool minus = false;
                int i = 0;
                if (text[0] == '-')
                {
                    minus = true;
                    ++i;
                    if (text.Length == 1)
                    {
                        return Error(out error, String.Format("Wrong number: {0}", text));
                    }
                }
                double dotFactor = 0.1;
                for (; i < text.Length; ++i)
                {
                    if (text[i] == '.')
                    {
                        dotFound = true;
                        continue;
                    }
                    int digit = text[i] - '0';
                    if (digit < 0 || digit > 9)
                    {
                        return Error(out error, String.Format("Wrong character in number: {0}", text));
                    }
                    if (minus)
                        digit = -digit;

                    if (dotFound)
                    {
                        value = value + digit * dotFactor;
                        dotFactor *= 0.1;
                    }
                    else
                        value = value * 10 + digit;
                }
            }
            catch (Exception)
            {
                return Error(out error, String.Format("Wrong player position: {1}", text));
            }
            return true;
        }

        private static bool ParsePlayerPosition(ref string error, string text, out int position)
        {
            position = 0;
            if (text.Length == 0)
            {
                return Error(out error, "No position found");
            }

            try
            {
                // This is much faster as parsing with the .Net functions.
                // This code was copied from metagame and allows to parse positions > 9 although this is not necessary.
                for (int i = 0; i < text.Length; ++i)
                {
                    int digit = text[i] - '0';
                    if (digit < 0 || digit > 9)
                    {
                        return Error(out error, String.Format("Wrong character in number: {0}", text));
                    }
                    position = position * 10 + digit;
                }
            }
            catch (Exception)
            {
                return Error(out error, String.Format("Wrong player position: {1}", text));
            }
            return true;
        }

        static bool Error(out string error, string errorValue)
        {
            error = errorValue;
            return false;
        }

        #endregion
    }
}
