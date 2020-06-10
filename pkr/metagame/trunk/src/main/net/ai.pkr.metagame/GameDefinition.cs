/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;
using System.IO;
using System.Xml;
using ai.lib.utils;
using ai.lib.algorithms;

namespace ai.pkr.metagame
{
    /// <summary>
    /// Kind of bet size limiting.
    /// </summary>
    public enum LimitKind
    {
        FixedLimit,
        PotLimit,
        NoLimit
    }

    /// <summary>
    /// Describes parameters of the meta-poker game. Is should be possible to describe (almost) 
    /// all possible poker games. 
    /// <para>
    ///  We use the following conventions:
    /// </para>
    /// <para>
    /// - The game is played with a deck of cards described by class DeckDefinition.
    /// </para><para>
    /// - A game consists of rounds.
    /// </para><para>
    /// - Round zero starts after posting blinds and antes.
    /// - Each round > 0 starts when all the players have made their "stopping" moves (calls or folds). 
    ///   The first actor in the round is dealer, it deals some combination of private, public or shared cards.
    ///   A sequence of deal actions must not be interrupted by a player action. By convention the cards are dealt 
    ///   in the following order: d, p, s. Private and public (d and p) cards are dealt from position 0.
    ///   After dealing player actions take place.
    /// </para><para>
    /// - Before the game there are forced bets: blinds and antes. 
    /// </para><para>
    /// - Forced bets before the game (blinds/antes) are counted as 1 bet if one of elements of 
    ///   BlindStructure[] is greater than the other (in other words, there is a big blind), 
    ///   otherwise they are counted as 0 bets. This is done in BlindsBetsCount()
    ///   Take it into accont when setting BetsCountLimits.
    /// </para><para>
    /// - There is a minimal bet for each round.
    /// </para><para>
    /// - Minimal bet for round 0 (called B0) is by convention set to 1 and is used as the unit
    ///   for other bets (including forced bets). All games and strategies parameters in this 
    ///   framework are measured in this unit.
    /// </para><para>
    /// - For things that are difficult to describe in a configuration (e.g. hand evaluation algorithm)
    ///   there is an extension point IGameRules. A game definition contains the type information
    ///   of the implementation of IGameRules. It will be instanciated and used when necessary.
    /// </para>
    /// </summary>
    [Serializable]
    [XmlRoot("GameDefinition", Namespace = "ai.pkr.metagame.GameDefinition.xsd")]
    public class GameDefinition
    {

        #region Constructors

        public GameDefinition()
        {
            Name = "";
            BetStructure = new double[0];
            BlindStructure = new double[0];
            PrivateCardsCount = new int[0];
            PublicCardsCount = new int[0];
            SharedCardsCount = new int[0];
            BetsCountLimits = new int[0];
            FirstActor = new int[0];
            FirstActorHeadsUp = new int[0];
            DeckDescrFile = new PropString();
            GameRulesType = new PropString();
            GameRulesAssemblyFile = new PropString();
        }

        /// <summary>
        /// Copy-constructor, create copies of value-type properties, strings and arrays of value-types.
        /// Objects like DeckDescriptor are copied by reference.
        /// </summary>
        public GameDefinition(GameDefinition other)
        {
            Name = other.Name;
            RoundsCount = other.RoundsCount;
            MinPlayers = other.MinPlayers;
            MaxPlayers = other.MaxPlayers;
            BetStructure = other.BetStructure.ShallowCopy();
            BlindStructure = other.BlindStructure.ShallowCopy();
            PrivateCardsCount = other.PrivateCardsCount.ShallowCopy();
            PublicCardsCount = other.PublicCardsCount.ShallowCopy();
            SharedCardsCount = other.SharedCardsCount.ShallowCopy();
            BetsCountLimits = other.BetsCountLimits.ShallowCopy();
            FirstActor = other.FirstActor.ShallowCopy();
            FirstActorHeadsUp = other.FirstActorHeadsUp.ShallowCopy();
            LimitKind = other.LimitKind;
            DeckDescrFile = other.DeckDescrFile;
            DeckDescr = other.DeckDescr;
            GameRulesAssemblyFile = other.GameRulesAssemblyFile;
            GameRulesType = other.GameRulesType;
            GameRulesCreationParams = other.GameRulesCreationParams;
            GameRules = other.GameRules;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Game name, e.g. "Holdem FL". It must be globally unique 
        /// and constant over time (bound to game rules) to allow bots 
        /// identify if they can play this game or not.
        /// </summary>
        public string Name
        {
            set;
            get;
        }

        /// <summary>
        /// Number of betting rounds.
        /// </summary>
        public int RoundsCount
        {
            set;
            get;
        }

        /// <summary>
        /// For the game server - minimal number of players.
        /// For strategies by convention defines the number of players in the game, unless
        /// another number in range [MinPlayers, MaxPlayers] is specified.
        /// </summary>
        public int MinPlayers
        {
            set;
            get;
        }

        /// <summary>
        /// For the game server - maximal number of players. 
        /// </summary>
        public int MaxPlayers
        {
            set;
            get;
        }

        /// <summary>
        /// For limit games, bet size for each round (amount that can be added to current bet).
        /// Example: [1, 1, 2, 2]
        /// For no-limit games, minimal initial bet size for each round.
        /// Example: [1, 1, 1, 1]
        /// 
        /// BetStructure[0] (called B0) must be 1.0. It is used as the unit for other bets/blinds.
        /// Other values must be fractions of B0.
        /// </summary>
        [XmlArrayItem("i")]
        public double[] BetStructure
        {
            set
            {
                _betStructure = value;
                if(_betStructure != null && _betStructure.Length > 0 && _betStructure[0] != 1.0)
                {
                    throw new ApplicationException(String.Format("B0 must be 1.0 but is {0}", _betStructure[0]));
                }
            }
            get
            {
                return _betStructure;
            }
        }

            /// <summary>
        /// Blind size for each player, starting from position 0 (small blind), measured in B0.
        /// Examples for 4 players:
        /// HE FL 10/20:        [0.5, 1, 0, 0]
        /// HE NL 5/10, Ante 1: [0.6, 1.1, 0.1, 0.1]
        /// 7 Card Stud 5/10 Ante 1: [0.1, 0.1, 0.1, 0.1]
        /// </summary>
        [XmlArrayItem("i")]
        public double[] BlindStructure
        {
            set;
            get;
        }

        /// <summary>
        /// Number of private player cards dealt for each round.
        /// </summary>
        [XmlArrayItem("i")]
        public int[] PrivateCardsCount
        {
            set;
            get;
        }

        /// <summary>
        /// Number of public player cards dealt for each round.
        /// </summary>
        [XmlArrayItem("i")]
        public int[] PublicCardsCount
        {
            set;
            get;
        }

        /// <summary>
        /// Number of shared cards dealt for each round.
        /// </summary>
        [XmlArrayItem("i")]
        public int[] SharedCardsCount
        {
            set;
            get;
        }

        /// <summary>
        /// Limit of number of bets (raises) for each round, not including blinds.
        /// </summary>
        [XmlArrayItem("i")]
        public int[] BetsCountLimits
        {
            set;
            get;
        }

        /// <summary>
        /// An array containing index of player which acts first 
        /// in the betting round, starting from SB.
        /// (Usually looks like [2,0,0,0])
        /// </summary>
        [XmlArrayItem("i")]
        public int[] FirstActor
        {
            set;
            get;
        }

        /// <summary>
        /// An array containing index of player which acts first in 2-player game.
        /// (Usually looks like [0,1,1,1])
        /// </summary>
        /// <seealso cref="FirstActor"/>
        [XmlArrayItem("i")]
        public int[] FirstActorHeadsUp
        {
            set;
            get;
        }

        /// <summary>
        /// Kind of bet size limiting.
        /// </summary>
        public LimitKind LimitKind
        {
            set;
            get;
        }

        #endregion

        #region Methods
        public int GetFirstActor(int round, int playersCount)
        {
            return playersCount == 2 ? FirstActorHeadsUp[round] : FirstActor[round];
        }

        /// <summary>
        /// Returns 1 if there are non-zero blinds, otherwize 0.
        /// Equal blinds/antes are also counted as a bet (called by all), this is the most generic approach.
        /// This should be taken into account in setting bet limit for the round 0 in a game definition.
        /// </summary>
        /// <returns></returns>
        public int GetBlindsBetsCount()
        {
            if (BlindStructure.Length == 0)
                return 0;
            return BlindStructure.Max() > 0 ? 1 : 0;
        }

        /// <summary>
        /// Returns an array containing hand sizes for each round.
        /// </summary>
        /// <returns></returns>
        public int[] GetHandSizes()
        {
            int[] handSizes = new int[RoundsCount];
            int size = 0;
            for (int r = 0; r < RoundsCount; ++r)
            {
                size += PrivateCardsCount[r] + PublicCardsCount[r] + SharedCardsCount[r];
                handSizes[r] = size;
            }
            return handSizes;
        }
        #endregion

        #region Deck

        public PropString DeckDescrFile
        {
            set;
            get;
        }

        [XmlIgnore]
        public DeckDescriptor DeckDescr
        {
            set;
            get;
        }


        #endregion

        #region IGameRules

        /// <summary>
        /// Optional full path to the assembly file where IGameRules type is located.
        /// </summary>
        public PropString GameRulesAssemblyFile
        {
            set;
            get;
        }

        /// <summary>
        /// Type of the IGameRules implementation. If GameRulesAssemblyFile is not specfied, must
        /// contain assembly name, for example: 
        /// "ai.pkr.metagame.nunit.GameRulesHelper, ai.pkr.metagame.1.nunit"
        /// </summary>
        public PropString GameRulesType
        {
            set;
            get;
        }

        public Props GameRulesCreationParams
        {
            set;
            get;
        }

        [XmlIgnore]
        public IGameRules GameRules
        {
            set;
            get;
        }

        #endregion

        #region Serialization

        public void ConstructFromXml(ConstructFromXmlParams parameters)
        {
            if (GameRulesType != null)
            {
                string typeName = GameRulesType.Get(parameters.Local);
                string assemblyFileName = GameRulesAssemblyFile.Get(parameters.Local);
                if (typeName != "")
                {
                    IGameRules gameRules = ClassFactory.CreateInstance<IGameRules>(typeName, assemblyFileName);
                    gameRules.OnCreate(GameRulesCreationParams);
                    GameRules = gameRules;
                }
            }
            if (DeckDescrFile != null)
            {
                string deckFile = DeckDescrFile.Get(parameters.Local);
                if (deckFile != "")
                {
                    DeckDescr = XmlSerializerExt.Deserialize<DeckDescriptor>(deckFile);
                }
            }
        }

        #endregion

        #region Implementation

        private double[] _betStructure;
        #endregion
    }
}