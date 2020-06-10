// Much of this code is derived from poker.eval (look for it on sourceforge.net).
// This library is covered by the LGPL Gnu license. See http://www.gnu.org/copyleft/lesser.html 
// for more information on this license.

// This code is a very fast, native C# Texas Holdem hand evaluator (containing no interop or unsafe code). 
// This code can enumarate 35 million 5 card hands per second and 29 million 7 card hands per second on my desktop machine.
// That's not nearly as fast as the heavily macro-ed poker.eval C library. However, this implementation is
// in roughly the same ballpark for speed and is quite usable in C#.

// The speed ups are mostly table driven. That means that there are several very large tables included in this file. 
// The code is divided up into several files they are:
//      HandEvaluator.cs - base hand evaluator
//      HandIterator.cs - methods that support IEnumerable and methods that validate the hand evaluator
//      HandAnalysis.cs - methods to aid in analysis of Texas Holdem Hands.

// Written (ported) by Keith Rule - Sept 2005

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;

namespace ai.pkr.stdpoker.nunit.ref_evaluator
{
    public partial class RefEvaluator : IComparable
    {

        #region Pocket169

        #region Pocket169 Table
        /// <summary>
        /// The 1326 possible pocket cards ordered by the 169 unique holdem combinations. The
        /// index is equivalent to the number value of Hand.PocketPairType.
        /// </summary>
        private static readonly ulong[][] Pocket169Table = {
	        new ulong [] {0x8004000000000, 0x8000002000000, 0x8000000001000, 0x4002000000, 0x4000001000, 0x2001000},
	        new ulong [] {0x4002000000000, 0x4000001000000, 0x4000000000800, 0x2001000000, 0x2000000800, 0x1000800},
	        new ulong [] {0x2001000000000, 0x2000000800000, 0x2000000000400, 0x1000800000, 0x1000000400, 0x800400},
	        new ulong [] {0x1000800000000, 0x1000000400000, 0x1000000000200, 0x800400000, 0x800000200, 0x400200},
	        new ulong [] {0x800400000000, 0x800000200000, 0x800000000100, 0x400200000, 0x400000100, 0x200100},
	        new ulong [] {0x400200000000, 0x400000100000, 0x400000000080, 0x200100000, 0x200000080, 0x100080},
	        new ulong [] {0x200100000000, 0x200000080000, 0x200000000040, 0x100080000, 0x100000040, 0x80040},
	        new ulong [] {0x100080000000, 0x100000040000, 0x100000000020, 0x80040000, 0x80000020, 0x40020},
	        new ulong [] {0x80040000000, 0x80000020000, 0x80000000010, 0x40020000, 0x40000010, 0x20010},
	        new ulong [] {0x40020000000, 0x40000010000, 0x40000000008, 0x20010000, 0x20000008, 0x10008},
	        new ulong [] {0x20010000000, 0x20000008000, 0x20000000004, 0x10008000, 0x10000004, 0x8004},
	        new ulong [] {0x10008000000, 0x10000004000, 0x10000000002, 0x8004000, 0x8000002, 0x4002},
	        new ulong [] {0x8004000000, 0x8000002000, 0x8000000001, 0x4002000, 0x4000001, 0x2001},
	        new ulong [] {0xC000000000000, 0x6000000000, 0x3000000, 0x1800},
	        new ulong [] {0x8002000000000, 0x8000001000000, 0x8000000000800, 0x4004000000000, 0x4000002000000, 0x4000000001000, 0x4001000000, 0x4000000800, 0x2002000000, 0x2000001000, 0x2000800, 0x1001000},
	        new ulong [] {0xA000000000000, 0x5000000000, 0x2800000, 0x1400},
	        new ulong [] {0x8001000000000, 0x8000000800000, 0x8000000000400, 0x2004000000000, 0x2000002000000, 0x2000000001000, 0x4000800000, 0x4000000400, 0x1002000000, 0x1000001000, 0x2000400, 0x801000},
	        new ulong [] {0x9000000000000, 0x4800000000, 0x2400000, 0x1200},
	        new ulong [] {0x8000800000000, 0x8000000400000, 0x8000000000200, 0x1004000000000, 0x1000002000000, 0x1000000001000, 0x4000400000, 0x4000000200, 0x802000000, 0x800001000, 0x2000200, 0x401000},
	        new ulong [] {0x8800000000000, 0x4400000000, 0x2200000, 0x1100},
	        new ulong [] {0x8000400000000, 0x8000000200000, 0x8000000000100, 0x804000000000, 0x800002000000, 0x800000001000, 0x4000200000, 0x4000000100, 0x402000000, 0x400001000, 0x2000100, 0x201000},
	        new ulong [] {0x8400000000000, 0x4200000000, 0x2100000, 0x1080},
	        new ulong [] {0x8000200000000, 0x8000000100000, 0x8000000000080, 0x404000000000, 0x400002000000, 0x400000001000, 0x4000100000, 0x4000000080, 0x202000000, 0x200001000, 0x2000080, 0x101000},
	        new ulong [] {0x8200000000000, 0x4100000000, 0x2080000, 0x1040},
	        new ulong [] {0x8000100000000, 0x8000000080000, 0x8000000000040, 0x204000000000, 0x200002000000, 0x200000001000, 0x4000080000, 0x4000000040, 0x102000000, 0x100001000, 0x2000040, 0x81000},
	        new ulong [] {0x8100000000000, 0x4080000000, 0x2040000, 0x1020},
	        new ulong [] {0x8000080000000, 0x8000000040000, 0x8000000000020, 0x104000000000, 0x100002000000, 0x100000001000, 0x4000040000, 0x4000000020, 0x82000000, 0x80001000, 0x2000020, 0x41000},
	        new ulong [] {0x8080000000000, 0x4040000000, 0x2020000, 0x1010},
	        new ulong [] {0x8000040000000, 0x8000000020000, 0x8000000000010, 0x84000000000, 0x80002000000, 0x80000001000, 0x4000020000, 0x4000000010, 0x42000000, 0x40001000, 0x2000010, 0x21000},
	        new ulong [] {0x8040000000000, 0x4020000000, 0x2010000, 0x1008},
	        new ulong [] {0x8000020000000, 0x8000000010000, 0x8000000000008, 0x44000000000, 0x40002000000, 0x40000001000, 0x4000010000, 0x4000000008, 0x22000000, 0x20001000, 0x2000008, 0x11000},
	        new ulong [] {0x8020000000000, 0x4010000000, 0x2008000, 0x1004},
	        new ulong [] {0x8000010000000, 0x8000000008000, 0x8000000000004, 0x24000000000, 0x20002000000, 0x20000001000, 0x4000008000, 0x4000000004, 0x12000000, 0x10001000, 0x2000004, 0x9000},
	        new ulong [] {0x8010000000000, 0x4008000000, 0x2004000, 0x1002},
	        new ulong [] {0x8000008000000, 0x8000000004000, 0x8000000000002, 0x14000000000, 0x10002000000, 0x10000001000, 0x4000004000, 0x4000000002, 0xA000000, 0x8001000, 0x2000002, 0x5000},
	        new ulong [] {0x8008000000000, 0x4004000000, 0x2002000, 0x1001},
	        new ulong [] {0x8000004000000, 0x8000000002000, 0x8000000000001, 0xC000000000, 0x8002000000, 0x8000001000, 0x4000002000, 0x4000000001, 0x6000000, 0x4001000, 0x2000001, 0x3000},
	        new ulong [] {0x6000000000000, 0x3000000000, 0x1800000, 0xC00},
	        new ulong [] {0x4001000000000, 0x4000000800000, 0x4000000000400, 0x2002000000000, 0x2000001000000, 0x2000000000800, 0x2000800000, 0x2000000400, 0x1001000000, 0x1000000800, 0x1000400, 0x800800},
	        new ulong [] {0x5000000000000, 0x2800000000, 0x1400000, 0xA00},
	        new ulong [] {0x4000800000000, 0x4000000400000, 0x4000000000200, 0x1002000000000, 0x1000001000000, 0x1000000000800, 0x2000400000, 0x2000000200, 0x801000000, 0x800000800, 0x1000200, 0x400800},
	        new ulong [] {0x4800000000000, 0x2400000000, 0x1200000, 0x900},
	        new ulong [] {0x4000400000000, 0x4000000200000, 0x4000000000100, 0x802000000000, 0x800001000000, 0x800000000800, 0x2000200000, 0x2000000100, 0x401000000, 0x400000800, 0x1000100, 0x200800},
	        new ulong [] {0x4400000000000, 0x2200000000, 0x1100000, 0x880},
	        new ulong [] {0x4000200000000, 0x4000000100000, 0x4000000000080, 0x402000000000, 0x400001000000, 0x400000000800, 0x2000100000, 0x2000000080, 0x201000000, 0x200000800, 0x1000080, 0x100800},
	        new ulong [] {0x4200000000000, 0x2100000000, 0x1080000, 0x840},
	        new ulong [] {0x4000100000000, 0x4000000080000, 0x4000000000040, 0x202000000000, 0x200001000000, 0x200000000800, 0x2000080000, 0x2000000040, 0x101000000, 0x100000800, 0x1000040, 0x80800},
	        new ulong [] {0x4100000000000, 0x2080000000, 0x1040000, 0x820},
	        new ulong [] {0x4000080000000, 0x4000000040000, 0x4000000000020, 0x102000000000, 0x100001000000, 0x100000000800, 0x2000040000, 0x2000000020, 0x81000000, 0x80000800, 0x1000020, 0x40800},
	        new ulong [] {0x4080000000000, 0x2040000000, 0x1020000, 0x810},
	        new ulong [] {0x4000040000000, 0x4000000020000, 0x4000000000010, 0x82000000000, 0x80001000000, 0x80000000800, 0x2000020000, 0x2000000010, 0x41000000, 0x40000800, 0x1000010, 0x20800},
	        new ulong [] {0x4040000000000, 0x2020000000, 0x1010000, 0x808},
	        new ulong [] {0x4000020000000, 0x4000000010000, 0x4000000000008, 0x42000000000, 0x40001000000, 0x40000000800, 0x2000010000, 0x2000000008, 0x21000000, 0x20000800, 0x1000008, 0x10800},
	        new ulong [] {0x4020000000000, 0x2010000000, 0x1008000, 0x804},
	        new ulong [] {0x4000010000000, 0x4000000008000, 0x4000000000004, 0x22000000000, 0x20001000000, 0x20000000800, 0x2000008000, 0x2000000004, 0x11000000, 0x10000800, 0x1000004, 0x8800},
	        new ulong [] {0x4010000000000, 0x2008000000, 0x1004000, 0x802},
	        new ulong [] {0x4000008000000, 0x4000000004000, 0x4000000000002, 0x12000000000, 0x10001000000, 0x10000000800, 0x2000004000, 0x2000000002, 0x9000000, 0x8000800, 0x1000002, 0x4800},
	        new ulong [] {0x4008000000000, 0x2004000000, 0x1002000, 0x801},
	        new ulong [] {0x4000004000000, 0x4000000002000, 0x4000000000001, 0xA000000000, 0x8001000000, 0x8000000800, 0x2000002000, 0x2000000001, 0x5000000, 0x4000800, 0x1000001, 0x2800},
	        new ulong [] {0x3000000000000, 0x1800000000, 0xC00000, 0x600},
	        new ulong [] {0x2000800000000, 0x2000000400000, 0x2000000000200, 0x1001000000000, 0x1000000800000, 0x1000000000400, 0x1000400000, 0x1000000200, 0x800800000, 0x800000400, 0x800200, 0x400400},
	        new ulong [] {0x2800000000000, 0x1400000000, 0xA00000, 0x500},
	        new ulong [] {0x2000400000000, 0x2000000200000, 0x2000000000100, 0x801000000000, 0x800000800000, 0x800000000400, 0x1000200000, 0x1000000100, 0x400800000, 0x400000400, 0x800100, 0x200400},
	        new ulong [] {0x2400000000000, 0x1200000000, 0x900000, 0x480},
	        new ulong [] {0x2000200000000, 0x2000000100000, 0x2000000000080, 0x401000000000, 0x400000800000, 0x400000000400, 0x1000100000, 0x1000000080, 0x200800000, 0x200000400, 0x800080, 0x100400},
	        new ulong [] {0x2200000000000, 0x1100000000, 0x880000, 0x440},
	        new ulong [] {0x2000100000000, 0x2000000080000, 0x2000000000040, 0x201000000000, 0x200000800000, 0x200000000400, 0x1000080000, 0x1000000040, 0x100800000, 0x100000400, 0x800040, 0x80400},
	        new ulong [] {0x2100000000000, 0x1080000000, 0x840000, 0x420},
	        new ulong [] {0x2000080000000, 0x2000000040000, 0x2000000000020, 0x101000000000, 0x100000800000, 0x100000000400, 0x1000040000, 0x1000000020, 0x80800000, 0x80000400, 0x800020, 0x40400},
	        new ulong [] {0x2080000000000, 0x1040000000, 0x820000, 0x410},
	        new ulong [] {0x2000040000000, 0x2000000020000, 0x2000000000010, 0x81000000000, 0x80000800000, 0x80000000400, 0x1000020000, 0x1000000010, 0x40800000, 0x40000400, 0x800010, 0x20400},
	        new ulong [] {0x2040000000000, 0x1020000000, 0x810000, 0x408},
	        new ulong [] {0x2000020000000, 0x2000000010000, 0x2000000000008, 0x41000000000, 0x40000800000, 0x40000000400, 0x1000010000, 0x1000000008, 0x20800000, 0x20000400, 0x800008, 0x10400},
	        new ulong [] {0x2020000000000, 0x1010000000, 0x808000, 0x404},
	        new ulong [] {0x2000010000000, 0x2000000008000, 0x2000000000004, 0x21000000000, 0x20000800000, 0x20000000400, 0x1000008000, 0x1000000004, 0x10800000, 0x10000400, 0x800004, 0x8400},
	        new ulong [] {0x2010000000000, 0x1008000000, 0x804000, 0x402},
	        new ulong [] {0x2000008000000, 0x2000000004000, 0x2000000000002, 0x11000000000, 0x10000800000, 0x10000000400, 0x1000004000, 0x1000000002, 0x8800000, 0x8000400, 0x800002, 0x4400},
	        new ulong [] {0x2008000000000, 0x1004000000, 0x802000, 0x401},
	        new ulong [] {0x2000004000000, 0x2000000002000, 0x2000000000001, 0x9000000000, 0x8000800000, 0x8000000400, 0x1000002000, 0x1000000001, 0x4800000, 0x4000400, 0x800001, 0x2400},
	        new ulong [] {0x1800000000000, 0xC00000000, 0x600000, 0x300},
	        new ulong [] {0x1000400000000, 0x1000000200000, 0x1000000000100, 0x800800000000, 0x800000400000, 0x800000000200, 0x800200000, 0x800000100, 0x400400000, 0x400000200, 0x400100, 0x200200},
	        new ulong [] {0x1400000000000, 0xA00000000, 0x500000, 0x280},
	        new ulong [] {0x1000200000000, 0x1000000100000, 0x1000000000080, 0x400800000000, 0x400000400000, 0x400000000200, 0x800100000, 0x800000080, 0x200400000, 0x200000200, 0x400080, 0x100200},
	        new ulong [] {0x1200000000000, 0x900000000, 0x480000, 0x240},
	        new ulong [] {0x1000100000000, 0x1000000080000, 0x1000000000040, 0x200800000000, 0x200000400000, 0x200000000200, 0x800080000, 0x800000040, 0x100400000, 0x100000200, 0x400040, 0x80200},
	        new ulong [] {0x1100000000000, 0x880000000, 0x440000, 0x220},
	        new ulong [] {0x1000080000000, 0x1000000040000, 0x1000000000020, 0x100800000000, 0x100000400000, 0x100000000200, 0x800040000, 0x800000020, 0x80400000, 0x80000200, 0x400020, 0x40200},
	        new ulong [] {0x1080000000000, 0x840000000, 0x420000, 0x210},
	        new ulong [] {0x1000040000000, 0x1000000020000, 0x1000000000010, 0x80800000000, 0x80000400000, 0x80000000200, 0x800020000, 0x800000010, 0x40400000, 0x40000200, 0x400010, 0x20200},
	        new ulong [] {0x1040000000000, 0x820000000, 0x410000, 0x208},
	        new ulong [] {0x1000020000000, 0x1000000010000, 0x1000000000008, 0x40800000000, 0x40000400000, 0x40000000200, 0x800010000, 0x800000008, 0x20400000, 0x20000200, 0x400008, 0x10200},
	        new ulong [] {0x1020000000000, 0x810000000, 0x408000, 0x204},
	        new ulong [] {0x1000010000000, 0x1000000008000, 0x1000000000004, 0x20800000000, 0x20000400000, 0x20000000200, 0x800008000, 0x800000004, 0x10400000, 0x10000200, 0x400004, 0x8200},
	        new ulong [] {0x1010000000000, 0x808000000, 0x404000, 0x202},
	        new ulong [] {0x1000008000000, 0x1000000004000, 0x1000000000002, 0x10800000000, 0x10000400000, 0x10000000200, 0x800004000, 0x800000002, 0x8400000, 0x8000200, 0x400002, 0x4200},
	        new ulong [] {0x1008000000000, 0x804000000, 0x402000, 0x201},
	        new ulong [] {0x1000004000000, 0x1000000002000, 0x1000000000001, 0x8800000000, 0x8000400000, 0x8000000200, 0x800002000, 0x800000001, 0x4400000, 0x4000200, 0x400001, 0x2200},
	        new ulong [] {0xC00000000000, 0x600000000, 0x300000, 0x180},
	        new ulong [] {0x800200000000, 0x800000100000, 0x800000000080, 0x400400000000, 0x400000200000, 0x400000000100, 0x400100000, 0x400000080, 0x200200000, 0x200000100, 0x200080, 0x100100},
	        new ulong [] {0xA00000000000, 0x500000000, 0x280000, 0x140},
	        new ulong [] {0x800100000000, 0x800000080000, 0x800000000040, 0x200400000000, 0x200000200000, 0x200000000100, 0x400080000, 0x400000040, 0x100200000, 0x100000100, 0x200040, 0x80100},
	        new ulong [] {0x900000000000, 0x480000000, 0x240000, 0x120},
	        new ulong [] {0x800080000000, 0x800000040000, 0x800000000020, 0x100400000000, 0x100000200000, 0x100000000100, 0x400040000, 0x400000020, 0x80200000, 0x80000100, 0x200020, 0x40100},
	        new ulong [] {0x880000000000, 0x440000000, 0x220000, 0x110},
	        new ulong [] {0x800040000000, 0x800000020000, 0x800000000010, 0x80400000000, 0x80000200000, 0x80000000100, 0x400020000, 0x400000010, 0x40200000, 0x40000100, 0x200010, 0x20100},
	        new ulong [] {0x840000000000, 0x420000000, 0x210000, 0x108},
	        new ulong [] {0x800020000000, 0x800000010000, 0x800000000008, 0x40400000000, 0x40000200000, 0x40000000100, 0x400010000, 0x400000008, 0x20200000, 0x20000100, 0x200008, 0x10100},
	        new ulong [] {0x820000000000, 0x410000000, 0x208000, 0x104},
	        new ulong [] {0x800010000000, 0x800000008000, 0x800000000004, 0x20400000000, 0x20000200000, 0x20000000100, 0x400008000, 0x400000004, 0x10200000, 0x10000100, 0x200004, 0x8100},
	        new ulong [] {0x810000000000, 0x408000000, 0x204000, 0x102},
	        new ulong [] {0x800008000000, 0x800000004000, 0x800000000002, 0x10400000000, 0x10000200000, 0x10000000100, 0x400004000, 0x400000002, 0x8200000, 0x8000100, 0x200002, 0x4100},
	        new ulong [] {0x808000000000, 0x404000000, 0x202000, 0x101},
	        new ulong [] {0x800004000000, 0x800000002000, 0x800000000001, 0x8400000000, 0x8000200000, 0x8000000100, 0x400002000, 0x400000001, 0x4200000, 0x4000100, 0x200001, 0x2100},
	        new ulong [] {0x600000000000, 0x300000000, 0x180000, 0xC0},
	        new ulong [] {0x400100000000, 0x400000080000, 0x400000000040, 0x200200000000, 0x200000100000, 0x200000000080, 0x200080000, 0x200000040, 0x100100000, 0x100000080, 0x100040, 0x80080},
	        new ulong [] {0x500000000000, 0x280000000, 0x140000, 0xA0},
	        new ulong [] {0x400080000000, 0x400000040000, 0x400000000020, 0x100200000000, 0x100000100000, 0x100000000080, 0x200040000, 0x200000020, 0x80100000, 0x80000080, 0x100020, 0x40080},
	        new ulong [] {0x480000000000, 0x240000000, 0x120000, 0x90},
	        new ulong [] {0x400040000000, 0x400000020000, 0x400000000010, 0x80200000000, 0x80000100000, 0x80000000080, 0x200020000, 0x200000010, 0x40100000, 0x40000080, 0x100010, 0x20080},
	        new ulong [] {0x440000000000, 0x220000000, 0x110000, 0x88},
	        new ulong [] {0x400020000000, 0x400000010000, 0x400000000008, 0x40200000000, 0x40000100000, 0x40000000080, 0x200010000, 0x200000008, 0x20100000, 0x20000080, 0x100008, 0x10080},
	        new ulong [] {0x420000000000, 0x210000000, 0x108000, 0x84},
	        new ulong [] {0x400010000000, 0x400000008000, 0x400000000004, 0x20200000000, 0x20000100000, 0x20000000080, 0x200008000, 0x200000004, 0x10100000, 0x10000080, 0x100004, 0x8080},
	        new ulong [] {0x410000000000, 0x208000000, 0x104000, 0x82},
	        new ulong [] {0x400008000000, 0x400000004000, 0x400000000002, 0x10200000000, 0x10000100000, 0x10000000080, 0x200004000, 0x200000002, 0x8100000, 0x8000080, 0x100002, 0x4080},
	        new ulong [] {0x408000000000, 0x204000000, 0x102000, 0x81},
	        new ulong [] {0x400004000000, 0x400000002000, 0x400000000001, 0x8200000000, 0x8000100000, 0x8000000080, 0x200002000, 0x200000001, 0x4100000, 0x4000080, 0x100001, 0x2080},
	        new ulong [] {0x300000000000, 0x180000000, 0xC0000, 0x60},
	        new ulong [] {0x200080000000, 0x200000040000, 0x200000000020, 0x100100000000, 0x100000080000, 0x100000000040, 0x100040000, 0x100000020, 0x80080000, 0x80000040, 0x80020, 0x40040},
	        new ulong [] {0x280000000000, 0x140000000, 0xA0000, 0x50},
	        new ulong [] {0x200040000000, 0x200000020000, 0x200000000010, 0x80100000000, 0x80000080000, 0x80000000040, 0x100020000, 0x100000010, 0x40080000, 0x40000040, 0x80010, 0x20040},
	        new ulong [] {0x240000000000, 0x120000000, 0x90000, 0x48},
	        new ulong [] {0x200020000000, 0x200000010000, 0x200000000008, 0x40100000000, 0x40000080000, 0x40000000040, 0x100010000, 0x100000008, 0x20080000, 0x20000040, 0x80008, 0x10040},
	        new ulong [] {0x220000000000, 0x110000000, 0x88000, 0x44},
	        new ulong [] {0x200010000000, 0x200000008000, 0x200000000004, 0x20100000000, 0x20000080000, 0x20000000040, 0x100008000, 0x100000004, 0x10080000, 0x10000040, 0x80004, 0x8040},
	        new ulong [] {0x210000000000, 0x108000000, 0x84000, 0x42},
	        new ulong [] {0x200008000000, 0x200000004000, 0x200000000002, 0x10100000000, 0x10000080000, 0x10000000040, 0x100004000, 0x100000002, 0x8080000, 0x8000040, 0x80002, 0x4040},
	        new ulong [] {0x208000000000, 0x104000000, 0x82000, 0x41},
	        new ulong [] {0x200004000000, 0x200000002000, 0x200000000001, 0x8100000000, 0x8000080000, 0x8000000040, 0x100002000, 0x100000001, 0x4080000, 0x4000040, 0x80001, 0x2040},
	        new ulong [] {0x180000000000, 0xC0000000, 0x60000, 0x30},
	        new ulong [] {0x100040000000, 0x100000020000, 0x100000000010, 0x80080000000, 0x80000040000, 0x80000000020, 0x80020000, 0x80000010, 0x40040000, 0x40000020, 0x40010, 0x20020},
	        new ulong [] {0x140000000000, 0xA0000000, 0x50000, 0x28},
	        new ulong [] {0x100020000000, 0x100000010000, 0x100000000008, 0x40080000000, 0x40000040000, 0x40000000020, 0x80010000, 0x80000008, 0x20040000, 0x20000020, 0x40008, 0x10020},
	        new ulong [] {0x120000000000, 0x90000000, 0x48000, 0x24},
	        new ulong [] {0x100010000000, 0x100000008000, 0x100000000004, 0x20080000000, 0x20000040000, 0x20000000020, 0x80008000, 0x80000004, 0x10040000, 0x10000020, 0x40004, 0x8020},
	        new ulong [] {0x110000000000, 0x88000000, 0x44000, 0x22},
	        new ulong [] {0x100008000000, 0x100000004000, 0x100000000002, 0x10080000000, 0x10000040000, 0x10000000020, 0x80004000, 0x80000002, 0x8040000, 0x8000020, 0x40002, 0x4020},
	        new ulong [] {0x108000000000, 0x84000000, 0x42000, 0x21},
	        new ulong [] {0x100004000000, 0x100000002000, 0x100000000001, 0x8080000000, 0x8000040000, 0x8000000020, 0x80002000, 0x80000001, 0x4040000, 0x4000020, 0x40001, 0x2020},
	        new ulong [] {0xC0000000000, 0x60000000, 0x30000, 0x18},
	        new ulong [] {0x80020000000, 0x80000010000, 0x80000000008, 0x40040000000, 0x40000020000, 0x40000000010, 0x40010000, 0x40000008, 0x20020000, 0x20000010, 0x20008, 0x10010},
	        new ulong [] {0xA0000000000, 0x50000000, 0x28000, 0x14},
	        new ulong [] {0x80010000000, 0x80000008000, 0x80000000004, 0x20040000000, 0x20000020000, 0x20000000010, 0x40008000, 0x40000004, 0x10020000, 0x10000010, 0x20004, 0x8010},
	        new ulong [] {0x90000000000, 0x48000000, 0x24000, 0x12},
	        new ulong [] {0x80008000000, 0x80000004000, 0x80000000002, 0x10040000000, 0x10000020000, 0x10000000010, 0x40004000, 0x40000002, 0x8020000, 0x8000010, 0x20002, 0x4010},
	        new ulong [] {0x88000000000, 0x44000000, 0x22000, 0x11},
	        new ulong [] {0x80004000000, 0x80000002000, 0x80000000001, 0x8040000000, 0x8000020000, 0x8000000010, 0x40002000, 0x40000001, 0x4020000, 0x4000010, 0x20001, 0x2010},
	        new ulong [] {0x60000000000, 0x30000000, 0x18000, 0xC},
	        new ulong [] {0x40010000000, 0x40000008000, 0x40000000004, 0x20020000000, 0x20000010000, 0x20000000008, 0x20008000, 0x20000004, 0x10010000, 0x10000008, 0x10004, 0x8008},
	        new ulong [] {0x50000000000, 0x28000000, 0x14000, 0xA},
	        new ulong [] {0x40008000000, 0x40000004000, 0x40000000002, 0x10020000000, 0x10000010000, 0x10000000008, 0x20004000, 0x20000002, 0x8010000, 0x8000008, 0x10002, 0x4008},
	        new ulong [] {0x48000000000, 0x24000000, 0x12000, 0x9},
	        new ulong [] {0x40004000000, 0x40000002000, 0x40000000001, 0x8020000000, 0x8000010000, 0x8000000008, 0x20002000, 0x20000001, 0x4010000, 0x4000008, 0x10001, 0x2008},
	        new ulong [] {0x30000000000, 0x18000000, 0xC000, 0x6},
	        new ulong [] {0x20008000000, 0x20000004000, 0x20000000002, 0x10010000000, 0x10000008000, 0x10000000004, 0x10004000, 0x10000002, 0x8008000, 0x8000004, 0x8002, 0x4004},
	        new ulong [] {0x28000000000, 0x14000000, 0xA000, 0x5},
	        new ulong [] {0x20004000000, 0x20000002000, 0x20000000001, 0x8010000000, 0x8000008000, 0x8000000004, 0x10002000, 0x10000001, 0x4008000, 0x4000004, 0x8001, 0x2004},
	        new ulong [] {0x18000000000, 0xC000000, 0x6000, 0x3},
	        new ulong [] {0x10004000000, 0x10000002000, 0x10000000001, 0x8008000000, 0x8000004000, 0x8000000002, 0x8002000, 0x8000001, 0x4004000, 0x4000002, 0x4001, 0x2002}
        };

        #endregion

        #region Pocket169 Enumeration
        /// <summary>
        /// An enumeration value for each of the 169 possible types of pocket cards.
        /// </summary>
        public enum PocketHand169Enum : int
        {
            /// <summary>
            /// Not a PocketPairType
            /// </summary>
            None = -1,
            /// <summary>
            /// Represents a pair of Aces (Pocket Rockets)
            /// </summary>
            PocketAA = 0,
            /// <summary>
            /// Represents a pair of Kings (Cowboys)
            /// </summary>
            PocketKK = 1,
            /// <summary>
            /// Represents a pair of Queens (Ladies)
            /// </summary>
            PocketQQ = 2,
            /// <summary>
            /// Represents a pair of Jacks (Fish hooks)
            /// </summary>
            PocketJJ = 3,
            /// <summary>
            /// Represents a pair of Tens (Rin Tin Tin)
            /// </summary>
            PocketTT = 4,
            /// <summary>
            /// Represents a pair of Nines (Gretzky)
            /// </summary>
            Pocket99 = 5,
            /// <summary>
            /// Represents a pair of Eights (Snowmen)
            /// </summary>
            Pocket88 = 6,
            /// <summary>
            /// Represents a pair of Sevens (Hockey Sticks)
            /// </summary>
            Pocket77 = 7,
            /// <summary>
            /// Represents a pair of Sixes (Route 66)
            /// </summary>
            Pocket66 = 8,
            /// <summary>
            /// Represents a pair of Fives (Speed Limit)
            /// </summary>
            Pocket55 = 9,
            /// <summary>
            /// Represents a pair of Fours (Sailboats)
            /// </summary>
            Pocket44 = 10,
            /// <summary>
            /// Represents a pair of Threes (Crabs)
            /// </summary>
            Pocket33 = 11,
            /// <summary>
            /// Represents a pair of Twos (Ducks)
            /// </summary>
            Pocket22 = 12,
            /// <summary>
            /// Represents Ace/King Suited (Big Slick)
            /// </summary>
            PocketAKs = 13,
            /// <summary>
            /// Represents Ace/King offsuit (Big Slick)
            /// </summary>
            PocketAKo = 14,
            /// <summary>
            /// Represents Ace/Queen Suited (Little Slick)
            /// </summary>
            PocketAQs = 15,
            /// <summary>
            /// Represents Ace/Queen offsuit (Little Slick)
            /// </summary>
            PocketAQo = 16,
            /// <summary>
            /// Represents Ace/Jack suited (Blackjack)
            /// </summary>
            PocketAJs = 17,
            /// <summary>
            /// Represents Ace/Jack offsuit (Blackjack)
            /// </summary>
            PocketAJo = 18,
            /// <summary>
            /// Represents Ace/Ten suited (Johnny Moss)
            /// </summary>
            PocketATs = 19,
            /// <summary>
            /// Represents Ace/Ten offsuit (Johnny Moss)
            /// </summary>
            PocketATo = 20,
            /// <summary>
            /// Represents Ace/Nine suited
            /// </summary>
            PocketA9s = 21,
            /// <summary>
            /// Represents Ace/Nine offsuit
            /// </summary>
            PocketA9o = 22,
            /// <summary>
            /// Represents Ace/Eight suited
            /// </summary>
            PocketA8s = 23,
            /// <summary>
            /// Represents Ace/Eight offsuit
            /// </summary>
            PocketA8o = 24,
            /// <summary>
            /// Represents Ace/seven suited
            /// </summary>
            PocketA7s = 25,
            /// <summary>
            /// Represents Ace/seven offsuit
            /// </summary>
            PocketA7o = 26,
            /// <summary>
            /// Represents Ace/Six suited
            /// </summary>
            PocketA6s = 27,
            /// <summary>
            /// Represents Ace/Six offsuit
            /// </summary>
            PocketA6o = 28,
            /// <summary>
            /// Represents Ace/Five suited
            /// </summary>
            PocketA5s = 29,
            /// <summary>
            /// Represents Ace/Five offsuit
            /// </summary>
            PocketA5o = 30,
            /// <summary>
            /// Represents Ace/Four suited
            /// </summary>
            PocketA4s = 31,
            /// <summary>
            /// Represents Ace/Four offsuit
            /// </summary>
            PocketA4o = 32,
            /// <summary>
            /// Represents Ace/Three suited
            /// </summary>
            PocketA3s = 33,
            /// <summary>
            /// Represents Ace/Three offsuit
            /// </summary>
            PocketA3o = 34,
            /// <summary>
            /// Represents Ace/Two suited
            /// </summary>
            PocketA2s = 35,
            /// <summary>
            /// Represents Ace/Two offsuit
            /// </summary>
            PocketA2o = 36,
            /// <summary>
            /// Represents King/Queen suited
            /// </summary>
            PocketKQs = 37,
            /// <summary>
            /// Represents King/Queen offsuit
            /// </summary>
            PocketKQo = 38,
            /// <summary>
            /// Represents King/Jack suited
            /// </summary>
            PocketKJs = 39,
            /// <summary>
            /// Represents King/Jack offsuit
            /// </summary>
            PocketKJo = 40,
            /// <summary>
            /// Represents King/Ten suited
            /// </summary>
            PocketKTs = 41,
            /// <summary>
            /// Represents King/Ten offsuit
            /// </summary>
            PocketKTo = 42,
            /// <summary>
            /// Represents King/Nine suited
            /// </summary>
            PocketK9s = 43,
            /// <summary>
            /// Represents King/Nine offsuit
            /// </summary>
            PocketK9o = 44,
            /// <summary>
            /// Represents King/Eight suited
            /// </summary>
            PocketK8s = 45,
            /// <summary>
            /// Represents King/Eight offsuit
            /// </summary>
            PocketK8o = 46,
            /// <summary>
            /// Represents King/Seven suited
            /// </summary>
            PocketK7s = 47,
            /// <summary>
            /// Represents King/Seven offsuit
            /// </summary>
            PocketK7o = 48,
            /// <summary>
            /// Represents King/Six suited
            /// </summary>
            PocketK6s = 49,
            /// <summary>
            /// Represents King/Six offsuit
            /// </summary>
            PocketK6o = 50,
            /// <summary>
            /// Represents King/Five suited
            /// </summary>
            PocketK5s = 51,
            /// <summary>
            /// Represents King/Five offsuit
            /// </summary>
            PocketK5o = 52,
            /// <summary>
            /// Represents King/Four suited
            /// </summary>
            PocketK4s = 53,
            /// <summary>
            /// Represents King/Four offsuit
            /// </summary>
            PocketK4o = 54,
            /// <summary>
            /// Represents King/Three suited
            /// </summary>
            PocketK3s = 55,
            /// <summary>
            /// Represents King/Three offsuit
            /// </summary>
            PocketK3o = 56,
            /// <summary>
            /// Represents King/Two suited
            /// </summary>
            PocketK2s = 57,
            /// <summary>
            /// Represents King/Two offsuit
            /// </summary>
            PocketK2o = 58,
            /// <summary>
            /// Represents Queen/Jack suited
            /// </summary>
            PocketQJs = 59,
            /// <summary>
            /// Represents Queen/Jack offsuit
            /// </summary>
            PocketQJo = 60,
            /// <summary>
            /// Represents Queen/Ten suited
            /// </summary>
            PocketQTs = 61,
            /// <summary>
            /// Represents Queen/Ten offsuit
            /// </summary>
            PocketQTo = 62,
            /// <summary>
            /// Represents Queen/Nine suited
            /// </summary>
            PocketQ9s = 63,
            /// <summary>
            /// Represents Queen/Nine offsuit
            /// </summary>
            PocketQ9o = 64,
            /// <summary>
            /// Represents Queen/Eight suited
            /// </summary>
            PocketQ8s = 65,
            /// <summary>
            /// Represents Queen/Eight offsuit
            /// </summary>
            PocketQ8o = 66,
            /// <summary>
            /// Represents Queen/Seven suited
            /// </summary>
            PocketQ7s = 67,
            /// <summary>
            /// Represents Queen/Seven offsuit
            /// </summary>
            PocketQ7o = 68,
            /// <summary>
            /// Represents Queen/Six suited
            /// </summary>
            PocketQ6s = 69,
            /// <summary>
            /// Represents Queen/Six offsuit
            /// </summary>
            PocketQ6o = 70,
            /// <summary>
            /// Represents Queen/Five suited
            /// </summary>
            PocketQ5s = 71,
            /// <summary>
            /// Represents Queen/Five offsuit
            /// </summary>
            PocketQ5o = 72,
            /// <summary>
            /// Represents Queen/Four suited
            /// </summary>
            PocketQ4s = 73,
            /// <summary>
            /// Represents Queen/Four offsuit
            /// </summary>
            PocketQ4o = 74,
            /// <summary>
            /// Represents Queen/Three suited
            /// </summary>
            PocketQ3s = 75,
            /// <summary>
            /// Represents Queen/Three offsuit
            /// </summary>
            PocketQ3o = 76,
            /// <summary>
            /// Represents Queen/Two suited
            /// </summary>
            PocketQ2s = 77,
            /// <summary>
            /// Represents Queen/Two offsuit
            /// </summary>
            PocketQ2o = 78,
            /// <summary>
            /// Represents Jack/Ten suited
            /// </summary>
            PocketJTs = 79,
            /// <summary>
            /// Represents Jack/Ten offsuit
            /// </summary>
            PocketJTo = 80,
            /// <summary>
            /// Represents Jack/Nine suited
            /// </summary>
            PocketJ9s = 81,
            /// <summary>
            /// Represents Jack/Nine offsuit
            /// </summary>
            PocketJ9o = 82,
            /// <summary>
            /// Represents Jack/Eight suited
            /// </summary>
            PocketJ8s = 83,
            /// <summary>
            /// Represents Jack/Eight offsuit
            /// </summary>
            PocketJ8o = 84,
            /// <summary>
            /// Represents Jack/Seven suited
            /// </summary>
            PocketJ7s = 85,
            /// <summary>
            /// Represents Jack/Seven offsuit
            /// </summary>
            PocketJ7o = 86,
            /// <summary>
            /// Represents Jack/Six suited
            /// </summary>
            PocketJ6s = 87,
            /// <summary>
            /// Represents Jack/Six offsuit
            /// </summary>
            PocketJ6o = 88,
            /// <summary>
            /// Represents Jack/Five suited
            /// </summary>
            PocketJ5s = 89,
            /// <summary>
            /// Represents Jack/Five offsuit
            /// </summary>
            PocketJ5o = 90,
            /// <summary>
            /// Represents Jack/Four suited.
            /// </summary>
            PocketJ4s = 91,
            /// <summary>
            /// Represents Jack/Four offsuit
            /// </summary>
            PocketJ4o = 92,
            /// <summary>
            /// Represents Jack/Three suited
            /// </summary>
            PocketJ3s = 93,
            /// <summary>
            /// Represents Jack/Three offsuit
            /// </summary>
            PocketJ3o = 94,
            /// <summary>
            /// Represents Jack/Two suited.
            /// </summary>
            PocketJ2s = 95,
            /// <summary>
            /// Represents Jack/Two offsuit
            /// </summary>
            PocketJ2o = 96,
            /// <summary>
            /// Represents Ten/Nine suited
            /// </summary>
            PocketT9s = 97,
            /// <summary>
            /// Represents Ten/Nine offsuit
            /// </summary>
            PocketT9o = 98,
            /// <summary>
            /// Represents Ten/Eigth suited
            /// </summary>
            PocketT8s = 99,
            /// <summary>
            /// Represents Ten/Eight offsuit
            /// </summary>
            PocketT8o = 100,
            /// <summary>
            /// Represents Ten/Seven suited
            /// </summary>
            PocketT7s = 101,
            /// <summary>
            /// Represents Ten/Seven offsuit
            /// </summary>
            PocketT7o = 102,
            /// <summary>
            /// Represents Ten/Six suited
            /// </summary>
            PocketT6s = 103,
            /// <summary>
            /// Represents Ten/Six offsuit
            /// </summary>
            PocketT6o = 104,
            /// <summary>
            /// Represents Ten/Five suited
            /// </summary>
            PocketT5s = 105,
            /// <summary>
            /// Represents Ten/Five offsuit
            /// </summary>
            PocketT5o = 106,
            /// <summary>
            /// Represents Ten/Four suited
            /// </summary>
            PocketT4s = 107,
            /// <summary>
            /// Represents Ten/Four offsuit
            /// </summary>
            PocketT4o = 108,
            /// <summary>
            /// Represents Ten/Three suited
            /// </summary>
            PocketT3s = 109,
            /// <summary>
            /// Represents Ten/Three offsuit
            /// </summary>
            PocketT3o = 110,
            /// <summary>
            /// Represents Ten/Two suited
            /// </summary>
            PocketT2s = 111,
            /// <summary>
            /// Represents Ten/Two offsuit
            /// </summary>
            PocketT2o = 112,
            /// <summary>
            /// Represents Nine/Eight suited
            /// </summary>
            Pocket98s = 113,
            /// <summary>
            /// Represents Nine/Eight offsuit
            /// </summary>
            Pocket98o = 114,
            /// <summary>
            /// Represents Nine/Seven suited
            /// </summary>
            Pocket97s = 115,
            /// <summary>
            /// Represents Nine/Seven offsuit
            /// </summary>
            Pocket97o = 116,
            /// <summary>
            /// Represents Nine/Six suited
            /// </summary>
            Pocket96s = 117,
            /// <summary>
            /// Represents Nine/Six offsuit
            /// </summary>
            Pocket96o = 118,
            /// <summary>
            /// Represents Nine/Five suited
            /// </summary>
            Pocket95s = 119,
            /// <summary>
            /// Represents Nine/Five offsuit
            /// </summary>
            Pocket95o = 120,
            /// <summary>
            /// Represents Nine/Four suited
            /// </summary>
            Pocket94s = 121,
            /// <summary>
            /// Represents Nine/Four offsuit
            /// </summary>
            Pocket94o = 122,
            /// <summary>
            /// Represents Nine/Three suited
            /// </summary>
            Pocket93s = 123,
            /// <summary>
            /// Represents Nine/Three offsuit
            /// </summary>
            Pocket93o = 124,
            /// <summary>
            /// Represents Nine/Two suited
            /// </summary>
            Pocket92s = 125,
            /// <summary>
            /// Represents Nine/Two offsuit
            /// </summary>
            Pocket92o = 126,
            /// <summary>
            /// Represents Eight/Seven Suited.
            /// </summary>
            Pocket87s = 127,
            /// <summary>
            /// Represents Eight/Seven offsuit
            /// </summary>
            Pocket87o = 128,
            /// <summary>
            /// Represents Eight/Six suited
            /// </summary>
            Pocket86s = 129,
            /// <summary>
            /// Represents Eight/Six offsuit
            /// </summary>
            Pocket86o = 130,
            /// <summary>
            /// Represents Eight/Five suited
            /// </summary>
            Pocket85s = 131,
            /// <summary>
            /// Represents Eight/Five offsuit
            /// </summary>
            Pocket85o = 132,
            /// <summary>
            /// Represents Eight/Four suited
            /// </summary>
            Pocket84s = 133,
            /// <summary>
            /// Represents Eight/Four offsuit
            /// </summary>
            Pocket84o = 134,
            /// <summary>
            /// Represents Eight/Three suited
            /// </summary>
            Pocket83s = 135,
            /// <summary>
            /// Represents Eight/Three offsuit
            /// </summary>
            Pocket83o = 136,
            /// <summary>
            /// Represents Eight/Two suited
            /// </summary>
            Pocket82s = 137,
            /// <summary>
            /// Represents Eight/Two offsuit
            /// </summary>
            Pocket82o = 138,
            /// <summary>
            /// Represents Seven/Six suited
            /// </summary>
            Pocket76s = 139,
            /// <summary>
            /// Represents Seven/Six offsuit
            /// </summary>
            Pocket76o = 140,
            /// <summary>
            /// Represents Seven/Five suited
            /// </summary>
            Pocket75s = 141,
            /// <summary>
            /// Represents Seven/Five offsuit
            /// </summary>
            Pocket75o = 142,
            /// <summary>
            /// Represents Seven/Four suited
            /// </summary>
            Pocket74s = 143,
            /// <summary>
            /// Represents Seven/Four offsuit
            /// </summary>
            Pocket74o = 144,
            /// <summary>
            /// Represents Seven/Three suited
            /// </summary>
            Pocket73s = 145,
            /// <summary>
            /// Represents Seven/Three offsuit
            /// </summary>
            Pocket73o = 146,
            /// <summary>
            /// Represents Seven/Two suited
            /// </summary>
            Pocket72s = 147,
            /// <summary>
            /// Represents Seven/Two offsuit
            /// </summary>
            Pocket72o = 148,
            /// <summary>
            /// Represents Six/Five suited
            /// </summary>
            Pocket65s = 149,
            /// <summary>
            /// Represents Six/Five offsuit
            /// </summary>
            Pocket65o = 150,
            /// <summary>
            /// Represents Six/Four suited
            /// </summary>
            Pocket64s = 151,
            /// <summary>
            /// Represents Six/Four offsuit
            /// </summary>
            Pocket64o = 152,
            /// <summary>
            /// Represents Six/Three suited
            /// </summary>
            Pocket63s = 153,
            /// <summary>
            /// Represents Six/Three offsuit
            /// </summary>
            Pocket63o = 154,
            /// <summary>
            /// Represents Six/Two suited
            /// </summary>
            Pocket62s = 155,
            /// <summary>
            /// Represents Six/Two offsuit
            /// </summary>
            Pocket62o = 156,
            /// <summary>
            /// Represents Five/Four suited
            /// </summary>
            Pocket54s = 157,
            /// <summary>
            /// Represents Five/Four offsuit
            /// </summary>
            Pocket54o = 158,
            /// <summary>
            /// Represents Five/Three suited
            /// </summary>
            Pocket53s = 159,
            /// <summary>
            /// Represents Five/Three offsuit
            /// </summary>
            Pocket53o = 160,
            /// <summary>
            /// Represents Five/Two suited
            /// </summary>
            Pocket52s = 161,
            /// <summary>
            /// Represents Five/Two offsuit
            /// </summary>
            Pocket52o = 162,
            /// <summary>
            /// Represent Four/Three suited
            /// </summary>
            Pocket43s = 163,
            /// <summary>
            /// Represents Four/Three offsuit
            /// </summary>
            Pocket43o = 164,
            /// <summary>
            /// Represents Four/Two suited
            /// </summary>
            Pocket42s = 165,
            /// <summary>
            /// Represents Four/Two offsuit
            /// </summary>
            Pocket42o = 166,
            /// <summary>
            /// Represents Three/Two suited
            /// </summary>
            Pocket32s = 167,
            /// <summary>
            /// Represents Three/Two offsuit
            /// </summary>
            Pocket32o = 168
        };
        #endregion

        #region Pocket169 Mask/Enum Lookup

        /// <exclude/>
        static private Dictionary<ulong, PocketHand169Enum> pocketdict = new Dictionary<ulong, PocketHand169Enum>();

        /// <summary>
        /// Given a pocket pair mask, the PocketPairType cooresponding to this mask
        /// will be returned. 
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static PocketHand169Enum PocketHand169Type(ulong mask)
        {
#if DEBUG
            // mask must contain exactly 2 cards
            if (BitCount(mask) != 2)
                throw new ArgumentOutOfRangeException("mask");
#endif

            // Fill in dictionary
            if (pocketdict.Count == 0)
            {
                for (int i = 0; i < Pocket169Table.Length; i++)
                {
                    foreach (ulong tmask in Pocket169Table[i])
                    {
                        pocketdict.Add(tmask, (PocketHand169Enum)i);
                    }
                }
            }

            if (pocketdict.ContainsKey(mask))
                return pocketdict[mask];

            return PocketHand169Enum.None;
        }
        #endregion
        #endregion

        #region Hands Enumerators

        /// <summary>
        /// Enables a foreach command to enumerate all possible ncard hands.
        /// </summary>
        /// <param name="numberOfCards">the number of cards in the hand (must be between 1 and 7)</param>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// // This method iterates through all possible 5 card hands and returns a count.
        /// public static long CountFiveCardHands()
        /// {
        ///     long count = 0;
        /// 
        ///     // Iterate through all possible 5 card hands
        ///     foreach (ulong mask in Hands(5))
        ///     {
        ///         count++;
        ///     }
        ///
        ///     // Validate results.
        ///     System.Diagnostics.Debug.Assert(count == 2598960);
        ///     return count;
        /// }
        /// </code>
        /// </example>
        public static IEnumerable<ulong> Hands(int numberOfCards)
        {
            int _i1, _i2, _i3, _i4, _i5, _i6, _i7, length;
            ulong _card1, _n2, _n3, _n4, _n5, _n6;

#if DEBUG
            // We only support 0-7 cards
            if (numberOfCards < 0 || numberOfCards > 7)
                throw new ArgumentOutOfRangeException("numberOfCards");
#endif

            switch (numberOfCards)
            {
                case 7:
                    for (_i1 = NumberOfCards - 1; _i1 >= 0; _i1--)
                    {
                        _card1 = CardMasksTable[_i1];
                        for (_i2 = _i1 - 1; _i2 >= 0; _i2--)
                        {
                            _n2 = _card1 | CardMasksTable[_i2];
                            for (_i3 = _i2 - 1; _i3 >= 0; _i3--)
                            {
                                _n3 = _n2 | CardMasksTable[_i3];
                                for (_i4 = _i3 - 1; _i4 >= 0; _i4--)
                                {
                                    _n4 = _n3 | CardMasksTable[_i4];
                                    for (_i5 = _i4 - 1; _i5 >= 0; _i5--)
                                    {
                                        _n5 = _n4 | CardMasksTable[_i5];
                                        for (_i6 = _i5 - 1; _i6 >= 0; _i6--)
                                        {
                                            _n6 = _n5 | CardMasksTable[_i6];
                                            for (_i7 = _i6 - 1; _i7 >= 0; _i7--)
                                            {
                                                yield return _n6 | CardMasksTable[_i7];
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case 6:
                    for (_i1 = NumberOfCards - 1; _i1 >= 0; _i1--)
                    {
                        _card1 = CardMasksTable[_i1];
                        for (_i2 = _i1 - 1; _i2 >= 0; _i2--)
                        {
                            _n2 = _card1 | CardMasksTable[_i2];
                            for (_i3 = _i2 - 1; _i3 >= 0; _i3--)
                            {
                                _n3 = _n2 | CardMasksTable[_i3];
                                for (_i4 = _i3 - 1; _i4 >= 0; _i4--)
                                {
                                    _n4 = _n3 | CardMasksTable[_i4];
                                    for (_i5 = _i4 - 1; _i5 >= 0; _i5--)
                                    {
                                        _n5 = _n4 | CardMasksTable[_i5];
                                        for (_i6 = _i5 - 1; _i6 >= 0; _i6--)
                                        {
                                            yield return _n5 | CardMasksTable[_i6];
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case 5:
                    for (_i1 = NumberOfCards - 1; _i1 >= 0; _i1--)
                    {
                        _card1 = CardMasksTable[_i1];
                        for (_i2 = _i1 - 1; _i2 >= 0; _i2--)
                        {
                            _n2 = _card1 | CardMasksTable[_i2];
                            for (_i3 = _i2 - 1; _i3 >= 0; _i3--)
                            {
                                _n3 = _n2 | CardMasksTable[_i3];
                                for (_i4 = _i3 - 1; _i4 >= 0; _i4--)
                                {
                                    _n4 = _n3 | CardMasksTable[_i4];
                                    for (_i5 = _i4 - 1; _i5 >= 0; _i5--)
                                    {
                                        yield return _n4 | CardMasksTable[_i5];
                                    }
                                }
                            }
                        }
                    }
                    break;
                case 4:
                    for (_i1 = NumberOfCards - 1; _i1 >= 0; _i1--)
                    {
                        _card1 = CardMasksTable[_i1];
                        for (_i2 = _i1 - 1; _i2 >= 0; _i2--)
                        {
                            _n2 = _card1 | CardMasksTable[_i2];
                            for (_i3 = _i2 - 1; _i3 >= 0; _i3--)
                            {
                                _n3 = _n2 | CardMasksTable[_i3];
                                for (_i4 = _i3 - 1; _i4 >= 0; _i4--)
                                {
                                    yield return _n3 | CardMasksTable[_i4];
                                }
                            }
                        }
                    }

                    break;
                case 3:
                    for (_i1 = NumberOfCards - 1; _i1 >= 0; _i1--)
                    {
                        _card1 = CardMasksTable[_i1];
                        for (_i2 = _i1 - 1; _i2 >= 0; _i2--)
                        {
                            _n2 = _card1 | CardMasksTable[_i2];
                            for (_i3 = _i2 - 1; _i3 >= 0; _i3--)
                            {
                                yield return _n2 | CardMasksTable[_i3];
                            }
                        }
                    }
                    break;
                case 2:
                    for (_i1 = NumberOfCards - 1; _i1 >= 0; _i1--)
                    {
                        _card1 = CardMasksTable[_i1];
                        for (_i2 = _i1 - 1; _i2 >= 0; _i2--)
                        {
                            yield return _card1 | CardMasksTable[_i2];
                        }
                    }
                    break;
                case 1:
                    length = CardMasksTable.Length;
                    for (_i1 = 0; _i1 < length; _i1++)
                    {
                        yield return CardMasksTable[_i1];
                    }
                    break;
                default:
                    Debug.Assert(false);
                    yield return 0UL;
                    break;
            }
        }

        /// <summary>
        /// Enables a foreach command to enumerate all possible ncard hands.
        /// </summary>
        /// <param name="shared">A bitfield containing the cards that must be in the enumerated hands</param>
        /// <param name="dead">A bitfield containing the cards that must not be in the enumerated hands</param>
        /// <param name="numberOfCards">the number of cards in the hand (must be between 1 and 7)</param>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// // Counts all remaining hands in a 7 card holdem hand.
        /// static long CountHands(string partialHand)
        /// {
        ///     long count = 0;
        /// 
        ///     // Parse hand and create a mask
        ///     ulong partialHandmask = Hand.ParseHand(partialHand);
        /// 
        ///     // iterate through all 7 card hands that share the cards in our partial hand.
        ///    foreach (ulong handmask in Hand.Hands(partialHandmask, 0UL, 7))
        ///    {
        ///        count++;
        ///    }
        /// 
        ///    return count;
        ///  }
        /// </code>
        /// </example>
        public static IEnumerable<ulong> Hands(ulong shared, ulong dead, int numberOfCards)
        {
            int _i1, _i2, _i3, _i4, _i5, _i6, _i7, length;
            ulong _card1, _card2, _card3, _card4, _card5, _card6, _card7;
            ulong _n2, _n3, _n4, _n5, _n6;

            dead |= shared;

            switch (numberOfCards - BitCount(shared))
            {
                case 7:
                    for (_i1 = NumberOfCards - 1; _i1 >= 0; _i1--)
                    {
                        _card1 = CardMasksTable[_i1];
                        if ((dead & _card1) != 0) continue;
                        for (_i2 = _i1 - 1; _i2 >= 0; _i2--)
                        {
                            _card2 = CardMasksTable[_i2];
                            if ((dead & _card2) != 0) continue;
                            _n2 = _card1 | _card2;
                            for (_i3 = _i2 - 1; _i3 >= 0; _i3--)
                            {
                                _card3 = CardMasksTable[_i3];
                                if ((dead & _card3) != 0) continue;
                                _n3 = _n2 | _card3;
                                for (_i4 = _i3 - 1; _i4 >= 0; _i4--)
                                {
                                    _card4 = CardMasksTable[_i4];
                                    if ((dead & _card4) != 0) continue;
                                    _n4 = _n3 | _card4;
                                    for (_i5 = _i4 - 1; _i5 >= 0; _i5--)
                                    {
                                        _card5 = CardMasksTable[_i5];
                                        if ((dead & _card5) != 0) continue;
                                        _n5 = _n4 | _card5;
                                        for (_i6 = _i5 - 1; _i6 >= 0; _i6--)
                                        {
                                            _card6 = CardMasksTable[_i6];
                                            if ((dead & _card6) != 0) continue;
                                            _n6 = _n5 | _card6;
                                            for (_i7 = _i6 - 1; _i7 >= 0; _i7--)
                                            {
                                                _card7 = CardMasksTable[_i7];
                                                if ((dead & _card7) != 0) continue;
                                                yield return _n6 | _card7 | shared;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case 6:
                    for (_i1 = NumberOfCards - 1; _i1 >= 0; _i1--)
                    {
                        _card1 = CardMasksTable[_i1];
                        if ((dead & _card1) != 0) continue;
                        for (_i2 = _i1 - 1; _i2 >= 0; _i2--)
                        {
                            _card2 = CardMasksTable[_i2];
                            if ((dead & _card2) != 0) continue;
                            _n2 = _card1 | _card2;
                            for (_i3 = _i2 - 1; _i3 >= 0; _i3--)
                            {
                                _card3 = CardMasksTable[_i3];
                                if ((dead & _card3) != 0) continue;
                                _n3 = _n2 | _card3;
                                for (_i4 = _i3 - 1; _i4 >= 0; _i4--)
                                {
                                    _card4 = CardMasksTable[_i4];
                                    if ((dead & _card4) != 0) continue;
                                    _n4 = _n3 | _card4;
                                    for (_i5 = _i4 - 1; _i5 >= 0; _i5--)
                                    {
                                        _card5 = CardMasksTable[_i5];
                                        if ((dead & _card5) != 0) continue;
                                        _n5 = _n4 | _card5;
                                        for (_i6 = _i5 - 1; _i6 >= 0; _i6--)
                                        {
                                            _card6 = CardMasksTable[_i6];
                                            if ((dead & _card6) != 0)
                                                continue;
                                            yield return _n5 | _card6 | shared;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case 5:
                    for (_i1 = NumberOfCards - 1; _i1 >= 0; _i1--)
                    {
                        _card1 = CardMasksTable[_i1];
                        if ((dead & _card1) != 0) continue;
                        for (_i2 = _i1 - 1; _i2 >= 0; _i2--)
                        {
                            _card2 = CardMasksTable[_i2];
                            if ((dead & _card2) != 0) continue;
                            _n2 = _card1 | _card2;
                            for (_i3 = _i2 - 1; _i3 >= 0; _i3--)
                            {
                                _card3 = CardMasksTable[_i3];
                                if ((dead & _card3) != 0) continue;
                                _n3 = _n2 | _card3;
                                for (_i4 = _i3 - 1; _i4 >= 0; _i4--)
                                {
                                    _card4 = CardMasksTable[_i4];
                                    if ((dead & _card4) != 0) continue;
                                    _n4 = _n3 | _card4;
                                    for (_i5 = _i4 - 1; _i5 >= 0; _i5--)
                                    {
                                        _card5 = CardMasksTable[_i5];
                                        if ((dead & _card5) != 0) continue;
                                        yield return _n4 | _card5 | shared;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case 4:
                    for (_i1 = NumberOfCards - 1; _i1 >= 0; _i1--)
                    {
                        _card1 = CardMasksTable[_i1];
                        if ((dead & _card1) != 0) continue;
                        for (_i2 = _i1 - 1; _i2 >= 0; _i2--)
                        {
                            _card2 = CardMasksTable[_i2];
                            if ((dead & _card2) != 0) continue;
                            _n2 = _card1 | _card2;
                            for (_i3 = _i2 - 1; _i3 >= 0; _i3--)
                            {
                                _card3 = CardMasksTable[_i3];
                                if ((dead & _card3) != 0) continue;
                                _n3 = _n2 | _card3;
                                for (_i4 = _i3 - 1; _i4 >= 0; _i4--)
                                {
                                    _card4 = CardMasksTable[_i4];
                                    if ((dead & _card4) != 0) continue;
                                    yield return _n3 | _card4 | shared;
                                }
                            }
                        }
                    }

                    break;
                case 3:
                    for (_i1 = NumberOfCards - 1; _i1 >= 0; _i1--)
                    {
                        _card1 = CardMasksTable[_i1];
                        if ((dead & _card1) != 0) continue;
                        for (_i2 = _i1 - 1; _i2 >= 0; _i2--)
                        {
                            _card2 = CardMasksTable[_i2];
                            if ((dead & _card2) != 0) continue;
                            _n2 = _card1 | _card2;
                            for (_i3 = _i2 - 1; _i3 >= 0; _i3--)
                            {
                                _card3 = CardMasksTable[_i3];
                                if ((dead & _card3) != 0) continue;
                                yield return _n2 | _card3 | shared;
                            }
                        }
                    }
                    break;
                case 2:
                    for (_i1 = NumberOfCards - 1; _i1 >= 0; _i1--)
                    {
                        _card1 = CardMasksTable[_i1];
                        for (_i2 = _i1 - 1; _i2 >= 0; _i2--)
                        {
                            yield return _card1 | CardMasksTable[_i2];
                        }
                    }
                    break;
                case 1:
                    length = CardMasksTable.Length;
                    for (_i1 = 0; _i1 < length; _i1++)
                    {
                        _card1 = CardMasksTable[_i1];
                        if ((dead & _card1) != 0) continue;
                        yield return _card1 | shared;
                    }
                    break;
                case 0:
                    yield return shared;
                    break;
                default:
                    yield return 0UL;
                    break;
            }
        }

        #endregion

        #region Random Enumerators

        /// <summary>
        /// Returns a rand hand with the specified number of cards and constrained
        /// to not contain any of the passed dead cards.
        /// </summary>
        /// <param name="dead">Mask for the cards that must not be returned.</param>
        /// <param name="ncards">The number of cards to return in this hand.</param>
        /// <param name="rand">An instance of the Random class.</param>
        /// <returns>A randomly chosen hand containing the number of cards requested.</returns>
        static private ulong GetRandomHand(ulong dead, int ncards, Random rand)
        {
            // Return a random hand.
            ulong mask = 0UL, card;

            for (int i = 0; i < ncards; i++)
            {
                do
                {
                    card = CardMasksTable[rand.Next(52)];
                } while (((dead | mask) & card) != 0);
                mask |= card;
            }

            return mask;
        }

        /// <summary>
        /// This function iterates through random hands returning the number of random hands specified
        /// in trials. Please note that a mask can be repeated.
        /// </summary>
        /// <param name="shared">Cards that must be in the hand.</param>
        /// <param name="dead">Cards that must not be in the hand.</param>
        /// <param name="ncards">The total number of cards in the hand.</param>
        /// <param name="trials">The total number of random hands to return.</param>
        /// <returns>Returns a random hand mask meeting the input specifications.</returns>
        public static IEnumerable<ulong> RandomHands(ulong shared, ulong dead, int ncards, int trials)
        {
#if DEBUG
            // Check Preconditions
            if (ncards < 0 || ncards > 7)
                throw new ArgumentOutOfRangeException("ncards");
#endif

            ulong deadmask = dead | shared;
            int cardcount = ncards - RefEvaluator.BitCount(shared);
            System.Random rand = new Random();

            for (int count = 0; count < trials; count++)
            {
                yield return GetRandomHand(deadmask, cardcount, rand) | shared;
            }
        }

        /// <summary>
        /// Iterates through random hands with ncards number of cards. This iterator
        /// will return the number of masks specifed in trials. Masks can be repeated.
        /// </summary>
        /// <param name="ncards">Number of cards required to be in the hand.</param>
        /// <param name="trials">Number of total mask to return.</param>
        /// <returns>A random hand as a hand mask.</returns>
        public static IEnumerable<ulong> RandomHands(int ncards, int trials)
        {
            return RandomHands(0UL, 0UL, ncards, trials);
        }

        /// <summary>
        /// C# Interop call to Win32 QueryPerformanceCount. This function should be removed
        /// if you need an interop free class definition.
        /// </summary>
        /// <param name="lpPerformanceCount">returns performance counter</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        /// <summary>
        /// C# Interop call to Win32 QueryPerformanceFrequency. This function should be removed
        /// if you need an interop free class definition.
        /// </summary>
        /// <param name="lpFrequency">returns performance frequence</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        /// <summary>
        /// Iterates through random hands that meets the specified requirements until the specified
        /// time duration has elapse. 
        /// 
        /// Please note that this iterator requires interop. If you need
        /// and interop free hand evaluator you should remove this function along with the other interop
        /// functions in this file.
        /// </summary>
        /// <param name="shared">These cards must be included in the returned hand</param>
        /// <param name="dead">These cards must not be included in the returned hand</param>
        /// <param name="ncards">The number of cards in the returned random hand.</param>
        /// <param name="duration">The amount of time to allow the generation of hands to occur. When elapsed, the iterator will terminate.</param>
        /// <returns>A hand mask</returns>
        public static IEnumerable<ulong> RandomHands(ulong shared, ulong dead, int ncards, double duration)
        {
            long start, freq, curtime;

#if DEBUG
            // Check Preconditions
            if (ncards < 0 || ncards > 7) throw new ArgumentOutOfRangeException("ncards");
            if (duration < 0.0) throw new ArgumentOutOfRangeException("duration");
#endif

            int cardcount = ncards - BitCount(shared);
            ulong deadmask = dead | shared;
            Random rand = new Random();

            QueryPerformanceFrequency(out freq);
            QueryPerformanceCounter(out start);

            do
            {
                yield return GetRandomHand(deadmask, ncards, rand) | shared;
                QueryPerformanceCounter(out curtime);
            } while (((curtime - start) / ((double)freq)) < duration);
        }

        /// <summary>
        /// Iterates through random hands that meets the specified requirements until the specified
        /// time duration has elapse. 
        /// 
        /// Please note that this iterator requires interop. If you need
        /// and interop free hand evaluator you should remove this function along with the other interop
        /// functions in this file.
        /// </summary>
        /// <param name="ncards">The number of cards in the returned hand.</param>
        /// <param name="duration">The amount of time to allow the generation of hands to occur. When elapsed, the iterator will terminate.</param>
        /// <returns>A hand mask.</returns>
        public static IEnumerable<ulong> RandomHands(int ncards, double duration)
        {
            return RandomHands(0UL, 0UL, ncards, duration);
        }

        #endregion
    }
}
