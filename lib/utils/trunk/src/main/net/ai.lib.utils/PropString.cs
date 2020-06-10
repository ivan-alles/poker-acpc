/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace ai.lib.utils
{
    /// <summary>
    /// An expandable property of type string. Use it instead of simple strings 
    /// in configuration classes to point out that properties can be used, for
    /// instance for file names.
    /// When XML-serialized, only RawValue value is actually saved/loaded.
    /// </summary>
    /// <seealso cref="Props"/>
    [Serializable]
    public class PropString: IXmlSerializable
    {
        public static implicit operator PropString(string rawValue)
        {
            PropString temp = new PropString(rawValue);
            return temp;
        }

        public PropString()
        {
        }

        public PropString(string rawValue)
        {
            RawValue = rawValue;
        }

        /// <summary>
        /// Returns the value expanded by props. null is always replaced by "".
        /// </summary>
        public string Get(Props props)
        {
            return props.Expand(RawValue) ?? "";
        }

        /// <summary>
        /// Returns the value expanded by Props.Global. null is always replaced by "".
        /// </summary>
        public string Get()
        {
            return Props.Global.Expand(RawValue) ?? "";
        }

        /// <summary>
        /// Returns the value expanded by Props.Global. null is always replaced by "".
        /// </summary>
        public static implicit operator string(PropString ps)
        {
            return ps.Get();
        }

        /// <summary>
        /// The raw, unexpanded value.
        /// </summary>
        public string RawValue
        {
            set;
            get;
        }

        #region IXmlSerializable Members

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }
            reader.MoveToContent();
            RawValue = reader.ReadContentAsString();
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(RawValue);
        }
        #endregion
    }
}
