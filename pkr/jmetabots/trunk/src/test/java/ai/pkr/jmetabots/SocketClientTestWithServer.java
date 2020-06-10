/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package ai.pkr.jmetabots;

import ai.pkr.metabots.remote.Protocol.*;
import junit.framework.TestCase;
import junit.*;
import static java.util.Arrays.*;

/**
 *
 * @author ivan
 * Tests SocketClient with the c# server. The server must be started before this test.
 * @see ai.pkr.metabots.nunit.SocketPlayer_Test.Test_RemoteClient().
 */
public class SocketClientTestWithServer extends TestCase {
    
    public SocketClientTestWithServer(String testName) {
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

    /**
     * Test of Run method, of class SocketClient.
     */
    public void testRun() throws Exception {
        System.out.println("Run");
        SocketClientImpl socketClient = new SocketClientImpl();
        socketClient.Run(9001);
        assertEquals(8, socketClient._callCounter);
    }

    public class SocketClientImpl extends SocketClient {
        public int _callCounter = 0;


        public PlayerInfo OnServerConnect() {
            PlayerInfo response = PlayerInfo.newBuilder().setName("SocketClientTest").build();
            _callCounter++;
            return response;
        }

        public void OnServerDisconnect(String reason) {
            _callCounter++;
            assertEquals("End of test", reason);
        }

        public void OnSessionBegin(String sessionName, GameDefinition gameDef, Props sessionParameters) {
            _callCounter++;
            assertEquals("Test session", sessionName);
            assertEquals("Test game", gameDef.getName());
            assertEquals(asList("p1", "p2"), sessionParameters.getNamesList());
            assertEquals(asList("v1", "v2"), sessionParameters.getValuesList());
        }

        public void OnSessionEnd() {
            _callCounter++;
        }

        public void OnGameBegin(String gameString) {
            _callCounter++;
            assertEquals("gamestring OnGameBegin", gameString);
        }

        public void OnGameUpdate(String gameString) {
            _callCounter++;
            assertEquals("gamestring OnGameUpdate", gameString);
        }

        public PokerAction OnActionRequired(String gameString) {
            _callCounter++;
            assertEquals("gamestring OnActionRequired", gameString);
            PokerAction response = PokerAction.newBuilder().setKind(Ak.r).setAmount(100).setPosition(-1).setCards("").build();
            return response;
        }

        public void OnGameEnd(String gameString) {
            _callCounter++;
            assertEquals("gamestring OnGameEnd", gameString);
        }
    }

}
