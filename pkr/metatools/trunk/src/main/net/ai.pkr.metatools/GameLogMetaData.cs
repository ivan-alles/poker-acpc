/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ai.pkr.metatools
{
    /// <summary>
    /// Parses log meta data.
    /// </summary>
    public class GameLogMetaData
    {
        private readonly Dictionary<string, string> _properties = new Dictionary<string, string>();

        private static Regex _reName = new Regex("^\\s*([^\\s]+)", RegexOptions.Compiled);
        private static Regex _reProp = new Regex("\\s+([^\\s]+)\\s*=\\s*'([^']*)'", RegexOptions.Compiled); 
        

        public static GameLogMetaData Parse(string logString)
        {
            Match m = _reName.Match(logString);
            if (!m.Success)
                return null;
            GameLogMetaData result = new GameLogMetaData();
            result.Name = m.Groups[1].Value;

            string propsString = logString.Substring(m.Length);

            m = _reProp.Match(propsString);
            while(m.Success)
            {
                string prop = m.Groups[1].Value;
                string val = m.Groups[2].Value;
                result.Properties[prop] = val;
                m = m.NextMatch();
            }

            return result;
        }

        public string Name
        {
            set; get;
        }

        public Dictionary<string, string> Properties
        {
            get { return _properties;}
        }
    }
}
