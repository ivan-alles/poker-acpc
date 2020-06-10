/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

namespace ai.lib.utils.nunit
{
    /// <summary>
    /// Unit tests for UnmanagedMemory. 
    /// </summary>
    [TestFixture]
    public unsafe class UnmanagedMemory_Test
    {
        #region Tests

        [Test]
        [Explicit]
        public unsafe void Test_BigArray()
        {
            Console.WriteLine("Size of element: {0}, sizeof IntPtr: {1}",
                sizeof(TestStruct), sizeof(IntPtr));
            Int64 arrSize = 200 * 1000000;
            Int64 byteSize = (Int64)(arrSize * (Int64)sizeof(TestStruct));
            Console.WriteLine("Allocating {0:#,#} bytes...", byteSize);

            TestStruct* pArray = (TestStruct*)UnmanagedMemory.AllocHGlobalEx(byteSize);

            Console.WriteLine("Allocated {0:#,#} bytes", byteSize);

            pArray[0].data[0] = 1;

            Console.WriteLine("Filling with data...");

            for (Int64 i = 0; i < arrSize; ++i )
            {
                pArray[i].data[0] = 10;
            }

            Console.WriteLine("Freeing memory");

            UnmanagedMemory.FreeHGlobal(pArray);
        }

        [Test]
        public void Test_Diag()
        {
            UnmanagedMemory.IsDiagOn = true;
            IntPtr p;
            byte* pb;
            bool isExceptionThrown;

            // Overwrite after
            try
            {
                isExceptionThrown = false;
                p = UnmanagedMemory.AllocHGlobal(10);
                pb = (byte*)p.ToPointer();
                *(pb + 10) = 0xFF;
                UnmanagedMemory.FreeHGlobal(p.ToPointer());
            }
            catch (ApplicationException)
            {
                isExceptionThrown = true;
            }
            Assert.IsTrue(isExceptionThrown);


            // Overwrite befor
            try
            {
                isExceptionThrown = false;
                p = UnmanagedMemory.AllocHGlobal(10);
                pb = (byte*)p.ToPointer();
                *(pb - 1) = 0xFF;
                UnmanagedMemory.FreeHGlobal(p.ToPointer());
            }
            catch (ApplicationException)
            {
                isExceptionThrown = true;
            }
            Assert.IsTrue(isExceptionThrown);

            // Overwrite befor and after
            try
            {
                isExceptionThrown = false;
                p = UnmanagedMemory.AllocHGlobal(10);
                pb = (byte*)p.ToPointer();
                *(pb + 10) = 0xFF;
                *(pb - 8) = 0xFF;
                UnmanagedMemory.FreeHGlobal(p.ToPointer());
            }
            catch (ApplicationException)
            {
                isExceptionThrown = true;
            }
            Assert.IsTrue(isExceptionThrown);

            UnmanagedMemory.IsDiagOn = false;
        }

        [Test]
        public void Test_AutoFreeMemory()
        {
            for (int i = 0; i < 100; ++i)
            {
                SmartPtr p = UnmanagedMemory.AllocHGlobalExSmartPtr(1000000000);
                unsafe
                {
                    // To prevent optimizing away.
                    ((byte*) p)[0] = 1;
                }

                // Ensure releasing of the pointer.
                p = null;            
            }
        }

        [Test]
        public void Test_SetMemory_Random()
        {
            int rngSeed = DateTime.Now.Millisecond;
            Console.WriteLine("Rng seed {0}", rngSeed);
            Random rng = new Random(rngSeed);
            for(int i = 0; i < 1000; ++i)
            {
                int byteSize = rng.Next(0, 10000);
                byte value = (byte)rng.Next(0, 256);
                DoTestSetMemory(byteSize, value);
            }
        }

        [Test]
        public void Test_ReadWrite_Random()
        {
            int rngSeed = DateTime.Now.Millisecond;
            Console.WriteLine("Rng seed {0}", rngSeed);
            Random rng = new Random(rngSeed);
            for (int r = 0; r < 1000; ++r)
            {
                int byteSize = rng.Next(0, 10000);

                using (SmartPtr p = UnmanagedMemory.AllocHGlobalExSmartPtr(byteSize))
                {
                    byte* pBytes = (byte*) p;
                    for (Int64 i = 0; i < byteSize; ++i)
                    {
                        pBytes[i] = (byte) rng.Next(0, 256);
                    }
                    DoTestReadWrite(p, byteSize);
                }
            }
        }

        #endregion

        #region Benchmarks

        [Test]
        [Category("Benchmark")]
        public void Benchmark_ReadWrite()
        {
            Int64 size = 100 * 1000000;
            using (SmartPtr p = UnmanagedMemory.AllocHGlobalExSmartPtr(size))
            {
                string fileName = Path.GetTempFileName();
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                DateTime start = DateTime.Now;
                using (BinaryWriter w = new BinaryWriter(File.Open(fileName, FileMode.CreateNew, FileAccess.Write)))
                {
                    UnmanagedMemory.Write(w, p, size);
                }
                double time = (DateTime.Now - start).TotalSeconds;
                Console.WriteLine("{0:#,#} bytes written to disk in {1:0.00000} s, {2:#,#} b/s", size, time, size/time);

                start = DateTime.Now;
                using (BinaryReader r = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read)))
                {
                    UnmanagedMemory.Read(r, p, size);
                }
                time = (DateTime.Now - start).TotalSeconds;
                Console.WriteLine("{0:#,#} bytes read from disk in {1:0.00000} s, {2:#,#} b/s", size, time, size/time);

                File.Delete(fileName);
            }
        }

        #endregion

        #region Implementation

        unsafe struct TestStruct
        {
            public fixed int data[12];
        }

        void DoTestSetMemory(int byteSize, byte value)
        {
            using (SmartPtr p = UnmanagedMemory.AllocHGlobalExSmartPtr(byteSize))
            {
                UnmanagedMemory.SetMemory(p.Ptr, byteSize, value);
                for (int i = 0; i < byteSize; ++i)
                {
                    Assert.AreEqual(value, ((byte*) p)[i]);
                }
            }
        }

        void DoTestReadWrite(IntPtr p, Int64 size)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);
            UnmanagedMemory.Write(w, p, size);
            byte[] buffer = ms.ToArray();
            ms = new MemoryStream(buffer);
            using (SmartPtr p1 = UnmanagedMemory.AllocHGlobalExSmartPtr(size))
            {
                BinaryReader r = new BinaryReader(ms);
                UnmanagedMemory.Read(r, p1.Ptr, size);
                byte* pBytes = (byte*) p;
                byte* pBytes1 = (byte*) p1;
                for (Int64 i = 0; i < size; ++i)
                {
                    Assert.AreEqual(pBytes[i], pBytes1[i], String.Format("Values differ at pos {0}, size {1}", i, size));
                }
                p1.Dispose();
            }
        }




        #endregion
    }
}
