/*This file is part of Fell Omen.

Fell Omen is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 2 of the License, or
(at your option) any later version.

Fell Omen is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar.  If not, see <http://www.gnu.org/licenses/>.*/

// Todo: own RNG to avoid dependency of Java version.

import java.awt.event.*;

import javax.swing.*;
import java.util.*;

import com.biotools.meerkat.Action; 
import com.biotools.meerkat.Card ;
//import com.biotools.meerkat.Deck ;
import com.biotools.meerkat.GameInfo ;
import com.biotools.meerkat.Player ;
import com.biotools.meerkat.util.Preferences ;


import poker.ai.HandPotential;
import poker.HandEvaluator;

import com.ibot.FellOmen_2_Impl;
import com.ibot.new2old;
//import com.ibot.HandPotential; 
import java.io.PrintStream;

//import ca.ualberta.cs.poker.free.academy25.PokerAcademyClient;

/** 
 * Ian Bot
 * 
 * Description: {Insert Description here}
 * 
 * @author Ian Fellows
 */
public class FellOmen_2 implements Player 
{
   private static final String ALWAYS_CALL_MODE = "ALWAYS_CALL_MODE";

   private int ourSeat;       // our seat for the current hand
   private Card c1, c2;       // our hole cards
   private GameInfo gi;       // general game information
   private Preferences prefs; // the configuration options for this bot
   private int preFlopHistory;
   private int flopHistory;
   private int turnHistory;
   private double flopHR;
   private double flopPOT;
   private double flopNPOT;
   private double turnHR;
   private double turnPOT;
   private double turnNPOT;
   private double riverHR;
   private int flopBDindex;
   private new2old converter;
   PrintStream _debugLog = null;


   private FellOmen_2_Impl ibot;   // The overridding action generator

   public FellOmen_2() throws Exception
   { 
   	ibot = new FellOmen_2_Impl();
   }

   public FellOmen_2_Impl getIbot()
   {
        return ibot;
   }
   
   /**
    * An event called to tell us our hole cards and seat number
    * @param c1 your first hole card
    * @param c2 your second hole card
    * @param seat your seat number at the table
    */
   public void holeCards(Card c1, Card c2, int seat) {
      this.c1 = c1;
      this.c2 = c2;
      this.ourSeat = seat;

   }

   /**
    * Requests an Action from the player
    * Called when it is the Player's turn to act.
    */
   public Action getAction() {
     
      double toCall = gi.getAmountToCall(ourSeat);
      
 

      if (gi.isPreFlop()) {
         return preFlopAction();
      }
	  else if(gi.isFlop())
		 return flopAction();
	  else if(gi.isTurn())
		 return turnAction();
	  else  
	  {
         return riverAction();
      }
   }
   
   /**
    * If you implement the getSettingsPanel() method, your bot will display
    * the panel in the Opponent Settings Dialog.
    * @return a GUI for configuring your bot (optional)
    */
   public JPanel getSettingsPanel() {
      JPanel jp = new JPanel();
      final JCheckBox acMode = new JCheckBox(
            "Always Call Mode", prefs.getBooleanPreference(ALWAYS_CALL_MODE));
      acMode.addItemListener(new ItemListener() {
         public void itemStateChanged(ItemEvent e) {
            prefs.setPreference(ALWAYS_CALL_MODE, acMode.isSelected());
         }        
      });
      jp.add(acMode);
      return jp;
   }
   

   /**
    * Get the current settings for this bot.
    */
   public Preferences getPreferences() {
      return prefs;
   }

   /**
    * Load the current settings for this bot.
    */
   public void init(Preferences playerPrefs) {
       try
       {
        this.prefs = playerPrefs;
        ibot.setDebug(getDebug());
        String logFile = prefs.getPreference("LOG_FILE", null);
        if(logFile != null)
            _debugLog = new PrintStream(logFile);
        ibot.setLogFile(_debugLog);
        ibot.setRngSeed(prefs.getLongPreference("RNG_SEED", 0));
        String strategyPath = prefs.getPreference("STRATEGY_PATH", null);
        System.out.println("Set strategy path to " + strategyPath);
        ibot.setStrategyPath(strategyPath);
       }
       catch(Exception e)
       {
        System.out.println("Error in init(): " + e.toString());
       }
   }

   /**
    * An example setting for this bot. It can be turned into
    * an always-call mode, or to a simple strategy.
    * @return true if always-call mode is active.
    */ 
   public boolean getAlwaysCallMode() {
//      return prefs.getBooleanPreference(ALWAYS_CALL_MODE, false);
      return true;
   }

   /**
    * @return true if debug mode is on.
    */
   public boolean getDebug() {
      return prefs.getBooleanPreference("DEBUG", false);
   }
   
   /**
    * print a debug statement.
    */
   public void debug(String str) {
      if (getDebug()) {
         System.out.println(str);
          if (_debugLog != null) {
              _debugLog.println(str);
          }
      }
   }
   
   /**
    * print a debug statement with no end of line character
    */
   public void debugb(String str) {
      if (getDebug()) {
         System.out.print(str);
          if (_debugLog != null) {
              _debugLog.print(str);
          }
      }  
   }

   /**
    * A new betting round has started.
    */ 
   public void stageEvent(int stage) 
   {
		if(gi.isFlop())
		{
			debug("board: " + gi.getBoard().toString());
			HandEvaluator 	he = new HandEvaluator();
			HandPotential   pot = new HandPotential();
			flopHR = he.handRank(c1.toString() , c2.toString() , gi.getBoard().toString() );// c1, c2, gi.getBoard() );
			flopPOT = pot.ppot_raw(c1.toString() , c2.toString() , gi.getBoard().toString() , true);//converter.convertCard(c1),converter.convertCard(c2),converter.convertHand(gi.getBoard()),true);//
			flopNPOT = pot.getLastNPot();
			flopBDindex = ibot.getFlopBoardIndex(gi.getBoard().toString());
			debug("flop board index = "+flopBDindex);
			debug("flop Hand Strength : " + flopHR + " flop potential: " + flopPOT + " flop negative potential : " + flopNPOT);
		}
		else if(gi.isTurn())
		{
			debug("board: " + gi.getBoard().toString());
			HandEvaluator 	he = new HandEvaluator();
			HandPotential   pot = new HandPotential();
			turnHR = he.handRank(c1.toString() , c2.toString() , gi.getBoard().toString() );// c1, c2, gi.getBoard() );
			turnPOT = pot.ppot_raw(c1.toString() , c2.toString() , gi.getBoard().toString() , true);//converter.convertCard(c1),converter.convertCard(c2),converter.convertHand(gi.getBoard()) , false);
			turnNPOT = pot.getLastNPot();
			debug("turn Hand Strength : " + turnHR + " turn potential: " + turnPOT + " turn negative potential : " + turnNPOT);			
		}
		else if(gi.isRiver())
		{
			debug("board: " + gi.getBoard().toString());
			HandEvaluator 	he = new HandEvaluator();
			riverHR = he.handRank(c1.toString() , c2.toString() , gi.getBoard().toString() );// c1, c2, gi.getBoard() );
			debug("river Hand Strength : " + riverHR);
		}
   }

   /**
    * A showdown has occurred.
    * @param pos the position of the player showing
    * @param c1 the first hole card shown
    * @param c2 the second hole card shown
    */
   public void showdownEvent(int seat, Card c1, Card c2) {}

   /**
    * A new game has been started.
    * @param gi the game stat information
    */
   public void gameStartEvent(GameInfo gInfo) {
      this.gi = gInfo;
	  
		//clean out our internal data
		this.preFlopHistory=-1;
		this.flopHistory=-1;
		this.turnHistory=-1;
		
		this.flopHR=-1;
		this.flopPOT=-1;
		this.flopNPOT=-1;
		this.turnHR=-1;
		this.turnPOT=-1;
		this.turnNPOT=-1;
		this.turnHR=-1;
		this.flopBDindex=-1;
   }
   
   /**
    * An event sent when all players are being dealt their hole cards
    */
   public void dealHoleCardsEvent() {
   }   

   /**
    * An action has been observed. 
    */
   public void actionEvent(int pos, Action act) 
   {
		int node = get_action_node(this.gi);
		
		//if the end of a round is detected, we record the play history of
		//the round.
		if(act.isCheckOrCall() & (node!=5) )
		{
		int terminalIndex = nodeToTerminalNodeIndex(node);
		debug("call action observed: " + act.toString());
		debug("position of action : " + pos);
		debug("node of action: " + node + " Terminal Index : " + terminalIndex);
		if(this.gi.isPreFlop())
			this.preFlopHistory=terminalIndex;
		else if(this.gi.isFlop())
			this.flopHistory=terminalIndex;
		else if(this.gi.isTurn())
			this.turnHistory=terminalIndex;
		}
		else
			debug("action observed: " + act.toString());
   }
   
   /**
    * The game info state has been updated
    * Called after an action event has been fully processed
    */
   public void gameStateChanged() {}  

   /**
    * The hand is now over. 
    */
   public void gameOverEvent() 
   {


   }

   /**
    * A player at pos has won amount with the hand handName
    */
   public void winEvent(int pos, double amount, String handName) {}
   
   
   /**
    * Decide what to do for a pre-flop action
    *
    * Uses a really simple hand selection, as a silly example.
    */
   private Action preFlopAction() 
   {
   	  return (ibot.preFlopAction(gi, c1, c2, ourSeat));
   }  
   private Action flopAction() 
   {
   	  return (ibot.flopAction(gi, flopHR, flopPOT, flopNPOT,flopBDindex, ourSeat, preFlopHistory));
   }      
   private Action turnAction() 
   {
   	  return (ibot.turnAction(gi, turnHR, turnPOT, turnNPOT,flopBDindex, ourSeat, preFlopHistory, flopHistory));
   }  
   private Action riverAction() 
   {
   	  return (ibot.riverAction(gi, riverHR,flopBDindex, ourSeat, preFlopHistory, flopHistory, turnHistory));
   }  



   
   protected int get_action_node(GameInfo gi)
   {
		return(ibot.get_action_node(gi));
   }
   
   public int nodeToTerminalNodeIndex(int node)
   {
		if(node==1)
			return 0;
		else if (node==2)
			return 1;
		else if (node==3)
			return 2;
		else if (node==4)
			return 3;
		else if (node==6)
			return 4;
		else if (node==7)
			return 5;
		else if (node==8)
			return 6;
		else if (node==9)
			return 7;
		else if (node==10)
			return 8;
		else
			return -1;
   }
   
   /**
     * A function for startme.bat to call
     */
//    public static void main(String[] args) throws Exception
//    {
//    	// Debug
//    	Properties sysProps = System.getProperties();
//    	sysProps.list(System.out);
//
//        FellOmen_2 tp = new FellOmen_2();
//        PokerAcademyClient pac = new PokerAcademyClient(tp);
//        System.out.println("Attempting to connect to "+args[0]+" on port "+args[1]+"...");
//
//        pac.connect(InetAddress.getByName(args[0]),Integer.parseInt(args[1]));
//        System.out.println("Successful connection!");
//        pac.run();
//
//    }

    public static void main(String[] args) throws Exception
    {
    	// Debug
    	Properties sysProps = System.getProperties();
    	sysProps.list(System.out);

        String name = "Fo";
        boolean debug = false;
        int port = 9001;
        int rngSeed = 0;
        String strategyPath = "C:\\Fo2-strategy";

        System.out.println("Parsing command line arguments.");

        for (int a = 0; a < args.length; ++a) {
            System.out.println(args[a]);
            if (args[a].equals("--debug")) {
                debug = true;
            } else if (args[a].startsWith("--port")) {
                if (args[a].length() > 6) {
                    port = Integer.parseInt(args[a].substring(6));
                }
            } else if (args[a].startsWith("--rng-seed")) {
                if (args[a].length() > 10) {
                    rngSeed = Integer.parseInt(args[a].substring(10));
                }
            } else if (args[a].startsWith("--strategy-path")) {
                if (args[a].length() > 15) {
                    strategyPath = args[a].substring(15);
                }
            } else {
                name = args[a];
            }
        }

        System.out.printf("Initializing the bot, name %s\n", name);

        Preferences preferences = new Preferences();
        preferences.setPreference("DEBUG", debug);
        preferences.setPreference("LOG_FILE", "c:\\temp\\" + name + ".log");
        preferences.setPreference("RNG_SEED", rngSeed);
        preferences.setPreference("STRATEGY_PATH", strategyPath);
        
        FellOmen_2 bot = new FellOmen_2();
        bot.init(preferences);

        System.out.printf("Starting socket client on port %d\n", port);

        FellOmen2SocketClient socketClient = new FellOmen2SocketClient();
        socketClient.setPlayer(bot, name);
        socketClient.Run(port);
    }
}