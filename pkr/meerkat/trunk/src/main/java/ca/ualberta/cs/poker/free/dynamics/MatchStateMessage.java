/*
 * GameStateMessage.java
 *
 *
 * Created on April 20, 2006, 3:33 PM
 */
package ca.ualberta.cs.poker.free.dynamics;

import ai.pkr.metabots.remote.Protocol.*;
import ai.pkr.jmetabots.*;

/**
 *
 * @author Martin Zinkevich
 */
public class MatchStateMessage {

    /**
     * The seat taken by the player who receives the message.
     * Small blind must have seat 1, big blind seat 0.
     * See Limit Protocol par. 3.
     */
    public int seatTaken;
    /**
     * The hand number, from 0-999.
     */
    public int handNumber;
    /**
     * Contains the hole cards, indexed by seat.
     * This player's cards are in hole[seatTaken]
     */
    public String[] hole;
    /**
     * Contains all of the cards on the board.
     */
    public String board;
    public String bettingSequence;
    GameRecord _gameRecord;
    int _position;

    public MatchStateMessage(GameRecord gameRecord, int position, String ourCards) {
        _gameRecord = gameRecord;
        _position = position;
        handNumber = Integer.parseInt(gameRecord.Id);
        seatTaken = 1 - _position;
        setCards();
        if(ourCards.length() > 0)
        {
            hole[seatTaken] = ourCards.replaceAll(" ", "");
        }
    }

//    /**
//     * Tests if this is the end of a stage.
//     * Note: this returns false at the showdown.
//     */
//    public boolean endOfStage() {
//        int round = _gameState.getRound();
//
//        if (round == 3) {
//            // Make sure it returns false at showdown, as originally designed by UoA.
//            return false;
//        }
//
//        int roundActionsCount = _gameState.getActions(round).getActionsCount();
//        if (roundActionsCount == 0) {
//            return false;
//        }
//
//        Protocol.ActionKind actionKind = _gameState.getActions(round).getActions(roundActionsCount - 1).getAction().getKind();
//
//        if (actionKind == Protocol.ActionKind.Raise) {
//            return false;
//        }
//
//        return true;
//    }

    public int getLastAction() {
        int actionsCount = _gameRecord.Actions.size();
        if (actionsCount == 0) {
            return -1;
        }

        Ak actionKind = _gameRecord.Actions.get(actionsCount - 1).getKind();
        if (actionKind == Ak.f) {
            return 0;
        } else if (actionKind == Ak.c) {
            return 1;
        } else if (actionKind == Ak.r) {
            return 2;
        }
        throw new RuntimeException("Unexpected player action");
    }

    private void setCards() {
        hole = new String[2];
        board = "";
        for(int a = 0; a < _gameRecord.Actions.size(); ++a)
        {
            PokerAction action = _gameRecord.Actions.get(a);
            int pos = action.getPosition();
            if(action.getKind() == Ak.d)
            {
                if(pos != -1)
                {
                    int seat = 1 - action.getPosition();
                    hole[seat] = action.getCards().replaceAll(" ", "");
                }
                else  {
                    board += action.getCards().replaceAll(" ", "");
                }
            }
        }
    }
}
