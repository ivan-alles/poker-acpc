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
package com.ibot;

import java.util.*;
import java.io.*;
import java.util.jar.*;
import java.util.zip.*;

import com.biotools.meerkat.Action;
import com.biotools.meerkat.Card;
//import com.biotools.meerkat.Deck ;
import com.biotools.meerkat.GameInfo;
import com.biotools.meerkat.Hand;

import poker.HandEvaluator;

//import com.ibot.HandPotential;
/** 
 * Ian Bot Impl
 * 
 * Description: {Insert Description here}
 * 
 * @author Ian Fellows
 */
public class FellOmen_2_Impl extends Object {
    // constants used:

    private final static int AHEAD = 0;
    private final static int TIED = 1;
    private final static int BEHIND = 2;
    private int preFlopHistory;
    private Random _rng = new Random();
    PrintStream _debugLog = null;
    boolean _isDebug;
    String _stategyPath = "C:\\fo2-strategy";


    public FellOmen_2_Impl() throws IOException {
    }
    
    public void setStrategyPath(String path) throws Exception
    {
        _stategyPath = path;
    }

    public void setLogFile(PrintStream logFile) throws Exception
    {
        _debugLog = logFile;
    }


    public void setRngSeed(long seed)
    {
        if(seed == 0)
            _rng = new Random();
        else
            _rng.setSeed(seed);
        debug("RNG seed set to " + Long.toString(seed));
    }

    /**
     * Decide what to do for a pre-flop action
     *
     * Uses a really simple hand selection, as a silly example.
     */
    public Action preFlopAction(GameInfo gi, Card c1, Card c2, int ourSeat) {
        Action response = null;
        String player;
        double toCall = gi.getAmountToCall(ourSeat);
        debug("Hand: [" + c1.toString() + "-" + c2.toString() + "] ");
        debug("is Button : " + (gi.getCurrentPlayerSeat() == gi.getButtonSeat()));

        if (!(gi.getCurrentPlayerSeat() == gi.getButtonSeat())) {
            player = "p1";
        } else {
            player = "p2";
        }
        debug("player: " + player);
        // Get the pre flop index,
        //	this function takes hole cards and returns table index,
        int preFlopIndex = this.getPreflopIndex(c1, c2);
        debug("preflop index: " + preFlopIndex);

        // Generate the action table file name
        debug("action node : " + get_action_node(gi));
        String actionTableFileName = player + "_strategy_" + get_action_node(gi) + ".txt";
        debug("preflop file: " + actionTableFileName);

        // Now that we have the pre flop index and the action table file name, look-up what action to take?
        //	0 is preflop round
        // strategy files are place in a folder called strategyFiles in /Resources/Java/
        // thus is a kludge until I figure out how to refer to the files within the jar
        double[][] actionDblArray = getActionTable(0, actionTableFileName);

        // Now use this knowledge to determine what to do.
        double raiseProb = actionDblArray[2][preFlopIndex];
        double callProb = actionDblArray[1][preFlopIndex];
        double foldProb = actionDblArray[0][preFlopIndex];
        debug("Prob[raise call fold] : " + raiseProb + " " + callProb + " " + foldProb);
        double rand = _rng.nextDouble();
        debug("random number = " + rand);
        if (rand < foldProb) {
            return Action.checkOrFoldAction(toCall);
        } else if (rand < (foldProb + callProb)) {
            return Action.callAction(toCall);
        } else if (rand < (foldProb + callProb + raiseProb)) {
            return Action.raiseAction(gi);
        }



        return (response);
    }

    public Action flopAction(GameInfo gi, double handRank, double potential, double negPotential, int flopBDindex, int ourSeat, int preflopHist) {
        Action response = null;
        String player;
        double toCall = gi.getAmountToCall(ourSeat);
        if (!(gi.getCurrentPlayerSeat() == gi.getButtonSeat())) {
            player = "p1";
        } else {
            player = "p2";
        }

        //calculate probability of winning against a random hand
        double rolloutHandStrength = handRank * (1 - negPotential) + (1 - handRank) * potential;

        //get index
        int flopIndex = this.getFlopIndex(rolloutHandStrength, potential);
        debug("rollout Hand Strength: " + rolloutHandStrength + " flop index: " + flopIndex);
        //find table
        String actionTableFileName = player + "_strategy" + preflopHist + "" + flopBDindex + "_" + get_action_node(gi) + ".txt";
        debug("flop file: " + actionTableFileName);

        // strategy files are place in a folder called strategyFiles in /Resources/Java/
        // thus is a kludge until I figure out how to refer to the files within the jar
        double[][] actionDblArray = getActionTable(1, actionTableFileName);

        // Now use this knowledge to determine what to do.
        double raiseProb = actionDblArray[2][flopIndex];
        double callProb = actionDblArray[1][flopIndex];
        double foldProb = actionDblArray[0][flopIndex];
        debug("Prob[raise call fold] : " + raiseProb + " " + callProb + " " + foldProb);
        double rand = _rng.nextDouble();
        debug("random number = " + rand);
        if (rand < foldProb) {
            return Action.checkOrFoldAction(toCall);
        } else if (rand < (foldProb + callProb)) {
            return Action.callAction(toCall);
        } else if (rand < (foldProb + callProb + raiseProb)) {
            return Action.raiseAction(gi);
        }

        return (response);
    }

    public Action turnAction(GameInfo gi, double handRank, double potential, double negPotential, int flopBDindex, int ourSeat, int preflopHist, int flopHist) {
        Action response = null;
        String player;
        double toCall = gi.getAmountToCall(ourSeat);
        if (!(gi.getCurrentPlayerSeat() == gi.getButtonSeat())) {
            player = "p1";
        } else {
            player = "p2";
        }

        //calculate probability of winning against a random hand
        double rolloutHandStrength = handRank * (1 - negPotential) + (1 - handRank) * potential;

        //get index
        int turnIndex = this.getTurnIndex(rolloutHandStrength, potential);
        debug("rollout Hand Strength: " + rolloutHandStrength + " turn index: " + turnIndex);

        //find table
        String actionTableFileName = player + "_strategy" + preflopHist + flopBDindex + flopHist + 0 + "_" + get_action_node(gi) + ".txt";
        debug("turn file: " + actionTableFileName);

        // strategy files are place in a folder called strategyFiles in /Resources/Java/
        // thus is a kludge until I figure out how to refer to the files within the jar
        double[][] actionDblArray = getActionTable(2, actionTableFileName);

        // Now use this knowledge to determine what to do.
        double raiseProb = actionDblArray[2][turnIndex];
        double callProb = actionDblArray[1][turnIndex];
        double foldProb = actionDblArray[0][turnIndex];
        debug("Prob[raise call fold] : " + raiseProb + " " + callProb + " " + foldProb);
        double rand = _rng.nextDouble();
        debug("random number = " + rand);
        if (rand < foldProb) {
            return Action.checkOrFoldAction(toCall);
        } else if (rand < (foldProb + callProb)) {
            return Action.callAction(toCall);
        } else if (rand < (foldProb + callProb + raiseProb)) {
            return Action.raiseAction(gi);
        }

        return (response);
    }

    public Action riverAction(GameInfo gi, double handRank, int flopBDindex, int ourSeat, int preflopHist, int flopHist, int turnHist) {
        Action response = null;
        String player;
        double toCall = gi.getAmountToCall(ourSeat);
        if (!(gi.getCurrentPlayerSeat() == gi.getButtonSeat())) {
            player = "p1";
        } else {
            player = "p2";
        }

        //calculate probability of winning against a random hand
        double rolloutHandStrength = handRank;

        //get index
        int riverIndex = this.getRiverIndex(rolloutHandStrength);
        debug("rollout Hand Strength: " + rolloutHandStrength + " river index: " + riverIndex);

        //find table
        String actionTableFileName = player + "_strategy" + preflopHist + flopBDindex + flopHist + 0 + turnHist + 0 + "_" + get_action_node(gi) + ".txt";
        debug("river file: " + actionTableFileName);

        // strategy files are place in a folder called strategyFiles in /Resources/Java/
        // thus is a kludge until I figure out how to refer to the files within the jar
        double[][] actionDblArray = getActionTable(3, actionTableFileName);

        // Now use this knowledge to determine what to do.
        double raiseProb = actionDblArray[2][riverIndex];
        double callProb = actionDblArray[1][riverIndex];
        double foldProb = actionDblArray[0][riverIndex];
        debug("Prob[raise call fold] : " + raiseProb + " " + callProb + " " + foldProb);
        double rand = _rng.nextDouble();
        debug("random number = " + rand);
        if (rand < foldProb) {
            return Action.checkOrFoldAction(toCall);
        } else if (rand < (foldProb + callProb)) {
            return Action.callAction(toCall);
        } else if (rand < (foldProb + callProb + raiseProb)) {
            return Action.raiseAction(gi);
        }

        return (response);
    }

    /**
     * returns the column from which to look up the raise/call/fold probabilities.
     */
    protected int getFlopIndex(double hs, double pot) {
        double[][] flopIndex = {{-.00001, 0.181544, 0, 1},
            {0.181544, 0.200837, 0, 0.18},
            {0.181544, 0.200837, 0.18, 1},
            {0.200837, 0.217374, 0, 0.19},
            {0.200837, 0.217374, 0.19, 1},
            {0.217374, 0.232018, 0, 0.184653},
            {0.217374, 0.232018, 0.184653, 0.209812},
            {0.217374, 0.232018, 0.209812, 1},
            {0.232018, 0.24624, 0, 0.187873},
            {0.232018, 0.24624, 0.187873, 0.216469},
            {0.232018, 0.24624, 0.216469, 1},
            {0.24624, 0.260174, 0, 0.14}, //start
            {0.24624, 0.260174, 0.14, 0.188539},
            {0.24624, 0.260174, 0.188539, 0.223861},
            {0.24624, 0.260174, 0.223861, 1},
            {0.260174, 0.273396, 0, 0.15},
            {0.260174, 0.273396, 0.15, 0.191511},
            {0.260174, 0.273396, 0.191511, 0.22},
            {0.260174, 0.273396, 0.22, 0.245},
            {0.260174, 0.273396, 0.2245, 1},
            {0.273396, 0.285754, 0, 0.12},//8
            {0.273396, 0.285754, 0.12, 0.18},
            {0.273396, 0.285754, 0.18, 0.22},
            {0.273396, 0.285754, 0.22, 0.27},
            {0.273396, 0.285754, 0.27, 1},
            {0.285754, 0.297962, 0, 0.14},//9
            {0.285754, 0.297962, 0.14, 0.17},
            {0.285754, 0.297962, 0.17, 0.2},
            {0.285754, 0.297962, 0.2, 0.24},
            {0.285754, 0.297962, 0.24, 0.275184},
            {0.285754, 0.297962, 0.275184, 1},
            {0.297962, 0.310096, 0, 0.15},//10
            {0.297962, 0.310096, 0.15, 0.17},
            {0.297962, 0.310096, 0.17, 0.2},
            {0.297962, 0.310096, 0.2, 0.25},
            {0.297962, 0.310096, 0.25, 0.29},
            {0.297962, 0.310096, 0.29, 1},
            {0.310096, 0.32187, 0, 0.14},//11
            {0.310096, 0.32187, 0.14, 0.17},
            {0.310096, 0.32187, 0.17, 0.2},
            {0.310096, 0.32187, 0.2, 0.25},
            {0.310096, 0.32187, 0.25, 0.295},
            {0.310096, 0.32187, 0.295, 1},
            {0.32187, 0.333163, 0, 0.15},//12
            {0.32187, 0.333163, 0.15, 0.18},
            {0.32187, 0.333163, 0.18, 0.2},
            {0.32187, 0.333163, 0.2, 0.25},
            {0.32187, 0.333163, 0.25, 0.305},
            {0.32187, 0.333163, 0.305, 1},
            {0.333163, 0.344575, 0, 0.14},//13
            {0.333163, 0.344575, 0.14, 0.175},
            {0.333163, 0.344575, 0.175, 0.2},
            {0.333163, 0.344575, 0.2, 0.25},
            {0.333163, 0.344575, 0.25, 0.305},
            {0.333163, 0.344575, 0.305, 1},
            {0.344575, 0.356258, 0, 0.15},//14
            {0.344575, 0.356258, 0.15, 0.21},
            {0.344575, 0.356258, 0.21, 0.301999},
            {0.344575, 0.356258, 0.301999, 1},
            {0.356258, 0.367857, 0, 0.14},//15
            {0.356258, 0.367857, 0.14, 0.197024},
            {0.356258, 0.367857, 0.197024, 0.297052},
            {0.356258, 0.367857, 0.297052, 1},
            {0.367857, 0.379221, 0, 0.15},//16
            {0.367857, 0.379221, 0.15, 0.200957},
            {0.367857, 0.379221, 0.200957, 0.25},
            {0.367857, 0.379221, 0.25, 0.301928},
            {0.367857, 0.379221, 0.301928, 1},
            {0.379221, 0.390339, 0, 0.15},//17
            {0.379221, 0.390339, 0.15, 0.200469},
            {0.379221, 0.390339, 0.200469, 0.25},
            {0.379221, 0.390339, 0.25, 0.307184},
            {0.379221, 0.390339, 0.307184, 1},
            {0.390339, 0.401287, 0, 0.15},//18
            {0.390339, 0.401287, 0.15, 0.2},
            {0.390339, 0.401287, 0.2, 0.26},
            {0.390339, 0.401287, 0.26, 0.319001},
            {0.390339, 0.401287, 0.319001, 1},
            {0.401287, 0.412022, 0, 0.16},//19
            {0.401287, 0.412022, 0.16, 0.22},
            {0.401287, 0.412022, 0.22, 0.27},
            {0.401287, 0.412022, 0.27, 0.329525},
            {0.401287, 0.412022, 0.329525, 1},
            {0.412022, 0.423171, 0, 0.15}, //end
            {0.412022, 0.423171, 0.15, 0.23},
            {0.412022, 0.423171, 0.23, 0.342425},
            {0.412022, 0.423171, 0.342425, 1},
            {0.423171, 0.434241, 0, 0.204641},
            {0.423171, 0.434241, 0.204641, 0.351605},
            {0.423171, 0.434241, 0.351605, 1},
            {0.434241, 0.445126, 0, 0.205289},
            {0.434241, 0.445126, 0.205289, 0.345118},
            {0.434241, 0.445126, 0.345118, 1},
            {0.445126, 0.45566, 0, 0.205622},
            {0.445126, 0.45566, 0.205622, 0.364398},
            {0.445126, 0.45566, 0.364398, 1},
            {0.45566, 0.465906, 0, 0.197749},
            {0.45566, 0.465906, 0.197749, 0.356551},
            {0.45566, 0.465906, 0.356551, 1},
            {0.465906, 0.476379, 0, 0.194802},
            {0.465906, 0.476379, 0.194802, 0.359017},
            {0.465906, 0.476379, 0.359017, 1},
            {0.476379, 0.487267, 0, 0.198988},
            {0.476379, 0.487267, 0.198988, 0.379558},
            {0.476379, 0.487267, 0.379558, 1},
            {0.487267, 0.499877, 0, 0.207355},
            {0.487267, 0.499877, 0.207355, 0.381521},
            {0.487267, 0.499877, 0.381521, 1},
            {0.499877, 0.51311, 0, 0.214409},
            {0.499877, 0.51311, 0.214409, 0.401484},
            {0.499877, 0.51311, 0.401484, 1},
            {0.51311, 0.526847, 0, 0.220147},
            {0.51311, 0.526847, 0.220147, 0.403477},
            {0.51311, 0.526847, 0.403477, 1},
            {0.526847, 0.539419, 0, 0.217194},
            {0.526847, 0.539419, 0.217194, 0.400222},
            {0.526847, 0.539419, 0.400222, 1},
            {0.539419, 0.551769, 0, 0.209808},
            {0.539419, 0.551769, 0.209808, 0.394505},
            {0.539419, 0.551769, 0.394505, 1},
            {0.551769, 0.563405, 0, 0.204326},
            {0.551769, 0.563405, 0.204326, 0.38023},
            {0.551769, 0.563405, 0.38023, 1},
            {0.563405, 0.575565, 0, 0.203777},
            {0.563405, 0.575565, 0.203777, 0.385543},
            {0.563405, 0.575565, 0.385543, 1},
            {0.575565, 0.588809, 0, 0.206088},
            {0.575565, 0.588809, 0.206088, 0.398419},
            {0.575565, 0.588809, 0.398419, 1},
            {0.588809, 0.60275, 0, 0.207607},
            {0.588809, 0.60275, 0.207607, 0.413887},
            {0.588809, 0.60275, 0.413887, 1},
            {0.60275, 0.617999, 0, 0.208423},
            {0.60275, 0.617999, 0.208423, 0.428492},
            {0.60275, 0.617999, 0.428492, 1},
            {0.617999, 0.635078, 0, 0.205276},
            {0.617999, 0.635078, 0.205276, 0.435578},
            {0.617999, 0.635078, 0.435578, 1},
            {0.635078, 0.652878, 0, 0.202151},
            {0.635078, 0.652878, 0.202151, 0.421534},
            {0.635078, 0.652878, 0.421534, 1},
            {0.652878, 0.671055, 0, 0.195768},
            {0.652878, 0.671055, 0.195768, 0.424436},
            {0.652878, 0.671055, 0.424436, 1},
            {0.671055, 0.690405, 0, 0.194021},
            {0.671055, 0.690405, 0.194021, 0.382227},
            {0.671055, 0.690405, 0.382227, 1},
            {0.690405, 0.709803, 0, 0.191548},
            {0.690405, 0.709803, 0.191548, 0.357075},
            {0.690405, 0.709803, 0.357075, 1},
            {0.709803, 0.730318, 0, 0.191921},
            {0.709803, 0.730318, 0.191921, 0.307036},
            {0.709803, 0.730318, 0.307036, 1},
            {0.730318, 0.75204, 0, 0.190881},
            {0.730318, 0.75204, 0.190881, 0.279885},
            {0.730318, 0.75204, 0.279885, 1},
            {0.75204, 0.774138, 0, 0.190348},
            {0.75204, 0.774138, 0.190348, 0.248434},
            {0.75204, 0.774138, 0.248434, 1},
            {0.774138, 0.796462, 0, 0.190203},
            {0.774138, 0.796462, 0.190203, 0.243945},
            {0.774138, 0.796462, 0.243945, 1},
            {0.796462, 0.818436, 0, 0.190549},
            {0.796462, 0.818436, 0.190549, 0.244617},
            {0.796462, 0.818436, 0.244617, 1},
            {0.818436, 0.840413, 0, 0.190436},
            {0.818436, 0.840413, 0.190436, 0.247491},
            {0.818436, 0.840413, 0.247491, 1},
            {0.840413, 0.867645, 0, 0.180515},
            {0.840413, 0.867645, 0.180515, 0.246644},
            {0.840413, 0.867645, 0.246644, 1},
            {0.867645, 0.918773, 0, 0.15226},
            {0.867645, 0.918773, 0.15226, 0.304464},
            {0.867645, 0.918773, 0.304464, 1},
            {0.918773, 1.000001, 0, 0.158128},
            {0.918773, 1.000001, 0.158128, 0.268578},
            {0.918773, 1.000001, 0.268578, 1}};
        int i = 0;
        while (i < 177) {
            if (hs >= flopIndex[i][0] && hs < flopIndex[i][1] && pot >= flopIndex[i][2] && (pot <= (flopIndex[i][3] + .0000000001))) {
                return (i);
            }
            i++;
        }
        debug("Invalid hs or pot, getFlopIndex: hs:" + hs + " pot:" + pot);
        return (-1);
    }

    protected int getTurnIndex(double hs, double pot) {
        double[][] turnIndex = {{-.00001, 0.112635, 0, 1},
            {0.112635, 0.140112, 0, 1},
            {0.140112, 0.158231, 0, 0.07}, //.00624 pot
            {0.140112, 0.158231, 0.07, 0.134934}, //.00624 pot
            {0.140112, 0.158231, 0.134934, 1},
            {0.158231, 0.175552, 0, 0.07}, //.00515 pot
            {0.158231, 0.175552, 0.07, 0.150267}, //.00515 pot
            {0.158231, 0.175552, 0.150267, 1},
            {0.175552, 0.190404, 0, 0.08}, //.00856 pot
            {0.175552, 0.190404, 0.08, 0.15186}, //.00856 pot
            {0.175552, 0.190404, 0.15186, 1},
            {0.190404, 0.205643, 0, 0.08}, //.00714 pot
            {0.190404, 0.205643, 0.08, 0.158162}, //.00714 pot
            {0.190404, 0.205643, 0.158162, 1},
            {0.205643, 0.220914, 0, 0.10}, //.00701
            {0.205643, 0.220914, 0.10, 0.175}, //.00701
            {0.205643, 0.220914, 0.175, 1},
            {0.220914, 0.23725, 0, 0.10}, //.00672
            {0.220914, 0.23725, 0.10, 0.19}, //.00672
            {0.220914, 0.23725, 0.19, 1},
            {0.23725, 0.250414, 0, 0.07}, //.00893
            {0.23725, 0.250414, 0.07, 0.14}, //.00893
            {0.23725, 0.250414, 0.14, 0.191047}, //.00893
            {0.23725, 0.250414, 0.191047, 1},
            {0.250414, 0.263761, 0, 0.07}, //.01552
            {0.250414, 0.263761, 0.07, 0.15}, //.01552
            {0.250414, 0.263761, 0.15, 0.195}, //.01552
            {0.250414, 0.263761, 0.195, 1},
            {0.263761, 0.276967, 0, 0.10}, //.01267
            {0.263761, 0.276967, 0.10, 0.17}, //.01267
            {0.263761, 0.276967, 0.17, 0.2}, //.01267
            {0.263761, 0.276967, 0.2, 1},
            {0.276967, 0.29274, 0, 0.13}, //.01256
            {0.276967, 0.29274, 0.13, 0.2}, //.01256
            {0.276967, 0.29274, 0.2, 1},
            {0.29274, 0.308045, 0, 0.13}, //.01371
            {0.29274, 0.308045, 0.13, 0.199747}, //.01371
            {0.29274, 0.308045, 0.199747, 1},
            {0.308045, 0.33514, 0, 0.08}, //.01700
            {0.308045, 0.33514, 0.08, 0.14}, //.01700
            {0.308045, 0.33514, 0.14, 0.185}, //.01700
            {0.308045, 0.33514, 0.185, 0.22}, //.01700
            {0.308045, 0.33514, 0.22, 1},
            {0.33514, 0.348079, 0, 0.08},
            {0.33514, 0.348079, 0.08, 0.14},
            {0.33514, 0.348079, 0.14, 0.185},
            {0.33514, 0.348079, 0.185, 0.22},
            {0.33514, 0.348079, 0.22, 1},
            {0.348079, 0.361749, 0.22, 1},
            {0.348079, 0.361749, 0, 0.08},
            {0.348079, 0.361749, 0.08, 0.14},
            {0.348079, 0.361749, 0.14, 0.2},
            {0.348079, 0.361749, 0.2, 1},
            {0.361749, 0.37798, 0.22, 1},
            {0.361749, 0.37798, 0, 0.08},
            {0.361749, 0.37798, 0.08, 0.14},
            {0.361749, 0.37798, 0.14, 0.2},
            {0.361749, 0.37798, 0.2, 1},//?
            {0.37798, 0.394672, 0.22, 1},
            {0.37798, 0.394672, 0, 0.08},
            {0.37798, 0.394672, 0.08, 0.14},
            {0.37798, 0.394672, 0.14, 0.2},
            {0.37798, 0.394672, 0.2, 1},
            {0.394672, 0.408401, 0.22, 1},
            {0.394672, 0.408401, 0, 0.08},
            {0.394672, 0.408401, 0.08, 0.15},
            {0.394672, 0.408401, 0.15, 0.20},
            {0.394672, 0.408401, 0.20, 1},
            {0.408401, 0.420667, 0, 0.201206}, //.0057
            {0.408401, 0.420667, 0.201206, 1},
            {0.420667, 0.433178, 0, 0.10}, //.006
            {0.420667, 0.433178, 0.10, 0.184245}, //.006
            {0.420667, 0.433178, 0.184245, 1},
            {0.433178, 0.446861, 0, 0.11}, //.008
            {0.433178, 0.446861, 0.11, 0.178305}, //.008
            {0.433178, 0.446861, 0.178305, 1},//
            {0.446861, 0.464581, 0, 0.208184}, //.003
            {0.446861, 0.464581, 0.208184, 1},
            {0.464581, 0.483303, 0, 0.235856}, //.003
            {0.464581, 0.483303, 0.235856, 1},
            {0.483303, 0.497797, 0, 0.179297}, //.001
            {0.483303, 0.497797, 0.179297, 1},
            {0.497797, 0.510063, 0, 0.166236}, //.001
            {0.497797, 0.510063, 0.166236, 1},
            {0.510063, 0.522367, 0, 0.169625},
            {0.510063, 0.522367, 0.169625, 1},
            {0.522367, 0.535865, 0, 0.179729},
            {0.522367, 0.535865, 0.179729, 1},
            {0.535865, 0.551387, 0, 0.180249}, //.001
            {0.535865, 0.551387, 0.180249, 1},
            {0.551387, 0.568491, 0, 0.175398}, //.001
            {0.551387, 0.568491, 0.175398, 1},
            {0.568491, 0.587829, 0, 0.167574}, //.001
            {0.568491, 0.587829, 0.167574, 1},
            {0.587829, 0.606547, 0, 0.180065}, //.002
            {0.587829, 0.606547, 0.180065, 1},
            {0.606547, 0.622899, 0, 0.180651}, //.001
            {0.606547, 0.622899, 0.180651, 1},
            {0.622899, 0.640206, 0, 0.148866},
            {0.622899, 0.640206, 0.148866, 1},
            {0.640206, 0.659114, 0, 0.138949},
            {0.640206, 0.659114, 0.138949, 1},
            {0.659114, 0.681613, 0, 0.156346},
            {0.659114, 0.681613, 0.156346, 1},
            {0.681613, 0.703517, 0, 0.162827},
            {0.681613, 0.703517, 0.162827, 1},
            {0.703517, 0.724198, 0, 0.157404},
            {0.703517, 0.724198, 0.157404, 1},
            {0.724198, 0.743526, 0, 0.132107},
            {0.724198, 0.743526, 0.132107, 1},
            {0.743526, 0.763078, 0, 0.122422},
            {0.743526, 0.763078, 0.122422, 1},
            {0.763078, 0.784933, 0, 0.145368}, //.001
            {0.763078, 0.784933, 0.145368, 1},
            {0.784933, 0.807641, 0, 0.158459},
            {0.784933, 0.807641, 0.158459, 1},
            {0.807641, 0.827716, 0, 0.143646}, //.001
            {0.807641, 0.827716, 0.143646, 1},
            {0.827716, 0.845784, 0, 0.147112},
            {0.827716, 0.845784, 0.147112, 1},
            {0.845784, 0.866982, 0, 0.165308},
            {0.845784, 0.866982, 0.165308, 1},
            {0.866982, 0.889622, 0, 0.15458}, //.002
            {0.866982, 0.889622, 0.15458, 1},
            {0.889622, 0.931107, 0, 0.157913}, //.002
            {0.889622, 0.931107, 0.157913, 1},
            {0.931107, 0.95, 0, 0.161481}, //.00083 hs
            {0.931107, 0.95, 0.161481, 1},
            {0.95, 0.98, 0, 0.0462963}, //.00145 hs .001
            {0.98, 1.00001, 0, 0.0462963},
            {0.95, 1.00001, 0.0462963, 1}};
        int i = 0;
        while (i < 132) {
            if (hs >= turnIndex[i][0] && hs < turnIndex[i][1] && pot >= turnIndex[i][2] && pot <= (turnIndex[i][3] + .0000000001)) {
                return (i);
            }
            i++;
        }
        debug("Invalid hs or pot, getTurnIndex: hs:" + hs + " pot:" + pot);
        return (-1);
    }

    /**
     * Given a hand rank (hs), returns the column from which to look up the raise/call/fold probabilities.
     */
    int getRiverIndex(double hs) {
        double[] riverIndex = {
            0.00000000, 0.02666667, 0.04000000, 0.05333333, 0.06666667, 0.08000000, 0.09333333, 0.10666667,
            0.12000000, 0.13333333, 0.14666667, 0.16000000, 0.17333333, 0.18666667, 0.20000000, 0.21333333, 0.22666667,
            0.24000000, 0.25333333, 0.26666667, 0.28000000, 0.29333333, 0.30666667, 0.32000000, 0.33333333, 0.34666667,
            0.36000000, 0.37333333, 0.38666667, 0.40000000, 0.41333333, 0.42666667, 0.44000000, 0.45333333, 0.46666667,
            0.48000000, 0.49333333, 0.50666667, 0.52000000, 0.53333333, 0.54666667, 0.56000000, 0.57333333, 0.58666667,
            0.60000000, 0.61333333, 0.62666667, 0.64000000, 0.65333333, 0.66666667, 0.68000000, 0.69333333, 0.70666667,
            0.72000000, 0.73333333, 0.74666667, 0.76000000, 0.77333333, 0.78666667, 0.80000000, 0.81333333, 0.82666667,
            0.84000000, 0.85333333, 0.86666667, 0.88000000, 0.89333333, 0.90666667, 0.92000000, 0.93333333, 0.94666667,
            0.96000000, 0.97333333, 0.98666667, .992, 1.00000001};
        int i = 0;
        while (i < 75) {
            if (hs >= riverIndex[i] && hs < riverIndex[i + 1]) {
                return i;
            }
            i++;
        }
        debug("Invalid hs , getRiverIndex: hs:" + hs);
        return (-1);
    }

    /**
     * returns the column from which to look up the raise/call/fold probabilities.
     */
    protected int getPreflopIndex(Card c1, Card c2) {
        int response;
        debug("suits equal: " + (c1.getSuit() == c2.getSuit()));
        if (c1.getSuit() == c2.getSuit()) {
            if (c1.getRank() > c2.getRank()) {
                response = c1.getRank() + 13 * c2.getRank();
            } else {
                response = c2.getRank() + 13 * c1.getRank();
            }
        } else {
            if (c1.getRank() > c2.getRank()) {
                response = c2.getRank() + 13 * c1.getRank();
            } else {
                response = c1.getRank() + 13 * c2.getRank();
            }
        }

        return (response);
    }

    public double flopWt(String boardString) {
        HandEvaluator he = new HandEvaluator();
        double[][] handWtMatrix = {
            {-0.0044917311, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {-0.01204164297, -0.0044706384, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {-0.0120408485, -0.0120373386, -0.0042061, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {-0.01203858199, -0.01203597557, -0.0120304154, 0.00810651, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {-0.01204107601, -0.01203756258, -0.0120331339, -0.0120257701, 0.00810881, 0, 0, 0, 0, 0, 0, 0, 0},
            {-0.0120415294, -0.01203756568, -0.0120329084, -0.0120248639, -0.0120133082, 0.00811071, 0, 0, 0, 0, 0, 0, 0},
            {-0.01203778972, -0.01203631787, -0.0120316625, -0.0120225991, -0.0120117198, -0.0119955197, 0.00811121, 0, 0, 0, 0, 0, 0},
            {-0.0120340496, -0.012032347, -0.0120293933, -0.0120190866, -0.0120079784, -0.0118935835, -0.003844951, 0.00811121, 0, 0, 0, 0, 0},
            {-0.0120265635, -0.012022032, -0.0120185198, -0.0120148921, -0.012002426, -0.004434038, -0.00327462, 0.02080775, 0.00811171, 0, 0, 0, 0},
            {-0.0120148926, -0.0120094551, -0.0120049201, -0.0119940241, -0.0103817244, -0.003218173, 0.019824, 0.02113291, 0.02147454, 0.00811211, 0, 0, 0},
            {-0.0119964221, -0.0119895062, -0.0119738051, -0.0041569518, -0.003138232, -0.000589030000000001, 0.01997247, 0.02128773, 0.0215729, 0.02158969, 0.00811211, 0, 0},
            {-0.0119348477, -0.0044007036, -0.0037202346, -0.003636921, -0.003496697, 0.00810559, 0.02026459, 0.02157162, 0.02161213, 0.02161695, 0.02162069, 0.00811211, 0},
            {-0.003433492, -0.002741126, 0.00588669, 0.02147659, 0.00605731, 0.02158769, 0.02161017, 0.02161795, 0.02162443, 0.02162513, 0.02162666, 0.0216274, 0.00811221}
        };
        Hand board = new Hand(boardString);
        Card c1 = new Card();
        Card c2 = new Card();
        Card tempCard = new Card();
        Hand bd = new Hand();
        double hr;
        tempCard.setCard(board.getCard(1).getRank(), 0);
        bd.addCard(tempCard);
        tempCard.setCard(board.getCard(2).getRank(), 1);
        bd.addCard(tempCard);
        tempCard.setCard(board.getCard(3).getRank(), 2);
        bd.addCard(tempCard);
        double runningTot = 0.0;
        for (int i = 0; i < 13; i++) {
            for (int j = i; j >= 0; j--) {

                c1.setCard(i, 0);
                //std::cout<<c1.toString()<<"  ";
                while (c1.getIndex() == bd.getCard(1).getIndex() || c1.getIndex() == bd.getCard(2).getIndex() || c1.getIndex() == bd.getCard(3).getIndex()) {
                    c1.setCard(i, c1.getSuit() + 1);
                    //if(c1.getSuit()>3) debug("error flopWt");
                }
                if (c1.getSuit() != 3) {
                    c2.setCard(j, 3);
                    hr = he.handRank(c1.toString(), c2.toString(), bd.toString());
                } else {
                    c2.setCard(j, 2);
                    if (c1.getRank() == c2.getRank()) {
                        hr = 0.0;
                    } else {
                        hr = he.handRank(c1.toString(), c2.toString(), bd.toString());
                    }
                }
                runningTot += hr * handWtMatrix[i][j];
            }
        }
        return runningTot;
    }

    /* /////////////////////////////////////////////////////////
    //Flop indexes
    //0-4 no suit matches , 5-9 two match , 10 all match
    //////////////////////////////////////////////////////////*/
    public int getFlopBoardIndex(String sBoard) {
        Hand board = new Hand(sBoard);
        if (board.size() != 3) {
            debug("getFlopBoardIndex: Board size = " + board.size());
        }


        double flopWt = flopWt(sBoard);
        debug("flop wt: " + flopWt);
        double[] perc = {-10000.0, 0.08579600, 0.12326560, 0.15785900, 0.19346760, 10000.0};
        int i = 0;
        while (i < 5) {
            if (flopWt >= perc[i] && flopWt < perc[i + 1]) {
                break;
            }
            i++;
        }
        if (i > 4) {
            debug("Invalid flop weight:" + flopWt);
        }
        //If all same suit//
        if ((board.getCard(1).getSuit() == board.getCard(2).getSuit()) && (board.getCard(2).getSuit() == board.getCard(3).getSuit())) {
            return 10;
        }
        //If all different suits//
        if ((board.getCard(1).getSuit() != board.getCard(2).getSuit()) && (board.getCard(1).getSuit() != board.getCard(3).getSuit()) && (board.getCard(2).getSuit() != board.getCard(3).getSuit())) {
            return i;
        }
        //If two match
        return i + 5;
    }

    /**
     * Gets the action probability table from a file. Each column
     * represents a raise/call/fold probability triple
     * thus, actionMatrix[2][i] is the probability of a raise with a hand index of i
     * actionMatrix[1][i] is the probability of a call with a hand index of i
     * actionMatrix[0][i] is the probability of a fold with a hand index of i
     * round=0-->preflop , 1-->flop , 2-->turn , 3-->river
     */
    protected double[][] getActionTable(int round, String fileName) {




        int size = 1;
        double[][] actionMatrix;
        Scanner inMatrix = null;

        // Determine the double array second dimension size by using the round number
        if (round == 0) {
            size = 169;
        } else if (round == 1) {
            size = 177;
        } else if (round == 2) {
            size = 131;
        } else if (round == 3) {
            size = 75;
        }
        actionMatrix = new double[3][size];
        //debug("size: "+size);
        debug("table : " + fileName);
        int k = 1;
        ZipEntry ze;
        while (k < 8) {
            try {
                String path = _stategyPath + "/" + k + ".zip";
                debug("trying " + path);
                JarFile jf = new JarFile(path);
                Enumeration<JarEntry> entries = jf.entries();
                //debug("trying jarENTRy");
                JarEntry je;// = jf.getJarEntry("StrategyFiles2/"+fileName);
                do {
                    je = entries.nextElement();
                    if (!entries.hasMoreElements()) {
                        break;
                    }


                    //debug(je.getName());
                    //if(!entries.hasMoreElements()) debug("reached end of zip file before table was found");
                } while (!je.getName().equals(k + "/" + fileName));

                if (je.getName().equals(k + "/" + fileName)) {
                    debug("table found: " + je.getName());
                    ze = je;
                    InputStream is = jf.getInputStream(ze);
                    BufferedReader br = new BufferedReader(new InputStreamReader(is));
                    inMatrix = new Scanner(br);
                    inMatrix.useLocale(Locale.US);
                    break;
                }
                debug("reached end of zip file before table was found");
            } catch (Exception e) {
                debug("error with file");
                e.printStackTrace();
                System.exit(1);
            }
            k++;

        }




        for (int j = 2; j > -1; --j) {
            for (int i = 0; i < size; ++i) {

                actionMatrix[j][i] = inMatrix.nextDouble();
                debugb(actionMatrix[j][i] + " ");
            }
            debug(" ");
        }

        inMatrix.close();

        return (actionMatrix);
    }

    //returns within round action node.
    public int get_action_node(GameInfo gi) {
        int numRaises = gi.getNumRaises();
        boolean isButton = (gi.getCurrentPlayerSeat() == gi.getButtonSeat());
        //debug("numRaises : " + numRaises);
        if (gi.isPreFlop()) {
            numRaises--;
            //player 1
            if (!isButton) {
                if (numRaises == 0) {
                    return 10;
                }
                if (numRaises == 1) {
                    return 4;
                }
                if (numRaises == 2) {
                    return 8;
                }
                if (numRaises == 3) {
                    return 2;
                }
                if (numRaises == 4) {
                    return 6;
                }
            } //player 2
            else {
                if (numRaises == 0) {
                    return 5;
                }
                if (numRaises == 1) {
                    return 9;
                }
                if (numRaises == 2) {
                    return 3;
                }
                if (numRaises == 3) {
                    return 7;
                }
                if (numRaises == 4) {
                    return 1;
                }
            }
        } else {
            //player 2
            if (isButton) {
                if (numRaises == 0) {
                    return 10;
                }
                if (numRaises == 1) {
                    return 4;
                }
                if (numRaises == 2) {
                    return 8;
                }
                if (numRaises == 3) {
                    return 2;
                }
                if (numRaises == 4) {
                    return 6;
                }
            } //player 1
            else {
                if (numRaises == 0) {
                    return 5;
                }
                if (numRaises == 1) {
                    return 9;
                }
                if (numRaises == 2) {
                    return 3;
                }
                if (numRaises == 3) {
                    return 7;
                }
                if (numRaises == 4) {
                    return 1;
                }
            }
        }
        return -1;
    }

    /**
     * @return true if debug mode is on.
     */
    public boolean getDebug() {
        return _isDebug;
    }

    public void setDebug(boolean value)
    {
        _isDebug = value;
    }

    /**
     * print a debug statement.
     */
    public void debug(String str) {
        if (getDebug()) {
            System.out.println(str);
            if(_debugLog != null)
                _debugLog.println(str);
        }
    }

    public void debugb(String str) {
        if (getDebug()) {
            System.out.print(str);
            if(_debugLog != null)
                _debugLog.print(str);
        }
    }

    public void testActionTableParsing() {
        double[][] dblArray = null;

        // round 3
        dblArray = getActionTable(3, "p1_strategy000000_1.txt");
        if (dblArray != null) {
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 50; j++) {
                    System.out.println("[" + i + "]::[" + j + "]::[" + dblArray[i][j] + "]");
                }
            }
        } else {
            System.out.println("An error has occured.");
        }

        // round 2
        dblArray = getActionTable(2, "p1_strategy0000_1.txt");
        if (dblArray != null) {
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 100; j++) {
                    System.out.println("[" + i + "]::[" + j + "]::[" + dblArray[i][j] + "]");
                }
            }
        } else {
            System.out.println("An error has occured.");
        }

        // round 1
        dblArray = getActionTable(1, "p1_strategy00_1.txt");
        if (dblArray != null) {
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 150; j++) {
                    System.out.println("[" + i + "]::[" + j + "]::[" + dblArray[i][j] + "]");
                }
            }
        } else {
            System.out.println("An error has occured.");
        }

        return;
    }

    public static void main(String[] args) throws Exception {
        FellOmen_2_Impl ibot = new FellOmen_2_Impl();

        // Test action table parsing
        ibot.testActionTableParsing();

        return;
    }
}
