package ai.pkr.meerkat;

import java.util.*;

import com.biotools.meerkat.Action;
import com.biotools.meerkat.Card;
//import com.biotools.meerkat.Deck ;
import com.biotools.meerkat.GameInfo;
import com.biotools.meerkat.Player;
import com.biotools.meerkat.util.Preferences;

//import com.ibot.HandPotential; 
import java.io.PrintStream;
import javax.swing.JPanel;

//import ca.ualberta.cs.poker.free.academy25.PokerAcademyClient;
/** 
 * Ian Bot
 * 
 * Description: {Insert Description here}
 * 
 * @author Ian Fellows
 */
public class MeerkatVerifierBot implements Player {
    private int ourSeat; // our seat for the current hand
    private GameInfo gi; // general game information
    private Preferences prefs; // the configuration options for this bot
    PrintStream _debugLog = null;
    private Random _rng = new Random();


    public MeerkatVerifierBot() throws Exception {
    }

    /**
     * An event called to tell us our hole cards and seat number
     * @param c1 your first hole card
     * @param c2 your second hole card
     * @param seat your seat number at the table
     */
    @Override
    public void holeCards(Card c1, Card c2, int seat) {
        this.ourSeat = seat;
        debug("Seat " + Integer.toString(seat) + "; pocket: " + c1.toString() + " " + c2.toString());
    }

    /**
     * Requests an Action from the player
     * Called when it is the Player's turn to act.
     */
    @Override
    public Action getAction() {
        int actionCode = _rng.nextInt(3);
        debugb("Board: " + gi.getBoard().toString() + "; action code: " + Integer.toString(actionCode));

        if(!gi.canRaise(ourSeat) && actionCode == 2)
            actionCode = 1;
        Action action = null;
        switch(actionCode)
        {
            case 0:
                action = Action.checkOrFoldAction(gi);
                break;
            case 1:
                action = Action.callAction(gi);
                break;
            case 2:
                action = Action.raiseAction(gi);
                break;
        }
        debug(" action: " + action.toString());
        return action;
    }

    /**
     * If you implement the getSettingsPanel() method, your bot will display
     * the panel in the Opponent Settings Dialog.
     * @return a GUI for configuring your bot (optional)
     */
    public JPanel getSettingsPanel() {
        return null;
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
    @Override
    public void init(Preferences playerPrefs) {
        try {
            this.prefs = playerPrefs;
            String logFile = prefs.getPreference("LOG_FILE", null);
            if (logFile != null) {
                _debugLog = new PrintStream(logFile);
            }
            setRngSeed(prefs.getLongPreference("RNG_SEED", 0));
        } catch (Exception e) {
            debug(e.toString());
        }
    }

    /**
     * A new betting round has started.
     */
    @Override
    public void stageEvent(int stage) {
        debug("Round: " + Integer.toString(stage));
    }

    /**
     * A showdown has occurred.
     * @param pos the position of the player showing
     * @param c1 the first hole card shown
     * @param c2 the second hole card shown
     */
    @Override
    public void showdownEvent(int seat, Card c1, Card c2) {
    }

    /**
     * A new game has been started.
     * @param gi the game stat information
     */
    @Override
    public void gameStartEvent(GameInfo gInfo) {
        this.gi = gInfo;
        debug("Game started, id:" + Long.toString(gi.getGameID()));
    }

    /**
     * An event sent when all players are being dealt their hole cards
     */
    @Override
    public void dealHoleCardsEvent() {
    }

    /**
     * An action has been observed.
     */
    @Override
    public void actionEvent(int pos, Action act) {
    }

    /**
     * The game info state has been updated
     * Called after an action event has been fully processed
     */
    @Override
    public void gameStateChanged() {
    }

    /**
     * The hand is now over.
     */
    @Override
    public void gameOverEvent() {
    }

    /**
     * A player at pos has won amount with the hand handName
     */
    @Override
    public void winEvent(int pos, double amount, String handName) {
        debug("Winner : " + Integer.toString(pos) + " ; amount: " + Double.toString(amount));
    }

    /**
     * @return true if debug mode is on.
     */
    private boolean getDebug() {
        return prefs.getBooleanPreference("DEBUG", false);
    }

    /**
     * print a debug statement.
     */
    private void debug(String str) {
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
    private void debugb(String str) {
        if (getDebug()) {
            System.out.print(str);
            if (_debugLog != null) {
                _debugLog.print(str);
            }
        }
    }

    private void setRngSeed(long seed)
    {
        debug("Rng seed set to " + Long.toString(seed));
        if(seed == 0)
            _rng = new Random();
        else
            _rng.setSeed(seed);
    }

    public static void main(String[] args) throws Exception {
        // Debug
        Properties sysProps = System.getProperties();
        sysProps.list(System.out);

        String name = "MeerkatVerifierBot";
        boolean debug = false;
        int port = 9001;
        int rngSeed = 0;

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
            } else {
                name = args[a];
            }
        }

        System.out.printf("Initializing the bot, name %s\n", name);

        Preferences preferences = new Preferences();
        preferences.setPreference("DEBUG", debug);
        preferences.setPreference("LOG_FILE", "c:\\temp\\" + name + ".log");
        preferences.setPreference("RNG_SEED", rngSeed);

        MeerkatVerifierBot bot = new MeerkatVerifierBot();
        bot.init(preferences);

        System.out.printf("Starting socket client on port %d\n", port);

        MeerkatSocketClient socketClient = new MeerkatSocketClient();
        socketClient.setPlayer(bot, name);
        socketClient.Run(port);
    }
}
