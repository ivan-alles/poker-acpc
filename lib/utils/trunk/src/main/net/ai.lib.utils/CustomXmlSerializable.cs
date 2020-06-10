/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace ai.lib.utils
{
    // A helper class to implement IXmlSerializable.
    public class CustomXmlSerializable
    {
        public static void ReadXml(object theObject, XmlReader reader)
        {
            // Now we are at the 1st element for our type, which is read by XmlSerializer.
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
                return;

            FieldInfo[] fields = theObject.GetType().GetFields(BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                if (SkipField(field))
                    continue;
                XmlSerializer serializer = new XmlSerializer(field.FieldType);
                object value = serializer.Deserialize(reader);
                field.SetValue(theObject, value);
            }
            reader.MoveToContent();
            // We have to read closing element for the type, XmlSerializer will not do it.
            reader.ReadEndElement(); 
        }

        public static void WriteXml(object theObject, XmlWriter writer)
        {
            FieldInfo[] fields = theObject.GetType().GetFields(BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                if (SkipField(field))
                    continue;
                XmlSerializer serializer = new XmlSerializer(field.FieldType);
                object value = field.GetValue(theObject);
                serializer.Serialize(writer, value);
            }
        }
        private static bool SkipField(FieldInfo fieldInfo)
        {
            Object[] attributes = fieldInfo.GetCustomAttributes(typeof(XmlIgnoreAttribute), false);
            return attributes.Length != 0;
        }
    }
}
