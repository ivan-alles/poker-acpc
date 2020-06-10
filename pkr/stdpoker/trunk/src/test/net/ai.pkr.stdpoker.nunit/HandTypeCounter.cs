/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ai.pkr.stdpoker.nunit
{
    class HandTypeCounter
    {
        private UInt32[] _handTypesCount = new UInt32[(int)HandValue.Kind._Count + 1];

        public UInt32 this[int i]
        {
            get { return _handTypesCount[i];}
        }

        public void Reset()
        {
            for (int i = 0; i < _handTypesCount.Length; ++i)
            {
                _handTypesCount[i] = 0;
            }
        }

        public void Count(UInt32 handVal)
        {
            HandValue.Kind handType = HandValue.GetKind(handVal);
            _handTypesCount[(int) handType]++;
            _handTypesCount[(int) HandValue.Kind._Count]++; // Total.
        }

        public void Verify(int cardCount)
        {
            UInt32[] exp = cardCount == 5 ? _Expected5 : (cardCount == 6 ? _Expected6 : _Expected7);
            for (int i = 0; i < _handTypesCount.Length; ++i)
            {
                Assert.AreEqual(exp[i], _handTypesCount[i]);
            }
        }

        public void Print()
        {
            Console.WriteLine("Number of combinations:");
            for (int i = 0; i < (int)HandValue.Kind._Count + 1; ++i)
            {
                Console.WriteLine("{0, 13}: {1,9}",
                                  ((HandValue.Kind) i).ToString(),
                                  _handTypesCount[i]);
            }
        }

        private static readonly UInt32[] _Expected5 =
        {
            1302540,
            1098240,
            123552,
            54912,
            10200,
            5108,
            3744,
            624,
            40,
            2598960
        };

        private static readonly UInt32[] _Expected6 =
        {
            6612900,
            9730740,
            2532816,
            732160,
            361620,
            205792,
            165984,
            14664,
            1844,
            20358520
        };

        private static readonly UInt32[] _Expected7 =
        {
            23294460,
            58627800,
            31433400,
            6461620,
            6180020,
            4047644,
            3473184,
            224848,
            41584,
            133784560
        };

    }
}
