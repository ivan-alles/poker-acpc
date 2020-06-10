This directory contains scripts to verify FO2 with prkserver. The idea is like this:
1. Play a session with two instances of FO2 in PokerAkademy and record the log
2. Convert the log to pkrserver format (Log1)
3. Play a session with deals from the log in pkrserver and write the log (Log2)
4. Compare Log1 and Log2.

At the moment the scripts and the documentation are not fully intact because 
they were not fully updated for newer versions of pkr libraries. But it is enough 
to get the idea and do the verification. I'll invest a minimal effort to make it running, 
this should be enough for now.


Verifying fellomen2 with Poker Akademy (the strategy must be installed).

1. Add the bot to PA. 
- Copy the the JAR and the *.pd file to PA\data\bots.
- Open PA
- Open Opponent manager
- Clone FellOmen_2, create 2 bots: Fo21 and Fo22
- Close PA

2. Set up MeerkatVerifierBot
- Open {user}\Application Data\PokerAcademyPro2\logs\players
- Edit Fo1.pd, set RNG_SEED to 1
- Edit Fo2.pd, set RNG_SEED to 2
- For both *.pd, activate DEBUG and set log file if necessary

3. Run the verification session in PA.
- Open PA, create a HE FL 10/20 game with Human, Fo21, Fo22.
- Set human's bankroll to 0
- Run the game
- After playing of some Ks of games stop the playing and export the game log.
- pa.fo2.he.fe.txt (in zip)  contains a log with HE FL game, Fo21(RngSeed=1) vs Fo22(RngSeed=2).

4. Run files:
 he.fl.2-prep.bat 
 he.fl.2-run.bat 
 he.fl.2-verify.bat 
 