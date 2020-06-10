/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

namespace ai.pkr.metastrategy
{
    /// <summary>
    /// A node in a strategy tree. It stores information about strategic probability in
    /// the game state identified by the node.
    /// <para>It is designed for the following use-cases:</para>
    /// <para>1. Store static strategy to be used in a bot. The bot must be
    /// able to find a node based on real-game information. This also implies that the strategies
    /// can be huge, and the design of this interface must allow compact implementations. It also must be possible to 
    /// store strategy of all players into the same tree provided that all players use the same game abstraction.</para>
    /// <para>2. Initial, intermediate or output storage in algos (e.g. input for GameValue, or input/output for SBR). 
    /// The algos must be able to convert this data from/to the internal representation.</para>
    /// </summary>
    /// Todo: think if we need this interface at all. 
    public interface IStrategyTreeNode
    {
        /// <summary>
        /// True if it is a dealer action.
        /// </summary>
        bool IsDealerAction { get; }

        /// <summary>
        /// True if it is a player action and the position matches.
        /// </summary>
        bool IsPlayerAction(int position);

        int Position { get; }
        int Card { get; }
        double Amount { get; }


        /// <summary>
        /// Probability of player taking this action,
        /// provided that Action is a player action.
        /// </summary>
        double Probab { get; }

        /// <summary>
        /// Returns a game string. Parameters may contain implementation-depeding data allowing special formatting,
        /// for instance card names.
        /// </summary>
        string ToStrategicString(object parameters);
    }
}