/*
 * train.cpp
 * author:      Kevin Waugh (waugh@cs.ualberta.ca)
 * date:        August 24 2008
 * description: train a leduc player
 */

#include <cassert>
#include <cstdio>
#include <cstdlib>
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

/* default number of iterations to run */
static const int ITERATIONS = 100000000;

/* time between display messages */
static const int DISPLAY_SECONDS = 30;

/* default random number generator seed */
static const int RNG_SEED = 31337;

/* epsilon */
static const double EPSILON = 1e-7;

/* program usage */
static void usage(const char * program);

/* regret based strategy */
class regret_strategy {
public:
  /* constructor */
  regret_strategy(int player, int preflop_buckets, int flop_buckets);

  /* copy constructor */
  regret_strategy(const regret_strategy& other);

  /* destructor */
  ~regret_strategy();

  /* assignment operator */
  regret_strategy& operator=(const regret_strategy& other);

  /* get regret for a situation */
  double * get_regret(leduc::sequence u, int bucket) const;

  /* get the average probability for a situation */
  double * get_average_probability(leduc::sequence u, int bucket) const;

  /* get the current regret strategy at a sequence/bucket combo */
  void get_probability(leduc::sequence u, int bucket, 
		double probability[3]) const;

  /* get the current normalized average strategy */
  void get_normalized_average_probability(leduc::sequence u, int bucket,
		double probability[3]) const;

private:
  /* copy from another regret strategy */
  void copy_from(const regret_strategy& other);

  /* destroy this regret strategy */
  void destroy();

private:
  double (**regret)[3], (**average_probability)[3];
  int player, preflop_buckets, flop_buckets;
};

/* update the regret */
static void update_regret(leduc::sequence u, int buckets[2][2], int hole[2], int board,
	int result, double reach[2], double chance, double ev[2], double cfr[2], 
	regret_strategy strat[2]);

/* recover the average strategy */
static void recover_average_strategy(int player, leduc::abstraction& abstraction,
	regret_strategy& regret_strat,
	leduc::strategy& strat); 

/* recover the average strategy on preflop */
static void recover_average_strategy(leduc::sequence u, int player, int hole,
	leduc::abstraction& abstraction,
	regret_strategy& regret_strat,
	leduc::strategy& strat);

/* recover the average strategy on the flop */
static void recover_average_strategy(leduc::sequence u, int player, int hole, int board,
	leduc::abstraction& abstraction,
	regret_strategy& regret_strat,
	leduc::strategy& strat);


/* entry point */
int main(int argc, char ** argv) {

  if (argc == 1) {

    /* display program usage if no arguments */
    usage(argv[0]);
  } else {

    /* initialize rng */
    srand(RNG_SEED);

    /* initialize game description */
    leduc::init();

    /* parse command line arguments */
    const char * abstraction_str[2];
    const char * output_str[2];
    int iterations;
    struct option opts[] = {
      {"abstraction1", required_argument, 0, 1},
      {"abstraction2", required_argument, 0, 2},
      {"output1", required_argument, 0, 3},
      {"output2", required_argument, 0, 4},
      {"iterations", required_argument, 0, 5},
      {"seed-time", no_argument, 0, 6},
      {0,0,0,0},
    };
    
    abstraction_str[0] = NULL;
    abstraction_str[1] = NULL;
    output_str[0]      = NULL;
    output_str[1]      = NULL;
    iterations         = ITERATIONS;

    for(int ch; (ch=getopt_long(argc, argv, "", opts, NULL)) != -1;) {
      
      switch(ch) {
				case 1: abstraction_str[0] = optarg; break;
				case 2: abstraction_str[1] = optarg; break;
				case 3: output_str[0] = optarg; break;
				case 4: output_str[1] = optarg; break;
				case 5: assert(sscanf(optarg, "%d", &iterations) == 1); break;
				case 6: srand(time(NULL)); break;
				default: assert(0);
      }
    }

    /* check arguments */
    assert(abstraction_str[0]);
    assert(abstraction_str[1]);
    assert(iterations > 0);

    if (!(output_str[0] || output_str[1])) {

      printf("WARNING: you aren't saving the strategies anywhere...\n");
    }
		/* create abstractions */
    printf("creating abstractions...\n");
    leduc::abstraction abstraction[2] = {
      leduc::abstraction(abstraction_str[0]),
      leduc::abstraction(abstraction_str[1])
    };
    
    /* print abstractions */
    for(int i=0; i<2; ++i) {

      printf("abstraction %d: %s\n", i+1, abstraction_str[i]);

      for(int j=0; j<3; ++j) {

				printf("  %c => ", leduc::RANK_TO_CHAR[j]);
				leduc::player_view view(0, j);
				if (abstraction[i].can_abstract_view(view)) {

					printf("%d\n", abstraction[i].abstract_view(view));
				} else {

					printf("-1\n");
				}
      }

      for(int j=0; j<3; ++j) {
				for(int k=0; k<3; ++k) {

					printf("  %c%c => ", leduc::RANK_TO_CHAR[j], leduc::RANK_TO_CHAR[k]);
					leduc::player_view view(0, j, k);
					if (abstraction[i].can_abstract_view(view)) {

						printf("%d\n", abstraction[i].abstract_view(view));
					} else {

						printf("-1\n");
					}
				}
      }

      printf("  %d preflop buckets\n"
				"  %d flop buckets\n",
				abstraction[i].preflop_buckets(),
				abstraction[i].flop_buckets());
    }

    /* initialize memory  */
    printf("initializing strategies...\n");
    regret_strategy regret_strat[2] = {
      regret_strategy(0,
				abstraction[0].preflop_buckets(),
				abstraction[0].flop_buckets()),
      regret_strategy(1,
				abstraction[1].preflop_buckets(),
				abstraction[1].flop_buckets()),
    };
  
    /* train away */
    printf("training for %d iterations...\n", iterations);

    time_t time_iter, time_start;
    time_start = time_iter = time(NULL);
    double acfr[2] = {0, 0};
    for(int i=1; i<=iterations; ++i) {
            
      if (!(i%1000)) {

				/* check if we should display output */
				time_t time_now = time(NULL);
	
				if (time_iter + DISPLAY_SECONDS < time_now) {
	
					printf("iteration: %d/%d (%.2lf%%)\n"
						"  elapsed:               %g min\n"
						"  eta:                   %g min\n"
						"  iterations per second: %g\n"
						"  average cfr:           %g %g\n",
						i, iterations, 100.*i/iterations, (time_now-time_start)/60.,
						1./60*(time_now-time_start)/i*(iterations-i),
						1.*i/(time_now-time_start),
						acfr[0], acfr[1]);
					time_iter = time_now;
				}
      }
      
      /* deal hand */
      int hole[2], board;
      leduc::deal_hand(hole[0], hole[1], board);
      
      /* abstract the hand */
      int buckets[2][2];
      for(int j=0; j<2; ++j) {
	
				leduc::player_view preflop(j, hole[j]);
				leduc::player_view flop(j, hole[j], board);

				if (abstraction[j].can_abstract_view(preflop)) {
					buckets[j][0] = abstraction[j].abstract_view(preflop);
				} else {
					printf("cannot view preflop\n");
					buckets[j][0] = -1;
				}

				if (abstraction[j].can_abstract_view(flop)) {
					buckets[j][1] = abstraction[j].abstract_view(flop);
				} else {
					printf("cannot view flop\n");
					buckets[j][1] = -1;
				}
      }

      /* update the regret */
      double ev[2], reach[2] = {1, 1};
      int result = leduc::evaluate_winner(hole[0], hole[1], board);
      double cfr[2] = {0, 0};
      update_regret(0, buckets, hole, board, result, reach, 1, ev, cfr, regret_strat);

      /* update average CFR */
      if (i == 1) {

        acfr[0] = cfr[0];
        acfr[1] = cfr[1];
      } else {

        for(int j=0; j<2; ++j) {
        
          acfr[j] = 1.*(i-1)/i*(acfr[j]+cfr[j]/(i-1));
        }
      }
    }

    /* save the strategies */
    printf("saving strategies...\n");

    for(int i=0; i<2; ++i) {

      if (output_str[i]) {
      
				printf("saving player %d to %s...\n", i+1, output_str[i]);

				/* open the file */
				FILE * file = fopen(output_str[i], "wt");
				assert(file);

				/* write the header */
				time_t time_now = time(NULL);
				fprintf(file, 
					"#\n"
					"# leduc strategy %s\n"
					"# made on:       %s"
					"# abstraction:   %s\n"
					"# opponent:      %s\n"
					"# iterations:    %d\n"
					"#\n",
					output_str[i],
					ctime(&time_now),
					abstraction_str[i],
					abstraction_str[1-i],
					iterations);

				/* recover the strategy */
				leduc::strategy strat;
				recover_average_strategy(i, abstraction[i], regret_strat[i], strat);
	
				/* write the strategy */
				strat.save(file);

				/* close the file */
				fclose(file);
      }
    }

    /* free game description */
    leduc::free();
  }

  /* /\ */
  return 0;
}

static void usage(const char * program) {

  printf("%s [arguments]\n"
		"\n"
		"arguments:\n"
		"  --abstraction1=[abstraction]\n"
		"  --abstraction2=[abstraction]\n"
		"  --output1=[file]\n"
		"  --output2=[file]\n"
		"  --iterations=[number of iterations]\n"
		"  --seed-time\n"
		, program
				 );
}

typedef double (*regret_value)[3];

regret_strategy::regret_strategy(int player, int preflop_buckets, int flop_buckets) 
  : player(player), preflop_buckets(preflop_buckets), flop_buckets(flop_buckets) {

  assert(player == 0 || player == 1);
  assert(preflop_buckets >= 0);
  assert(flop_buckets >= 0);

  regret              = new regret_value[leduc::INTERNAL_SEQUENCES];  
  average_probability = new regret_value[leduc::INTERNAL_SEQUENCES];
  for(int i=0; i<leduc::INTERNAL_SEQUENCES; ++i) {
  
    regret[i] = average_probability[i] = NULL;
  }

  for(int i=0; i<leduc::INTERNAL_SEQUENCES; ++i) {
    
    leduc::sequence u(i);
    if (u.whose_turn() == player) {
    
      int n;
      if (u.get_round() == 0) {
		
				n = preflop_buckets;
      } else {

				n = flop_buckets;
      }

      if (n) {
	
				regret[i]              = new double[n][3];
				average_probability[i] = new double[n][3];
				for(int j=0; j<n; ++j) {
	  
					for(int k=0; k<3; ++k) {
	    
						regret[i][j][k] = average_probability[i][j][k] = 0;
					}
				}
      }
    }
  }
}

regret_strategy::regret_strategy(const regret_strategy& other) {

  copy_from(other);
}

regret_strategy::~regret_strategy() {

  destroy();
}

regret_strategy& regret_strategy::operator=(const regret_strategy& other) {

  if (&other != this) {

    copy_from(other);
  }

  return *this;
}

double * regret_strategy::get_regret(leduc::sequence u, int bucket) const {

  assert(!u.is_terminal());
  assert(u.whose_turn() == player);
  assert(bucket >= 0);
  
  if (u.get_round() == 0) {

    assert(bucket < preflop_buckets);
  } else {

    assert(bucket < flop_buckets);
  }

  return regret[u.get_id()][bucket];
}

double * regret_strategy::get_average_probability(leduc::sequence u, int bucket) const {

  assert(!u.is_terminal());
  assert(u.whose_turn() == player);
  assert(bucket >= 0);
  
  if (u.get_round() == 0) {

    assert(bucket < preflop_buckets);
  } else {

    assert(bucket < flop_buckets);
  }

  return average_probability[u.get_id()][bucket];
}

void regret_strategy::get_probability(leduc::sequence u, int bucket, 
	double probability[3]) const {
  
  assert(!u.is_terminal());
  assert(u.whose_turn() == player);
  assert(bucket >= 0);

  if (u.get_round() == 0) {

    assert(bucket < preflop_buckets);
  } else {

    assert(bucket < flop_buckets);
  }

  /* compute sum of positive regret */
  double sum = 0;  
  for(int i=0; i<leduc::NUM_ACTIONS; ++i) {
    
    sum += max(0., regret[u.get_id()][bucket][i]);
  }
      
  /* compute probability as the proportion of positive regret */
  if (sum > EPSILON) {

    for(int i=0; i<leduc::NUM_ACTIONS; ++i) {
      
      probability[i] = max(0., regret[u.get_id()][bucket][i])/sum;
    }
  } else {

    double p = 1./u.num_actions();
    for(int i=0; i<leduc::NUM_ACTIONS; ++i) {
      
      probability[i] = (u.can_do_action(i)?1:0)*p;
    }
  }
}

void regret_strategy::get_normalized_average_probability(leduc::sequence u, int bucket,
	double probability[3]) const {

  assert(!u.is_terminal());
  assert(u.whose_turn() == player);
  assert(bucket >= 0);

  if (u.get_round() == 0) {

    assert(bucket < preflop_buckets);
  } else {

    assert(bucket < flop_buckets);
  }  

  /* compute sum */
  double sum = 0;  
  for(int i=0; i<leduc::NUM_ACTIONS; ++i) {
    
    sum += average_probability[u.get_id()][bucket][i];
  }
      
  /* normalize */
  if (sum > EPSILON) {

    for(int i=0; i<leduc::NUM_ACTIONS; ++i) {
      
      probability[i] = average_probability[u.get_id()][bucket][i]/sum;
    }
  } else {

    double p = 1./u.num_actions();
    for(int i=0; i<leduc::NUM_ACTIONS; ++i) {
      
      probability[i] = (u.can_do_action(i)?1:0)*p;
    }
  }
}

void regret_strategy::copy_from(const regret_strategy& other) {

  destroy();
  player          = other.player;
  preflop_buckets = other.preflop_buckets;
  flop_buckets    = other.flop_buckets;

  regret              = new regret_value[leduc::INTERNAL_SEQUENCES];
  average_probability = new regret_value[leduc::INTERNAL_SEQUENCES];
  for(int i=0; i<leduc::INTERNAL_SEQUENCES; ++i) {

    regret[i] = average_probability[i] = NULL;
  }

  for(int i=0; i<leduc::INTERNAL_SEQUENCES; ++i) {
  
    leduc::sequence u(i);
    if (u.whose_turn() == player) {
      
      int n;
      if (u.get_round() == 0) {
	
				n = preflop_buckets;
      } else {

				n = flop_buckets;
      }

      if (n) {
				regret[i]              = new double[n][3];
				average_probability[i] = new double[n][3];
				
				for(int j=0; j<n; ++j) {
					
					for(int k=0; k<3; ++k) {
						
						regret[i][j][k]              = other.regret[i][j][k];
						average_probability[i][j][k] = other.average_probability[i][j][k];
					}
				}
      }
    }
  }
}

void regret_strategy::destroy() {

  if (regret) {

    for(int i=0; i<leduc::INTERNAL_SEQUENCES; ++i) {

      if (regret[i]) {

				delete [] regret[i];
      }
    }

    delete [] regret;
  }

  if (average_probability) {

    for(int i=0; i<leduc::INTERNAL_SEQUENCES; ++i) {
    
      if (average_probability[i]) {
      
				delete [] average_probability[i];
      }
    }

    delete [] average_probability;
  }
}

static void update_regret(leduc::sequence u, int buckets[2][2], int hole[2], int board, 
	int result, double reach[2], double chance, double ev[2], double cfr[2],
	regret_strategy strat[2]) {

  if (u.is_terminal()) {
    
    /* sequence is terminal */
    int amount = u.win_amount();
		
    if (u.is_fold()) {
			
      if (u.who_folded() == 0) {
				
				ev[0] = -amount*reach[1]*chance;
				ev[1] = amount*reach[0]*chance;
      } else {
				
				ev[0] = amount*reach[1]*chance;
				ev[1] = -amount*reach[0]*chance;
      }
			
    } else {
			
      /* sequence is a showdown */
      ev[0] = result*reach[1]*amount*chance;
      ev[1] = -result*reach[0]*amount*chance;
    }
		
  } else if (reach[0] < EPSILON && reach[1] < EPSILON) {
    
    /* cutoff, do nothing */
    ev[0] = ev[1] = 0;
		
  } else {
    
    /* some convience variables */
    int player   = u.whose_turn();
    int opponent = leduc::opposite_player(player);
    int round    = u.get_round();
		
		/* player is using regret minimizing strategy */
		double * average_probability = strat[player].get_average_probability(u, buckets[player][round]);
		double * regret = strat[player].get_regret(u, buckets[player][round]); 
    
		/* get the probabilty tuple for each player */
		double probability[3];
		strat[player].get_probability(u, buckets[player][round], probability);    
    
		/* first average the strategy for the player */
		for(int i=0; i<3; ++i) {
			
			average_probability[i] += reach[player]*probability[i];
		}
      
		/* now compute the regret on each of our actions */
		double expected = 0, sum = 0;
		double old_reach = reach[player];
		double delta_regret[3];
		for(int i=0; i<3; ++i) {
			
			if (u.can_do_action(i)) {
				
				reach[player] = old_reach*probability[i];
				update_regret(u.do_action(i), buckets, hole, board, result, reach, chance, ev, cfr, strat);
        
				delta_regret[i] = ev[player];
				expected += ev[player]*probability[i];
				sum      += ev[opponent];
			}
		}
    
		/* restore reachability value */
		reach[player] = old_reach;
      
		/* subtract off expectation */
		for(int i=0; i<3; ++i) {
			
			if (u.can_do_action(i)) {
				
				delta_regret[i] -= expected;
				regret[i]       += delta_regret[i];
				cfr[player]     += max(0., delta_regret[i]);
			}
		}
    
		/* set return value */
		ev[player]   = expected;
		ev[opponent] = sum;    
	}
}

static void recover_average_strategy(int player, leduc::abstraction& abstraction,
	regret_strategy& regret_strat, 
	leduc::strategy& strat) {

  for(int i=0; i<3; ++i) {
  
    recover_average_strategy(0, player, i, abstraction, regret_strat, strat);
  }
}

static void recover_average_strategy(leduc::sequence u, int player, int hole,
	leduc::abstraction& abstraction,
	regret_strategy& regret_strat,
	leduc::strategy& strat) {

  if (u.is_terminal()) {
    
    /* do nothing at terminal */
  } else if (u.get_round() == 1) {
    
    /* advance the round */
    for(int i=0; i<3; ++i) {

      recover_average_strategy(u, player, hole, i, 
				abstraction, regret_strat, strat);
    }    
  } else {
    
    if (u.whose_turn() == player) {
      
      /* recover the average strategy */
      leduc::player_view view(player, hole, u);
      if (abstraction.can_abstract_view(view)) {

				double probability[3];
				regret_strat.get_normalized_average_probability(u, abstraction.abstract_view(view),
					probability);
				strat.set_strategy(view, probability);
      }
    }

    /* recurse */
    for(int i=0; i<3; ++i) {
      
      if (u.can_do_action(i)) {

				recover_average_strategy(u.do_action(i), player, hole, abstraction,
					regret_strat, strat);
      }
    }
  } 
}

static void recover_average_strategy(leduc::sequence u, int player, int hole, int board,
	leduc::abstraction& abstraction,
	regret_strategy& regret_strat, 
	leduc::strategy& strat) {

  if (u.is_terminal()) {
    
    /* do nothing at terminal */
  } else {
    
    if (u.whose_turn() == player) {
      
      /* recover the average strategy */
      leduc::player_view view(player, hole, board, u);
      if (abstraction.can_abstract_view(view)) {

				double probability[3];
				regret_strat.get_normalized_average_probability(u, abstraction.abstract_view(view),
					probability);
				strat.set_strategy(view, probability);
      }
    }

    /* recurse */
    for(int i=0; i<3; ++i) {
      
      if (u.can_do_action(i)) {

				recover_average_strategy(u.do_action(i), player, hole, board, abstraction,
					regret_strat, strat);
      }
    }
  } 
}

