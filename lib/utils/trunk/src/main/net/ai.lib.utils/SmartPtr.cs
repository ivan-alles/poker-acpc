/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ai.lib.utils
{
    /// <summary>
    /// A wrapper for unmanaged memory. A pointer can be assigned only once in the constructor. 
    /// It will be freed by a call Dispose() (recommended) or in the destructor.
    /// <para>BE CAREFUL with local smart pointers!!!</para>
    /// <para>SmartPtr ptr = new SmartPtr(UnmanagedMemory.AllocateHGlobal(byteSize));</para>
    /// <para>... do something, but beware that GC can kill ptr unless you access it later!</para>
    /// <para>// ((byte*)ptr)[0] = 1;  uncommenting of this line will prevent ptr from desruction.</para>
    /// <para>It is recommended to enclose local smart pointers to using statement.</para>
    /// </summary>
    /// <seealso cref="UnmanagedMemory"/>
    public sealed class SmartPtr: IDisposable
    {
        /// <summary>
        /// The pointer to unmanaged memory. 
        /// For convinience and performance cast it to your specific type (e.g. byte*) 
        /// and store it to a local variable.
        /// </summary>
        public IntPtr Ptr
        {
            get;
            private set;
        }

        public SmartPtr(IntPtr ptr)
        {
            Ptr = ptr;
        }

        /// <summary>
        /// Frees the pointer explicitely. This is the recommended to free memory,
        /// because GC does not work well with object allocating unmanaged memory.
        /// </summary>
        public void Dispose()
        {
            if (Ptr != IntPtr.Zero)
            {
                UnmanagedMemory.FreeHGlobal(Ptr);
                Ptr = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }

        ~SmartPtr()
        {
            Dispose();
        }

        public static implicit operator IntPtr(SmartPtr p)
        {
            return p.Ptr;
        }

        public static unsafe explicit operator void *(SmartPtr p)
        {
            return (void*)p.Ptr;
        }

        public static unsafe explicit operator byte*(SmartPtr p)
        {
            return (byte*)p.Ptr;
        }

        public static unsafe explicit operator char*(SmartPtr p)
        {
            return (char*)p.Ptr;
        }

        public static unsafe explicit operator Int16*(SmartPtr p)
        {
            return (Int16*)p.Ptr;
        }

        public static unsafe explicit operator UInt16*(SmartPtr p)
        {
            return (UInt16*)p.Ptr;
        }

        public static unsafe explicit operator Int32*(SmartPtr p)
        {
            return (Int32*)p.Ptr;
        }

        public static unsafe explicit operator UInt32*(SmartPtr p)
        {
            return (UInt32*)p.Ptr;
        }

        public static unsafe explicit operator Int64*(SmartPtr p)
        {
            return (Int64*)p.Ptr;
        }

        public static unsafe explicit operator UInt64*(SmartPtr p)
        {
            return (UInt64*)p.Ptr;
        }
    }
}
