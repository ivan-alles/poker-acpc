/*
 * best_response.cpp
 * author:      Kevin Waugh (waugh@cs.ualberta.ca)
 * date:        August 25 2008
 * description: compute best response value against a strategy
 */

#include <algorithm>
#include <cassert>
#include <cstdio>
#include <ctime>
#include <getopt.h>
#include "abstraction.hpp"
#include "game.hpp"
#include "strategy.hpp"

#ifdef  NDEBUG
#undef assert
#define assert(_Expression)     _Expression
#endif

using namespace std;

/* epsilon */
static const double EPSILON = 1e-7;

/* display program usage */
static void usage(const char * program);

class terminal_probabilities {
public:
  /* constructor */
  terminal_probabilities();

  /* copy constructor */
  terminal_probabilities(const terminal_probabilities& other);

  /* destructor */
  ~terminal_probabilities();

  /* assignment operator */
  terminal_probabilities& operator=(const terminal_probabilities& other);

  /* set a terminal probability */
  void set_probability(leduc::sequence u, int hand, double prob);

  /* get a terminal probability */
  double get_probability(leduc::sequence u, int hand) const;

private:
  /* copy from a different terminal probabilty object */
  void copy_from(const terminal_probabilities& other);

private:
  double ** probability;
};

/* compute terminal probabilities */
static void compute_terminal_probabilities(int player, const leduc::strategy& strategy,
					   terminal_probabilities& terminal_probs);

/* compute terminal probabilities on preflop */
static void compute_terminal_probabilities(leduc::sequence u, int player, int hole, 
					   double reach, 
					   const leduc::strategy& strategy,
					   terminal_probabilities& terminal_probs);

/* compute terminal probabilities on flop */
static void compute_terminal_probabilities(leduc::sequence u, int player, int hole, 
					   int board, double reach,
					   const leduc::strategy& strategy,
					   terminal_probabilities& terminal_probs);

/* compute best response value */
static double compute_best_response(leduc::sequence u, int player, int b1, int b2,
                                    const leduc::abstraction& abstraction,
				    const terminal_probabilities& terminal_probs,
				    leduc::strategy& out);

/* chance probability of a hand being dealt */
static double chance_prob(int hole0, int hole1);			

/* chance probability of a hand being dealt */
static double chance_prob(int hole0, int hole1, int board);     

/* entry point */
int main(int argc, char ** argv) {

  /* initialize leduc game definition */
  leduc::init();

  if (argc == 1) {
    
    /* display program usage */
    usage(argv[0]);
  } else {

    /* parse command line arguments */
    const char * strategy_str, * output_str;
    int player;
    leduc::abstraction abstraction;
    struct option opts[] = {
      {"strategy", required_argument, 0, 1},
      {"player", required_argument, 0, 2},
      {"output", required_argument, 0, 3},
      {"abstraction", required_argument, 0, 4},
      {0,0,0,0},
    };

    strategy_str = NULL;
    player       = 0;
    output_str   = 0;
    for(int ch; (ch=getopt_long(argc, argv, "", opts, NULL)) != -1;) {
      
      switch(ch) {
      case 1: strategy_str = optarg; break;
      case 2: assert(sscanf(optarg, "%d", &player) == 1); --player; break;
      case 3: output_str = optarg; break;
      case 4: abstraction = leduc::abstraction(optarg); break;
      default: assert(0);
      }
    }

    assert(strategy_str);
    assert(player == 0 || player == 1);
    assert(abstraction.is_complete());
    assert(abstraction.is_perfect_recall());

    if (!output_str) {

      printf("WARNING: strategy will not be saved.\n");
    }
    
    /* load strategy */
    leduc::strategy strategy;
    printf("loading strategy from %s...\n", strategy_str);
    strategy.load(strategy_str);

    /* compute terminal probabilities */
    terminal_probabilities terminal_probs;
    printf("computing terminal probabilities...\n");
    compute_terminal_probabilities(leduc::opposite_player(player), strategy,
				   terminal_probs);

    /* compute best response */
    leduc::strategy out;
    printf("computing best response...\n");
    double value = compute_best_response(0, player, -1, -1, abstraction, terminal_probs, out);
    printf("value for player %d against %s is %g\n", player+1, strategy_str, 
	  value);

    /* save the strategy if requested */
    if (output_str) {
      
      printf("saving best response to %s...\n", output_str);
      
      /* open the file */
      FILE * file = fopen(output_str, "wt");
      assert(file);

      time_t time_now = time(NULL);
      fprintf(file,
	      "#\n"
	      "# best response leduc strategy %s\n"
	      "# made on:  %s"
	      "# opponent: %s\n"
	      "# value:    %g\n"
	      "#\n",
	      output_str,
	      ctime(&time_now),
	      strategy_str,
	      value);

      /* write the strategy */
      out.save(file);

      /* close the file */
      fclose(file);
    }
  }

  /* free leduc game definition */
  leduc::free();

  /* /\ */
  return 0;
}

static void usage(const char * program) {

  printf("%s [arguments]\n"
	 "\n"
	 "arguments:\n"
	 "  --strategy=[strat]\n"
	 "  --player=[player]\n"
	 "  --output=[best response strategy]\n"
         "  --abstraction=[best response player's abstraction]\n"
	 , program
	 );
}

terminal_probabilities::terminal_probabilities() {

  probability = new double*[leduc::TERMINAL_SEQUENCES];
  for(int i=0; i<leduc::TERMINAL_SEQUENCES; ++i) {
  
    int n;
    if (leduc::sequence(leduc::INTERNAL_SEQUENCES+i).get_round() == 0) {

      n = 3;
    } else {
      
      n = 9;
    }
    probability[i] = new double[n];
    
    for(int j=0; j<n; ++j) {
    
      probability[i][j] = -1;
    }
  }
}

terminal_probabilities::terminal_probabilities(const terminal_probabilities& other) {

  copy_from(other);
}

terminal_probabilities::~terminal_probabilities() {

  for(int i=0; i<leduc::TERMINAL_SEQUENCES; ++i) {
  
    delete [] probability[i];
  }

  delete [] probability;
}

terminal_probabilities& terminal_probabilities::operator=(const terminal_probabilities& other) {

  if (&other != this) {
  
    copy_from(other);
  }

  return *this;
}

void terminal_probabilities::set_probability(leduc::sequence u, int hand, double prob) {
  
  assert(u.is_terminal());
  assert(hand >= 0);
  if (u.get_round() == 0) {
  
    assert(hand < 3);
  } else {
  
    assert(hand < 9);
  }
  assert(prob > -EPSILON);
  assert(prob < 1+EPSILON);
  
  probability[u.get_id()-leduc::INTERNAL_SEQUENCES][hand] = prob;
  //printf("TP: h:%d s:%s p:%f\n", hand, u.to_string().c_str(), prob);
}

double terminal_probabilities::get_probability(leduc::sequence u, int hand) const {

  assert(u.is_terminal());
  assert(hand >= 0);
  if (u.get_round() == 0) {
  
    assert(hand < 3);
  } else {
  
    assert(hand < 9);
  }
  
  double prob = probability[u.get_id()-leduc::INTERNAL_SEQUENCES][hand];
  assert(prob > -EPSILON);

  return prob;
}

void terminal_probabilities::copy_from(const terminal_probabilities& other) {

  for(int i=0; i<leduc::TERMINAL_SEQUENCES; ++i) {
  
    int n;
    if (leduc::sequence(i).get_round() == 0) {
      
      n = 3;
    } else {
    
      n = 9;
    }

    for(int j=0; j<n; ++j) {
    
      probability[i][j] = other.probability[i][j];
    }
  }
}

static void compute_terminal_probabilities(int player, 
					   const leduc::strategy& strategy,
					   terminal_probabilities& terminal_probs) {

  for(int i=0; i<3; ++i) {
  
    compute_terminal_probabilities(0, player, i, 1, strategy, terminal_probs);
  }
}

static void compute_terminal_probabilities(leduc::sequence u, int player, int hole, 
					   double reach, 
					   const leduc::strategy& strategy,
					   terminal_probabilities& terminal_probs) {

  if (u.is_terminal()) {

    /* store the reach probability */
    terminal_probs.set_probability(u, hole, reach);
  } else if (u.get_round() == 1) {

    /* move on to next round */
    for(int i=0; i<3; ++i) {
    
      compute_terminal_probabilities(u, player, hole, i, reach, strategy,
				     terminal_probs);
    }
  } else if (u.whose_turn() == player) {
    
    const double * probability = strategy.get_strategy(leduc::player_view(player, hole, u));

    /* recurse modifying the reach probability */
    for(int i=0; i<leduc::NUM_ACTIONS; ++i) {
      
      if (u.can_do_action(i)) {
	compute_terminal_probabilities(u.do_action(i), player, hole, reach*probability[i], 
				       strategy, terminal_probs);
      }
    }

  } else {
    
    /* recurse normally */
    for(int i=0; i<leduc::NUM_ACTIONS; ++i) {

      if (u.can_do_action(i)) {
      
	compute_terminal_probabilities(u.do_action(i), player, hole, reach, strategy,
				       terminal_probs);
      }
    }
  }
}

/* compute terminal probabilities on flop */
static void compute_terminal_probabilities(leduc::sequence u, int player, int hole, 
					   int board, double reach,
					   const leduc::strategy& strategy,
					   terminal_probabilities& terminal_probs) {
  
  if (u.is_terminal()) {

    /* store the reach probability */
    terminal_probs.set_probability(u, 3*hole + board, reach); 
  } else if (u.whose_turn() == player) {
    
    const double * probability = strategy.get_strategy(leduc::player_view(player, hole, board, u));

    /* recurse modifying the reach probability */
    for(int i=0; i<leduc::NUM_ACTIONS; ++i) {
      
      if (u.can_do_action(i)) {
	compute_terminal_probabilities(u.do_action(i), player, hole, board, reach*probability[i], 
				       strategy, terminal_probs);
      }
    }

  } else {
    
    /* recurse normally */
    for(int i=0; i<leduc::NUM_ACTIONS; ++i) {

      if (u.can_do_action(i)) {
      
	compute_terminal_probabilities(u.do_action(i), player, hole, board,
				       reach, strategy,
				       terminal_probs);
      }
    }
  }
}

static double compute_best_response(leduc::sequence u, int player, int b1, int b2,
                                    const leduc::abstraction& abstraction,
				    const terminal_probabilities& terminal_probs,
				    leduc::strategy& out) {

  if (u.is_terminal()) {

    /* compute the ev at the terminal */
    if (u.is_showdown()) {

      double ev = 0;
      for(int i=0; i<9; ++i) {
        
        leduc::player_view view(player, i/3, i%3, u);
        if (abstraction.abstract_view(view) == b2) {

          for(int j=0; j<3; ++j) {

            if (!(i%3 == j && i/3 == j)) {

              double p = terminal_probs.get_probability(u, i%3 + 3*j);
			  double chance_pr = chance_prob(i/3, j, i%3);
			  double loc_ev = leduc::evaluate_winner(i/3, j, i%3)*u.win_amount()*p*chance_pr;
              ev += loc_ev;
			  //printf("%d %d %d %s ev:%f sp:%f cp:%f wa:%f\n", view.get_hole(), j, view.get_board(), u.to_string().c_str(), ev, p, chance_pr, (double)u.win_amount());
            }
          }
        }
      }
      return ev;
    } else {

      assert(u.is_fold());
      
      int result = 1;
      if (u.who_folded() == player) {

        result = -1;
      }

      double p = 0;
      if (u.get_round()) {

        for(int i=0; i<9; ++i) {
          
          leduc::player_view view(player, i/3, i%3, u);
          if (abstraction.abstract_view(view) == b2) {
           
            for(int j=0; j<3; ++j) {
        
              if (!(i/3 == j && i%3 == j)) {
               
                p += terminal_probs.get_probability(u, i%3 + 3*j)*chance_prob(i/3, j, i%3);
              }
            }
          }
        }
      } else {
        for(int i=0; i<3; ++i) {
          
          leduc::player_view view(player, i, u);
          if (abstraction.abstract_view(view) == b1) {
           
            for(int j=0; j<3; ++j) {
        
              p += terminal_probs.get_probability(u, j)*chance_prob(i, j);
            }
          }
        }
      }
      
      return p*result*u.win_amount();
    }
  } else if (b1 == -1) {

    /* loop over all next buckets */
    double v = 0;
    for(int i=0, n=abstraction.preflop_buckets(); i<n; ++i) {
      
      v += compute_best_response(u, player, i, -1, abstraction,
                                 terminal_probs, out);
    }
    return v;
  } else if (u.get_round() == 1 && b2 == -1) {

    /* loop over all next buckets */
    double v = 0;
    for(int i=0, n=abstraction.flop_buckets(); i<n; ++i) {

      if (abstraction.can_extend(b1, i)) {
       
        v += compute_best_response(u, player, b1, i, abstraction,
                                   terminal_probs, out);
      }
    }
    return v;
  } else if (u.whose_turn() == player) {

    /* loop over all next actions and find maximum */
    double maxv = -100;
    int maxi = -1;
    for(int i=0; i<3; ++i) {
      
      if (u.can_do_action(i)) {

        double v = compute_best_response(u.do_action(i), player, b1, b2,
                                         abstraction, terminal_probs, out);
        if (v > maxv) {
          
          maxv = v;
          maxi = i;
        }
      }
    }
    
    /* set output strategy */
    double tuple[3] = {0}; tuple[maxi] = 1;
    if (u.get_round()) {

      for(int i=0; i<9; ++i) {
        
        leduc::player_view view(player, i/3, i%3, u);
        if (abstraction.abstract_view(view) == b2) {
          
          out.set_strategy(view, tuple);
        }
      }
    } else {

      for(int i=0; i<3; ++i) {
        
        leduc::player_view view(player, i, u);
        if (abstraction.abstract_view(view) == b1) {
          
          out.set_strategy(view, tuple);
        }
      }
    }
    
    return maxv;
  } else {
    
    /* recurse on all actions and sum response */
    double v = 0;
    for(int i=0; i<3; ++i) {

      if (u.can_do_action(i)) {

        v += compute_best_response(u.do_action(i), player, b1, b2, abstraction,
                                   terminal_probs, out);
      }          
    }
    return v;
  }
}

static double chance_prob(int hole0, int hole1) {

  if (hole0 == hole1) {
    
    return 1./15;
  } else {

    return 2./15;
  }
}

static double chance_prob(int hole0, int hole1, int board) {

  int cnt[3] = {2,2,2};
  int card[3] = {hole0, hole1, board};
  double prob = 1;

  for(int i=0; i<3; ++i) {
  
    prob *= cnt[card[i]]--;
  }

  return prob/6/5/4;
}
