/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
namespace ai.lib.utils
{
    /// <summary>
    /// Is used by XmlSerializerExt to pass parameters to ConstructFromXml.
    /// </summary>
    public class ConstructFromXmlParams
    {
        public ConstructFromXmlParams()
        {
            Local = new Props();
        }

        /// <summary>
        /// If the object is deserialized from a file, contains the original 
        /// XML file name, converted to absolute path. Using of the absolute path
        /// allows resolving references later on even if the current directory changes.
        /// 
        /// If the object is deserialized from a stream, is set to null.
        /// </summary>
        public string XmlFile
        {
            set;
            get;
        }

        /// <summary>
        /// Contains a copy of Props.Global with additional properties:
        /// <para>
        /// loc.xml.File: same as the property XmlFile.
        /// </para>
        /// <para>
        /// loc.xml.Dir: absolute directory of the parent XML file. Contains empty string 
        /// if XML is not loaded from a file.</para>
        /// </summary>
        public Props Local
        {
            private set;
            get;
        }
    }
}