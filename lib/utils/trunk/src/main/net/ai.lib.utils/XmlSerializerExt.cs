/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Reflection;

namespace ai.lib.utils
{

    /// <summary>
    /// Helpers for XML serialization and deserialization.
    /// <remarks>
    /// Deserialization:
    /// If a class has a method ConstructFromXml(ConstructFromXmlParams parameters),
    /// it will be called by deserialization methods. This can be used to finalize XML deserialization,
    /// e.g. load other objects referenced by file name, etc. 
    /// If deserialized by file name, a relative path is converted to an absolute path using current directory.
    /// No path resolution is done.
    /// </remarks>
    /// </summary>
    public static class XmlSerializerExt
    {
        #region Serialization to text stream or file
        public static void Serialize<T>(T obj, TextWriter tw)
        {
            XmlWriterSettings s = new XmlWriterSettings();
            s.Indent = true;
            Serialize(obj, tw, s);
        }

        public static void Serialize<T>(T obj, string fileName)
        {
            XmlWriterSettings s = new XmlWriterSettings();
            s.Indent = true;
            Serialize(obj, fileName, s);
        }

        public static void Serialize<T>(T obj, string fileName, XmlWriterSettings settings)
        {
            using(TextWriter tw = new StreamWriter(fileName))
            {
                Serialize(obj, tw, settings);
                tw.Close();
            }
        }

        public static void Serialize<T>(T obj, TextWriter tw, XmlWriterSettings settings)
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            using (XmlWriter wr = XmlWriter.Create(tw, settings))
            {
                s.Serialize(wr, obj);
                wr.Close();
            }
        }

        #endregion

        #region Serialization to text stream or file as extension methods

        public static void XmlSerialize<T>(this T obj, TextWriter tw)
        {
            Serialize(obj, tw);
        }

        public static void XmlSerialize<T>(this T obj, string fileName)
        {
            Serialize(obj, fileName);
        }

        public static void XmlSerialize<T>(this T obj, string fileName, XmlWriterSettings settings)
        {
            Serialize(obj, fileName, settings);
        }

        public static void XmlSerialize<T>(this T obj, TextWriter tw, XmlWriterSettings settings)
        {
            Serialize(obj, tw, settings);
        }

        #endregion

        #region Serializaton to binary stream 

        /// <summary>
        /// XML-serializes an object into a binary stream.
        /// </summary>
        public static void SerializeBin<T>(T obj, BinaryWriter w)
        {
            MemoryStream ms = new MemoryStream(10000);
            XmlWriterSettings ws = new XmlWriterSettings { Indent = false };
            using (TextWriter tw = new StreamWriter(ms))
            {
                XmlSerializerExt.Serialize(obj, tw, ws);
            }
            byte[] xmlData = ms.ToArray();
            w.Write(xmlData.Length);
            w.Write(xmlData);
        }

        #endregion

        #region Serializaton to binary stream as extension method

        public static void XmlSerializeBin<T>(this T obj, BinaryWriter w)
        {
            SerializeBin(obj, w);
        }

        #endregion

        #region Deserialization from text stream or file

        public static void Deserialize<T>(out T obj, TextReader tr)
        {
            DeserializeImpl(out obj, tr, new ConstructFromXmlParams());
        }

        public static void Deserialize<T>(out T obj, string fileName)
        {
            string absolutePath = fileName;
            if(!Path.IsPathRooted(absolutePath))
            {
                absolutePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            }
            using (TextReader tr = new StreamReader(absolutePath))
            {
                ConstructFromXmlParams p = new ConstructFromXmlParams
                                                 {
                                                     XmlFile = absolutePath
                                                 };
                p.Local.UpdateFrom(Props.Global);
                p.Local.Set("loc.xml.File", p.XmlFile);
                p.Local.Set("loc.xml.Dir", Path.GetDirectoryName(absolutePath) + Path.DirectorySeparatorChar);
                DeserializeImpl(out obj, tr, p);
                tr.Close();
            }
        }

        public static T Deserialize<T>(string fileName)
        {
            T obj;
            Deserialize(out obj, fileName);
            return obj;
        }

        public static T Deserialize<T>(TextReader tr)
        {
            T obj;
            Deserialize(out obj, tr);
            return obj;
        }

        #endregion

        #region Deserialization from binary stream 

        /// <summary>
        /// XML-deserializes an object from a binary stream.
        /// </summary>
        public static T DeserializeBin<T>(BinaryReader r)
        {
            Int32 xmlDataLength = r.ReadInt32();
            byte[] xmlData = new byte[xmlDataLength];
            r.Read(xmlData, 0, xmlDataLength);
            MemoryStream ms = new MemoryStream(xmlData);
            T obj;
            using (TextReader tr = new StreamReader(ms))
            {
                obj = XmlSerializerExt.Deserialize<T>(tr);
            }
            return obj;
        }

        #endregion


        #region Implementaion

        static void DeserializeImpl<T>(out T obj, TextReader tr, ConstructFromXmlParams parameters)
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            obj = (T)s.Deserialize(tr);
            CallConstructFromXml(obj, parameters);
        }

        static void CallConstructFromXml<T>(T obj, ConstructFromXmlParams parameters)
        {
            try
            {

                MethodInfo methodInfo = typeof (T).GetMethod("ConstructFromXml");
                if (methodInfo != null)
                {
                    methodInfo.Invoke(obj, new object[] {parameters});
                }
            }
            catch (Exception e)
            {
                string xmlFile = parameters.XmlFile ?? "null";
                // Create an exception wrapper containing information about the type name
                // because this is a very important information to analyze the problem and
                // this information is usually missing in the original exception.
                ApplicationException excWrapper = new ApplicationException(
                    String.Format("Type: {0}, file: {1}: CallConstructFromXml() failed, see inner exception for details.", 
                    typeof (T).Name, xmlFile), e);
                throw excWrapper;
            }
        }

        #endregion
    }
}
