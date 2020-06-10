#
# Makefile
# author:      Kevin Waugh (waugh@cs.ualberta.ca)
# date:        August 24 2008
# description: Makefile for leduc test suite
#

CXX            = g++
CXXFLAGS       = -Wall
ifdef DEBUG
  CXXFLAGS    += -g
else
  CXXFLAGS    += -O3
endif
LDLIBS         = -lm
TARGETS        = best_response play train 

.PHONY: all clean

all: $(TARGETS)

clean:
	-rm -f *.o *.d *~ $(TARGETS)

best_response: best_response.o abstraction.o game.o strategy.o


play: play.o game.o strategy.o

train: train.o abstraction.o game.o strategy.o

%: %.o
	$(CXX) $(LDFLAGS) -o $@ $^ $(LDLIBS)

%.o: %.cpp
	$(CXX) -MMD $(CXXFLAGS) -c $< -o $@ 

-include $(wildcard *.d)

