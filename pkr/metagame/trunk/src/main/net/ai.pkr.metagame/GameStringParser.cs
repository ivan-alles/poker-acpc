/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace ai.pkr.metagame
{
    /// <summary>
    /// Parses game string.
    /// <remarks>Regular expressions are not used because they were too slow (6 times).</remarks>
    /// </summary>
    internal static class GameStringParser
    {
        internal static bool Parse(string gameString, out string error, GameRecord gameRecord)
        {
            try
            {
                if (String.IsNullOrEmpty(gameString))
                    return Error(out error, "Empty game log");

                error = "";
                int curPos = 0;
                int playerCount = 0;

                if (!ReadGameId(gameString, ref curPos, gameRecord, ref error))
                    return false;

                if (!ReadPlayers(gameString, ref curPos, gameRecord, ref error, ref playerCount)) 
                    return false;

                char terminator;
                if (!ParseActions(gameString, curPos, playerCount, gameRecord.Actions, ref error, out terminator))
                {
                    return false;
                }

                if (terminator == '?')
                {
                    return Error(out error, "No game string terminator found.");
                }
                gameRecord.IsGameOver = terminator == '.';
                return true;
            }
            catch(Exception e)
            {
                error = e.ToString();
            }
            return false;
        }

        /// <summary>
        /// Parses actions is a string in game string format.
        /// </summary>
        /// <param name="gameString">string of actions</param>
        /// <param name="startPos">starting position in gameString</param>
        /// <param name="playerCount">Number of players to verify actions.</param>
        /// <param name="actions">List to put parsed actions to</param>
        /// <param name="error">Error message</param>
        /// <param name="terminator">Terminator character, if found or '?'</param>
        /// <returns>True if success.</returns>
        internal static bool ParseActions(string gameString, int startPos, int playerCount, List<PokerAction> actions, ref string error, out char terminator)
        {
            char curChar;
            terminator = '?';
            for (;startPos < gameString.Length;)
            {
                // Skip whitespace
                startPos = SkipWhiteSpace(gameString, startPos);

                int nextPos = ReadActionBeginOrTerminator(gameString, startPos);
                curChar = gameString[nextPos - 1];
                if(curChar == ';' || curChar == '.')
                {
                    // Terminator.
                    if(startPos + 1 < nextPos)
                    {
                        return Error(out error, "Wrong text before terminator: " + gameString.Substring(startPos));
                    }
                    terminator = curChar;
                    return true;
                }
                PokerAction parsedAction = new PokerAction();
                Ak kind;
                if(!PokerAction.AkFromChar(curChar, out kind))
                {
                    return Error(out error, "Wrong character: " + curChar.ToString());
                }
                parsedAction.Kind = kind;
                switch(parsedAction.Kind)
                {
                    case Ak.r:
                    case Ak.c:
                    case Ak.f:
                        if (!ParsePlayerPosition(ref error, gameString.Substring(startPos, nextPos - startPos - 1), playerCount, parsedAction))
                            return false;
                        startPos = nextPos;
                        bool amountFound;
                        double amount;
                        if (!ReadAmount(gameString, ref startPos, ref error, out amountFound, out amount)) 
                            return false;
                        if (amountFound)
                        {
                            if (parsedAction.Kind == Ak.f)
                                return Error(out error, "No amount of fold is allowed.");
                            parsedAction.Amount = amount;
                        }
                        else if (parsedAction.Kind == Ak.r)
                        {
                            return Error(out error, "Amount of raise is required");
                        }
                        if(!VerifyAndAddAction(parsedAction, actions, ref error))
                            return false;
                        continue;
                    case Ak.d:
                        if (!ParsePlayerPosition(ref error, gameString.Substring(startPos, nextPos - startPos - 1), playerCount, parsedAction))
                            return false;
                        startPos = nextPos;
                        if (!ReadCards(gameString, ref startPos, ref error, parsedAction))
                            return false;
                        if (!VerifyAndAddAction(parsedAction, actions, ref error))
                            return false;
                        continue;
                    case Ak.b:
                        // This can be in calls from other classes that parse action lists.
                        startPos = nextPos;
                        if (!VerifyAndAddAction(parsedAction, actions, ref error))
                            return false;
                        break;
                    default:
                        throw new ApplicationException("Don't know how to parse action kind: " + parsedAction.Kind.ToString());
                }
            }
            return true;
        }

        private static bool VerifyAndAddAction(PokerAction parsedAction, List<PokerAction> actions, ref string error)
        {
            try
            {
                parsedAction.Verify();
            }
            catch(Exception e)
            {
                error = e.ToString();
                return false;
            }
            actions.Add(parsedAction);
            return true;
        }

        private static bool ReadCards(string gameString, ref int curPos, ref string error, PokerAction parsedAction)
        {
            curPos = SkipWhiteSpace(gameString, curPos);
            int nextPos;
            for (nextPos = curPos; nextPos < gameString.Length; ++nextPos)
            {
                switch (gameString[nextPos])
                {
                    case '{':
                        goto leftBraceFound;
                }
            }
            return Error(out error, "{ before the card list not found");
            leftBraceFound:
            curPos = nextPos + 1;

            for (nextPos = curPos; nextPos < gameString.Length; ++nextPos)
            {
                switch (gameString[nextPos])
                {
                    case '}':
                        goto rightBraceFound;
                }
            }
            return Error(out error, "} after the card list not found");
            rightBraceFound:
            parsedAction.Cards = gameString.Substring(curPos, nextPos - curPos);
            // Allow empty card lists, this is required for strategy algorithms.
            curPos = nextPos + 1;
            return true;
        }

        private static bool ReadPlayers(string gameString, ref int curPos, GameRecord gameRecord, ref string error, ref int playerCount)
        {
            for (; curPos < gameString.Length; )
            {
                curPos = SkipWhiteSpace(gameString, curPos);
                int nextPos;
                for (nextPos = curPos; nextPos < gameString.Length; ++nextPos)
                {
                    switch(gameString[nextPos])
                    {
                        case '{':
                            goto leftBraceFound;
                        case ';':
                            curPos++;
                            return true;
                    }
                }
                return Error(out error, "{ after player name not found");
            leftBraceFound:

                GameRecord.Player parsedPlayer = new GameRecord.Player();

                parsedPlayer.Name = gameString.Substring(curPos, nextPos - curPos);
                curPos = nextPos + 1;

                double amount;
                bool amountFound;

                curPos = SkipWhiteSpace(gameString, curPos);
                if (!ReadAmount(gameString, ref curPos, ref error, out amountFound, out amount))
                    return false;
                if(!amountFound)
                {
                    return Error(out error, "No stack amount found");
                }
                parsedPlayer.Stack = amount;

                curPos = SkipWhiteSpace(gameString, curPos);
                if (!ReadAmount(gameString, ref curPos, ref error, out amountFound, out amount))
                    return false;
                if (!amountFound)
                {
                    return Error(out error, "No blind amount found");
                }
                parsedPlayer.Blind = amount;

                curPos = SkipWhiteSpace(gameString, curPos);
                if (!ReadAmount(gameString, ref curPos, ref error, out amountFound, out amount))
                    return false;
                if (!amountFound)
                {
                    return Error(out error, "No result amount found");
                }
                parsedPlayer.Result = amount;

                gameRecord.Players.Add(parsedPlayer);
                playerCount++;

                for (; curPos < gameString.Length; ++curPos)
                {
                    if (gameString[curPos] == '}')
                        goto rightBraceFound;
                }
                return Error(out error, "} after player name not found");
            rightBraceFound:
                curPos++;
            }
            return Error(out error, "Players terminator not found");
        }

        private static bool ReadGameId(string gameString, ref int curPos, GameRecord gameRecord, ref string error)
        {
            curPos = SkipWhiteSpace(gameString, curPos);
            int nextPos;
            for (nextPos = curPos; nextPos < gameString.Length; ++nextPos)
            {
                switch (gameString[nextPos])
                {
                    case ';':
                        goto gameIdSeparatorFound;
                }
            }
            return Error(out error, "Game Id not found");
            gameIdSeparatorFound:

            gameRecord.Id = gameString.Substring(curPos, nextPos - curPos);
            curPos = nextPos + 1;
            return true;
        }

        private static bool ReadAmount(string gameString, ref int curPos, ref string error, out bool amountFound, out double amount)
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
            if (gameString[curPos - 1] == '.')
            {
                // Special case - game terminator (or a number like "123.", but we do not allow this.
                curPos--;
            }
            if (curPos - startPos == 0)
            {
                amountFound = false;
                return true;
            }
            string amountText = gameString.Substring(startPos, curPos - startPos);

            if (!ParseAmount(ref error, "amount", amountText, out amount))
            {
                amountFound = false;
                return false;
            }
            amountFound = true;
            return true;
        }

        private static int ReadActionBeginOrTerminator(string gameString, int curPos)
        {
            for(;curPos < gameString.Length; ++curPos)
            {
                switch(gameString[curPos])
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
                        break;
                    default:
                        return curPos + 1;
                }
            }
            return curPos + 1;
        }


        private static int SkipWhiteSpace(string gameString, int curPos)
        {
            for (; curPos < gameString.Length; ++curPos)
            {
                switch (gameString[curPos])
                {
                    case ' ':
                    case '\t':
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
                if(text[0] == '-')
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
                    if(text[i] == '.')
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

        private static bool ParsePlayerPosition(ref string error, string text, int playerCount, PokerAction action)
        {
            if(text.Length == 0)
            {
                action.Position = -1;
                return true;
            }

            try
            {
                // This is much faster as parsing with the .Net functions.
                action.Position = 0;
                for(int i = 0; i < text.Length; ++i)
                {
                    int digit = text[i] - '0';
                    if(digit < 0 || digit > 9)
                    {
                        return Error(out error, String.Format("Wrong character in number: {0}", text));
                    }
                    action.Position = action.Position * 10 + digit;
                }
            }
            catch(Exception )
            {
                return Error(out error, String.Format("Wrong player position: {1}", text));
            }
            if (action.Position >= playerCount)
            {
                return Error(out error, "Player position out of range number of players: " + text);
            }
            return true;
        }

        static bool Error(out string error, string errorValue)
        {
            error = errorValue;
            return false;
        }
    }

}