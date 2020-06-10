/*
 * strategy.hpp
 * author:      Kevin Waugh (waugh@cs.ualberta.ca) 
 * date:        August 24 2008
 * description: a leduc strategy
 */
#ifndef _STRATEGY_HPP_
#define _STRATEGY_HPP_

#include <cstdio>
#include "game.hpp"

namespace leduc {
  
class strategy {
public:
  /* create the null strategy */
  strategy();

  /* copy constructor */
  strategy(const strategy& other);

  /* destructor */
  ~strategy();

  /* assignment operator */
  strategy& operator=(const strategy& other);

  /* load in a strategy from disk */
  void load(const char * path);
  
  /* save a strategy to a FILE */
  void save(FILE * file) const;

  /* does strategy handle a situation? */
  bool have_strategy(const player_view& view) const;

  /* get the strategy for a situation */
  const double * get_strategy(const player_view& view) const;

  /* set the strategy for a situation */
  void set_strategy(const player_view& view, const double * tuple);

  /* average with another strategy */
  void average(const strategy& other, double w1, double w2);

private:
  /* copy strategy from another */
  void copy_from(const strategy& other);

  /* destroy this strategy */
  void destroy();

  /* load a single strategy */
  void load_single(const char * path);

  /* average with another strategy */
  void average(const strategy& other, sequence u, int player, int hole, int board, 
               double r1, double r2);

private:
  double *** tuples; /* the strategy */
};

};

#endif /* _STRATEGY_HPP_ */
