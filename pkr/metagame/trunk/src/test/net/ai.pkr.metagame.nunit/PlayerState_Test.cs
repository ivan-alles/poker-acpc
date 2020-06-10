/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using ai.lib.utils;

namespace ai.pkr.metagame.nunit
{
    [TestFixture]
    public class PlayerState_Test
    {
        [Test]
        public void Test_Equals()
        {
            PlayerState ps1 = new PlayerState();
            Assert.IsTrue(ps1.Equals(ps1));
            Assert.IsFalse(ps1.Equals(null));
            Assert.IsFalse(ps1.Equals(4));
        }

        [Test]
        public void Test_XmlSerialization()
        {
            PlayerState ps1 = new PlayerState();
            ps1.Stack = 100;
            ps1.Bet = 23;
            ps1.Hand = "Ah Ad";

            StringBuilder sb = new StringBuilder();
            using (TextWriter tw = new StringWriter(sb))
            {
                ps1.XmlSerialize(tw);
            }

            Console.WriteLine(sb.ToString());

            PlayerState ps2;

            using (TextReader tr = new StringReader(sb.ToString()))
            {
                XmlSerializerExt.Deserialize(out ps2, tr);
            }

            Assert.AreEqual(ps1, ps2);
        }
    }
}
