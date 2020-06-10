/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package ai.pkr.jmetabots;

import java.util.List;
// Use this instead of metagame classes as workaround.
import ai.pkr.metabots.remote.Protocol.*;
import java.util.ArrayList;
import java.util.regex.*;

/**
 *
 * @author ivan
 */
public class GameRecord {

    public class Player
    {
        public String Name;
        public double Stack;
        public double Blind;
        public double Result;
    }

    public String Id;
    public Boolean IsGameOver;
    public List<Player> Players;
    public List<PokerAction> Actions;

    public GameRecord(String gameString) throws Exception
    {
        Players = new ArrayList<Player>();
        Actions = new ArrayList<PokerAction>();

        Pattern reGameString = Pattern.compile("([^;]*);([^;]+);([^;.]*)([;.])");
        Matcher matcher = reGameString.matcher(gameString);
        if(! matcher.find())
        {
            throw new Exception("Wrong game string " + gameString);
        }
        Id = matcher.group(1);
        String playersStr = matcher.group(2);
        String actionsStr = matcher.group(3);
        String terminatorStr = matcher.group(4);

        IsGameOver = terminatorStr.equals(".");

        Pattern rePlayer = Pattern.compile("\\s*([^\\s{]+)\\s*\\{\\s*([-0-9.]+)\\s+([-0-9.]+)\\s+([-0-9.]+)\\s*\\}");
        matcher = rePlayer.matcher(playersStr);

        while (matcher.find())
        {
            Player player = new Player();
            player.Name = matcher.group(1);
            player.Stack = Double.parseDouble(matcher.group(2));
            player.Blind = Double.parseDouble(matcher.group(3));
            player.Result = Double.parseDouble(matcher.group(4));

            Players.add(player);
        }

        Pattern reActionDeal = Pattern.compile("^\\s*([0-9]+)([pd])\\s*\\{([^}]*)\\}");
        Pattern reAction = Pattern.compile("^\\s*([0-9]+)([a-zA-Z])([-0-9.]+)?");
        Pattern reActionSharedDeal = Pattern.compile("^\\s*([d])\\s*\\{([^\\}]*)\\}");


        for(;;)
        {
            PokerAction.Builder actionBld = PokerAction.newBuilder();
            Matcher mActionDeal = reActionDeal.matcher(actionsStr);
            if(mActionDeal.find())
            {
                actionBld.setPosition(Integer.parseInt(mActionDeal.group(1)));
                actionBld.setKind(getActionKind(mActionDeal.group(2).charAt(0)));
                actionBld.setCards(mActionDeal.group(3));
                actionBld.setAmount(-1.0);
                Actions.add(actionBld.build());
                actionsStr = actionsStr.substring(mActionDeal.end());
                continue;
            }
            Matcher mAction = reAction.matcher(actionsStr);
            if(mAction.find())
            {
                actionBld.setPosition(Integer.parseInt(mAction.group(1)));
                actionBld.setKind(getActionKind(mAction.group(2).charAt(0)));
                actionBld.setCards("");
                if(mAction.group(3) == null)
                {
                    actionBld.setAmount(-1.0);
                }
                else
                {
                    actionBld.setAmount(Double.parseDouble(mAction.group(3)));
                }
                Actions.add(actionBld.build());
                actionsStr = actionsStr.substring(mAction.end());
                continue;
            }
            Matcher mActionSharedDeal = reActionSharedDeal.matcher(actionsStr);
            if(mActionSharedDeal.find())
            {
                actionBld.setPosition(-1);
                actionBld.setKind(getActionKind(mActionSharedDeal.group(1).charAt(0)));
                actionBld.setCards(mActionSharedDeal.group(2));
                actionBld.setAmount(-1.0);
                Actions.add(actionBld.build());
                actionsStr = actionsStr.substring(mActionSharedDeal.end());
                continue;
            }
            break;
        }
    }

    private Ak getActionKind(char actionChar)
    {
        switch(actionChar)
        {
            case 'b': return Ak.b;
            case 'd': return Ak.d;
            case 'r': return Ak.r;
            case 'c': return Ak.c;
            case 'f': return Ak.f;
        }
        return Ak.b;
    }
}
