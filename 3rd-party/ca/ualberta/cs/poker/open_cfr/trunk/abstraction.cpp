/*
 * abstraction.cpp
 * author:      Kevin Waugh (waugh@cs.ualberta.ca)
 * date:        August 24 2008
 * description: abstraction for a leduc player
 */

#include <algorithm>
#include <cassert>
#include <string>
#include <fstream>
#include "game.hpp"
#include "abstraction.hpp"

#ifdef  NDEBUG
#undef assert
#define assert(_Expression)     _Expression
#endif

using namespace std;

namespace leduc {

abstraction::abstraction() {
  
  for(int i=0; i<3; ++i) {

    preflop[i] = i;
  }

  for(int i=0; i<9; ++i) {

    flop[i] = i;
  }
}

abstraction::abstraction(const char * abstraction_str) {

  assert(abstraction_str);
	

  /* initialize abstraction to the null abstraction */
  for(int i=0; i<3; ++i) {

    preflop[i] = -1;
  }
  for(int i=0; i<9; ++i) {

    flop[i] = -1;
  }

	/* Check to see if the abstraction string is a text file */
  string line;
  ifstream file (abstraction_str);
	const char * str;
  if (file.is_open()) {
		getline(file,line);			
		printf("Abstraction from file is = %s\n",line.c_str());
    str = line.c_str();
    file.close();
	}  else  {
		str = abstraction_str;
	}

	assert(str);
	
  /* parse the abstraction string */
  /*
   * format is HAND,HAND,HAND:HAND,HAND,HAND
   * i.e. groups of hands are colon delimeted and each
   *      hand within a group is comma delimited.
   * groups of hands must be the same type (preflop/flop) and
   * all hands in a group will be put in the same bucket
   * preflop hands are J, Q, K
   * flop hands are JJ, JQ, JK, QJ, QQ, QK, KJ, KQ, KK where the first
   * card denotes the hole card
   */
  int next_preflop = 0, next_flop = 0;
  while(*str) {
    
    /* skip colons */
    for(; *str == ':'; ++str);

    if (!*str) {

      break;
    }

    assert(CHAR_TO_RANK[(int)*str] != -1);
    
    /* is this a preflop bucket or a flop bucket? */
    if (CHAR_TO_RANK[(int)*(str+1)] == -1) {
      
      /* preflop */
      const char * q = str;
      int bucket = next_preflop++;
      for(;;) {
	
	/* get the next card in this bucket */
	int hole = CHAR_TO_RANK[(int)*q];
	assert(hole != -1);

	/* make sure we haven't abstracted this card yet */
	assert(preflop[hole] == -1);
	preflop[hole] = bucket;
      	
	/* check if this is last card in bucket */
	if (*(q+1) == 0 || *(q+1) == ':') {

	  /* last card, break out */
	  ++q;
	  break;
	} else {
	
	  /* not last card, go to next one */
	  assert(*(q+1) == ',');
	  q += 2;
	}
      }
      
      str = q;
    } else {
    
      /* flop */
      const char * q = str;
      int bucket = next_flop++;
      for(;;) {

	/* get the next hole and board cards in this bucket */
	int hole = CHAR_TO_RANK[(int)*q];
	int board = CHAR_TO_RANK[(int)*(q+1)];
	assert(hole != -1);
	assert(board != -1);
	int hand = 3*hole + board;
	
	/* make sure we haven't abstracted this hand yet */
	assert(flop[hand] == -1);
	flop[hand] = bucket;

	/* check if this is the last hand in bucket */
	if (*(q+2) == 0 || *(q+2) == ':') {

	  /* last hand, break out */
	  q += 2;
	  break;
	} else {

	  /* not last hand, go to next one */
	  assert(*(q+2) == ',');
	  q += 3;
	}
      }

      str = q;
    }
  }
}

bool abstraction::can_abstract_view(const player_view& view) const {

  if (view.get_board() == -1) {

    return preflop[view.get_hand()] >= 0;
  } else {

    return flop[view.get_hand()] >= 0;
  }
}

int abstraction::abstract_view(const player_view& view) const {

  assert(can_abstract_view(view));

  if (view.get_board() == -1) {
   
    return preflop[view.get_hand()];
  } else {
    
    return flop[view.get_hand()];
  }
}

int abstraction::preflop_buckets() const {

  int highest = -1;
  for(int i=0; i<3; ++i) {
    
    highest = max(highest, preflop[i]);
  }
  
  return highest + 1;
}

int abstraction::flop_buckets() const {

  int highest = -1;
  for(int i=0; i<9; ++i) {
    
    highest = max(highest, flop[i]);
  }

  return highest + 1;
}

bool abstraction::is_perfect_recall() const {

  int seen_postflop[9] = {-1, -1, -1,
                          -1, -1, -1,
                          -1, -1, -1};

  /* loop over all preflop cards and abstract them */
  for(int i=0; i<3; ++i) {

    /* can we abstract this preflop view? */
    player_view pf_view(0, i);
    if (can_abstract_view(pf_view)) {
      
      /* now loop over all flop extensions of this preflop */
      int pf_bucket = abstract_view(pf_view);     
      for(int j=0; j<3; ++j) {

        /* can we abstract this flop extension? */
        player_view f_view(0, i, j);                
        if (can_abstract_view(f_view)) {

          int f_bucket = abstract_view(f_view);
          if (seen_postflop[f_bucket] == -1) {
            
            /* we have not seen this flop bucket before, mark our preflop bucket */
            seen_postflop[f_bucket] = pf_bucket;
          } else if (seen_postflop[f_bucket] != pf_bucket) {

            /* we have seen this flop bucket before, but from a different preflop bucket */
            return false;
          }
        }
      }
    }
  }

  return true;
}

bool abstraction::can_extend(int pre, int post) const {

  assert(pre >= 0);
  assert(pre < preflop_buckets());
  assert(post >= 0);
  assert(post < flop_buckets());
  
  for(int i=0; i<3; ++i) {

    /* check if this card abstracts into the desired preflop bucket */
    player_view pf_view(0, i);
    if (can_abstract_view(pf_view) && abstract_view(pf_view) == pre) {
      
      for(int j=0; j<3; ++j) {

        /* check if this extension abstracts to the desired flop bucket */
        player_view f_view(0, i, j);
        if (can_abstract_view(f_view) && abstract_view(f_view) == post) {

          return true;
        }
      }
    }
  }

  /* couldn't find an extension */
  return false;
}

bool abstraction::is_complete() const {

  /* check preflop hands */
  for(int i=0; i<3; ++i) {

    if (preflop[i] == -1) {

      return false;
    }
  }

  /* check flop hands */
  for(int i=0; i<9; ++i) {

    if (flop[i] == -1) {

      return false;
    }
  }

  return true;
}

};

