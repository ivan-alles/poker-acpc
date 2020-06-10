/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

namespace ai.lib.algorithms
{
    /// <summary>
    /// Helps formatting evaluated expressions.
    /// </summary>
    public class ExprFormatter
    {
        public ExprFormatter(string eval, string format)
        {
            Expr = eval;
            Format = format;
        }

        /// <summary>
        /// Creates a new instance from a descriptor string in the following format:
        /// &lt;expr&gt;[;&lt;fmt&gt;], for example: "s[d].Node.GameState.Round;r:{1}"
        /// If format is ommitted, the default format "{0}:{1}" is used.
        /// </summary>
        public ExprFormatter(string descriptor)
        {
            Expr = descriptor;
            int sep = descriptor.IndexOf(';');
            if (sep != -1)
            {
                Expr = descriptor.Substring(0, sep);
                if (sep < descriptor.Length - 1)
                {
                    Format = descriptor.Substring(sep + 1);
                }
            }
        }

        public string Expr
        {
            set;
            get;
        }

        public string Format
        {
            set
            {
                _format = value;
            }
            get
            {
                return _format;
            }
        }

        #region Implementation
        string _format = "{0}:{1}";
        #endregion
    }
}