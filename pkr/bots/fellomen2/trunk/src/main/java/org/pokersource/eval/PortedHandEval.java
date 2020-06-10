// Decompiled by Jad v1.5.8g. Copyright 2001 Pavel Kouznetsov.
// Jad home page: http://www.kpdus.com/jad.html
// Decompiler options: packimports(3) 
// Source File Name:   PortedHandEval.java

package org.pokersource.eval;

import org.pokersource.eval.tables.CardMasks;
import org.pokersource.eval.tables.NBitsTable;
import org.pokersource.eval.tables.StraightTable;
import org.pokersource.eval.tables.TopCardTable;
import org.pokersource.eval.tables.TopFiveCardsTable;

public final class PortedHandEval
{

    public PortedHandEval()
    {
    }

    public static final int EvalHigh(int ai[], int ai1[])
    {
        long l = 0L;
        for(int i = 0; i < ai.length; i++)
        {
            int j = ai1[i] * 13 + ai[i];
            l |= CardMasks.cardMasksTable[j];
        }

        return EvalHigh(l, ai.length);
    }

    public static final int EvalHigh(int ai[])
    {
        long l = 0L;
        for(int i = 1; i <= ai[0]; i++)
            l |= CardMasks.cardMasksTable[ai[i]];

        return EvalHigh(l, ai[0]);
    }

    public static final int EvalHigh(long l, int i)
    {
        int l3 = StdDeck_CardMask_SPADES(l);
        int i3 = StdDeck_CardMask_CLUBS(l);
        int j3 = StdDeck_CardMask_DIAMONDS(l);
        int k3 = StdDeck_CardMask_HEARTS(l);
        int l2 = i3 | j3 | k3 | l3;
        int j = 0;
        char c = NBitsTable.nBitsTable[l2];
        int k2 = i - c;
        if(c >= '\005')
        {
            if(NBitsTable.nBitsTable[l3] >= '\005')
            {
                if(StraightTable.straightTable[l3] != 0)
                    return HandVal_HANDTYPE_VALUE(8) + HandVal_TOP_CARD_VALUE(StraightTable.straightTable[l3]);
                j = HandVal_HANDTYPE_VALUE(5) + TopFiveCardsTable.topFiveCardsTable[l3];
            } else
            if(NBitsTable.nBitsTable[i3] >= '\005')
            {
                if(StraightTable.straightTable[i3] != 0)
                    return HandVal_HANDTYPE_VALUE(8) + HandVal_TOP_CARD_VALUE(StraightTable.straightTable[i3]);
                j = HandVal_HANDTYPE_VALUE(5) + TopFiveCardsTable.topFiveCardsTable[i3];
            } else
            if(NBitsTable.nBitsTable[j3] >= '\005')
            {
                if(StraightTable.straightTable[j3] != 0)
                    return HandVal_HANDTYPE_VALUE(8) + HandVal_TOP_CARD_VALUE(StraightTable.straightTable[j3]);
                j = HandVal_HANDTYPE_VALUE(5) + TopFiveCardsTable.topFiveCardsTable[j3];
            } else
            if(NBitsTable.nBitsTable[k3] >= '\005')
            {
                if(StraightTable.straightTable[k3] != 0)
                    return HandVal_HANDTYPE_VALUE(8) + HandVal_TOP_CARD_VALUE(StraightTable.straightTable[k3]);
                j = HandVal_HANDTYPE_VALUE(5) + TopFiveCardsTable.topFiveCardsTable[k3];
            } else
            {
                byte byte0 = StraightTable.straightTable[l2];
                if(byte0 > 0)
                    j = HandVal_HANDTYPE_VALUE(4) + HandVal_TOP_CARD_VALUE(byte0);
            }
            if(j != 0 && k2 < 3)
                return j;
        }
        switch(k2)
        {
        case 0: // '\0'
            return HandVal_HANDTYPE_VALUE(0) + TopFiveCardsTable.topFiveCardsTable[l2];

        case 1: // '\001'
            int l1 = l2 ^ (i3 ^ j3 ^ k3 ^ l3);
            j = HandVal_HANDTYPE_VALUE(1) + HandVal_TOP_CARD_VALUE(TopCardTable.topCardTable[l1]);
            int i4 = l2 ^ l1;
            int l4 = TopFiveCardsTable.topFiveCardsTable[i4] >> 4 & 0xfffffff0;
            j += l4;
            return j;

        case 2: // '\002'
            int i2 = l2 ^ (i3 ^ j3 ^ k3 ^ l3);
            if(i2 != 0)
            {
                int j4 = l2 ^ i2;
                j = HandVal_HANDTYPE_VALUE(2) + (TopFiveCardsTable.topFiveCardsTable[i2] & 0xff000) + HandVal_THIRD_CARD_VALUE(TopCardTable.topCardTable[j4]);
                return j;
            } else
            {
                int j1 = (i3 & j3 | k3 & l3) & (i3 & k3 | j3 & l3);
                j = HandVal_HANDTYPE_VALUE(3) + HandVal_TOP_CARD_VALUE(TopCardTable.topCardTable[j1]);
                int k4 = l2 ^ j1;
                byte byte4 = TopCardTable.topCardTable[k4];
                j += HandVal_SECOND_CARD_VALUE(byte4);
                k4 ^= 1 << byte4;
                j += HandVal_THIRD_CARD_VALUE(TopCardTable.topCardTable[k4]);
                return j;
            }
        }
        int i1 = k3 & j3 & i3 & l3;
        if(i1 != 0)
        {
            byte byte1 = TopCardTable.topCardTable[i1];
            j = HandVal_HANDTYPE_VALUE(7) + HandVal_TOP_CARD_VALUE(byte1) + HandVal_SECOND_CARD_VALUE(TopCardTable.topCardTable[l2 ^ 1 << byte1]);
            return j;
        }
        int j2 = l2 ^ (i3 ^ j3 ^ k3 ^ l3);
        if(NBitsTable.nBitsTable[j2] != k2)
        {
            int k1 = (i3 & j3 | k3 & l3) & (i3 & k3 | j3 & l3);
            j = HandVal_HANDTYPE_VALUE(6);
            byte byte2 = TopCardTable.topCardTable[k1];
            j += HandVal_TOP_CARD_VALUE(byte2);
            int i5 = (j2 | k1) ^ 1 << byte2;
            j += HandVal_SECOND_CARD_VALUE(TopCardTable.topCardTable[i5]);
            return j;
        }
        if(j != 0)
        {
            return j;
        } else
        {
            int k = HandVal_HANDTYPE_VALUE(2);
            byte byte3 = TopCardTable.topCardTable[j2];
            k += HandVal_TOP_CARD_VALUE(byte3);
            byte byte5 = TopCardTable.topCardTable[j2 ^ 1 << byte3];
            k += HandVal_SECOND_CARD_VALUE(byte5);
            k += HandVal_THIRD_CARD_VALUE(TopCardTable.topCardTable[l2 ^ 1 << byte3 ^ 1 << byte5]);
            return k;
        }
    }

    private static int HandVal_SECOND_CARD_VALUE(int i)
    {
        return i << 12;
    }

    private static int HandVal_THIRD_CARD_VALUE(int i)
    {
        return i << 8;
    }

    private static final int HandVal_TOP_CARD_VALUE(int i)
    {
        return i << 16;
    }

    private static final int HandVal_HANDTYPE_VALUE(int i)
    {
        return i << 26;
    }

    private static int StdDeck_CardMask_SPADES(long l)
    {
        return (int)(l & 65535L);
    }

    private static int StdDeck_CardMask_HEARTS(long l)
    {
        return (int)(l >> 16 & 65535L);
    }

    private static int StdDeck_CardMask_DIAMONDS(long l)
    {
        return (int)(l >> 32 & 65535L);
    }

    private static int StdDeck_CardMask_CLUBS(long l)
    {
        return (int)(l >> 48 & 65535L);
    }

    private static final int StdRules_HandType_NOPAIR = 0;
    private static final int StdRules_HandType_ONEPAIR = 1;
    private static final int StdRules_HandType_TWOPAIR = 2;
    private static final int StdRules_HandType_TRIPS = 3;
    private static final int StdRules_HandType_STRAIGHT = 4;
    private static final int StdRules_HandType_FLUSH = 5;
    private static final int StdRules_HandType_FULLHOUSE = 6;
    private static final int StdRules_HandType_QUADS = 7;
    private static final int StdRules_HandType_STFLUSH = 8;
    private static final int StdRules_HandType_FIRST = 0;
    private static final int StdRules_HandType_LAST = 8;
    private static final int StdRules_HandType_COUNT = 9;
    private static final int HandVal_CARD_WIDTH = 4;
    private static final int HandVal_CARD_MASK = 15;
    private static final int HandVal_FIFTH_CARD_MASK = 15;
    private static final int HandVal_TOP_CARD_MASK = 0xf0000;
    private static final int HandVal_SECOND_CARD_MASK = 61440;
    private static final int HandVal_THIRD_CARD_SHIFT = 8;
    private static final int HandVal_SECOND_CARD_SHIFT = 12;
    private static final int NUM_RANKS = 13;
    private static final int HandVal_TOP_CARD_SHIFT = 16;
    private static final int StdDeck_Rank_COUNT = 13;
    private static final int HandVal_TYPE_SHIFT = 26;
}
