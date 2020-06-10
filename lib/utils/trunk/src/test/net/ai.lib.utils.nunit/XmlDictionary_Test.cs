/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using NUnit.Framework;
using System.Xml;
using System.IO;

using StringDict = ai.lib.utils.XmlDictionary<string, string>;

namespace ai.lib.utils.nunit
{
    [TestFixture]
    public class XmlDictionary_Test
    {
        [Test]
        public void Constructors()
        {
            StringDict dict;

            // Test default constructor
            dict = new StringDict();

            // Test construction from a standard dictionary
            Dictionary<string, string> standardDict = new Dictionary<string,string>();
            standardDict["Johnny"] = "Chan";
            standardDict["David"] = "Sklansky";

            dict = new XmlDictionary<string,string>(standardDict);

            Assert.AreEqual(standardDict.Count, dict.Count);
            Assert.AreEqual(standardDict["Johnny"], dict["Johnny"]);
            Assert.AreEqual(standardDict["David"], dict["David"]);

            // Test construction from equality comparer.
            dict = new StringDict(standardDict.Comparer);
            Assert.AreEqual(standardDict.Comparer, dict.Comparer);

            // Constructor with capacity
            dict = new StringDict(5);

            // IDictionary and comparer
            dict = new StringDict(standardDict, standardDict.Comparer);
            Assert.AreEqual(standardDict.Comparer, dict.Comparer);

            // Comparer and capacity
            dict = new StringDict(10, standardDict.Comparer);
            Assert.AreEqual(standardDict.Comparer, dict.Comparer);
        }

        [Test]
        public void GetSchema()
        {
            StringDict dict = new StringDict();
            Assert.AreEqual(null, dict.GetSchema());
        }


        [Test]
        public void Serialization_and_deserialization()
        {
            StringDict dict = new StringDict();
            TestSerialization(dict); // Test an empty dictionary
            dict["Donald"] = "Duck";
            dict["John"] = "Lennon";
            TestSerialization(dict); // Test a non-empty dictionary
        }

        private static void TestSerialization(StringDict dict)
        {
            MemoryStream ms = new MemoryStream();
            XmlSerializer xs = new XmlSerializer(dict.GetType());
            xs.Serialize(ms, dict);
            ms.Seek(0, SeekOrigin.Begin);
            XmlSerializer xd = new XmlSerializer(dict.GetType());
            StringDict dict1 = (StringDict)xd.Deserialize(ms);
            Assert.AreEqual(dict, dict1);
        }
    }

}
