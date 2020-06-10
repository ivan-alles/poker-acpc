/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace ai.lib.utils
{

    /// <summary>
    /// Contains a map containing properties and their values. 
    /// <para>
    /// Can replaces a text containing properties with their values. Use the following format:
    /// ${name}, varname must not be empty can contains letters, digits, and charachters '-', '_', '.'.
    /// Names of properties are case-sensitive, it is recommended to use PascalCasing.
    /// </para>
    /// <para>
    /// Static property Props.Global contains global instance of Props class used throughout the entire application.
    /// It is initialized with standard properties (see SetStandardProps()).
    /// </para>
    /// <para>
    /// The user can overwrite standard properties and define new properties in run-time 
    /// from a command-line, profile files or via API. This is a very powerful way of 
    /// parametrizing and customizing applications.
    /// </para>
    /// <para>
    /// If the object is loaded from XML, the following is added to the properies:
    /// ${loc.xml.Dir} : directory of the original XML file
    /// ${loc.xml.File} : full name and path of the original XML file
    /// ${loc.xml.FileName} : file name without extension and path
    /// ${loc.xml.FileExt} : file name without extension and path
    /// This allows to reference other files using XML file location as base path: 
    /// &lt;p n="SomeFileName" v="${loc.xml.Dir}file-name.txt" /&gt;
    /// </para>
    /// </summary>
    [Serializable]
    public class Props: IXmlSerializable
    {
        #region Static members
        /// <summary>
        /// Global instance used by all applications.
        /// </summary>
        public static Props Global
        {
            get;
            private set;
        }

        /// <summary>
        /// Implicitely initializes from a dictionary.
        /// </summary>
        public static implicit operator Props(Dictionary<string, string> dict)
        {
            Props temp = new Props(dict);
            return temp;
        }

        /// <summary>
        /// Implicitely initializes from an array of strings.
        /// </summary>
        public static implicit operator Props(string [] arr)
        {
            Props temp = new Props(arr);
            return temp;
        }

        #endregion

        #region Public constructors

        public Props()
        {
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public Props(Props other)
        {
            UpdateFrom(other);
        }

        /// <summary>
        /// Copies properties from a dictionary.
        /// </summary>
        public Props(Dictionary<string, string> dict)
        {
            _props = new Dictionary<string, string>(dict);
        }

        /// <summary>
        /// Copies properties from an array of strings, format {name1, value1, name2, value2, ...}.
        /// </summary>
        public Props(string [] arr)
        {
            if(arr.Length % 2 != 0)
            {
                throw new ApplicationException(String.Format("Number of key/value pairs must be even, was {0}",
                                                             arr.Length));
            }
            for(int i = 0; i < arr.Length; i+= 2)
            {
                _props[arr[i]] = arr[i+1];
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Number of properties in the map.
        /// </summary>
        public int Count
        {
            get { return _props.Count; }
        }

        /// <summary>
        /// Returns a copy of all names of the properties.
        /// </summary>
        public string[] Names
        {
            get { return _props.Keys.ToArray(); }
        }

        /// <summary>
        /// Sets a property. Existing will be rewritten. To erase, set to null.
        /// </summary>
        public void Set(string prop, string val)
        {
            lock (thisLock)
            {
                _props[prop] = val;
            }
        }

        /// <summary>
        /// Gets an expanded value of a property. If is is not set, returns null.
        /// </summary>
        public string Get(string prop)
        {
            return Expand(GetRaw(prop));
        }

        /// <summary>
        /// Gets an expanded value of a property. If is is not set, 
        /// the expanded default value is returned.
        /// </summary>
        public string GetDefault(string prop, string defaultValue)
        {
            return Expand(GetRawDefault(prop, defaultValue));
        }

        /// <summary>
        /// Gets the unexpanded value of a property. If it is not set, returns null.
        /// </summary>
        public string GetRaw(string prop)
        {
            string result = null;
            lock (thisLock)
            {
                _props.TryGetValue(prop, out result);
            }
            return result;
        }

        /// <summary>
        /// Gets the unexpanded value of a property. If is is not set, 
        /// the unexpanded default value is returned.
        /// </summary>
        public string GetRawDefault(string prop, string defaultValue)
        {
            string value = Get(prop);
            return value == null ? defaultValue : value;
        }

        /// <summary>
        /// Expands text containing properties with their values.
        /// If there is no such a property or the value is null, the property is not replaced.
        /// </summary>
        public string Expand(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            lock (thisLock)
            {
                // Do not use static regular expression because of multithreading and
                // recursive calls.
                Replacer replacer = new Replacer {_parent = this};
                return ExpandInternal(text, replacer);
            }
        }

        /// <summary>
        /// Expands text containing properties with their values.
        /// If there is no such a property or the value is null, the property is not replaced.
        /// Additionally replaces properties like ${0}, ${1}, etc. with the corresponding
        /// textual representations from parameters.
        /// </summary>
        public string Expand(string text, params object[] parameters)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            lock (thisLock)
            {
                // Do not use static regular expression because of multithreading and
                // recursive calls.
                Replacer replacer = new Replacer { _parent = this, Params = parameters };
                return ExpandInternal(text, replacer);
            }
        }

        /// <summary>
        /// Sets standard properties. The current values will be cleared first.
        /// <para>Standard properties are:</para>
        /// <para>Environment (prefix env), see UpdateEnvironmentVariables().</para>
        /// <para>Run-time: </para>
        /// <para>- ai.Root: root folder of ai structure. Default: one folder up from the codebase directory of this assembly.</para>
        /// <para>- ai.InitCD: current directory. Default: current directory in the moment 
        ///         when this class is first accessed. Usually it happens early because the command line parser 
        ///         will set properties at the programm startup.</para>
        /// <para>- bds.BinDir: BDS bin directory.</para>
        /// <para>- bds.TestDir: BDS directory for test resources.</para>
        /// <para>- bds.CfgDir: BDS config directory.</para>
        /// <para>- bds.DataDir: BDS data directory.</para>
        /// <para>- bds.VarDir: BDS var directory (for logs, etc).</para>
        /// <para>- bds.TestOutputDir: root directory for test output.</para>
        /// <para>- bds.TraceDir: root directory for trace files.</para>
        /// <para>
        /// All directories end with directory separator character by default. This is not guaranteed for env namespace
        /// and if the user overwrites a standard variable.</para>
        /// </summary>
        public void SetStandardProps()
        {
            lock (thisLock)
            {
                _props.Clear();
                UpdateEnvironmentVariables();

                // Add run-time properties:
                string codeBase = CodeBase.Get();
                string rootPath = Path.Combine(Path.GetDirectoryName(codeBase), ".." + Path.DirectorySeparatorChar);
                if (EnvironmentExt.IsUnix() && !Path.IsPathRooted(rootPath))
                {
                    // Fix for mono under linux, it returns a path from root, but without '/' at the
                    // beginning.
                    rootPath = '/' + rootPath;
                }

                Set("ai.Root", rootPath);
                Set("ai.InitCD", Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar);
                Set("ai.DirSep", new string(Path.DirectorySeparatorChar, 1));

                Set("bds.BinDir", "${ai.Root}" + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar);
                Set("bds.TestDir", "${ai.Root}" + Path.DirectorySeparatorChar + "test" + Path.DirectorySeparatorChar);
                Set("bds.CfgDir", "${ai.Root}" + Path.DirectorySeparatorChar + "cfg" + Path.DirectorySeparatorChar);
                Set("bds.DataDir", "${ai.Root}" + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar);
                Set("bds.VarDir", "${ai.Root}" + Path.DirectorySeparatorChar + "var" + Path.DirectorySeparatorChar);
                Set("bds.TestOutputDir", "${bds.VarDir}" + Path.DirectorySeparatorChar + "test-output" + Path.DirectorySeparatorChar);
                Set("bds.TraceDir", "${bds.VarDir}" + Path.DirectorySeparatorChar + "trace" + Path.DirectorySeparatorChar);
            }
        }

        /// <summary>
        /// Re-reads environment properties. 
        /// Call it to explicitely update the properties.
        /// For the sake of determinism, the environment property names will be normalized to 
        /// upper case on platforms with case-insensitive property lookup. 
        /// </summary>
        public void UpdateEnvironmentVariables()
        {
            lock (thisLock)
            {
                // Delete properties with prefix env. from the _props.
                List<string> envKeys = new List<string>(_props.Count);
                foreach (string key in _props.Keys)
                {
                    if (key.StartsWith("env."))
                        envKeys.Add(key);
                }
                foreach (string key in envKeys)
                {
                    _props.Remove(key);
                }

                // Now add environment properties.
                IDictionary envDict = Environment.GetEnvironmentVariables();
                foreach (object key in envDict.Keys)
                {
                    object value = envDict[key];
                    string keyStr = key.ToString();
                    keyStr = keyStr.ToUpperInvariant();
                    _props["env." + keyStr] = value.ToString();
                }
            }
        }

        /// <summary>
        /// Merges update with this object. If a property exists in both maps, 
        /// update overwrites. 
        /// </summary>
        /// <param name="update"></param>
        public void UpdateFrom(Props update)
        {
            foreach (KeyValuePair<string, string> kvp in update._props)
            {
                _props[kvp.Key] = kvp.Value;
            }
        }

        #endregion

        #region IXmlSerializable Members

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            while (reader.MoveToNextAttribute())
            {
                if (reader.Name == "XmlMergeWithGlobal")
                {
                    _xmlMergeWithGlobal = bool.Parse(reader.Value);
                }
                else
                {
                    XmlError("Unknown attribute", reader.Name);
                }
            }

            reader.Read();
            if (wasEmpty)
                return;
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        string key = "";
                        string value = "";

                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "n")
                            {
                                key = reader.Value;
                            }
                            else if (reader.Name == "v")
                            {
                                value = reader.Value;
                            }
                            else
                            {
                                XmlError("Unknown attribute", reader.Name);
                            }
                        }
                        _props.Add(key, value);
                        reader.MoveToContent();
                        reader.Read();
                        break;
                    default:
                        // Skip comments and pray this is correct.
                        reader.MoveToContent();
                        break;
                }
            }
            reader.ReadEndElement();

            // Do it here (not in xml-constructor), in case we are reading an element 
            // of this type in another XML file.
            if (XmlMergeWithGlobal)
            {
                MergeWithGlobal();
            }
        }

        private void XmlError(string error, string wrongParam)
        {
            throw new ApplicationException(String.Format("{0} {1}",
                                                         error, wrongParam));
        }

        public void WriteXml(XmlWriter writer)
        {
            // Do not write default value.
            if (XmlMergeWithGlobal)
            {
                writer.WriteAttributeString("XmlMergeWithGlobal", _xmlMergeWithGlobal.ToString());
            }
            foreach (KeyValuePair<string, string> kvp in _props)
            {
                writer.WriteStartElement("p");
                writer.WriteAttributeString("n", kvp.Key);
                writer.WriteAttributeString("v", kvp.Value);
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Determines if this object will be merged with global properties (Props.Global) 
        /// after XML deserialization. During merging, if any property exists in both objects, this object
        /// overwrites.
        /// <para>
        /// Default value is false, because:</para>
        /// <para>
        /// 1. There are many use-cases that requires that, for example, passing this props to 
        /// a remote computer with its own global properties.</para>
        /// <para>
        /// 2. If in a particular case it is required to always merge,
        /// in can be easily done in code. But the reverse (un-merging) is not possible.
        /// </para>
        /// </summary>
        public bool XmlMergeWithGlobal
        {
            set
            {
                _xmlMergeWithGlobal = value;
            }
            get
            {
                return _xmlMergeWithGlobal;
            }
        }

        public void ConstructFromXml(ConstructFromXmlParams parameters)
        {
            // This allows to use path to xml file in the values of properties.
            string inheritedProp;
            inheritedProp = "loc.xml.File";
            string file = parameters.Local.GetRaw(inheritedProp);
            Set(inheritedProp, file);
            Set("loc.xml.FileName", Path.GetFileNameWithoutExtension(file));
            Set("loc.xml.FileExt", Path.GetExtension(file));
            inheritedProp = "loc.xml.Dir";
            Set(inheritedProp, parameters.Local.GetRaw(inheritedProp));
        }

        #endregion

        #region Implementation

        class Replacer
        {
            internal Props _parent;
            internal string OnMatch(Match match)
            {
                string propName = match.Groups[1].Value;
                string value;
                if (Params != null)
                {
                    int paramIdx;
                    if (int.TryParse(propName, out paramIdx))
                    {
                        // Property like ${0}
                        return _parent.ExpandInternal(Params[paramIdx].ToString(), this);
                    }
                }
                if (!_parent._props.TryGetValue(propName, out value) || value == null)
                {
                    // Return initial piece of text that looks like a property
                    return match.Groups[0].Value;
                }
                return _parent.ExpandInternal(value, this);
            }
            internal object [] Params;
        }

        static Props()
        {
            Global = new Props();
            Global.SetStandardProps();
        }

        private string ExpandInternal(string text, Replacer replacer)
        {
            string result = Regex.Replace(text, _reProp, replacer.OnMatch);
            return result;
        }

        private void MergeWithGlobal()
        {
            Props tmp = new Props(Props.Global);
            tmp.UpdateFrom(this);
            _props = tmp._props;
        }

        private Dictionary<string, string> _props = new Dictionary<string, string>();
        static private Object thisLock = new Object();
        static string _reProp = @"\$\{([^$)]+)\}";

        private bool _xmlMergeWithGlobal = false;


        #endregion

    }
}
