/*
 * play.cpp
 * author:      Kevin Waugh (waugh@cs.ualberta.ca)
 * date:        August 26 2008
 * description: play two strategies against each other in leduc
 */

#include <cassert>
#include <cstdio>
#include <ctime>
#include <getopt.h>
#include "game.hpp"
#include "strategy.hpp"

#ifdef  NDEBUG
#undef assert
#define assert(_Expression)     _Expression
#endif

/* random number generator seed */
static const int RNG_SEED = 31337;

/* display program usage */
static void usage(const char * program);

/* compute EV */
static double compute_ev(leduc::strategy strategies[2]);

/* compute EV preflop */
static double compute_ev(leduc::sequence u, double reach,
			 int hole[2],
			 leduc::strategy strategies[2]);

/* compute EV after flop */
static double compute_ev(leduc::sequence u, double reach,
			 int hole[2], int board,
			 leduc::strategy strategies[2]);

/* entry point */
int main(int argc, char ** argv) {

  /* initialize leduc game definition */
  leduc::init();

  /* seed random number generator */
  srand(RNG_SEED);
  
  if (argc == 1) {
  
    /* display usage */
    usage(argv[0]);
  } else {
  
    /* parse command line arguments */
    const char * strategy_str[2];
    struct option opts[] = {
      {"strategy1", required_argument, 0, 1},
      {"strategy2", required_argument, 0, 2},
      {0,0,0,0},
    };

    strategy_str[0] = NULL;
    strategy_str[1] = NULL;
    
    for(int ch; (ch=getopt_long(argc, argv, "", opts, NULL)) != -1;) {
    
      switch(ch) {
      case 1: strategy_str[0] = optarg; break;
      case 2: strategy_str[1] = optarg; break;
      default: assert(0);
      }
    }

    assert(strategy_str[0]);
    assert(strategy_str[1]);

    leduc::strategy strategies[2];
    
    /* load strategies */
    printf("loading strategy %s for seat 1...\n", strategy_str[0]);
    strategies[0].load(strategy_str[0]);
    
    printf("loading strategy %s for seat 2...\n", strategy_str[1]);
    strategies[1].load(strategy_str[1]);

    printf("computing expected value in terms of player 1...\n");

    /* compute EV */
    double value = compute_ev(strategies);

    printf("EV = %g\n", value);
  }

  /* free leduc subsystem */
  leduc::free();

  /* /\ */
  return 0;
}

static void usage(const char * program) {

  printf("%s [arguments]\n"
	 "\n"
	 "arguments:\n"
	 "  --strategy1=[strategy]\n"
	 "  --strategy2=[strategy]\n",
	 program
	 );
}

static double compute_ev(leduc::strategy strategies[2]) {

  /* loop over all possible dealings of hole cards and recurse on preflop */
  double sum = 0;
  for(int i=0; i<3; ++i) {
    
    for(int j=0; j<3; ++j) {
    
      int hole[2] = {i, j};
      sum += compute_ev(0, (i==j?1:2)/15., hole, strategies);
    }
  }
  
  return sum;
}

/* compute EV preflop */
static double compute_ev(leduc::sequence u, double reach,
			 int hole[2],
			 leduc::strategy strategies[2]) {

  if (u.is_terminal()) {
    
    /* terminal can't be showdown from preflop */
    assert(u.is_fold());
    
    /* check who folded and return ev */
    int amount = u.win_amount();
    if (u.who_folded() == 0) {
    
      return -reach*amount;
    } else {
    
      return reach*amount;
    }
    
  } else if (u.get_round() == 1) {
    
    /* recurse on flop */
    double sum = 0;
    for(int i=0; i<3; ++i) {
    
      if (hole[0] == i && hole[1] == i) {
	
	/* impossible deal, do not recurse */
      } else if (hole[0] == i || hole[1] == i) {
      
	sum += compute_ev(u, reach/4, hole, i, strategies);
      } else {

	sum += compute_ev(u, reach/2, hole, i, strategies);
      }
    }

    return sum;
  } else {

    int player = u.whose_turn();
    const double * tuple = strategies[player].get_strategy(leduc::player_view(player, 
									hole[player],
									u));
    
    /* recurse on all actions */
    double sum = 0;
    for(int i=0; i<3; ++i) {

      if (u.can_do_action(i)) {
      
	sum += compute_ev(u.do_action(i), reach*tuple[i], hole, strategies);
      }
      
    }

    return sum;
  }
}

/* compute EV after flop */
static double compute_ev(leduc::sequence u, double reach,
			 int hole[2], int board,
			 leduc::strategy strategies[2]) {

  if (u.is_terminal()) {
        
    /* check who folded and return ev */
    int amount = u.win_amount();
    if (u.is_fold()) {

      /* fold, check who folded */
      if (u.who_folded() == 0) {
    
	return -reach*amount;
      } else {
	
	return reach*amount;
      }
    } else {
    
      /* showdown, return ev of winner */
      return reach*amount*leduc::evaluate_winner(hole[0], hole[1], board);
    }
  } else {

    int player = u.whose_turn();
    const double * tuple = strategies[player].get_strategy(leduc::player_view(player, 
									      hole[player],
									      board,
									      u));
    
    /* recurse on all actions */
    double sum = 0;
    for(int i=0; i<3; ++i) {

      if (u.can_do_action(i)) {
      
	sum += compute_ev(u.do_action(i), reach*tuple[i], hole, board, strategies);
      }
      
    }

    return sum;
  }  
  
}
