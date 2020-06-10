@echo off 
echo ---------- build-all started ---------- > build-all.log

call build-one bds\scripts %* >> build-all.log 2>&1
call build-one bds\pom-net %* >> build-all.log 2>&1

call build-one lib\utils %*   >> build-all.log 2>&1
call build-one lib\algorithms %* >> build-all.log 2>&1
call build-one lib\kmeans %*     >> build-all.log 2>&1

call build-one bds\tools %*      >> build-all.log 2>&1

call build-one dev\tools %*      >> build-all.log 2>&1

call build-one pkr\metagame %*   >> build-all.log 2>&1
call build-one pkr\metabots %*   >> build-all.log 2>&1
call build-one pkr\metastrategy %* >> build-all.log 2>&1
call build-one pkr\metatools %*    >> build-all.log 2>&1
call build-one pkr\stdpoker %*     >> build-all.log 2>&1
call build-one pkr\ctmcgen %*      >> build-all.log 2>&1
call build-one pkr\fictpl  %*      >> build-all.log 2>&1

call build-one pkr\holdem\gamedef %*       >> build-all.log 2>&1
call build-one pkr\holdem\strategy\core %* >> build-all.log 2>&1
call build-one pkr\holdem\strategy\hs %* >> build-all.log 2>&1
call build-one pkr\holdem\strategy\hssd %* >> build-all.log 2>&1
call build-one pkr\holdem\strategy\hand-value %* >> build-all.log 2>&1
call build-one pkr\holdem\strategy\ca %* >> build-all.log 2>&1

call build-one pkr\bots\patience %* >> build-all.log 2>&1

echo ---------- build-all finished ---------- >> build-all.log

grep "BUILD ERROR" build-all.log
