/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace ai.lib.utils
{
    /// <summary>
    /// Extends class Marshal adding operations with unmanaged memory.
    /// </summary>
    public static class UnmanagedMemory
    {
        #region Allocate and free memory

        /// <summary>
        /// A convinience method for Marshal.AllocateHGlobal(), taking an Int64 as parameter.
        /// </summary>
        public static IntPtr AllocHGlobal(long byteSize)
        {
            long allocSize = byteSize;
            if (IsDiagOn)
            {
                allocSize += 2 * PATTERN_SIZE;
            }
            IntPtr size = (IntPtr)allocSize;
            IntPtr ptr = Marshal.AllocHGlobal(size);
            // Console.WriteLine("Alloc: {0}", ptr);

            if (IsDiagOn)
            {
                unsafe
                {
                    byte[] pattern = ValueToPattern((ulong)byteSize);
                    byte* p = (byte*) ptr.ToPointer();
                    for (long i = 0; i < PATTERN_SIZE; ++i)
                    {
                        // Write the pattern before and after user data.
                        p[i] = pattern[i];
                        p[i + byteSize + PATTERN_SIZE] = pattern[i];
                    }
                    ptr = (IntPtr)(p + PATTERN_SIZE);
                }
            }

            return ptr;
        }


        /// <summary>
        /// Allocates unmanaged memory by a call to Marshal.AllocateHGlobal().
        /// The memory must be freed by the call to Marshal.FreeHGlobal().
        /// If not enough memory is present, forces GC to collect garbage and waits for all finalizers. 
        /// If your code wraps unmanaged memory into smart pointers or something similar,
        /// the unused memory will be freed automatically. 
        /// This automatization may be slow, so it's better to manually 
        /// allocate and release unmanaged memory and use it only for safety in case of exceptions.
        /// </summary>
        public static IntPtr AllocHGlobalEx(Int64 byteSize)
        {
            try
            {
                return AllocHGlobal(byteSize);
            }
            catch(OutOfMemoryException)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return AllocHGlobal(byteSize);
            }
        }

        /// <summary>
        /// The same as AllocHGlobalEx(), returning a smart pointer.
        /// </summary>
        public static SmartPtr AllocHGlobalExSmartPtr(Int64 byteSize)
        {
            return new SmartPtr(AllocHGlobalEx(byteSize));
        }

        /// <summary>
        /// Frees unmanages memory.
        /// Does nothing for null pointers. 
        /// </summary>
        public static unsafe void FreeHGlobal(IntPtr ptr)
        {
            FreeHGlobal((void*)ptr.ToPointer());
        }


        /// <summary>
        /// Frees unmanages memory.
        /// Does nothing for null pointers. 
        /// </summary>
        public static unsafe void FreeHGlobal(void * ptr)
        {
            if (ptr == null)
            {
                return;
            }
            if (IsDiagOn)
            {
                unsafe
                {
                    byte* p = (byte*) ptr;
                    byte[] pattern1 = MemoryToPattern(p - PATTERN_SIZE);
                    ulong size1 = (PatternToValue(pattern1));
                    byte[] pattern2;
                    try
                    {
                        pattern2 = MemoryToPattern(p + size1);
                    }
                    catch (AccessViolationException e)
                    {
                        // If we got this exception, we are reading from the wrong memory, 
                        // so the pattern is wrong.
                        throw new ApplicationException(
                            "Access violation exception was thrown, probably guard section was overwritten", e);
                    }
                    ulong size2 = PatternToValue(pattern2);
                    if (size1 != size2)
                    {

                        throw new ApplicationException(
                            String.Format("Guard section overwritten, before: {0}, after: {1}",
                            PatternToString(pattern1), PatternToString(pattern2)));
                    }
                    ptr = p - PATTERN_SIZE;
                }
            }
            // Console.WriteLine("Free: {0}", new IntPtr(ptr));

            Marshal.FreeHGlobal(new IntPtr(ptr));
        }

        /// <summary>
        /// Activates memory diagnostics. 
        /// Make sure that all deallocations occur with the same setting of this property as it 
        /// was for the corresponding allocation.
        /// </summary>
        public static bool IsDiagOn
        {
            set;
            get;
        }

        #endregion

        /// <summary>
        /// Sets memory to a given value.
        /// </summary>
        public static unsafe void SetMemory(IntPtr p, Int64 count, byte value)
        {
            // Fill fast with UInt32 values
            UInt32 uValue = value;
            uValue = uValue + (uValue << 8) + (uValue << 16) + (uValue << 24);
            UInt32* pUInt32 = (UInt32*) p.ToPointer();

            Int64 blocksCount = count / 4;
            for (Int64 i = 0; i < blocksCount; ++i, ++pUInt32)
            {
                *pUInt32 = uValue;
            }

            // Fill the tail with bytes.
            byte* pBytes = (byte*) p.ToPointer();
            for (Int64 i = blocksCount*4; i < count; ++i)
            {
                pBytes[i] = value;
            }
        }

        /// <summary>
        /// Size of intermediate buffer for file read/write. This buffer speeds up reading
        /// (the factor depends on the machine and disk type) although the data is actually
        /// copied twice.
        /// </summary>
        private const Int64 MAX_STREAM_BUFFER_SIZE = 1024*50;

        public unsafe static void Write(BinaryWriter w, IntPtr p, Int64 byteSize)
        {
            byte[] buffer = new byte[Math.Min(byteSize, MAX_STREAM_BUFFER_SIZE)];
            byte* pBytes = (byte*)p;
            for (Int64 pos = 0; pos < byteSize; pos += buffer.Length)
            {
                Int32 len = (int)Math.Min(buffer.Length, byteSize - pos);
                IntPtr startPtr = new IntPtr(pBytes + pos);
                Marshal.Copy(startPtr, buffer, 0, len);
                w.Write(buffer, 0, len);
            }
        }

        public unsafe static void Read(BinaryReader r, IntPtr p, Int64 byteSize)
        {
            byte[] buffer = new byte[Math.Min(byteSize, MAX_STREAM_BUFFER_SIZE)];
            byte* pBytes = (byte*)p;
            for (Int64 pos = 0; pos < byteSize; pos += buffer.Length)
            {
                Int32 len = (int)Math.Min(buffer.Length, byteSize - pos);
                r.Read(buffer, 0, len);
                IntPtr startPtr = new IntPtr(pBytes + pos);
                Marshal.Copy(buffer, 0, startPtr, len);
            }
        }

        #region Copy from managed to unmanaged

        public static unsafe void Copy(UInt32[] src, UInt32* dst)
        {
            fixed (UInt32* fp = src)
            {
                UInt32* psrc = fp;
                for (int i = src.Length; i > 0; --i, ++dst, ++psrc)
                    *dst = *psrc;
            }
        }

        public static unsafe void Copy(UInt64[] src, UInt64* dst)
        {
            fixed (UInt64* fp = src)
            {
                UInt64* psrc = fp;
                for (int i = src.Length; i > 0; --i, ++dst, ++psrc)
                    *dst = *psrc;
            }
        }

        public static unsafe void Copy(byte[] src, byte* dst)
        {
            fixed (byte* fp = src)
            {
                byte* psrc = fp;
                for (int i = src.Length; i > 0; --i, ++dst, ++psrc)
                    *dst = *psrc;
            }
        }
        #endregion

        #region Implementation

        const int PATTERN_SIZE = 8;
        /// <summary>
        /// A mask to xor with the memory block size to make a guarding section.
        /// The mask ensures that the most of zero bytes in the size change values, this
        /// increases the probability that the overwriting will be detected.
        /// </summary>
        const ulong PATTERN_MASK = 0xACDCDEADBEEFABBAL;
        static byte[] ValueToPattern(ulong value)
        {
            value ^= PATTERN_MASK;
            byte[] pattern = new byte[PATTERN_SIZE];
            for (int i = 0; i < pattern.Length; ++i)
            {
                pattern[i] = (byte)((value >> (i * 8)) & 0xFF);
            }
            return pattern;
        }

        static unsafe byte[] MemoryToPattern(byte * p)
        {
            byte[] pattern = new byte[PATTERN_SIZE];
            for (long i = 0; i < PATTERN_SIZE; ++i)
            {
                pattern[i] = p[i];
            }
            return pattern;
        }

        static ulong PatternToValue(byte[] pattern)
        {
            ulong value = 0;
            for (int i = 0; i < pattern.Length; ++i)
            {
                value += (ulong)pattern[i] << (i * 8);
            }
            value ^= PATTERN_MASK;
            return value;
        }

        static string PatternToString(byte[] pattern)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < pattern.Length; ++i)
            {
                sb.AppendFormat("{0:X} ", pattern[i]);
            }
            return sb.ToString();
        }

        #endregion

        #region Useful debug code

#if false

        public struct AllocInfo
        {
            public IntPtr Address;
            public int Kind;
        }

        public static readonly AllocInfo[] AllocInfos = new AllocInfo[4096];
        public static int AllocInfosIdx = -1;

        public static void AddAllocInfo(IntPtr Address, int kind)
        {
            int idx = Interlocked.Increment(ref AllocInfosIdx);
            AllocInfos[idx].Address = Address;
            AllocInfos[idx].Kind = kind;
        }

        public static void PrintAllocInfos()
        {
            for (int i = 0; i <= AllocInfosIdx; ++i)
            {
                Console.WriteLine("Addr: {0,16:X} kind: {1}", AllocInfos[i].Address, AllocInfos[i].Kind);
            }
        }
#endif
        #endregion
    }
}
