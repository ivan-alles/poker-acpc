/*
 * game.hpp
 * author: Kevin Waugh (waugh@cs.ualberta.ca)
 * date:   August 24 2008
 * description: game and state descriptions for leduc hold'em
 */
#ifndef _GAME_HPP_
#define _GAME_HPP_

#include <string>

namespace leduc {

/* constants */
extern const int RAISE;
extern const int CALL;
extern const int FOLD;
extern const int NUM_ACTIONS;
extern const char ACTION_TO_CHAR[];
extern const int CHAR_TO_ACTION[];
extern const int NUM_RANKS;
extern const char RANK_TO_CHAR[];
extern const int CHAR_TO_RANK[];
extern const char ROUND_SEPERATOR;
extern const int& INTERNAL_SEQUENCES;
extern const int& TERMINAL_SEQUENCES;

/* initialize the leduc game description */
void init();

/* free the leduc game description */
void free();

/* who is the opponent of a player */
int opposite_player(int player);

/* deal a random hand */
void deal_hand(int& hole0, int& hole1, int& board);

/* who wins a hand? 1 = player 0, -1 = player 1, 0 = draw */
int evaluate_winner(int hole0, int hole1, int board);

/* a leduc betting sequence */
class sequence {
public:  
  /* construct root betting sequence */
  sequence();

  /* construct betting sequence from id */
  sequence(int u);

  /* construct a betting sequence from a string */
  sequence(const char * str);

  /* get the id for this type of node */
  int get_id() const;

  /* what round are we in? */
  int get_round() const;

  /* whose turn is it? */
  int whose_turn() const;
  
  /* does this action start the round? */
  bool starts_round() const;

  /* is terminal state? */
  bool is_terminal() const;

  /* is fold state? */
  bool is_fold() const;

  /* is showdown? */
  bool is_showdown() const;

  /* who wins at a fold? */
  int winner_at_fold() const;

  /* win amount at terminal */
  int win_amount() const;

  /* who folded at a fold? */
  int who_folded() const;

  /* get sequence as string */
  std::string to_string() const;

  /* number of actions appliciable from a non-chance state? */
  int num_actions() const;
  
  /* is an action applicable from a non-chance state? */
  bool can_do_action(int v) const;

  /* do action at a non-chance state */
  sequence do_action(int v) const;

  /* get the parent sequence */
  sequence parent() const;

private:
  int u; /* betting sequence for the game */
};

/* a player's view of the action */
class player_view {
public:
  /* constructor, player's view at beginning of game */
  player_view(int player, int hole);

  /* constructor, player's view at beginning of game with board card exposed */
  player_view(int player, int hole, int board);

  /* constructor, player's view in preflop situation */
  player_view(int player, int hole, sequence u);

  /* constructor, player's view in postflop situation */
  player_view(int player, int hole, int board, sequence u);

  /* player's hole card */
  int get_hole() const;

  /* board card */
  int get_board() const;
  
  /* get betting sequence */
  sequence get_sequence() const;

  /* which player are we? */
  int get_player() const;

  /* get the player's hand */
  int get_hand() const;

private:
  int player, hole, board;
  sequence u;
};

};

#endif /* _GAME_HPP_ */
