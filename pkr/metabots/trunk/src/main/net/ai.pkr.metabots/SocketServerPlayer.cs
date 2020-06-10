/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading;
using ai.lib.utils;
using ai.pkr.metagame;
using ai.pkr.metabots.remote;
using ProtoBuf;
using GameDefinition=ai.pkr.metagame.GameDefinition;


namespace ai.pkr.metabots
{
    /// <summary>
    /// A server-side player to connect to remote client via TCP sockets.
    /// </summary>
    public class SocketServerPlayer:  IPlayer
    {

        #region Public interface

        public void Disconnect()
        {
            _tc.Close();
        }

        public bool IsConnected
        {
            get { return _tc != null; }
        }

        public PlayerInfo PlayerInfo
        {
            get;
            set;
        }

        #endregion


        #region IPlayer Members

        public void OnCreate(string name, ai.lib.utils.Props creationParameters)
        {
            // Not supported for remote players.
            throw new NotImplementedException();
        }

        public PlayerInfo OnServerConnect()
        {
            WriteCommand((int)remote.IPlayer_FunctionIDs.OnServerConnect);
            
            remote.Header responseHeader;
            MemoryStream responseData;
            ReadMessage(out responseHeader, out responseData);
            remote.PlayerInfo rm = Serializer.Deserialize<remote.PlayerInfo>(responseData);
            return rm.FromRemote();
        }

        public void OnServerDisconnect(string reason)
        {
            remote.IPlayer_OnServerDisconnect command = new remote.IPlayer_OnServerDisconnect();
            command.Reason = reason;
            WriteCommand((int) remote.IPlayer_FunctionIDs.OnServerDisconnect, command);
            // No need to wait for response, end of connection.
        }


        public void OnSessionBegin(string sessionName, GameDefinition gameDef, ai.lib.utils.Props sessionParameters)
        {
            remote.IPlayer_OnSessionBegin command = new remote.IPlayer_OnSessionBegin();
            command.sessionName = sessionName;
            command.gameDef = (new remote.GameDefinition()).ToRemote(gameDef);
            command.sessionParameters =  (new remote.Props()).ToRemote(sessionParameters);
            WriteCommand((int)remote.IPlayer_FunctionIDs.OnSessionBegin, command);

            // No response required.
        }

        public void OnSessionEvent(ai.lib.utils.Props parameters)
        {
            // Todo: implement.
        }

        public void OnSessionEnd()
        {
            // No data.
            WriteCommand((int)remote.IPlayer_FunctionIDs.OnSessionEnd);
            // No response required.
        }

        public void OnGameBegin(string gameString)
        {
            WriteCommand((int)remote.IPlayer_FunctionIDs.OnGameBegin, gameString);
            // No response required.
        }

        public void OnGameUpdate(string gameString)
        {
            WriteCommand((int)remote.IPlayer_FunctionIDs.OnGameUpdate, gameString);
            // No response required.
        }

        public metagame.PokerAction OnActionRequired(string gameString)
        {
            WriteCommand((int)remote.IPlayer_FunctionIDs.OnActionRequired, gameString);

            remote.Header responseHeader;
            MemoryStream responseData;
            ReadMessage(out responseHeader, out responseData);
            remote.PokerAction rm = Serializer.Deserialize<remote.PokerAction>(responseData);
            return rm.FromRemote();
        }

        public void OnGameEnd(string gameString)
        {
            WriteCommand((int)remote.IPlayer_FunctionIDs.OnGameEnd, gameString);
            // No response required.        
        }

        #endregion

        #region Implementation

        internal SocketServerPlayer(TcpClient tc)
        {
            _tc = tc;
        }

        private void ReadMessage(out remote.Header header, out MemoryStream data)
        {
            MemoryStream ms = ReadFixedSizeBlock(remote.Header.SIZE);
            header = remote.Header.ReadFrom(ms);
            if (header.DataLength > 0)
            {
                data = ReadFixedSizeBlock(header.DataLength);
            }
            else
            {
                data = new MemoryStream();
            }
        }

        private MemoryStream ReadFixedSizeBlock(int size)
        {
            byte[] buffer = new byte[size];
            int totalRead = 0;
            for (; ; )
            {
                int read = _tc.GetStream().Read(buffer, totalRead, buffer.Length - totalRead);
                totalRead += read;
                if (totalRead >= buffer.Length)
                    break;
            }
            return new MemoryStream(buffer);
        }

        private void WriteCommand<T>(int functionID, T command)
        {
            MemoryStream ms = new MemoryStream();
            if (command != null)
            {
                Serializer.Serialize(ms, command);
            }
            Header commandHeader = new Header(functionID);
            commandHeader.DataLength = (int)ms.Length;
            commandHeader.WriteTo(_tc.GetStream());
            _tc.GetStream().Write(ms.ToArray(), 0, commandHeader.DataLength);
            _tc.GetStream().Flush();
        }

        private void WriteCommand(int functionID)
        {
            Header commandHeader = new Header(functionID);
            commandHeader.DataLength = 0;
            commandHeader.WriteTo(_tc.GetStream());
            _tc.GetStream().Flush();
        }

        #region Data members

        private TcpClient _tc;

        #endregion
        
        #endregion

    }
}
