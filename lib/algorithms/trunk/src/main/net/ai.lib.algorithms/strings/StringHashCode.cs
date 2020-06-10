/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ai.lib.algorithms.strings
{
    /// <summary>
    /// This class calculates a hash code for a .NET string. It uses a .NET 3.5 implementation for 32-bit.
    /// It is useful because .NET 3.5 for 64-bit architecture has trouble with strings containing 0-characters.
    /// Hashing stops on them, therefore the tail of the string does not influence the hash value.
    /// </summary>
	public static class StringHashCode
	{
        public static int Get(string thisString)
        {
            // Code copied and adjusted from .NET 3.5 implementation.
            // Now it uses the same algo for both 32 and 64-bit platforms.
            unsafe
            {
                fixed (char* src = thisString)
                {
                    Debug.Assert(src[thisString.Length] == '\0', "src[this.Length] == '\\0'");
                    Debug.Assert(((int)src) % 4 == 0, "Managed string should start at 4 bytes boundary");

                    int hash1 = (5381 << 16) + 5381;
#if false 
                    // 64-bit
                    int hash1 = 5381;
#endif
                    int hash2 = hash1;

                    // 32bit machines. 
                    int* pint = (int*)src;
                    int len = thisString.Length;
                    while (len > 0)
                    {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                        if (len <= 2)
                        {
                            break;
                        }
                        hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
                        pint += 2;
                        len -= 4;
                    }
#if false
            // 64-bit machines
            int c;
            char* s = src;
            while ((c = s[0]) != 0)
            {
                hash1 = ((hash1 << 5) + hash1) ^ c;
                c = s[1];
                if (c == 0)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ c;
                s += 2;
            }
#endif
                    return hash1 + (hash2 * 1566083941);
                }
            }
        }
	}
}
