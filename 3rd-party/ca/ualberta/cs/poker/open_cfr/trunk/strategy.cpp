/*
 * strategy.cpp
 * author:      Kevin Waugh (waugh@cs.ualberta.ca) 
 * date:        August 24 2008
 * description: a leduc strategy
 */

#include <cassert>
#include <cstdio>
#include <cstring>
#include <string>
#include "game.hpp"
#include "strategy.hpp"

#ifdef  NDEBUG
#undef assert
#define assert(_Expression)     _Expression
#endif

using namespace std;

namespace leduc {

static const double EPSILON = 1e-4;

strategy::strategy() : tuples(NULL) {
}

strategy::strategy(const strategy& other) {

  copy_from(other);
}

strategy::~strategy() {

  destroy();
}

strategy& strategy::operator=(const strategy& other) {

  if (&other != this) {

    copy_from(other);
  }

  return *this;
}

void strategy::load(const char * path) {

  /* loop through comma delimited strings */
  for(const char * p=path; *p;) {

    /* skip initial commas */
    for(; *p == ','; ++p);
    
    if (*p) {
      
      /* we found the start of a file path, find the end */
      const char * q = p;
      for(; *q && *q != ','; ++q);

      /* extract the file path and load the file */
      string file_path(p, q-p);
      load_single(file_path.c_str());

      p = q;
    }
  }
}

void strategy::save(FILE * file) const {
  
  assert(file);

  if (tuples) {
  
    for(int i=0; i<INTERNAL_SEQUENCES; ++i) {

      if (tuples[i]) {
      
	string seq_str = sequence(i).to_string();
	if (sequence(i).get_round() == 0) {

	  for(int j=0; j<3; ++j) {
	    
	    if (tuples[i][j]) {

	      fprintf(file, "%c:%s:", RANK_TO_CHAR[j], seq_str.c_str());
	      for(int k=0; k<NUM_ACTIONS; ++k) {

		fprintf(file, " %.09lf", tuples[i][j][k]);
	      }
	      fprintf(file, "\n");
	    }
	  }
	} else {
	
	  for(int j=0; j<9; ++j) {
	    
	    if (tuples[i][j]) {

	      fprintf(file, "%c%c:%s:", RANK_TO_CHAR[j/3], 
		      RANK_TO_CHAR[j%3], seq_str.c_str());
	      for(int k=0; k<NUM_ACTIONS; ++k) {

		fprintf(file, " %.09lf", tuples[i][j][k]);
	      }
	      fprintf(file, "\n");
	    }
	  }
	}
      }
    }
  }

}

bool strategy::have_strategy(const player_view& view) const {
  
  if (tuples) {
    
    int u = view.get_sequence().get_id();
    if (tuples[u]) {
    
      return tuples[u][view.get_hand()];
    }
  }

  /* fall through => do not have strategy */
  return false;
}

const double * strategy::get_strategy(const player_view& view) const {

  assert(have_strategy(view));

  return tuples[view.get_sequence().get_id()][view.get_hand()];
}

void strategy::set_strategy(const player_view& view, const double * tuple) {

  assert(tuple);

  if (!tuples) {
  
    tuples = new double**[INTERNAL_SEQUENCES];
    for(int i=0; i<INTERNAL_SEQUENCES; ++i) {
    
      tuples[i] = NULL;
    }
  }

  int u = view.get_sequence().get_id();
  if (!tuples[u]) {
    
    int n;
    if (view.get_sequence().get_round() == 0) {
      
      n = 3;
    } else {

      n = 9;
    }
    tuples[u] = new double*[n];
    for(int i=0; i<n; ++i) {

      tuples[u][i] = NULL;
    }
  }
  
  int v = view.get_hand();
  if (!tuples[u][v]) {

    tuples[u][v] = new double[NUM_ACTIONS];
  }

  double sum = 0;
  for(int i=0; i<NUM_ACTIONS; ++i) {

    /* make sure we have 0 probability on illegal actions */
    assert(tuple[i] < EPSILON || view.get_sequence().can_do_action(i));

    if (tuple[i] < 0) {
      tuples[u][v][i] = 0;
    } else {
      tuples[u][v][i] = tuple[i];
      sum += tuple[i];
    }
  }

  /* probabilities must sum to 1 */
  /* assert(sum > 1-EPSILON);
     assert(sum < 1+EPSILON); */
  assert(sum > EPSILON);
  for(int i=0; i<NUM_ACTIONS; ++i) {
    tuples[u][v][i] /= sum;
  }
}

void strategy::average(const strategy& other, double w1, double w2) {
  
  for(int i=0; i<2; ++i) {

    average(other, 0, i, -1, -1, w1, w2);
  }
}

void strategy::copy_from(const strategy& other) {

  destroy();
  
  if (other.tuples) {
    
    tuples = new double**[INTERNAL_SEQUENCES];
    for(int i=0; i<INTERNAL_SEQUENCES; ++i) {
    
      tuples[i] = NULL;
    }

    for(int i=0; i<INTERNAL_SEQUENCES; ++i) {

      if (other.tuples[i]) {
	
	int n;
	if (sequence(i).get_round() == 0) {

	  n = 3;
	} else {

	  n = 9;
	}
	
	tuples[i] = new double*[n];
	for(int j=0; j<n; ++j) {
	
	  tuples[i][j] = NULL;
	}

	for(int j=0; j<n; ++j) {
	
	  if (other.tuples[i][j]) {
	  
	    tuples[i][j] = new double[NUM_ACTIONS];

	    for(int k=0; k<NUM_ACTIONS; ++k) {

	      tuples[i][j][k] = other.tuples[i][j][k];
	    }
	  }
	}
      }
    }
  }
}

void strategy::destroy() {
  
  if (tuples) {

    for(int i=0; i<INTERNAL_SEQUENCES; ++i) {
      
      if (tuples[i]) {

	int n;
	if (sequence(i).get_round() == 0) {
	  
	  n = 3;
	} else {
	  
	  n = 9;
	}
	
	for(int j=0; j<n; ++j) {
	
	  if (tuples[i][j]) {
	  
	    delete [] tuples[i][j];
	  }
	}

	delete [] tuples[i];
      }
    }

    delete [] tuples;
  }
}

void strategy::load_single(const char * path) {

  assert(path);

  FILE * file = fopen(path, "rt");
  assert(file);
  
  double * tuple = new double[NUM_ACTIONS];
  for(char line[1024]; fgets(line, 1024, file);) {

    /* check if this line is a comment */
    if (line[0] != '#') {
      
      /* parse the line */
      char hand[1024], betting_sequence[1024];
      int i;
      assert(sscanf(line, "%[^:]:%[^:]:%n", hand, betting_sequence, &i) >= 2);

      /* parse betting sequence */
      sequence u(betting_sequence);
           
      /* parse hand */
      int hole, board = -1;
      hole = CHAR_TO_RANK[(int)hand[0]];

      if (u.get_round() == 1) {
	board = CHAR_TO_RANK[(int)hand[1]];
	assert(board != -1);
      }
      
      /* create the player view */
      player_view view(u.whose_turn(), hole, board, u);

      /* read in the tuple */
      for(int j=0; j<NUM_ACTIONS; ++j) {

	int k;
	assert(sscanf(line+i, "%lf%n", &tuple[j], &k) >= 1);
	
	i += k;
      }

      /* set tuple in strategy */
      set_strategy(view, tuple);
    }

  }

  delete[] tuple;

  fclose(file);
}

void strategy::average(const strategy& other, sequence u, int player, int hole, int board, 
                       double r1, double r2) {

  if (u.is_terminal()) {

    /* do nothing at terminal */
  } else if (hole == -1) {

    /* deal hole card */
    for(int i=0; i<3; ++i) {

      average(other, u, player, i, -1, r1, r2);
    }
  } else if (u.get_round() == 1 && board == -1) {

    /* deal board */
    for(int i=0; i<3; ++i) {

      average(other, u, player, hole, i, r1, r2);
    }
  } else if (u.whose_turn() == player) {

    /* check that both strategies have an entry */
    player_view view(player, hole, board, u);

    if (!have_strategy(view)) {
      assert(!other.have_strategy(view));
    } else {
      assert(other.have_strategy(view));
      const double * s1 = get_strategy(view), * s2 = other.get_strategy(view);
      double avg[3];

      for(int i=0, n=u.num_actions(); i<3; ++i) {
        
        if (u.can_do_action(i)) {
          
          if (r1+r2 > EPSILON) {
            
            avg[i] = (s1[i]*r1 + s2[i]*r2)/(r1+r2);
          } else {
            
            avg[i] = 1./n;
          }
          
          average(other, u.do_action(i), player, hole, board, r1*s1[i], r2*s2[i]);
        } else {
          
          avg[i] = 0;
        }
      }

      /* set strategy to average */
      set_strategy(view, avg);
    }
  } else {

    /* other player's turn .. just recurse */
    for(int i=0; i<3; ++i) {

      if (u.can_do_action(i)) {

        average(other, u.do_action(i), player, hole, board, r1, r2);
      }
    }
  }

}

};
