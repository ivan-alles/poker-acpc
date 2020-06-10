// Decompiled by Jad v1.5.8g. Copyright 2001 Pavel Kouznetsov.
// Jad home page: http://www.kpdus.com/jad.html
// Decompiler options: packimports(3) 
// Source File Name:   Deck.java

package org.pokersource.game;

import java.io.PrintStream;
import java.util.Random;

// Referenced classes of package org.pokersource.game:
//            Card, Hand

public class Deck
{

    public Deck()
    {
        gCards = new Card[52];
        r = new Random();
        r2 = new Random();
        position = '\0';
        for(int i = 0; i < 52; i++)
            gCards[i] = new Card(i);

    }

    public Deck(long l)
    {
        this();
        if(l == 0L)
            l = System.currentTimeMillis();
        r.setSeed(l);
        r2.setSeed(l / 2L + 52L);
    }

    public Deck(long l, long l1)
    {
        this();
        if(l == 0L)
            l = System.currentTimeMillis();
        r.setSeed(l);
        r2.setSeed(l1);
    }

    public synchronized void reset()
    {
        position = '\0';
    }

    public synchronized void shuffle()
    {
        for(int i = 0; i < 52; i++)
        {
            int j = i + randInt(52 - i);
            Card card = gCards[j];
            gCards[j] = gCards[i];
            gCards[i] = card;
        }

        position = '\0';
    }

    public synchronized Card deal()
    {
        return position >= '4' ? null : gCards[position++];
    }

    public synchronized Card dealCard()
    {
        return extractRandomCard();
    }

    public synchronized int findCard(Card card)
    {
        int i = position;
        for(int j = card.getIndex(); i < 52 && j != gCards[i].getIndex(); i++);
        return i >= 52 ? -1 : i;
    }

    private synchronized int findDiscard(Card card)
    {
        int i = 0;
        int j;
        for(j = card.getIndex(); i < position && j != gCards[i].getIndex(); i++);
        return j != gCards[i].getIndex() ? -1 : i;
    }

    public synchronized void extractHand(Hand hand)
    {
        for(int i = 1; i <= hand.size(); i++)
            extractCard(hand.getCard(i));

    }

    public synchronized void extractCard(Card card)
    {
        int i = findCard(card);
        if(i != -1)
        {
            Card card1 = gCards[i];
            gCards[i] = gCards[position];
            gCards[position] = card1;
            position++;
        } else
        {
            System.err.println("*** ERROR: could not find card " + card);
            Thread.currentThread();
            Thread.dumpStack();
        }
    }

    public synchronized Card extractRandomCard()
    {
        int i = position + randInt2(52 - position);
        Card card = gCards[i];
        gCards[i] = gCards[position];
        gCards[position] = card;
        position++;
        return card;
    }

    public synchronized Card pickRandomCard()
    {
        return gCards[position + randInt(52 - position)];
    }

    public synchronized void replaceCard(Card card)
    {
        int i = findDiscard(card);
        if(i != -1)
        {
            position--;
            Card card1 = gCards[i];
            gCards[i] = gCards[position];
            gCards[position] = card1;
        }
    }

    public synchronized int getTopCardIndex()
    {
        return position;
    }

    public synchronized int cardsLeft()
    {
        return 52 - position;
    }

    public synchronized Card getCard(int i)
    {
        return gCards[i];
    }

    public String toString()
    {
        StringBuffer stringbuffer = new StringBuffer();
        stringbuffer.append("* ");
        for(int i = 0; i < position; i++)
            stringbuffer.append(gCards[i].toString() + " ");

        stringbuffer.append("\n* ");
        for(int j = position; j < 52; j++)
            stringbuffer.append(gCards[j].toString() + " ");

        return stringbuffer.toString();
    }

    private int randInt(int i)
    {
        return (int)(r.nextDouble() * (double)i);
    }

    private int randInt2(int i)
    {
        return (int)(r2.nextDouble() * (double)i);
    }

    public static final int NUM_CARDS = 52;
    private Card gCards[];
    private char position;
    private Random r;
    private Random r2;
}
