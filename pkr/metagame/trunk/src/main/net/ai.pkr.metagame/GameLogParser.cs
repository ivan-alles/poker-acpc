/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ai.pkr.metagame
{
    /// <summary>
    /// Parses game log files.
    /// </summary>
    public class GameLogParser
    {

        public delegate void OnGameRecordHandler(GameLogParser source, GameRecord gameRecord);
        public delegate void OnMetaDataHandler(GameLogParser source, string metaData);
        public delegate void OnErrorHandler(GameLogParser source, string error);

        public event OnGameRecordHandler OnGameRecord;
        public event OnMetaDataHandler OnMetaData;
        public event OnErrorHandler OnError;

        public GameLogParser()
        {
            IncludeFiles = _includeFiles;
        }

        public int CurrentLine
        {
            set;
            get;
        }

        public string CurrentFile
        {
            set;
            get;
        }

        public int GamesCount
        {
            set;
            get;
        }

        public int ErrorCount
        {
            set;
            get;
        }

        public int FilesCount
        {
            set;
            get;
        }

        public long LastFilePosition
        {
            set;
            get;
        }

        /// <summary>
        /// A regular expression applied to each file name (with absolute path).
        /// Only files that match this regular expression are parsed. Default value is ".*".
        /// </summary>
        public string IncludeFiles
        {
            set
            {
                _includeFiles = value;
                _reIncludeFiles = new Regex(_includeFiles, RegexOptions.Compiled);   
            }
            get
            {
                return _includeFiles;
            }
        }

        /// <summary>
        /// Switches on/off printing progress messages to console.
        /// </summary>
        public bool Verbose
        {
            set;
            get;
        }

        /// <summary>
        /// Parses directory or file recursively.
        /// </summary>
        public void ParsePath(string path)
        {
            string absPath = path;
            if(!Path.IsPathRooted(path))
            {
                absPath = Path.Combine(Directory.GetCurrentDirectory(), path);
            }

            FileAttributes attr = File.GetAttributes(absPath);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                ParseDir(absPath);
            else
                ParseFile(absPath);
        }

        /// <summary>
        /// Parses a directory recursively.
        /// </summary>
        public void ParseDir(string dir)
        {
            string[] files = Directory.GetFiles(dir);
            foreach (string fileName in files)
            {
                ParseFile(fileName);
            }
            string[] dirs = Directory.GetDirectories(dir);
            foreach (string childDir in dirs)
            {
                ParsePath(childDir);
            }
        }

        /// <summary>
        /// Parses a file.
        /// </summary>
        public void ParseFile(string file)
        {
            ParseFile(file, 0);
        }

        /// <summary>
        /// Parses a file from a given position.
        /// </summary>
        public void ParseFile(string file, long startPosition)
        {
            if(!_reIncludeFiles.IsMatch(file))
            {
                if (Verbose)
                {
                    Console.WriteLine("Skip file: {0}", file);
                }
                return;
            }
            if (Verbose)
            {
                Console.WriteLine("Opening game log file: {0}", file);
            }
            FilesCount++;
            CurrentFile = file;
            CurrentLine = 0;
            using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                stream.Seek(startPosition, SeekOrigin.Begin);
                using (StreamReader sr = new StreamReader(stream))
                {
                    while (!sr.EndOfStream)
                    {
                        CurrentLine++;
                        string gameString = sr.ReadLine();
                        if (_reEmptyLine.IsMatch(gameString))
                            continue;
                        if(gameString.Length > 0)
                        {
                            switch(gameString[0])
                            {
                                case COMMENT_CHAR:
                                    continue;
                                case META_DATA_CHAR:
                                    if (OnMetaData != null)
                                    {
                                        OnMetaData(this, gameString.Substring(1));
                                    }
                                    continue;
                            }
                        }
                        string error = "unknown error";
                        GameRecord gameRecord = GameRecord.Parse(gameString, out error);
                        if (gameRecord == null)
                        {
                            ProcessError(error);
                        }
                        else
                        {
                            GamesCount++;
                            if (OnGameRecord != null)
                            {
                                OnGameRecord(this, gameRecord);
                            }
                        }
                    }
                    LastFilePosition = stream.Position;
                }
            }
        }

        public string GetDefaultErrorText(string error)
        {
            return String.Format("{0}({1}): {2}", CurrentFile, CurrentLine, error);
        }


        public void ProcessError(string error)
        {
            ErrorCount++;

            if (Verbose)
            {
                Console.Error.WriteLine(GetDefaultErrorText(error));
            }
            if(OnError != null)
            {
                OnError(this, error);
            }
        }

        private readonly static Regex _reEmptyLine = new Regex(@"^\s*$", RegexOptions.Compiled);
        private string _includeFiles = ".*";
        private Regex _reIncludeFiles;

        private const char COMMENT_CHAR = '#';
        private const char META_DATA_CHAR = '>';
    }
}