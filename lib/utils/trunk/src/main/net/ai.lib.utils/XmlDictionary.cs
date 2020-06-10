/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;

namespace ai.lib.utils
{
    /// <summary>
    /// A dictionary that can be XML-serialized.
    /// </summary>
    [Serializable]
    public class XmlDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public XmlDictionary()
            : base()
        {
        }
        public XmlDictionary(IDictionary<TKey, TValue> dictionary)
            : base(dictionary)
        {
        }
        public XmlDictionary(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
        }
        public XmlDictionary(int capacity)
            : base(capacity)
        {
        }
        public XmlDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : base(dictionary, comparer)
        {
        }
        public XmlDictionary(int capacity, IEqualityComparer<TKey> comparer)
            : base(capacity, comparer)
        {
        }
        protected XmlDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey), _keyRootAttr);
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), _valRootAttr);
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
                return;
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                TKey key = (TKey)keySerializer.Deserialize(reader);
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                this.Add(key, value);
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey), _keyRootAttr);
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), _valRootAttr);
            foreach (KeyValuePair<TKey, TValue> kvp in this)
            {
                keySerializer.Serialize(writer, kvp.Key);
                valueSerializer.Serialize(writer, kvp.Value);
            }
        }
        #endregion

        private static XmlRootAttribute _keyRootAttr = new XmlRootAttribute("k");
        private static XmlRootAttribute _valRootAttr = new XmlRootAttribute("v");

    }
}
