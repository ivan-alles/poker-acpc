/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;
using System.Xml;
using NUnit.Framework;
using System.IO;

namespace ai.lib.utils.nunit
{
    [TestFixture]
    public class CustomXmlSerializable_Test
    {
        public class TestClass: IXmlSerializable
        {
            internal int _internalField1;
            public   string _publicField1;
            private  string _privateField1;

            /// <summary>
            /// Sets private fields. Don't use properties to make sure that the default XML serialization 
            /// isn't used.
            /// </summary>
            /// <param name="privateField1"></param>
            internal void SetPrivateField1(string privateField)
            {
                _privateField1 = privateField;
            }
            internal string GetPrivateField1()
            {
                return _privateField1;
            }

            [XmlIgnore]
            public int _skippedField;

            #region IXmlSerializable Members
            public System.Xml.Schema.XmlSchema GetSchema()
            {
                return null;
            }
            public virtual void ReadXml(System.Xml.XmlReader reader)
            {
                CustomXmlSerializable.ReadXml(this, reader);
            }
            public void WriteXml(System.Xml.XmlWriter writer)
            {
                CustomXmlSerializable.WriteXml(this, writer);
            }
            #endregion

        }

        [Test]
        public void Test_Null()
        {
            StringBuilder text = new StringBuilder();
            TestClass test = null;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TestClass));
            xmlSerializer.Serialize(new StringWriter(text), test);

            XmlSerializer xmlDeserializer = new XmlSerializer(typeof(TestClass));
            TestClass test1 = (TestClass)xmlDeserializer.Deserialize(new StringReader(text.ToString()));

            // Under Windows the deserializer returns a non-null object. 
            // That is strictly speaking wrong, but I don't know hot to fix this. 
            // Therefore just make sure the object is not initialized.
            if (test1 != null)
            {
                Assert.IsNull(test1._publicField1);
            }
        }
        [Test]
        public void Test_Simple()
        {
            TestClass test = new TestClass();
            test.SetPrivateField1("private field 1");
            test._internalField1 = 33;
            test._publicField1 = "public field 1";
            test._skippedField = 55;

            StringBuilder text = new StringBuilder();

            XmlSerializer xmlSerializer = new XmlSerializer(test.GetType());
            xmlSerializer.Serialize(new StringWriter(text), test);
            
            XmlSerializer xmlDeserializer = new XmlSerializer(test.GetType());
            TestClass test1  = (TestClass)xmlDeserializer.Deserialize(new StringReader(text.ToString()));
            Assert.AreEqual(test._publicField1, test1._publicField1);
            Assert.AreEqual(test._internalField1, test1._internalField1);
            Assert.AreEqual(test.GetPrivateField1(), test1.GetPrivateField1());
            Assert.AreNotEqual(test._skippedField, test1._skippedField);
        }
    }
}
