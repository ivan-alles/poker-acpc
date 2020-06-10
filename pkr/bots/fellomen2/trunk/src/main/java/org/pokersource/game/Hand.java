// Decompiled by Jad v1.5.8g. Copyright 2001 Pavel Kouznetsov.
// Jad home page: http://www.kpdus.com/jad.html
// Decompiler options: packimports(3) 
// Source File Name:   Hand.java

package org.pokersource.game;

import java.util.StringTokenizer;

// Referenced classes of package org.pokersource.game:
//            Card

public class Hand
{

    public Hand()
    {
        cards = new int[8];
        cards[0] = 0;
    }

    public Hand(String s)
    {
        cards = new int[8];
        cards[0] = 0;
        StringTokenizer stringtokenizer = new StringTokenizer(s, " -");
        do
        {
            if(!stringtokenizer.hasMoreTokens())
                break;
            String s1 = stringtokenizer.nextToken();
            if(s1.length() == 2)
            {
                Card card = new Card(s1.charAt(0), s1.charAt(1));
                if(card.getIndex() != -1)
                    addCard(card);
            }
        } while(true);
    }

    public Hand(Hand hand)
    {
        cards = new int[8];
        cards[0] = hand.size();
        for(int i = 1; i <= cards[0]; i++)
            cards[i] = hand.cards[i];

    }

    public int size()
    {
        return cards[0];
    }

    public void removeCard()
    {
        if(cards[0] > 0)
            cards[0]--;
    }

    public void makeEmpty()
    {
        cards[0] = 0;
    }

    public boolean addCard(Card card)
    {
        if(card == null)
            return false;
        if(cards[0] == 7)
        {
            return false;
        } else
        {
            cards[0]++;
            cards[cards[0]] = card.getIndex();
            return true;
        }
    }

    public boolean addCard(int i)
    {
        if(cards[0] == 7)
        {
            return false;
        } else
        {
            cards[0]++;
            cards[cards[0]] = i;
            return true;
        }
    }

    public Card getCard(int i)
    {
        if(i < 1 || i > cards[0])
            return null;
        else
            return new Card(cards[i]);
    }

    public void setCard(int i, Card card)
    {
        if(cards[0] < i)
        {
            return;
        } else
        {
            cards[i] = card.getIndex();
            return;
        }
    }

    public int[] getCardArray()
    {
        return cards;
    }

    public void sort()
    {
        for(boolean flag = true; flag;)
        {
            flag = false;
            int i = 1;
            while(i < cards[0]) 
            {
                if(cards[i] < cards[i + 1])
                {
                    flag = true;
                    int j = cards[i];
                    cards[i] = cards[i + 1];
                    cards[i + 1] = j;
                }
                i++;
            }
        }

    }

    public String toString()
    {
        String s = new String();
        for(int i = 1; i <= cards[0]; i++)
            s = s + " " + getCard(i).toString();

        return s;
    }

    public static final int MAX_CARDS = 7;
    private int cards[];
}
