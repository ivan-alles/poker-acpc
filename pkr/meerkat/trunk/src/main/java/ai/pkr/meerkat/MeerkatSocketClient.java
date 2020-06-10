/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package ai.pkr.meerkat;

import ai.pkr.metabots.remote.Protocol.*;
import ai.pkr.metabots.remote.Protocol;
import ai.pkr.jmetabots.*;

import ca.ualberta.cs.poker.free.academy25.GameInfoDynamics;
import ca.ualberta.cs.poker.free.academy25.GameInfoImpl;
import ca.ualberta.cs.poker.free.dynamics.MatchStateMessage;
import com.biotools.meerkat.*;

import java.util.List;
import java.util.ArrayList;

/** A SocketClient that allows to play a Meerkat Player.
 * PokerAcademyClient was a prototype for this class.
 *
 * Limitations:
 *  - Only HE FL 10/20 2 players is supported, raise limit must be 4.
 *
 */
public class MeerkatSocketClient extends SocketClient {

    public MeerkatSocketClient()
    {
        _dynamics = new GameInfoDynamics();
        _gameinfo = new GameInfoImpl(_dynamics);
    }

    public void setPlayer(Player player, String name) {
        _player = player;
        _name = name;
    }

    @Override
    public Protocol.PlayerInfo OnServerConnect() {
        return Protocol.PlayerInfo.newBuilder().setName(_name).build();
    }


    @Override
    public void OnServerDisconnect(String reason) {
        System.out.print("Disconnected from server, reason: ");
        System.out.println(reason);
    }

    @Override
    public void OnSessionBegin(String sessionName, GameDefinition gameDef, Props sessionParameters) {
        _gameDef = gameDef;
        _sessionParameters = sessionParameters;
    }

    @Override
    public void OnSessionEnd() throws Exception {
    }

    @Override
    public void OnGameBegin(String gameString) throws Exception {
        _gameRecord = new GameRecord(gameString);
        _position = -1;
        for(int p = 0; p < _gameRecord.Players.size(); ++p)
        {
            if(_gameRecord.Players.get(p).Name.equals(_name))
            {
                _position = p;
                break;
            }
        }
        _ourCards = "";
        _lastActions = new ArrayList<PokerAction>();
        _message = new MatchStateMessage(_gameRecord, _position, _ourCards);
    }

    @Override
    public void OnGameUpdate(String gameString) throws Exception {
    }

    @Override
    public PokerAction OnActionRequired(String gameString) throws Exception {
        _gameRecord = new GameRecord(gameString);
        _message = new MatchStateMessage(_gameRecord, _position, _ourCards);
        
        handleNewActions();

        com.biotools.meerkat.Action mktAction = _player.getAction();
        PokerAction.Builder builder = PokerAction.newBuilder().setAmount(mktAction.getAmount());
        if (mktAction == null || mktAction.isFold()) {
            builder.setKind(Ak.f);
        } else if (mktAction.isCheckOrCall()) {
            builder.setKind(Ak.c);
        } else if (mktAction.isBetOrRaise()) {
            builder.setKind(Ak.r);
        } else {
            throw new Exception("Unknown action type");
        }
        builder.setPosition(-1);
        builder.setCards("");
        return builder.build();
    }

    @Override
    public void OnGameEnd(String gameString) throws Exception  {
        _gameRecord = new GameRecord(gameString);
        _message = new MatchStateMessage(_gameRecord, _position, _ourCards);
        handleShowdown();
    }

    // --------------------- PokerAcademyClient code begin  ------------------
    /**
     * Not sure why I can't just run new Hand(str),
     * but this will work for now.
     */
    public static Hand getHand(String str) {
        Hand h = new Hand();
        for (int i = 0; i < str.length(); i += 2) {
            Card c = new Card(str.substring(i, i + 2));
            h.addCard(c);
        }
        return h;
    }

    /**
     * Called at the start of the game
     */
    private void handleStartGame() {
        _dynamics.doNewGame(_message.handNumber, _position);
        _player.gameStartEvent(_gameinfo);
        _player.stageEvent(0);
        // Small blind
        _player.actionEvent(_gameinfo.getSmallBlindSeat(), Action.smallBlindAction(_gameinfo.getSmallBlindSize()));
        _dynamics.doPostSmallBlind();
        _player.gameStateChanged();
        // Big blind
        _dynamics.currentPlayerSeat = _dynamics.getOtherSeat(_dynamics.button);
        _player.actionEvent(_dynamics.getOtherSeat(_gameinfo.getSmallBlindSeat()), Action.bigBlindAction(_gameinfo.getBigBlindSize()));
        _dynamics.doPostBigBlind();
        _player.gameStateChanged();
        _dynamics.currentPlayerSeat = _dynamics.button;
        _player.dealHoleCardsEvent();

        //System.out.println("Hole cards:" + _message.hole[_message.seatTaken]);
        Hand hole = getHand(_message.hole[_message.seatTaken]);
        //System.out.println("Hole cards converted:"+hole);
        _player.holeCards(hole.getFirstCard(), hole.getLastCard(), 0);
    }

//    /**
//     * Called whenever an action is sent FROM the server.
//     */
//    private void handleAction() {
//        int index = message.getLastAction();
//        switch (index) {
//            case 0:
//                handleFold();
//                break;
//            case 1:
//                handleCall();
//                break;
//            case 2:
//                handleRaise();
//                break;
//            default:
//                break;
//        }
//    }

    /**
     * Called whenever a call action is sent FROM the server.
     */
    private void handleCall() {
        _player.actionEvent(_gameinfo.getCurrentPlayerSeat(), Action.callAction(_gameinfo));
        _dynamics.doPostCheckOrCall();
        _player.gameStateChanged();
        if (_gameinfo.getNumToAct() == 0) {
            if (_gameinfo.getStage() == Holdem.RIVER) {
//                handleShowdown();
            } else {
                handleStage();
            }
        } else {
            _dynamics.changeCurrentSeat();
        }
    }

    /**
     * Called whenever a raise action is sent FROM the server.
     */
    private void handleRaise() {
        _player.actionEvent(_gameinfo.getCurrentPlayerSeat(), Action.raiseAction(_gameinfo));
        _dynamics.doPostBetOrRaise();
        _player.gameStateChanged();
        _dynamics.changeCurrentSeat();
    }

    private void handleFold() {
        _player.actionEvent(_gameinfo.getCurrentPlayerSeat(), Action.foldAction(_gameinfo));
        _dynamics.doPostFold();
        _player.gameStateChanged();
        _dynamics.doPreWinEvent(_dynamics.getOtherSeat(_gameinfo.getCurrentPlayerSeat()));
        _player.winEvent(_gameinfo.getCurrentPlayerSeat(), _gameinfo.getTotalPotSize(), null);
        _dynamics.doPreGameOver();
        _player.gameOverEvent();
    }

    private void handleStage() {
        _dynamics.setBoard(_message.board);
        _dynamics.doPreStageEvent(_dynamics.stage + 1);
        _player.stageEvent(_dynamics.stage);
    }

    /**
     * At present, an empty string is sent with each win event.
     */
    private void handleShowdown() {
        // System.out.println("handleShowdown:Client:"+getClientID()+currentGameStateString+":stage:"+gameinfo.getStage());
        handleShowCardsAtShowdown(0);
        handleShowCardsAtShowdown(1);
        int winnerPos = -1;
        if(_gameRecord.Players.get(_position).Result > _gameRecord.Players.get(1 - _position).Result) {
            winnerPos = _position;
        }
        else if(_gameRecord.Players.get(_position).Result < _gameRecord.Players.get(1 - _position).Result) {
            winnerPos = 1 - _position;
        }

        if (winnerPos == -1) {
            _dynamics.doPreTieEvent(0);
            _player.winEvent(0, _gameinfo.getTotalPotSize() / 2.0, "");
            _dynamics.doPreTieEvent(1);
            _player.winEvent(1, _gameinfo.getTotalPotSize() / 2.0, "");
        } else {
            // Old code (winner == seat (SB: 1, BB: 0)).
            // Need to flip winner if we are in a different seat
            // dynamics.doPreWinEvent((message.seatTaken == 0) ? winner : (1 - winner));
            // Old code end
            _dynamics.doPreWinEvent((_message.seatTaken == 0) ? 1 - winnerPos : (1 - winnerPos));
            _player.winEvent(_gameinfo.getCurrentPlayerSeat(), _gameinfo.getTotalPotSize(), "");
        }
        _dynamics.doPreGameOver();
        _player.gameOverEvent();
    }

    /**
     * Show a particular player's card at the showdown.
     * Note: there is no mucking.
     */
    private void handleShowCardsAtShowdown(int seat) {
        int serverSeat = (_message.seatTaken == 0) ? seat : (1 - seat);
        Hand hole = getHand(_message.hole[serverSeat]);
        _dynamics.hole[serverSeat] = new Hand(hole);
        _player.showdownEvent(seat, hole.getFirstCard(), hole.getLastCard());
    }

    /**
     * Called whenever the state is changed.
     */
    /*
    public void handleStateChange() {
        if (gameinfo == null) {
            dynamics = new GameInfoDynamics();
            gameinfo = new GameInfoImpl(dynamics);
            handleStartGame();
        } else {
            long oldHandNumber = gameinfo.getGameID();
            //int oldStage = gameinfo.getStage();
            MatchStateMessage message = new MatchStateMessage(currentGameStateString);
            if (oldHandNumber != message.handNumber) {
                handleStartGame();
            } else {
                handleAction();
            }
        }
        if (gameinfo.getCurrentPlayerSeat() == 0) {

            // System.out.println("ACT:Client:"+getClientID()+currentGameStateString+":roundBets:"+dynamics.roundBets);

            Action a = player.getAction();
            if (a == null) {
                // sendFold();
            } else if (a.isCheckOrCall()) {
                //sendCall();
            } else if (a.isBetOrRaise()) {
                //sendRaise();
            } else {
                //sendFold();
            }
        }
    }
    */
    // --------------------- PokerAcademyClient code end    ------------------

    
    public void handleDeal(PokerAction dealAction) {
        _ourCards = dealAction.getCards();
        _message = new MatchStateMessage(_gameRecord, _position, _ourCards);
        if(dealAction.getPosition() == _position)
        {
            handleStartGame();
        }
    }

    private void handleNewActions()
    {
        ArrayList<PokerAction> curActions = new ArrayList(_gameRecord.Actions);
        for(int a = _lastActions.size(); a < curActions.size(); ++a)
        {
            Ak actionKind = curActions.get(a).getKind();
            int pos = curActions.get(a).getPosition();
            if (actionKind == Ak.f) {
                handleFold();
            } else if (actionKind == Ak.c) {
                handleCall();
            } else if (actionKind == Ak.r) {
                handleRaise();
            }
            else if (actionKind == Ak.d) {
                if(pos != -1)
                {
                    // Deal poket cards
                    handleDeal(curActions.get(a));
                }
            }
            else {
                throw new RuntimeException("Unexpected player action " + actionKind.toString());
            }
        }
        _lastActions = curActions;
    }

    protected GameInfoDynamics _dynamics;
    protected GameInfoImpl _gameinfo;
    protected Player _player;
    protected GameRecord _gameRecord;
    protected ArrayList<PokerAction> _lastActions;
    // Position received from prkserver (0 or 1).
    protected int _position;
    protected Props _sessionParameters;
    protected MatchStateMessage _message;
    protected GameDefinition _gameDef;
    protected String _ourCards;
    protected String _name;
}
