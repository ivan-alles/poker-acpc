/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ai.pkr.metabots.remote
{
    /// <summary>
    /// Message header for remote protocol.
    /// Uses big endian to transfer data.
    /// </summary>
    public class Header
    {
        public Header()
        {
        }

        public Header(int functionId)
        {
            FunctionId = functionId;
        }

        public int FunctionId
        {
            set;
            get;
        }
        public int DataLength
        {
            set;
            get;
        }

        public static int SIZE = 8;

        public void WriteTo(Stream s)
        {
            s.WriteByte((byte)((FunctionId >> 24) & 0xFF));
            s.WriteByte((byte)((FunctionId >> 16) & 0xFF));
            s.WriteByte((byte)((FunctionId >> 8) & 0xFF));
            s.WriteByte((byte)((FunctionId >> 0) & 0xFF));

            s.WriteByte((byte)((DataLength >> 24) & 0xFF));
            s.WriteByte((byte)((DataLength >> 16) & 0xFF));
            s.WriteByte((byte)((DataLength >> 8) & 0xFF));
            s.WriteByte((byte)((DataLength >> 0) & 0xFF));
        }

        public static Header ReadFrom(Stream s)
        {
            Header h = new Header();

            h.FunctionId = (s.ReadByte() << 24) |
                            (s.ReadByte() << 16) |
                            (s.ReadByte() << 8) |
                            (s.ReadByte() << 0);

            h.DataLength = (s.ReadByte() << 24) |
                            (s.ReadByte() << 16) |
                            (s.ReadByte() << 8) |
                            (s.ReadByte() << 0);
            return h;
        }
    }
}
