# Calculate best response for a strategy
# Parameters
# $1: iteration count

./train --abstraction1=J:Q:K:JJ:JQ:JK:QJ:QQ:QK:KJ:KQ:KK --abstraction2=J:Q:K:JJ:JQ:JK:QJ:QQ:QK:KJ:KQ:KK --output1=f1.txt --output2=f2.txt --iterations=$1 --seed-time

./best_response --abstraction=J:Q:K:JJ:JQ:JK:QJ:QQ:QK:KJ:KQ:KK --strategy=f1.txt --player=2 --output=br.txt

./best_response --abstraction=J:Q:K:JJ:JQ:JK:QJ:QQ:QK:KJ:KQ:KK --strategy=f2.txt --player=1 --output=br.txt


