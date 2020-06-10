/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ai.lib.utils;

namespace ai.pkr.stdpoker.tablegen
{
    class LutTopFiveCardsGenerator: TableGenerator
    {
        internal LutTopFiveCardsGenerator()
        {
            Namespace = "ai.pkr.stdpoker.generated";
            Comment = "// Maps a 13-bit card mask to a top 5 cards in the mask. Format is the same as in the hand value.";
            Name = "LutTopFiveCards";
            Type = "UInt32";
            UseUnmanagedMemory = true;
        }
        static readonly UInt32 _elementCount = 1 << 13;

        static UInt32 GetTopFiveCards(UInt32 value)
        {
            UInt32 topCard = 12;
            UInt32 topFiveCards = 0;
            UInt32 count = 0;
            while (value != 0)
            {
                if ((value & 0x1000) != 0)
                {
                    count++;
                    topFiveCards |= topCard << 4*(int)(5 - count);
                    if (count == 5)
                        break;
                }
                value <<= 1;
                topCard--;
            }
            return topFiveCards;
        }

        protected override void PrintContent(TextWriter wr)
        {
            for (UInt32 i = 0; i < _elementCount; ++i)
            {
                if (i % 16 == 0)
                {
                    if (i != 0) wr.WriteLine();
                    wr.Write("             ");
                }
                wr.Write("0x{0:x5}, ", GetTopFiveCards(i));
            }
            wr.WriteLine();
        }
    }
}
