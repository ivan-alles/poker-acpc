/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package ai.pkr.jmetabots;

import junit.framework.TestCase;
import ai.pkr.metabots.remote.Protocol.*;


/**
 *
 * @author ivan
 */
public class GameRecordTest extends TestCase {
    
    public GameRecordTest(String testName) {
        super(testName);
    }

    @Override
    protected void setUp() throws Exception {
        super.setUp();
    }

    @Override
    protected void tearDown() throws Exception {
        super.tearDown();
    }

    public void testConstructorFinished() throws Exception {

        String gameString =
            "125;FellOmen2{20 5 80} Probe{-40 10 0}; 0d{9s 8s} 1d{Qh Jd} 0r10 1r10 0r10 d{5c 8d 6s} 1c 0c d{Th} 1c 0c d{2s} 1c 0c.";

        GameRecord gr = new GameRecord(gameString);
        assertEquals("125", gr.Id);
        assertTrue(gr.IsGameOver);
        assertEquals(2, gr.Players.size());
        assertEquals("FellOmen2", gr.Players.get(0).Name);
        assertEquals(20.0, gr.Players.get(0).Stack);
        assertEquals(5.0, gr.Players.get(0).Blind);
        assertEquals(80.0, gr.Players.get(0).Result);
        assertEquals("Probe", gr.Players.get(1).Name);
        assertEquals(-40.0, gr.Players.get(1).Stack);
        assertEquals(10.0, gr.Players.get(1).Blind);
        assertEquals(0.0, gr.Players.get(1).Result);
        assertEquals(14, gr.Actions.size());
        VerifyActionRecord(0, Ak.d, "9s 8s", -1, gr.Actions.get(0));
        VerifyActionRecord(1, Ak.d, "Qh Jd", -1, gr.Actions.get(1));
        VerifyActionRecord(0, Ak.r, "", 10, gr.Actions.get(2));
        VerifyActionRecord(1, Ak.r, "", 10, gr.Actions.get(3));
        VerifyActionRecord(0, Ak.r, "", 10, gr.Actions.get(4));
        VerifyActionRecord(-1, Ak.d, "5c 8d 6s", -1, gr.Actions.get(5));
        VerifyActionRecord(1, Ak.c, "", -1, gr.Actions.get(6));
        VerifyActionRecord(0, Ak.c, "", -1, gr.Actions.get(7));
        VerifyActionRecord(-1, Ak.d, "Th", -1, gr.Actions.get(8));
        VerifyActionRecord(1, Ak.c, "", -1, gr.Actions.get(9));
        VerifyActionRecord(0, Ak.c, "", -1, gr.Actions.get(10));
        VerifyActionRecord(-1, Ak.d, "2s", -1, gr.Actions.get(11));
        VerifyActionRecord(1, Ak.c, "", -1, gr.Actions.get(12));
        VerifyActionRecord(0, Ak.c, "", -1, gr.Actions.get(13));
    }

    public void testConstructorUnfinished() throws Exception
    {
        // No Id, unfinished
        String gameString = ";FellOmen2{20.1 .5 0.0} Probe{-40.44 1.0 0};;";

        GameRecord gr = new GameRecord(gameString);
        assertEquals("", gr.Id);
        assertFalse(gr.IsGameOver);
        assertEquals(2, gr.Players.size());
        assertEquals("FellOmen2", gr.Players.get(0).Name);
        assertEquals(20.1, gr.Players.get(0).Stack);
        assertEquals(.5, gr.Players.get(0).Blind);
        assertEquals(0.0, gr.Players.get(0).Result);
        assertEquals("Probe", gr.Players.get(1).Name);
        assertEquals(-40.44, gr.Players.get(1).Stack);
        assertEquals(1.0, gr.Players.get(1).Blind);
        assertEquals(0.0, gr.Players.get(1).Result);
        assertEquals(0, gr.Actions.size());
    }

    private void VerifyActionRecord(int pos, Ak kind, String cards, double amount, PokerAction ar)
    {
        assertEquals(pos, ar.getPosition());
        assertEquals(kind, ar.getKind());
        assertEquals(cards, ar.getCards());
        assertEquals(amount, ar.getAmount());
    }

}
