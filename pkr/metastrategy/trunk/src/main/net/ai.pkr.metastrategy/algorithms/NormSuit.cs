/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using ai.lib.algorithms;

namespace ai.pkr.metastrategy.algorithms
{
    /// <summary>
    /// Converts a hand to a suit-equivalent hand by normalizng suits. 
    /// The resulting hand has exactly the same strength as the original hand.
    /// The suit containing the biggest high-card-hand becomes suit 0, 2-nd best - suit 1, etc.
    /// If suits have the same rank, the suit with less index wins.
    /// Example: for StdDeck: AhKc -> AcKd.
    /// </summary>
    /// <remarks>The deck must follow the layout conventions of DeckDescriptor.</remarks>
    public class NormSuit
    {
        private readonly int[] _transformTable;
        private readonly uint[] _tempTable = new uint[4];
        private int _suitCount = 0;

        public NormSuit()
        {
            _transformTable = new int[] { -1, -1, -1, -1 };
        }

        /// <summary>
        /// Copies conversion parameters from the other NormSuit.
        /// </summary>
        public NormSuit(NormSuit other)
        {
            _transformTable = new int[4];
            CopyFrom(other);
        }

        /// <summary>
        /// Copies conversion parameters from the other NormSuit.
        /// </summary>
        public void CopyFrom(NormSuit other)
        {
            _transformTable[0] = other._transformTable[0];
            _transformTable[1] = other._transformTable[1];
            _transformTable[2] = other._transformTable[2];
            _transformTable[3] = other._transformTable[3];
            _suitCount = other._suitCount;
        }

        /// <summary>
        /// Converts the hand. Can be called subsequently e.g. on pocket, flop, turn and river.
        /// </summary>
        public CardSet Convert(CardSet hand)
        {
            UInt64 bits = 0;
            int newSuits = 0;
            // Work with 32-bit halfs, it is faster on both 32- and 64-bits architectures.
            uint half = (uint)hand.bits;
            // Suite 0
            uint s = (half & 0xFFFF);
            if (s > 0)
            {
                if (_transformTable[0] >= 0)
                    bits |= (ulong)s << _transformTable[0];
                else
                {
                    newSuits++;
                    _tempTable[0] = s;
                }
            }
            // Suite 1
            s = (half >> 16);
            if (s > 0)
            {
                if (_transformTable[1] >= 0)
                    bits |= (ulong) s << _transformTable[1];
                else
                {
                    newSuits++;
                    _tempTable[1] = s;
                }
            }
            half = (uint)(hand.bits >> 32);
            // Suite 2
            s = (half & 0xFFFF);
            if (s > 0)
            {
                if (_transformTable[2] >= 0)
                    bits |= (ulong) s << _transformTable[2];
                else
                {
                    newSuits++;
                    _tempTable[2] = s;
                }
            }
            // Suite 3
            s = (half >> 16);
            if (s > 0)
            {
                if (_transformTable[3] >= 0)
                    bits |= (ulong) s << _transformTable[3];
                else 
                {
                    newSuits++;
                    _tempTable[3] = s;
                }
            }
            if(newSuits > 0)
            {
                int permutIdx = SortSuits(_tempTable[0], _tempTable[1], _tempTable[2], _tempTable[3]);
                for(int i = 0; i < newSuits; ++i)
                {
                    int origSuite = _suitePemuts[permutIdx, i];
                    _transformTable[origSuite] = _suitCount << 4; // 0, 16, 32, 48
                    _suitCount++;
                    bits |= (ulong)_tempTable[origSuite] << _transformTable[origSuite];
                    _tempTable[origSuite] = 0;
                }
            }

            CardSet result = new CardSet();
            result.bits = bits;
            return result;
        }

        /// <summary>
        /// Resets conversion parameters to use the object to convert a new hand.
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < _transformTable.Length; ++i)
                _transformTable[i] = -1;
            _suitCount = 0;
        }

        public static int CountSuits(CardSet hand)
        {
            int cnt = 0;
            uint half = (uint) hand.bits;
            // Suite 0
            if ((half & 0xFFFF) != 0) ++cnt;
            // Suite 1
            if ((half >> 16) != 0) ++cnt;
            half = (uint) (hand.bits >> 32);
            // Suite 2
            if ((half & 0xFFFF) != 0) ++cnt;
            // Suite 3
            if ((half >> 16) != 0) ++cnt;
            return cnt;
        }

        private static readonly int[,] _suitePemuts = new int[,]
                                                          {
                                                              {0, 1, 2, 3}, //  0
                                                              {0, 1, 3, 2}, //  1
                                                              {0, 2, 1, 3}, //  2
                                                              {0, 2, 3, 1}, //  3
                                                              {0, 3, 2, 1}, //  4
                                                              {0, 3, 1, 2}, //  5
                                                              {1, 0, 2, 3}, //  6
                                                              {1, 0, 3, 2}, //  7
                                                              {1, 2, 0, 3}, //  8
                                                              {1, 2, 3, 0}, //  9
                                                              {1, 3, 2, 0}, // 10
                                                              {1, 3, 0, 2}, // 11
                                                              {2, 1, 0, 3}, // 12
                                                              {2, 1, 3, 0}, // 13
                                                              {2, 0, 1, 3}, // 14
                                                              {2, 0, 3, 1}, // 15
                                                              {2, 3, 0, 1}, // 16
                                                              {2, 3, 1, 0}, // 17
                                                              {3, 1, 2, 0}, // 18
                                                              {3, 1, 0, 2}, // 19
                                                              {3, 2, 1, 0}, // 20
                                                              {3, 2, 0, 1}, // 21
                                                              {3, 0, 2, 1}, // 22
                                                              {3, 0, 1, 2}, // 23
                                                          };

        /// <summary>
        /// Returns an index in _suitePemuts, corresponding to ordered in descending order suits.
        /// </summary>
        private static int SortSuits(uint s0, uint s1, uint s2, uint s3)
        {
            if (s0 >= s1)
            {
                if (s2 >= s3)
                {
                    if (s0 >= s2)
                    {
                        if (s1 >= s2)
                        {
                            return 0;
                        }
                        else
                        {
                            if (s1 >= s3)
                            {
                                return 2;
                            }
                            else
                            {
                                return 3;
                            }
                        }
                    }
                    else
                    {
                        if (s0 >= s3)
                        {
                            if (s1 >= s3)
                            {
                                return 14;
                            }
                            else
                            {
                                return 15;
                            }
                        }
                        else
                        {
                            return 16;
                        }
                    }
                }
                else
                {
                    if (s0 >= s3)
                    {
                        if (s1 >= s2)
                        {
                            if (s1 >= s3)
                            {
                                return 1;
                            }
                            else
                            {
                                return 5;
                            }
                        }
                        else
                        {
                            return 4;
                        }
                    }
                    else
                    {
                        if (s0 >= s2)
                        {
                            if (s1 >= s2)
                            {
                                return 23;
                            }
                            else
                            {
                                return 22;
                            }
                        }
                        else
                        {
                            return 21;
                        }
                    }
                }
            }
            else
            {
                if (s2 >= s3)
                {
                    if (s0 >= s3)
                    {
                        if (s0 >= s2)
                        {
                            return 6;
                        }
                        else
                        {
                            if (s1 >= s2)
                            {
                                return 8;
                            }
                            else
                            {
                                return 12;
                            }
                        }
                    }
                    else
                    {
                        if (s1 >= s2)
                        {
                            return 9;
                        }
                        else
                        {
                            if (s1 >= s3)
                            {
                                return 13;
                            }
                            else
                            {
                                return 17;
                            }
                        }
                    }
                }
                else
                {
                    if (s0 >= s2)
                    {
                        if (s0 >= s3)
                        {
                            return 7;
                        }
                        else
                        {
                            if (s1 >= s3)
                            {
                                return 11;
                            }
                            else
                            {
                                return 19;
                            }
                        }
                    }
                    else
                    {
                        if (s1 >= s2)
                        {
                            if (s1 >= s3)
                            {
                                return 10;
                            }
                            else
                            {
                                return 18;
                            }
                        }
                        else
                        {
                            return 20;
                        }
                    }
                }
            }

        }
    }
}