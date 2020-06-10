/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai.lib.utils
{
    /// <summary>
    /// CRC32 algorithm. 
    /// </summary>
    /// Developer notes:
    /// This class is not placed into algoritms library because this library
    /// requires CRC32.
    public static class Crc32
    {
        static readonly uint[] _table;

        public static uint Compute(byte[] bytes)
        {
            uint crc = 0xffffffff;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = (uint)((crc >> 8) ^ _table[index]);
            }
            return ~crc;
        }

        static Crc32()
        {
            uint poly = 0xedb88320;
            _table = new uint[256];
            uint temp = 0;
            for (uint i = 0; i < _table.Length; ++i)
            {
                temp = i;
                for (int j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                    {
                        temp = (uint)((temp >> 1) ^ poly);
                    }
                    else
                    {
                        temp >>= 1;
                    }
                }
                _table[i] = temp;
            }
        }
    }
}
