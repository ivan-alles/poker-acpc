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
    class LutStraightGenerator: TableGenerator
    {
        internal LutStraightGenerator()
        {
            Namespace = "ai.pkr.stdpoker.generated";
            Comment = "// Maps 13-bit card mask to a high card of a straight (0 - no straight).";
            Name = "LutStraight";
            Type = "byte";
            UseUnmanagedMemory = true;
        }
     
        static readonly UInt32 _elementCount = 1 << 13;


        static UInt32 GetStraightValue(UInt32 value)
        {
            UInt32 highCard = 12;
            UInt32 currentCard = 12;
            UInt32 count = 0;

            // To detect 5-high straight. 
            value <<= 1;
            value |= (value & 0x2000) >> 13;

            while (value != 0)
            {
                if ((value & 0x2000) != 0)
                {
                    if (++count == 5)
                        return highCard;
                }
                else
                {
                    count = 0;
                    highCard = currentCard - 1;
                }
                currentCard--;
                value <<= 1;
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
                wr.Write("{0,2}, ", GetStraightValue(i));
            }
            wr.WriteLine();
        }
    }
}
