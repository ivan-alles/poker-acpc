/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace ai.lib.algorithms.convert
{
    /// <summary>
    /// A helper class for algorithms that dump binary data to/from plain line-based text files.
    /// </summary>
    public static class TextDumpHelper
    {
        private static Regex _reComment = new Regex("^\\s*#", RegexOptions.Compiled);
        private static Regex _reWhitespace = new Regex("^\\s*$", RegexOptions.Compiled);
        private static char[] _separators = new char[] { ' ', '\t' };

        /// <summary>
        /// Converts a double to a bin string (without loss of precision).
        /// </summary>
        public static string DoubleToBinString(double d)
        {
            Int64 i = BitConverter.DoubleToInt64Bits(d);
            string s = i.ToString("X");
            return s;
        }

        /// <summary>
        /// Converts a bin-doble string to double (without loss of precision).
        /// </summary>
        public static double BinStringToDouble(string s)
        {
            Int64 i = Int64.Parse(s, NumberStyles.AllowHexSpecifier);
            double d = BitConverter.Int64BitsToDouble(i);
            return d;
        }

        /// <summary>
        /// Reads the value of the tag with the expected name.
        /// </summary>
        public static string ReadTag(TextReader r, ref int lineNumber, string tag)
        {
            string line = ReadLine(r, ref lineNumber);
            string actTag, value;
            Split(line, out actTag, out value);
            if (actTag != tag)
            {
                throw new ApplicationException(String.Format("Line {0}: wrong tag '{0}', expected '{1}'", lineNumber, actTag, tag));
            }
            return value;
        }

        /// <summary>
        /// Splitss the line to tag and value.
        /// </summary>
        public static void Split(string line, out string tag, out string value)
        {
            int p = line.IndexOfAny(_separators);
            tag = line.Substring(0, p);
            value = line.Substring(p + 1).TrimStart(_separators);
        }

        /// <summary>
        /// Reads a line, skips comments and whitespace.
        /// </summary>
        public static string ReadLine(TextReader r, ref int lineNumber)
        {
            for(;;)
            {
                string line  = r.ReadLine();
                lineNumber++;
                if(line == null || !(_reComment.IsMatch(line) || _reWhitespace.IsMatch(line)))
                {
                    return line;
                }
            }
        }
    }
}