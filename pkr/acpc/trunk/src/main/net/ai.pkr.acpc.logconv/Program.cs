/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ai.pkr.acpc.logconv
{
    class Program
    {
/* ACPC log example.
 
STATE:0:rrc/rc/rrc/cc:5d5c|9hQd/8dAs8s/4h/6d:80|-80:P1|P2
STATE:1:rrc/rf:6sKs|5dJd/2sTh2h:30|-30:P2|P1
STATE:2:f:Ah8s|2h4c:5|-5:P1|P2
STATE:3:rc/cc/crf:4s8s|6hJd/QhJs9d/Qc:-20|20:P2|P1
STATE:4:rc/crf:5s4c|Kc7d/KsAsJd:-20|20:P1|P2
STATE:5:rrc/rc/rc/rc:KhTh|8s7d/9c8d5c/Qd/Td:80|-80:P2|P1
STATE:6:rrc/rc/crf:Kc8h|5cAs/9dAh4d/7h:-40|40:P1|P2
 
*/
        static int Main(string[] args)
        {
            Regex rePlayers = new Regex("^.*\\:([^|]+)\\|([^|]+)$");
            Regex rePreFlop = new Regex("^STATE\\:([0-9]+)\\:[^\\:]*\\:(..)(..)\\|(..)(..)");
            Regex reFlop = new Regex("^/(..)(..)(..)");
            Regex rePostFlop = new Regex("^/(..)");

            for(;;)
            {
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }

                //string line = "STATE:0:rrc/rc/rrc/cc:5d5c|9hQd/8dAs8s/4h/6d:80|-80:P1|P2";

                Match mp = rePlayers.Match(line);
                if(!mp.Success)
                {
                    Console.WriteLine("Wrong player names in: {0}", line);
                    return 1;
                }
 
                Match m;
                m = rePreFlop.Match(line);
                if(!m.Success)
                {
                    Console.WriteLine("Wrong flop in: {0}", line);
                    return 1;
                }

                string gs = String.Format(
                    "{0}; {1}{{0 0.5 0}} {2}{{0 1 0}}; 0d{{{3} {4}}} 1d{{{5} {6}}}",
                    m.Groups[1].Value,
                    mp.Groups[2].Value, mp.Groups[1].Value,
                    m.Groups[4].Value, m.Groups[5].Value,
                    m.Groups[2].Value, m.Groups[3].Value);

                string l = line.Substring(m.Length);
                m = reFlop.Match(l);
                if (m.Success)
                {
                    gs += String.Format(" d{{{0} {1} {2}}}", m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value);
                    l = l.Substring(m.Length);
                    m = rePostFlop.Match(l);
                    if (m.Success)
                    {
                        gs += String.Format(" d{{{0}}}", m.Groups[1].Value);
                        l = l.Substring(m.Length);
                        m = rePostFlop.Match(l);
                        if (m.Success)
                        {
                            gs += String.Format(" d{{{0}}}", m.Groups[1].Value);
                        }
                    }
                }
                gs += ".";
                Console.WriteLine(gs);
            }
            return 0;
        }
    }
}
