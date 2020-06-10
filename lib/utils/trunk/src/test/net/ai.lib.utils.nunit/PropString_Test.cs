/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;

namespace ai.lib.utils.nunit
{
    /// <summary>
    /// Unit tests for PropString. 
    /// </summary>
    [TestFixture]
    public class PropString_Test
    {
        #region Tests

        [Test]
        public void Test_Value()
        {
            PropString ps = new PropString();
            ps.RawValue = "${ai.Root}";
            Assert.AreEqual(Props.Global.Get("ai.Root"), ps.Get(Props.Global));

            ps.RawValue = "";
            Assert.AreEqual("", ps.Get(Props.Global));

            ps.RawValue = null;
            Assert.AreEqual("", ps.Get(Props.Global));

        }


        [Test]
        public void Test_Xml()
        {
            XmlData xd1 = new XmlData();

            xd1.Field1 = new PropString { RawValue = "${ai.Root}_Field1" };
            xd1.Field2 = new PropString { RawValue  = null };
            xd1.Field3 = new PropString { RawValue = ""};

            xd1.ArrayField1 = new PropString[]
                                  {
                                      new PropString {RawValue = "${ai.Root}_ArrayField1.1"},
                                      new PropString {RawValue = "${ai.Root}_ArrayField1.2"},
                                      new PropString {RawValue = ""},
                                      new PropString {RawValue = null},
                                  };

            StringBuilder sb = new StringBuilder();
            using (TextWriter tw = new StringWriter(sb))
            {
                xd1.XmlSerialize(tw);
            }

            Console.WriteLine(sb.ToString());

            XmlData xd2;

            using (TextReader textReader = new StringReader(sb.ToString()))
            {
                XmlSerializerExt.Deserialize(out xd2, textReader);
            }

            Assert.IsNotNull(xd2);
            Assert.AreEqual(xd1.Field1.RawValue, xd2.Field1.RawValue, "Field1");
            Assert.AreEqual(xd1.Field2.RawValue, xd2.Field2.RawValue, "Field2");

            // Under mono serialization a a bit different and this field is set to null.
            // Therefore allow both
            Assert.IsTrue(string.IsNullOrEmpty(xd2.Field3.RawValue), "Field3");

            for (int i = 0; i < xd1.ArrayField1.Length; i++)
            {
                // Check for mono
                if (string.IsNullOrEmpty(xd1.ArrayField1[i].RawValue))
                {
                    Assert.IsTrue(string.IsNullOrEmpty(xd2.ArrayField1[i].RawValue));
                }
                else
                {
                    Assert.AreEqual(xd1.ArrayField1[i].RawValue, xd2.ArrayField1[i].RawValue);
                }
            }
        }

        #endregion

        #region Benchmarks
        #endregion

        #region Implementation

        public class XmlData
        {
            public PropString Field1;

            public PropString Field2;

            public PropString Field3;

            public PropString[] ArrayField1;
        }

        #endregion
    }
}
