
import ai.pkr.metabots.remote.Protocol.*;
import ai.pkr.meerkat.MeerkatSocketClient;

/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

/**
 *
 * @author alles
 */
public class FellOmen2SocketClient extends MeerkatSocketClient {
    @Override
    public void OnSessionBegin(String sessionName, GameDefinition gameDef, Props sessionParameters) {
        super.OnSessionBegin(sessionName, gameDef, sessionParameters);
        for(int i = 0; i < sessionParameters.getNamesCount(); ++i)
        {
            if(sessionParameters.getNames(i).equals("RngSeed"));
            long rngSeed = Long.parseLong(sessionParameters.getValues(i));
            FellOmen_2 fo2 = (FellOmen_2)_player;
            fo2.getIbot().setRngSeed(rngSeed);
        }
    }
}
