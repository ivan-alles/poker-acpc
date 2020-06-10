/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package ai.pkr.jmetabots;
import ai.pkr.metabots.remote.*;
import com.google.protobuf.MessageLite;
import java.net.*;
import java.io.*;
/**
 *
 * @author alles
 */
public abstract class SocketClient {
    
    public SocketClient() 
    {
    }

    public abstract Protocol.PlayerInfo OnServerConnect() throws Exception;
    public abstract void OnServerDisconnect(String reason) throws Exception;

    public abstract void OnSessionBegin(String sessionName, Protocol.GameDefinition gameDef, Protocol.Props sessionParameters) throws Exception;
    public abstract void OnSessionEnd() throws Exception;

    public abstract void OnGameBegin(String gameString) throws Exception;
    public abstract void OnGameUpdate(String gameString) throws Exception;
    public abstract Protocol.PokerAction OnActionRequired(String gameString) throws Exception;
    public abstract void OnGameEnd(String gameString) throws Exception;

    /**
     * Main message loop. Connects to server and does message exchange until server disconnects.
     * @throws UnknownHostException
     * @throws IOException
     * @throws InterruptedException
     */
    public void Run(int port) throws UnknownHostException, IOException, InterruptedException, Exception
    {
        InetAddress addr = InetAddress.getByName("localhost");
        _s = new Socket(addr, port);

        while(true)
        {
            Command command = ReceiveCommand();
            if (command.header.FunctionId == Protocol.IPlayer_FunctionIDs.OnServerConnect.getNumber()) {
                Protocol.PlayerInfo response = OnServerConnect();
                SendResponse(response);
            }
            else if(command.header.FunctionId == Protocol.IPlayer_FunctionIDs.OnServerDisconnect.getNumber()) {
                Protocol.IPlayer_OnServerDisconnect data = Protocol.IPlayer_OnServerDisconnect.parseFrom(command.data);
                OnServerDisconnect(data.getReason());
                break; // End of server connection.
            }
            else if(command.header.FunctionId == Protocol.IPlayer_FunctionIDs.OnSessionBegin.getNumber()) {
                Protocol.IPlayer_OnSessionBegin data = Protocol.IPlayer_OnSessionBegin.parseFrom(command.data);
                OnSessionBegin(data.getSessionName(), data.getGameDef(), data.getSessionParameters());
            }
            else if(command.header.FunctionId == Protocol.IPlayer_FunctionIDs.OnSessionEnd.getNumber()) {
                OnSessionEnd();
            }
            else if(command.header.FunctionId == Protocol.IPlayer_FunctionIDs.OnGameBegin.getNumber()) {
                Protocol.IPlayer_GameString data = Protocol.IPlayer_GameString.parseFrom(command.data);
                OnGameBegin(data.getGameString());
            }
            else if(command.header.FunctionId == Protocol.IPlayer_FunctionIDs.OnGameUpdate.getNumber()) {
                Protocol.IPlayer_GameString data = Protocol.IPlayer_GameString.parseFrom(command.data);
                OnGameUpdate(data.getGameString());
            }
            else if(command.header.FunctionId == Protocol.IPlayer_FunctionIDs.OnActionRequired.getNumber()) {
                Protocol.IPlayer_GameString data = Protocol.IPlayer_GameString.parseFrom(command.data);
                Protocol.PokerAction response = OnActionRequired(data.getGameString());
                SendResponse(response);
            }
            else if(command.header.FunctionId == Protocol.IPlayer_FunctionIDs.OnGameEnd.getNumber()) {
                Protocol.IPlayer_GameString data = Protocol.IPlayer_GameString.parseFrom(command.data);
                OnGameEnd(data.getGameString());
            }
            else
            {
                throw new UnsupportedOperationException("Unknown function ID received");
            }
        }
    }

    class Command
    {
        Header header;
        byte[] data;
    }

    private void SendResponse(MessageLite o) throws IOException {
        //Protocol.PlayerInfo.newBuilder().setName("Remote Bott").build();
        Header header = new Header();
        header.DataLength = o.getSerializedSize();
        header.writeTo(_s.getOutputStream());
        o.writeTo(_s.getOutputStream());
        _s.getOutputStream().flush();
    }

    private Command ReceiveCommand() throws IOException, InterruptedException {
        Command command = new Command();
        byte [] buffer = new byte[Header.SIZE];
        ReadFixedSizeBlock(buffer);
        ByteArrayInputStream memStream = new ByteArrayInputStream(buffer);
        command.header = Header.readFrom(memStream);
        if(command.header.DataLength > 0)
        {
            command.data = new byte[command.header.DataLength];
            ReadFixedSizeBlock(command.data);
        }
        return command;
    }
    
     private void ReadFixedSizeBlock(byte[] buffer) throws IOException, InterruptedException {
        int totalRead = 0;
        for (;;) {
            int read = _s.getInputStream().read(buffer, totalRead, buffer.length - totalRead);
            totalRead += read;
            if (totalRead >= buffer.length) {
                break;
            }
        }
    }

    Socket _s;
}
