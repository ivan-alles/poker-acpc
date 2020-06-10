/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ai.lib.utils;

namespace ai.pkr.fictpl
{
    /// <summary>
    /// Fast bit array.
    /// </summary>
    /// Developer notes:
    /// I tried to use an array for bitmasks, but it was only unsignificantly faster.
    /// I also tried to create an iterator to go through bits incrementally, but is was much 
    /// slower than a regular index calculation.
    /// Using unmanaged memory for the bit array actually decreased the performance.
    public unsafe class FastBitArray
    {
        public FastBitArray(UInt32 bitCount)
        {
            _bitCount = bitCount;
            UInt32 arrSize = _bitCount / BITS_IN_ELEMENT + 1;
            //_arrayPtr = UnmanagedMemory.AllocHGlobalExSmartPtr(arrSize * (BITS_IN_ELEMENT / 8));
            //_array = (UInt32*)_arrayPtr;
            _array = new UInt32[arrSize];
        }

        public UInt32 Length
        {
            get { return _bitCount; }
        }

        public UInt32[] Bits
        {
            get { return _array; }
        }

        public void Set(UInt32 i, bool value)
        {
            Debug.Assert(i < _bitCount);

            UInt32 elIdx = i >> ELEMENT_IDX_SHIFT;
            int bitIdx = (int)i & BIT_IDX_MASK;
            if (value)
            {
                _array[elIdx] |= (1U << bitIdx);
            }
            else
            {
                _array[elIdx] &= ~(1U << bitIdx);
            }
        }


        public bool Get(UInt32 i)
        {
            Debug.Assert(i < _bitCount);
            int bitIdx = (int)i & BIT_IDX_MASK;
            return (_array[i >> ELEMENT_IDX_SHIFT] & (1U << bitIdx)) != 0;
        }
#if false // Const iterator is slower as Get().
        /// An attempt to build an iterator that is faster than direct access with Get().
        public struct ConstIterator
        {
            public ConstIterator(FastBitArray arr, UInt32 startPos)
            {
                Debug.Assert(startPos < arr._bitCount);
                _array = arr._array;
                _elIdx = startPos >> ELEMENT_IDX_SHIFT;
                _bitIdx = (int)startPos & BIT_IDX_MASK;
                _curEl = _array[_elIdx];
                Value = (_curEl & (1U << _bitIdx)) != 0;
            }

            public bool Value;

            public void Next()
            {
                if (++_bitIdx == BITS_IN_ELEMENT)
                {
                    _bitIdx = 0;
                    _curEl = _array[++_elIdx];
                }
                Value = (_curEl & (1U << _bitIdx)) != 0;
            }

            UInt32 _elIdx;
            UInt32 _curEl;
            int _bitIdx;
            readonly UInt32 [] _array;
        }
#endif

        const int BITS_IN_ELEMENT = 32;
        const int ELEMENT_IDX_SHIFT = 5;
        const int BIT_IDX_MASK = 0x1F;


        readonly UInt32 _bitCount;
        /// <summary>
        /// Array with bits. I tried to used an unmanaged array, but strangely it is a bit slower than
        /// a managed one
        /// </summary>
        readonly UInt32 [] _array;
        //SmartPtr _arrayPtr;
   

        //public void  Dispose()
        //{
        //    _arrayPtr.Dispose();
        //}

    }
}
