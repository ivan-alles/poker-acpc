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
    class LutTopCardGenerator: TableGenerator
    {
        internal LutTopCardGenerator()
        {
            Namespace = "ai.pkr.stdpoker.generated";
            Comment = "// Maps a 13-bit card mask to a top card in the mask.";
            Name = "LutTopCard";
            Type = "UInt32";
            UseUnmanagedMemory = true;
        }

        static readonly UInt32 _elementCount = 1 << 13;

        static UInt32 GetTopCard(UInt32 value)
        {
            UInt32 topCard = 12;
            while (value != 0)
            {
                if((value & 0x1000) != 0)
                {
                    return topCard;
                }
                value <<= 1;
                topCard--;
            }
            return 0;
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
                wr.Write("{0,2}, ", GetTopCard(i));
            }
            wr.WriteLine();
        }
    }
}
