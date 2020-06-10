//
//  new2old.java
//  
//
//  Created by Ian Fellows on 6/14/07.
//  Copyright 2007 __MyCompanyName__. All rights reserved.
//

package com.ibot;

import com.biotools.meerkat.*;
import com.biotools.meerkat.Action;
//import poker.Card;
//import poker.Hand;

public class new2old {

public poker.Card convertCard(Card c1)
{
	poker.Card c2 = new poker.Card(c1.getIndex());
	System.out.println("Card onverted : "+c2.toString());
	return c2;
}

public poker.Hand convertHand(Hand hnd)
{
	poker.Hand hnd2 = new poker.Hand(hnd.toString());
	System.out.println("hand converted : "+hnd2.toString());
	return hnd2;
}

}
