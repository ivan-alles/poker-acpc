/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;

namespace ai.pkr.metabots.remote
{
    /// <summary>
    /// Extension method to convert Remote types to our types and vice versa. 
    /// 
    /// Remote messages that do not correspont to local types are not converted
    /// by this class, this must be done in the corresponding code.
    /// 
    /// Extension methods always use remote objects as this, because we
    /// do not want to pollute the local objects with this methods.
    /// 
    /// ToRemote() functions require an existing Remote object (to be able to use
    /// extension method) and convert a local object to it.
    /// 
    /// FromRemote() functions create a new local object and convert a remote 
    /// object to it.
    /// </summary>
    public static class ProtocolExtensionMethods
    {
        private static Dictionary<metagame.Ak, remote.Ak> _AkToRemote;
        private static Dictionary<remote.Ak, metagame.Ak> _AkFromRemote;

        static ProtocolExtensionMethods()
        {
            _AkToRemote = new Dictionary<ai.pkr.metagame.Ak, remote.Ak>();
            _AkFromRemote = new Dictionary<remote.Ak, ai.pkr.metagame.Ak>();

            _AkToRemote[metagame.Ak.b] = remote.Ak.b;
            _AkToRemote[metagame.Ak.d] = remote.Ak.d;
            _AkToRemote[metagame.Ak.r] = remote.Ak.r;
            _AkToRemote[metagame.Ak.c] = remote.Ak.c;
            _AkToRemote[metagame.Ak.f] = remote.Ak.f;

            _AkFromRemote[remote.Ak.b] = metagame.Ak.b;
            _AkFromRemote[remote.Ak.d] = metagame.Ak.d;
            _AkFromRemote[remote.Ak.r] = metagame.Ak.r;
            _AkFromRemote[remote.Ak.c] = metagame.Ak.c;
            _AkFromRemote[remote.Ak.f] = metagame.Ak.f;
        }

        #region ai.lib.utills

        public static remote.Props ToRemote(this remote.Props rm, ai.lib.utils.Props o)
        {
            // Pass unresolved values.
            rm.Names.AddRange(o.Names);
            foreach(string name in o.Names)
            {
                rm.Values.Add(o.GetRaw(name));
            }
            return rm;
        }

        public static ai.lib.utils.Props FromRemote(this remote.Props rm)
        {
            ai.lib.utils.Props o = new ai.lib.utils.Props();
            for (int i = 0; i < rm.Names.Count; ++i)
            {
                o.Set(rm.Names[i], rm.Values[i]);
            }
            return o;
        }
        
        #endregion


        #region ai.pkr.metagame

        public static remote.PokerAction ToRemote(this remote.PokerAction rm, metagame.PokerAction o)
        {
            rm.Kind = _AkToRemote[o.Kind];
            rm.Position = o.Position;
            rm.Amount = o.Amount;
            rm.Cards = o.Cards;
            return rm;
        }

        public static metagame.PokerAction FromRemote(this remote.PokerAction rm)
        {
            metagame.PokerAction o = 
                new metagame.PokerAction(_AkFromRemote[rm.Kind], rm.Position, rm.Amount, rm.Cards);
            return o;
        }

        public static remote.DeckDescriptor ToRemote(this remote.DeckDescriptor rm, metagame.DeckDescriptor o)
        {
            if (o == null)
                return rm;
            rm.Name = o.Name;
            rm.CardNames.AddRange(o.CardNames);
            UInt64[] bitsArray = new ulong[o.CardSets.Length];
            for (int i = 0; i < bitsArray.Length; ++i)
            {
                bitsArray[i] = o.CardSets[i].bits;
            }
            rm.CardSets.AddRange(bitsArray);
            return rm;
        }

        public static metagame.DeckDescriptor FromRemote(this remote.DeckDescriptor rm)
        {
            CardSet[] cardSets = new CardSet[rm.CardSets.Count];
            for (int i = 0; i < cardSets.Length; ++i)
            {
                cardSets[i].bits = rm.CardSets[i];
            }
            metagame.DeckDescriptor o = new metagame.DeckDescriptor(rm.Name, rm.CardNames.ToArray(), cardSets);
            return o;
        }

        public static remote.GameDefinition ToRemote(this remote.GameDefinition rm, metagame.GameDefinition o)
        {
            rm.Name = o.Name;
            rm.RoundsCount = o.RoundsCount;
            rm.MinPlayers = o.MinPlayers;
            rm.MaxPlayers = o.MaxPlayers;
            rm.BetStructure.AddRange(o.BetStructure);
            rm.BlindStructure.AddRange(o.BlindStructure);
            rm.PrivateCardsCount.AddRange(o.PrivateCardsCount);
            rm.PublicCardsCount.AddRange(o.PublicCardsCount);
            rm.SharedCardsCount.AddRange(o.SharedCardsCount);
            rm.BetsCountLimits.AddRange(o.BetsCountLimits);
            rm.FirstActor.AddRange(o.FirstActor);
            rm.FirstActorHeadsUp.AddRange(o.FirstActorHeadsUp);
            rm.LimitKind = (remote.LimitKind)o.LimitKind;
            rm.DeckDescriptor = (new remote.DeckDescriptor()).ToRemote(o.DeckDescr);
            return rm;
        }

        public static metagame.GameDefinition FromRemote(this remote.GameDefinition rm)
        {
            metagame.GameDefinition o = new metagame.GameDefinition();
            o.Name = rm.Name;
            o.RoundsCount = rm.RoundsCount;
            o.MinPlayers = rm.MinPlayers;
            o.MaxPlayers = rm.MaxPlayers;
            o.BetStructure = rm.BetStructure.ToArray();
            o.BlindStructure = rm.BlindStructure.ToArray();
            o.PrivateCardsCount = rm.PrivateCardsCount.ToArray();
            o.PublicCardsCount = rm.PublicCardsCount.ToArray();
            o.SharedCardsCount = rm.SharedCardsCount.ToArray();
            o.BetsCountLimits = rm.BetsCountLimits.ToArray();
            o.FirstActor = rm.FirstActor.ToArray();
            o.FirstActorHeadsUp = rm.FirstActorHeadsUp.ToArray();
            o.LimitKind = (metagame.LimitKind)rm.LimitKind;
            o.DeckDescr = rm.DeckDescriptor.FromRemote();
            return o;
        }

        #endregion

        #region ai.pkr.metabots


        public static remote.PlayerInfo ToRemote(this remote.PlayerInfo rm, metabots.PlayerInfo o)
        {
            rm.Name = o.Name;
            return rm;
        }

        public static metabots.PlayerInfo FromRemote(this remote.PlayerInfo rm)
        {
            metabots.PlayerInfo o = new metabots.PlayerInfo();
            o.Name = rm.Name;
            return o;
        }

        #endregion
    }
}
