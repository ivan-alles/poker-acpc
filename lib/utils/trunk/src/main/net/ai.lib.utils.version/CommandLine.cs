/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using ai.lib.utils.commandline;

namespace ai.lib.utils.version
{
    /// <summary>
    /// Command line. 
    /// </summary>
    class CommandLine
    {
        #region File parameters

        [DefaultArgument(ArgumentType.Multiple | ArgumentType.Required, LongName = "file",
            HelpText = "File(s) to process.")] 
        public string[] Files = null;

        #endregion

        #region Options

        [Argument(ArgumentType.AtMostOnce, LongName = "set-user-descr", ShortName = "u",
        DefaultValue = null, HelpText = "Sets user description.")]
        public string SetUserDescr = null;


        [Argument(ArgumentType.AtMostOnce, LongName = "show-names", ShortName = "n",
            DefaultValue = false, HelpText = "Print file names.")]
        public bool PrintFileNames = false;

        #endregion
    }
}