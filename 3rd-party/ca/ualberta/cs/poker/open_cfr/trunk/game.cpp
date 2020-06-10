/*
 * game.cpp
 * author: Kevin Waugh (waugh@cs.ualberta.ca)
 * date:   August 24 2008
 * description: game and state descriptions for leduc hold'em
 */

#include <algorithm>
#include <cassert>
#include <string>
#include "game.hpp"

#ifdef  NDEBUG
#undef assert
#define assert(_Expression)     _Expression
#endif

using namespace std;

namespace leduc {

/* action constants */
const int RAISE       = 0;
const int CALL        = 1;
const int FOLD        = 2;
const int NUM_ACTIONS = 3;
const char ACTION_TO_CHAR[] = {'r', 'c', 'f'};
const int CHAR_TO_ACTION[256] = {
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x00 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x10 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x20 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x30 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x40 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x50 */
  -1, -1, -1,  1, -1, -1,  2, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x60 */
  -1, -1,  0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x70 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x80 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x90 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0xA0 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0xB0 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0xC0 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0xD0 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0xE0 */
};

/* rank constants */
const int NUM_RANKS = 3;
const char RANK_TO_CHAR[] = {'J', 'Q', 'K'};
const int CHAR_TO_RANK[] = {
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x00 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x10 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x20 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x30 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  0,  2, -1, -1, -1, -1, /* 0x40 */
  -1,  1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x50 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x60 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x70 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x80 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0x90 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0xA0 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0xB0 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0xC0 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0xD0 */
  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, /* 0xE0 */
};

/* string constants */
const char ROUND_SEPERATOR = '/';

/* 
 * leduc is a two round, two player game
 * each player antes 1 chip
 * each player is dealt 1 hole card from a deck of JJQQKK
 * the first player starts each round of betting
 * after the first round of betting 1 community flop card is revealed
 * the best hand is a pair, followed by highest card
 * each bet is 2 chips in the first round, and 4 chips in the second
 * there is a maximum of two bets/raises per round.
 */

/* number of bets/raises that can be made per round */
static const int BETS_PER_ROUND[2] = {2, 2};

/* first player to start a round */
static const int FIRST_TO_ACT[2] = {0, 0};

/* antes for each player */
static const int ANTES[2] = {1, 1};

/* bet size per round */
static const int BET_SIZE[2] = {2, 4};

/* number of internal, terminal and total sequences */
static int num_internal, num_terminal, num_total;

/* round for each internal sequence */
static int * round;

/* whose turn for each internal sequence */
static int * whose_turn;

/* who wins at each terminal sequence */
static int * winner;

/* how much is won at terminal sequence */
static int * win_amount;

/* number of actions at an internal sequence */
static int * num_actions;

/* transition table at an internal sequence */
static int (*transition)[3];

/* parent of each sequence */
static int * parent;

/* has leduc been initialized? */
static bool initialized;

/* number of internal sequences */
const int& INTERNAL_SEQUENCES = num_internal;

/* number of terminal sequences */
const int& TERMINAL_SEQUENCES = num_terminal;

/* count the number of each type of betting sequences */
static void count_sequences(int round, int raises, bool first_action,
			    int& internal, int& terminal);

/* construct the betting sequences */
static int construct_sequences(int player, int round, int raises, int pot[2],
			       bool first_action, int& internal, int& terminal);

void init() {
  
  /* sanity check on tables */
  assert(CHAR_TO_ACTION[(int)'r'] == RAISE);
  assert(CHAR_TO_ACTION[(int)'c'] == CALL);
  assert(CHAR_TO_ACTION[(int)'f'] == FOLD);
  assert(CHAR_TO_RANK[(int)'J'] == 0);
  assert(CHAR_TO_RANK[(int)'Q'] == 1);
  assert(CHAR_TO_RANK[(int)'K'] == 2);

  /* count the number of each type of betting sequence first */
  count_sequences(0, 0, true, num_internal, num_terminal);
  num_total = num_internal + num_terminal;

  /* now that we know the counts we can construct the betting sequences */
  /* first allocate memory */
  round       = new int[num_internal];
  whose_turn  = new int[num_internal];
  winner      = new int[num_terminal];
  win_amount  = new int[num_terminal];
  num_actions = new int[num_internal];
  transition  = new int[num_internal][3];
  parent      = new int[num_total];

  /* now construct the sequences */
  int i = 0, j = 0, k[2] = {ANTES[0], ANTES[1]};
  construct_sequences(FIRST_TO_ACT[0], 0, 0, k, true, i, j);

  /* root has no parent */
  parent[0] = -1; 

  initialized = true;
}

void free() {
  
  assert(initialized);

  /* free the memory */
  delete [] round;
  delete [] whose_turn;
  delete [] winner;
  delete [] num_actions;
  delete [] transition;
  delete [] parent;

  initialized = false;
}

int opposite_player(int player) {

  assert(player == 0 || player == 1);

  return 1-player;
}

void deal_hand(int& hole0, int& hole1, int& board) {
  static int deck[6] = {0,0,1,1,2,2};
  
  swap(deck[0], deck[1 + rand()%5]);
  swap(deck[1], deck[2 + rand()%4]);
  swap(deck[2], deck[3 + rand()%3]);

  hole0 = deck[0];
  hole1 = deck[1];
  board = deck[2];
}

int evaluate_winner(int hole0, int hole1, int board) {

  assert(hole0 >= 0);
  assert(hole0 < NUM_RANKS);
  assert(hole1 >= 0);
  assert(hole1 < NUM_RANKS);
  assert(board >= 0);
  assert(board < NUM_RANKS);
  assert(!((hole0 == hole1) && (hole0 == board)));

  /* first check for pairs, then high card */
  if (hole0 == board) {

    return 1;
  } else if (hole1 == board) {

    return -1;
  } else if (hole0 > hole1) {

    return 1;
  } else if (hole1 > hole0) {
    
    return -1;
  } else {

    return 0;
  }
}

sequence::sequence() : u(0) {

  assert(initialized);
}

sequence::sequence(int u) : u(u) {

  assert(initialized);
}

sequence::sequence(const char * str) {

  assert(initialized);
  assert(str);
  assert(*str == ROUND_SEPERATOR);
  ++str;

  /* walk betting sequence str */
  u = 0;
  for(const char * p=str; *p;) {
    
    /* find next state */
    int v = CHAR_TO_ACTION[(int)*(p++)];
    assert(v != -1);
    assert(u < num_internal);
    int w = transition[u][v];
    assert(w != -1);

    /* should there be a round seperator too? */
    if (w < num_internal && round[w] > round[u]) {
      assert(*(p++) == ROUND_SEPERATOR);
    }

    u = w;
  } 
}

int sequence::get_id() const {
  
  assert(initialized);
  
  return u;
}

int sequence::get_round() const {

  assert(initialized);

  if (is_terminal()) {

    /* get round from parent non-terminal state */
    return round[leduc::parent[u]];
  } else {
    
    return round[u];
  }
}

int sequence::whose_turn() const {

  assert(!is_terminal());

  return leduc::whose_turn[u];
}

bool sequence::starts_round() const {

  assert(initialized);

  /* if last sequence was in round 1 and we're in round 2, or we started round 1 */
  return !u || (u < num_internal && round[u] == 1 && round[leduc::parent[u]] == 0);
}

bool sequence::is_terminal() const {

  assert(initialized);

  return u >= num_internal;
}

bool sequence::is_fold() const {
  
  return is_terminal() && (winner[u-num_internal] == 0 || 
			   winner[u-num_internal] == 1);
}

bool sequence::is_showdown() const {

  return is_terminal() && winner[u-num_internal] == -1;
}

int sequence::winner_at_fold() const {
  
  assert(is_fold());

  return winner[u-num_internal];
}

int sequence::win_amount() const {

  assert(is_terminal());

  return leduc::win_amount[u-num_internal];
}

int sequence::who_folded() const {

  return opposite_player(winner_at_fold());
}

string sequence::to_string() const {
  
  if (u) {

    /* 
     * trace up to root, looking what action takes us from each
     * parent to each child
     */
    string str;
    for(int v=leduc::parent[u],w=u; v!=-1; w=v,v=leduc::parent[v]) {
      
      /* find what action moves us from parent to us */
      int i;
      for(i=0; i<NUM_ACTIONS; ++i) {

	if (transition[v][i] == w) {

	  break;
	}
      }
      
      assert(i < NUM_ACTIONS);

      /* check if we should add a round seperator */
      if (w < num_internal && round[v] < round[w]) {

	str.push_back(ROUND_SEPERATOR);
      } 

      /* add the action */
      str.push_back(ACTION_TO_CHAR[i]);
    }
    str.push_back(ROUND_SEPERATOR);

    /* string is in the reverse order it needs to be in */
    reverse(str.begin(), str.end());
    return str;
  } else {
  
    /* root has empty betting string */
    return string(1, ROUND_SEPERATOR);
  }
}

int sequence::num_actions() const {
  
  assert(!is_terminal());

  return leduc::num_actions[u];
}

bool sequence::can_do_action(int v) const {

  assert(!is_terminal());
  assert(v >= 0);
  assert(v < NUM_ACTIONS);

  return transition[u][v] != -1;
}

sequence sequence::do_action(int v) const {
  
  assert(can_do_action(v));
  
  return sequence(transition[u][v]);
}

sequence sequence::parent() const {
  
  assert(u);

  return sequence(leduc::parent[u]);
}

player_view::player_view(int player, int hole) 
  : player(player), hole(hole), board(-1), u(0) {
  
  assert(player == 0 || player == 1);
  assert(hole >= 0);
  assert(hole < NUM_RANKS);
}

player_view::player_view(int player, int hole, int board) 
  : player(player), hole(hole), board(board), u(0) {

  assert(player == 0 || player == 1);
  assert(hole >= 0);
  assert(hole < NUM_RANKS);
  assert(board >= -1);
  assert(board < NUM_RANKS);
}

player_view::player_view(int player, int hole, sequence u) 
  : player(player), hole(hole), board(-1), u(u) {

  assert(player == 0 || player == 1);
  assert(hole >= 0);
  assert(hole < NUM_RANKS);
  assert(board >= -1);
  assert(board < NUM_RANKS);
}

player_view::player_view(int player, int hole, int board, sequence u) 
  : player(player), hole(hole), board(board), u(u) {

  assert(player == 0 || player == 1);
  assert(hole >= 0);
  assert(hole < NUM_RANKS);
  assert(board >= -1);
  assert(board < NUM_RANKS);
}

int player_view::get_hole() const {

  return hole;
}

int player_view::get_board() const {

  return board;
}

sequence player_view::get_sequence() const {

  return u;
}

int player_view::get_player() const {

  return player;
}

int player_view::get_hand() const {

  if (board == -1) {

    return hole;
  } else {
  
    return 3*hole + board;
  }
}

static void count_sequences(int round, int raises, bool first_action,
			    int& internal, int& terminal) {

  /* count this as an internal sequence */
  ++internal;

  /* can we raise? */
  if (raises < BETS_PER_ROUND[round]) {
    
    /* we can raise, take this action */
    count_sequences(round, raises+1, false, internal, terminal);
  }
  
  /* we can always call */
  if (first_action) {

    /* call does not end round */
    assert(raises == 0);
    count_sequences(round, raises, false, internal, terminal);
  } else {

    /* call ends round */
    if (round) {

      /* we've hit a showdown */
      ++terminal;
    } else {

      /* move to second round */
      count_sequences(1, 0, true, internal, terminal);
    }
  }

  /* can we fold? */
  if (raises) {
    
    /* we can fold because we are faced with a bet */
    ++terminal;
  } 

}

static int construct_sequences(int player, int round, int raises, int pot[2],
			       bool first_action, int& internal, int& terminal) {
  
  /* set the information for this round */
  int u = internal++;
  leduc::round[u] = round;
  whose_turn[u]   = player;
  num_actions[u]  = 1; /* we can always call */

  /* how much is to call? */
  int opponent = opposite_player(player);
  int to_call = pot[opponent] - pot[player];

  assert(to_call >= 0);

  /* can we raise? */
  if (raises < BETS_PER_ROUND[round]) {

    /* count this action */
    ++num_actions[u];

    /* make raise and construct child sequence */
    pot[player] += to_call + BET_SIZE[round];
    int v = construct_sequences(opponent, round, raises+1,
				pot, false, internal, terminal);
    
    /* undo raise to pot */
    pot[player] -= to_call + BET_SIZE[round];

    /* set transition for us and parent for child sequence */
    transition[u][0] = v;
    parent[v]        = u;
  } else {
  
    /* cannot raise */
    transition[u][0] = -1;
  }

  /* can always call */
  if (first_action) {

    assert(pot[0] == pot[1]);

    /* call does not end round, construct child sequence */
    int v = construct_sequences(opponent, round, 0, pot, false, 
				internal, terminal);
    
    /* set transition for us and parent for child sequence */
    transition[u][1] = v;
    parent[v]        = u;
  } else {
    
    /* call ends round */
    if (round) {

      /* last round => showdown */
      int v = terminal++;
      winner[v]     = -1;
      win_amount[v] = pot[player] + to_call;
      
      /* set transition and parent */
      transition[u][1]       = v+num_internal;
      parent[v+num_internal] = u;
    } else {

      /* move to second round, construct child sequence */
      pot[player] += to_call;
      int v = construct_sequences(FIRST_TO_ACT[1], 1, 0, pot, true,
				  internal, terminal);

      /* undo call to pot */
      pot[player] -= to_call;

      /* set transition for us and parent for child sequence */
      transition[u][1] = v;
      parent[v]        = u;
    }
  }

  /* can we fold? */
  if (raises) {
    
    assert(to_call);

    /* count this action */
    ++num_actions[u];

    /* create terminal state */
    int v = terminal++;
    winner[v]     = opponent;
    win_amount[v] = pot[player];
    
    /* set transition and parent */
    transition[u][2]       = v+num_internal;
    parent[v+num_internal] = u;
  } else {
  
    /* cannot fold */
    transition[u][2] = -1;
  }

  /* return our sequence id */
  return u;
}

};
