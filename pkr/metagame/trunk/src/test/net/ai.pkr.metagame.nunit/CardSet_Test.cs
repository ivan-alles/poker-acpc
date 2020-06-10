/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.pkr.metagame.nunit
{
    [TestFixture]
    public class CardSet_Test
    {
        [Test]
        public void Test_Sizeof()
        {
            unsafe
            {
                int sizeOfMask = sizeof (CardSet);
                Assert.AreEqual(8, sizeOfMask);
            }
        }

        [Test]
        public void Test_Constants()
        {
            Assert.AreEqual(0, CardSet.Empty.bits);
        }

        [Test]
        public void Test_Constructors()
        {
            Assert.AreEqual(0x01, new CardSet(new CardSet { bits = 0x00 }, new CardSet { bits = 0x01 }).bits);
            Assert.AreEqual(0xF1, new CardSet(new CardSet { bits = 0xF0 }, new CardSet { bits = 0x01 }).bits);
            Assert.AreEqual(0xF1, new CardSet(new CardSet { bits = 0xF0 }, new CardSet { bits = 0x31 }).bits);
        }

        [Test]
        public void Test_IsEmpty()
        {
            Assert.IsTrue(CardSet.Empty.IsEmpty());
            Assert.IsTrue(new CardSet { bits = 0x0 }.IsEmpty());
            Assert.IsFalse(new CardSet { bits = 0x11 }.IsEmpty());
        }

        [Test]
        public void Test_Clear()
        {
            CardSet cs = new CardSet{bits = 0xF0000000000000ul};
            Assert.IsFalse(cs.IsEmpty());
            cs.Clear();
            Assert.IsTrue(cs.IsEmpty());
        }

        [Test]
        public void Test_CountCards()
        {
            Assert.AreEqual(0, new CardSet { bits = 0x00 }.CountCards());
            Assert.AreEqual(1, new CardSet { bits = 0x10 }.CountCards());
            Assert.AreEqual(64, new CardSet { bits = 0xFFFFFFFFFFFFFFFFul }.CountCards());
            Assert.AreEqual(32, new CardSet { bits = 0xC3C3C3C3C3C3C3C3ul }.CountCards());
        }

        [Test]
        public void Test_ToString()
        {
            Assert.AreEqual("2c", new CardSet { bits = 0x01 }.ToString());
            Assert.AreEqual("Ac 2c 2d", new CardSet { bits = 0x11001 }.ToString());
        }

        [Test]
        public void Test_Equality()
        {
#pragma warning disable 1718 // No warning for comparison to self.

            CardSet c1 = new CardSet {bits = 0x02};
            CardSet c2 = new CardSet {bits = 0xF0};
            CardSet c3 = new CardSet {bits = 0x02};

            object o1 = c1;
            object o2 = c2;
            object o3 = c3;

            Assert.IsTrue(c1 == c1);
            Assert.IsFalse(c1 != c1);
            Assert.IsTrue(c1.Equals(c1));
            Assert.IsTrue(c1.Equals(o1));
            Assert.IsTrue(o1.Equals(c1));
            Assert.IsTrue(o1.Equals(o1));
            Assert.IsTrue(object.Equals(c1, c1));
            Assert.AreEqual(c1.GetHashCode(), c1.GetHashCode());

            Assert.IsTrue(c1 == c3);
            Assert.IsFalse(c1 != c3);
            Assert.IsTrue(c1.Equals(c3));
            Assert.IsTrue(c1.Equals(o3));
            Assert.IsTrue(o1.Equals(c3));
            Assert.IsTrue(o1.Equals(o3));
            Assert.IsTrue(object.Equals(c1, c3));
            Assert.AreEqual(c1.GetHashCode(), c3.GetHashCode());

            Assert.IsFalse(c1 == c2);
            Assert.IsTrue(c1 != c2);
            Assert.IsFalse(c1.Equals(c2));
            Assert.IsFalse(c1.Equals(o2));
            Assert.IsFalse(o1.Equals(c2));
            Assert.IsFalse(o1.Equals(o2));
            Assert.IsFalse(object.Equals(c1, c2));

#pragma warning restore 1718
        }

        [Test]
        public void Test_UnionWith()
        {
            CardSet cs;
            cs.bits = 0x00;
            cs.UnionWith(new CardSet {bits = 0x01});
            Assert.AreEqual(0x01, cs.bits);

            cs.bits = 0xF0;
            cs.UnionWith(new CardSet { bits = 0x01 });
            Assert.AreEqual(0xF1, cs.bits);

            cs.bits = 0xF3;
            cs.UnionWith(new CardSet { bits = 0x33 });
            Assert.AreEqual(0xF3, cs.bits);
        }

        [Test]
        public void Test_Operator_Or()
        {
            Assert.AreEqual(0x01, (new CardSet { bits = 0x01 } | new CardSet { bits = 0x00 }).bits);
            Assert.AreEqual(0x37, (new CardSet { bits = 0x01 } | new CardSet { bits = 0x36 }).bits);
            Assert.AreEqual(0xF6, (new CardSet { bits = 0x22 } | new CardSet { bits = 0xF6 }).bits);
        }

        [Test]
        public void Test_Contains()
        {
            Assert.IsTrue(CardSet.Empty.Contains(CardSet.Empty));
            Assert.IsFalse(CardSet.Empty.Contains(new CardSet { bits = 0x01 }));
            Assert.IsTrue(new CardSet { bits = 0x01 }.Contains(CardSet.Empty));
            Assert.IsTrue(new CardSet { bits = 0x01 }.Contains(new CardSet { bits = 0x01}));

            Assert.IsTrue(new CardSet { bits = 0xFF }.Contains(new CardSet { bits = 0x01 }));
            Assert.IsFalse(new CardSet { bits = 0x01 }.Contains(new CardSet { bits = 0xFF }));

            Assert.IsFalse(new CardSet { bits = 0xFF }.Contains(new CardSet { bits = 0x100 }));
            Assert.IsFalse(new CardSet { bits = 0x100 }.Contains(new CardSet { bits = 0xFF }));
        }

        [Test]
        public void Test_IsIntersectingWith()
        {
            Assert.IsFalse(CardSet.Empty.IsIntersectingWith(CardSet.Empty));
            Assert.IsFalse(CardSet.Empty.IsIntersectingWith(new CardSet { bits = 0x01 }));
            Assert.IsFalse(new CardSet { bits = 0x01 }.IsIntersectingWith(CardSet.Empty));

            Assert.IsTrue(new CardSet { bits = 0x01 }.IsIntersectingWith(new CardSet { bits = 0x01 }));

            Assert.IsTrue(new CardSet { bits = 0x01 }.IsIntersectingWith(new CardSet { bits = 0xFF }));
            Assert.IsTrue(new CardSet { bits = 0xF1 }.IsIntersectingWith(new CardSet { bits = 0x01 }));

            Assert.IsFalse(new CardSet { bits = 0x01 }.IsIntersectingWith(new CardSet { bits = 0x04 }));
            Assert.IsFalse(new CardSet { bits = 0x04 }.IsIntersectingWith(new CardSet { bits = 0x01 }));
        }

        [Test]
        public void Test_Intersect()
        {
            Assert.AreEqual(0x0, CardSet.Intersect(CardSet.Empty, CardSet.Empty).bits);
            Assert.AreEqual(0x0, CardSet.Intersect(CardSet.Empty, new CardSet { bits = 0x13 }).bits);
            Assert.AreEqual(0x0, CardSet.Intersect(new CardSet { bits = 0x13 }, CardSet.Empty).bits);

            Assert.AreEqual(0x13, CardSet.Intersect(new CardSet { bits = 0x13 }, new CardSet { bits = 0x13 }).bits);

            Assert.AreEqual(0x11, CardSet.Intersect(new CardSet { bits = 0x11 }, new CardSet { bits = 0xFF }).bits);
            Assert.AreEqual(0x11, CardSet.Intersect(new CardSet { bits = 0xFF }, new CardSet { bits = 0x11 }).bits);

            Assert.AreEqual(0x11, CardSet.Intersect(new CardSet { bits = 0xF11 }, new CardSet { bits = 0xFF }).bits);
            Assert.AreEqual(0x11, CardSet.Intersect(new CardSet { bits = 0xFF }, new CardSet { bits = 0xF11 }).bits);

            Assert.AreEqual(0x10, CardSet.Intersect(new CardSet { bits = 0x11 }, new CardSet { bits = 0xF10 }).bits);
            Assert.AreEqual(0x10, CardSet.Intersect(new CardSet { bits = 0xF10 }, new CardSet { bits = 0x11 }).bits);
        }

        [Test]
        public void Test_Remove()
        {
            CardSet cs;
            cs.bits = 0x0;
            cs.Remove(CardSet.Empty);
            Assert.AreEqual(0x0, cs.bits);

            cs.bits = 0x0;
            cs.Remove(new CardSet{bits = 0xF3});
            Assert.AreEqual(0x0, cs.bits);

            cs.bits = 0x34;
            cs.Remove(CardSet.Empty);
            Assert.AreEqual(0x34, cs.bits);

            cs.bits = 0x34;
            cs.Remove(cs);
            Assert.AreEqual(0x00, cs.bits);

            cs.bits = 0x34;
            cs.Remove(new CardSet { bits = 0x01 });
            Assert.AreEqual(0x34, cs.bits);

            cs.bits = 0x34;
            cs.Remove(new CardSet { bits = 0x04 });
            Assert.AreEqual(0x30, cs.bits);
        }


        [Test]
        public void Test_Operator_And()
        {
            Assert.AreEqual(0x0, (CardSet.Empty & CardSet.Empty).bits);
            Assert.AreEqual(0x0, (CardSet.Empty & new CardSet { bits = 0x13 }).bits);
            Assert.AreEqual(0x0, (new CardSet { bits = 0x13 } & CardSet.Empty).bits);

            Assert.AreEqual(0x13, (new CardSet { bits = 0x13 } & new CardSet { bits = 0x13 }).bits);

            Assert.AreEqual(0x11, (new CardSet { bits = 0x11 } & new CardSet { bits = 0xFF }).bits);
            Assert.AreEqual(0x11, (new CardSet { bits = 0xFF } & new CardSet { bits = 0x11 }).bits);

            Assert.AreEqual(0x11, (new CardSet { bits = 0xF11 } & new CardSet { bits = 0xFF }).bits);
            Assert.AreEqual(0x11, (new CardSet { bits = 0xFF } & new CardSet { bits = 0xF11 }).bits);

            Assert.AreEqual(0x10, (new CardSet { bits = 0x11 } & new CardSet { bits = 0xF10 }).bits);
            Assert.AreEqual(0x10, (new CardSet { bits = 0xF10 } & new CardSet { bits = 0x11 }).bits);
        }
    }
}