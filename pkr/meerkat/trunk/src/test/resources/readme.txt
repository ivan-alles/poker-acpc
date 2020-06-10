Running meerkat verifier bot.

1. Add the bot to PA. 
- Copy the the JAR and the *.pd file to PA\data\bots.
- Open PA
- Open Opponent manager
- Clone MeerkatVerifierBot, create 2 bots: Mvb1 and Mvb2
- Close PA

2. Set up MeerkatVerifierBot
- Open {user}\Application Data\PokerAcademyPro2\logs\players
- Edit Mvb1.pd, set RNG_SEED to 1
- Edit Mvb2.pd, set RNG_SEED to 2
- For both *.pd, activate DEBUG and set log file if necessary

3. Run the verification session in PA.
- Open PA, create a HE FL 10/20 game with Human, Mvb1, Mvb2.
- Set human's bankroll to 0
- Run the game
- After playing of some Ks of games stop the playing and export the game log.
- pa.mv.he.fe.txt (in zip)  contains a log with HE FL game, Mvb1(RngSeed=1) vs Mvb2(RngSeed=2).

4. Run files:
 he.fl.2-prep.bat 
 he.fl.2-run.bat 
 he.fl.2-verify.bat 

