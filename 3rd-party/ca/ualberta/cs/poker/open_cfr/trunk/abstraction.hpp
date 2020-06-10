/*
 * abstraction.hpp
 * author:      Kevin Waugh (waugh@cs.ualberta.ca)
 * date:        August 24 2008
 * description: abstraction for a leduc player
 */
#ifndef _ABSTRACTION_HPP_
#define _ABSTRACTION_HPP_

#include "game.hpp"

namespace leduc {

class abstraction {
public:
  /* create the full game abstraction */
  abstraction();
  
  /* create an abstraction from a string */
  abstraction(const char * str);
  
  /* can we abstract a preflop hand? */
  bool can_abstract_view(const player_view& view) const;

  /* abstract a preflop hand */
  int abstract_view(const player_view& view) const;

  /* how many preflop buckets? */
  int preflop_buckets() const;

  /* how many flop buckets? */
  int flop_buckets() const;

  /* is an abstraction perfect recall? */
  bool is_perfect_recall() const;

  /* can we extend a bucket sequence from a preflop bucket to a postflop bucket? */
  bool can_extend(int pre, int post) const;

  /* is an abstraction complete? */
  bool is_complete() const;

private:
  int preflop[3], flop[9];
};

};

#endif /* _ABSTRACTION_HPP_ */
