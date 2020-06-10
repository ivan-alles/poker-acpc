/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ai.pkr.metagame;
using NUnit.Framework;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using ai.lib.algorithms;
using ai.pkr.stdpoker;
using ai.pkr.metastrategy;


namespace ai.pkr.research
{
    /// <summary>
    /// Calculates Sklansky-Chubukov ratings. 
    /// </summary>
    /// <remarks>Very long running (> 1 hour).</remarks>
    [TestFixture]
    [Explicit]
    public class Sklansky_Chubukov_Test
    {
        [Serializable]
        class Pocket
        {
            public readonly int _c1;
            public readonly int _c2;
            public readonly string _text;

            public Dictionary<CardSet, double> _evDict = new Dictionary<CardSet, double>(1225);

            public Pocket(string s)
            {
                if (s.Length != 2 && s.Length != 3)
                    throw new ArgumentException();

                _text = s;

                int r1 = StdDeck.RankFromChar(s[0]);
                int r2 = StdDeck.RankFromChar(s[1]);

                _c1 = StdDeck.IndexFromRankAndSuit(r1, (int)StdDeck.Suit.Clubs);

                if (s.Length == 2)
                {
                    _c2 = StdDeck.IndexFromRankAndSuit(r2, (int)StdDeck.Suit.Diamonds);
                }
                else if (s[2] == 's')
                {
                    _c2 = StdDeck.IndexFromRankAndSuit(r2, (int)StdDeck.Suit.Clubs);
                }
                else if (s[2] == 'o')
                {
                    _c2 = StdDeck.IndexFromRankAndSuit(r2, (int)StdDeck.Suit.Diamonds);
                }
                else
                    throw new ArgumentException();
            }
        }

        static readonly int _boardsCount = (int)EnumAlgos.CountCombin(48, 5);


        class Job
        {
            public Job(Pocket[] pockets)
            {
                _pockets = pockets;
            }

            void Evaluate(ref CardSet board)
            {
                CardSet h1 = new CardSet(board, _mPocket);
                CardSet h2 = new CardSet(board, _mOther);
                UInt32 v1 = CardSetEvaluator.Evaluate(ref h1);
                UInt32 v2 = CardSetEvaluator.Evaluate(ref h2);
                if (v1 > v2) _evCount += 2;
                if (v1 == v2) _evCount += 1;
            }

            public void Start()
            {
                _thread = new Thread(Calculate);
                _thread.Start();
            }

            public void Wait()
            {
                _thread.Join();
            }

            void Calculate()
            {
                foreach (Pocket p in _pockets)
                {
                    _mPocket = new CardSet(
                        StdDeck.Descriptor.CardSets[(int)p._c1],
                        StdDeck.Descriptor.CardSets[(int)p._c2]);
                    List<double> evList = new List<double>(1225);
                    for (int c1 = StdDeck.Descriptor.Size - 1; c1 >= 0; --c1)
                    {
                        CardSet m1 = StdDeck.Descriptor.CardSets[c1];
                        if (m1.IsIntersectingWith(_mPocket)) continue;
                        for (int c2 = c1 - 1; c2 >= 0; --c2)
                        {
                            CardSet m2 = new CardSet(m1, StdDeck.Descriptor.CardSets[c2]);
                            if (m2.IsIntersectingWith(_mPocket)) continue;
                            _evCount = 0;
                            _mOther = m2;
                            CardEnum.Combin(StdDeck.Descriptor, 5, CardSet.Empty, _mPocket | m2, Evaluate);
                            double ev = 0.5 * _evCount / _boardsCount;
                            p._evDict.Add(m2, ev);
                        }
                    }

                }
            }
            public Pocket[] _pockets;
            private int _evCount;
            CardSet _mPocket, _mOther;
            Thread _thread;
        }


        [Test]
        public void CalculatePairVsPair()
        {
            Pocket[] pockets = new Pocket[_scOriginals.Length];
            for (int i = 0; i < pockets.Length; ++i)
            {
                pockets[i] = new Pocket(_scOriginals[i]._pocket);
            }

            DateTime startTime = DateTime.Now;
            int jobsCount = Environment.ProcessorCount;
            Job[] jobs = new Job[jobsCount];
            int tasksCount = pockets.Length;
            int sliceSize = (tasksCount + jobsCount - 1) / jobsCount;
            for (int j = 0; j < jobsCount; j++)
            {
                int s = sliceSize * j;
                int l = Math.Min(sliceSize, tasksCount - s);
                Job job = new Job(pockets.Slice(s, l));
                job.Start();
                jobs[j] = job;
            }
            for (int j = 0; j < jobsCount; j++)
            {
                jobs[j].Wait();
            }
            TimeSpan time = DateTime.Now - startTime;
            Console.WriteLine("Calculated in {0} seconds", time.TotalSeconds);
            using (Stream file = File.Open("PocketResult.bin", FileMode.Create))
            {
                BinaryFormatter binFormatter = new BinaryFormatter();
                binFormatter.Serialize(file, pockets);
                file.Close();
            }
        }

        static class NonStopAssert
        {
            public static int Count = 0;
            public static void Reset()
            {
                Count = 0;
            }

            public static void AreEqual(int exp, int act)
            {
                if (exp != act)
                {
                    Console.WriteLine("ERROR: values are not equal: {0} {1}", exp, act);
                    Count++;
                }
            }

            public static void AreEqual(double exp, double act, double delta)
            {
                if (Math.Abs(exp - act) > delta)
                {
                    Console.WriteLine("ERROR: values are not equal: {0} {1}", exp, act);
                    Count++;
                }
            }
        }

        class MaskEvComparer: IComparer<CardSet>
        {
            private Dictionary<CardSet, double> _evDict;
            public MaskEvComparer(Dictionary<CardSet, double> evDict)
            {
                _evDict = evDict;
            }

            #region IComparer<CardSet> Members

            public int Compare(CardSet x, CardSet y)
            {
                double vx = _evDict[x];
                double vy = _evDict[y];
                if (vx < vy) return -1;
                if (vx > vy) return 1;
                return 0;
            }

            #endregion
        }

        [Test]
        public void CalculateRatings()
        {
            NonStopAssert.Reset();
            Pocket[] pockets;
            using (Stream file = File.Open("PocketResult.bin", FileMode.Open))
            {
                BinaryFormatter binFormatter = new BinaryFormatter();
                pockets = (Pocket[])binFormatter.Deserialize(file);
                file.Close();
            }

            foreach (Pocket p in pockets)
            {
                List<CardSet> evList = new List<CardSet>(p._evDict.Keys);
                evList.Sort(new MaskEvComparer(p._evDict));
                Double scNumber = Double.PositiveInfinity;
                Double sumEv = 0;
                int i;
                int callCount;

                for (i = 0; i < evList.Count; ++i)
                {
                    double ev = p._evDict[evList[i]];
                    sumEv += ev;
                    callCount = i + 1;
                    int foldCount = 1225 - callCount;
                    double divisor = (double)callCount - 2 * sumEv;
                    double curSc = divisor == 0.0 ? Double.PositiveInfinity : (2 * sumEv + 3 * foldCount) / divisor;
                    if (curSc > scNumber || curSc < 0 /*Special case for AA*/)
                    {
                        sumEv -= ev;
                        break;
                    }
                    scNumber = curSc;
                }
                callCount = i;
                double winPerc = sumEv / callCount;
                Console.WriteLine("{0} call {1} win% {2} sc {3}", p._text, callCount, winPerc, scNumber);

                SCOriginal scOriginal = null;
                for(i = 0; i < _scOriginals.Length; ++i)
                {
                    if (_scOriginals[i]._pocket == p._text)
                    {
                        scOriginal = _scOriginals[i];
                        break;
                    }
                }

                Assert.IsNotNull(scOriginal);
                NonStopAssert.AreEqual(scOriginal._callCount, callCount);
                NonStopAssert.AreEqual(scOriginal._winPerc, winPerc, EPSILON);
                NonStopAssert.AreEqual(scOriginal._scNumber, scNumber, EPSILON);
            }
            if (NonStopAssert.Count != 0)
                Assert.Fail("There were deviations from published SC numbers");
        }

        private const double EPSILON = 0.000001;


        class SCOriginal
        {
            public string _pocket;
            public int _callCount;
            public double _winPerc;
            public double _scNumber;
            public SCOriginal(string pocket, int callCount, double winPerc, double scNumber)
            {
                _pocket = pocket;
                _callCount = callCount;
                _winPerc = winPerc;
                _scNumber = scNumber;
            }
        }

        SCOriginal[] _scOriginals = new SCOriginal[]
        {
#if true // Test data
            new SCOriginal("AA", 1, 0.500000, Double.PositiveInfinity),
            new SCOriginal("KK", 7, 0.226177, 953.995465),
            new SCOriginal("AKs", 75, 0.457697, 554.509992),
            new SCOriginal("QQ", 13, 0.207007, 478.008197),
#else     // All data
            new SCOriginal("AA", 1, 0.500000, Double.PositiveInfinity),
            new SCOriginal("KK", 7, 0.226177, 953.995465),
            new SCOriginal("AKs", 75, 0.457697, 554.509992),
            new SCOriginal("QQ", 13, 0.207007, 478.008197),
            new SCOriginal("AKo", 79, 0.433132, 331.887184),
            new SCOriginal("JJ", 19, 0.201104, 319.213589),
            new SCOriginal("AQs", 84, 0.424149, 274.211191),
            new SCOriginal("TT", 25, 0.198947, 239.821017),
            new SCOriginal("AQo", 93, 0.403144, 192.670217),
            new SCOriginal("99", 31, 0.197142, 191.413933),
            new SCOriginal("AJs", 96, 0.401528, 183.221336),
            new SCOriginal("88", 41, 0.226651, 159.296894),
            new SCOriginal("ATs", 108, 0.385544, 138.913083),
            new SCOriginal("AJo", 105, 0.379834, 136.310470),
            new SCOriginal("77", 61, 0.285621, 134.847705),
            new SCOriginal("66", 103, 0.355264, 115.348532),
            new SCOriginal("ATo", 117, 0.362908, 106.264712),
            new SCOriginal("A9s", 123, 0.367405, 104.124788),
            new SCOriginal("55", 153, 0.389493, 98.629873),
            new SCOriginal("A8s", 135, 0.361211, 89.865649),
            new SCOriginal("KQs", 256, 0.429500, 86.627695),
            new SCOriginal("44", 275, 0.431528, 81.979590),
            new SCOriginal("A9o", 129, 0.339884, 81.716196),
            new SCOriginal("A7s", 147, 0.356565, 79.175905),
            new SCOriginal("KJs", 265, 0.419399, 72.621257),
            new SCOriginal("A5s", 171, 0.367031, 72.292128),
            new SCOriginal("A8o", 141, 0.332789, 70.956513),
            new SCOriginal("A6s", 159, 0.352858, 70.744533),
            new SCOriginal("A4s", 183, 0.366358, 66.650529),
            new SCOriginal("33", 455, 0.454268, 65.440821),
            new SCOriginal("KTs", 277, 0.411707, 62.805558),
            new SCOriginal("A7o", 155, 0.329722, 62.747746),
            new SCOriginal("A3s", 195, 0.366882, 62.275315),
            new SCOriginal("KQo", 265, 0.400723, 58.771664),
            new SCOriginal("A2s", 207, 0.366815, 58.141993),
            new SCOriginal("A5o", 181, 0.340952, 56.542087),
            new SCOriginal("A6o", 171, 0.329477, 56.151230),
            new SCOriginal("A4o", 202, 0.347061, 51.939490),
            new SCOriginal("KJo", 277, 0.391325, 50.838788),
            new SCOriginal("QJs", 418, 0.432774, 49.515440),
            new SCOriginal("A3o", 220, 0.351305, 48.445438),
            new SCOriginal("22", 709, 0.467553, 48.054119),
            new SCOriginal("K9s", 295, 0.392879, 47.812358),
            new SCOriginal("A2o", 240, 0.355839, 45.172344),
            new SCOriginal("KTo", 289, 0.383383, 44.946538),
            new SCOriginal("QTs", 430, 0.426952, 43.809464),
            new SCOriginal("K8s", 307, 0.378141, 39.910810),
            new SCOriginal("K7s", 325, 0.378587, 37.330652),
            new SCOriginal("JTs", 570, 0.440073, 36.106522),
            new SCOriginal("K9o", 301, 0.361114, 35.754152),
            new SCOriginal("K6s", 337, 0.375940, 34.890001),
            new SCOriginal("QJo", 433, 0.404082, 32.816822),
            new SCOriginal("Q9s", 457, 0.409880, 32.519706),
            new SCOriginal("K5s", 349, 0.371933, 32.303331),
            new SCOriginal("K8o", 324, 0.351582, 30.473887),
            new SCOriginal("K4s", 367, 0.371425, 30.163283),
            new SCOriginal("QTo", 445, 0.398126, 29.716401),
            new SCOriginal("K7o", 344, 0.353033, 28.541184),
            new SCOriginal("K3s", 379, 0.369025, 28.381805),
            new SCOriginal("K2s", 394, 0.367883, 26.730843),
            new SCOriginal("Q8s", 469, 0.394731, 26.718552),
            new SCOriginal("K6o", 368, 0.355714, 26.675708),
            new SCOriginal("J9s", 597, 0.422213, 25.712524),
            new SCOriginal("K5o", 408, 0.363569, 24.680974),
            new SCOriginal("Q9o", 459, 0.377014, 23.419539),
            new SCOriginal("JTo", 585, 0.411106, 23.085252),
            new SCOriginal("K4o", 458, 0.373684, 22.845021),
            new SCOriginal("Q7s", 484, 0.381931, 22.685237),
            new SCOriginal("T9s", 721, 0.434081, 22.491482),
            new SCOriginal("Q6s", 499, 0.382276, 21.785164),
            new SCOriginal("K3o", 508, 0.383123, 21.392219),
            new SCOriginal("J8s", 609, 0.406766, 20.636243),
            new SCOriginal("Q5s", 514, 0.379236, 20.321860),
            new SCOriginal("K2o", 555, 0.389958, 19.999415),
            new SCOriginal("Q8o", 479, 0.363775, 19.819326),
            new SCOriginal("Q4s", 547, 0.381543, 18.916352),
            new SCOriginal("J9o", 597, 0.389470, 17.799380),
            new SCOriginal("Q3s", 568, 0.380696, 17.734011),
            new SCOriginal("T8s", 733, 0.418399, 17.465705),
            new SCOriginal("J7s", 624, 0.393116, 17.194521),
            new SCOriginal("Q7o", 520, 0.359844, 17.077335),
            new SCOriginal("Q2s", 591, 0.380441, 16.641032),
            new SCOriginal("Q6o", 566, 0.370110, 16.295139),
            new SCOriginal("98s", 841, 0.427277, 15.293343),
            new SCOriginal("Q5o", 652, 0.386607, 15.034981),
            new SCOriginal("J8o", 613, 0.374112, 14.867761),
            new SCOriginal("T9o", 721, 0.402190, 14.832206),
            new SCOriginal("J6s", 648, 0.383218, 14.718597),
            new SCOriginal("T7s", 748, 0.404171, 14.199426),
            new SCOriginal("J5s", 686, 0.388455, 14.048416),
            new SCOriginal("Q4o", 748, 0.400659, 13.662167),
            new SCOriginal("J4s", 751, 0.396332, 12.955471),
            new SCOriginal("J7o", 657, 0.368521, 12.666038),
            new SCOriginal("Q3o", 857, 0.415272, 12.503232),
            new SCOriginal("97s", 853, 0.412903, 12.251417),
            new SCOriginal("T8o", 733, 0.385474, 12.156984),
            new SCOriginal("J3s", 792, 0.398770, 12.040344),
            new SCOriginal("T6s", 767, 0.391983, 11.921088),
            new SCOriginal("Q2o", 975, 0.428097, 11.302950),
            new SCOriginal("J2s", 891, 0.412488, 11.138727),
            new SCOriginal("87s", 945, 0.422015, 11.110552),
            new SCOriginal("J6o", 755, 0.378294, 10.780675),
            new SCOriginal("98o", 841, 0.394874, 10.271257),
            new SCOriginal("T7o", 765, 0.374878, 10.204755),
            new SCOriginal("96s", 878, 0.401527, 10.097673),
            new SCOriginal("J5o", 855, 0.395413, 9.987293),
            new SCOriginal("T5s", 886, 0.401897, 9.946900),
            new SCOriginal("T4s", 949, 0.408748, 9.260066),
            new SCOriginal("86s", 969, 0.410324, 8.994746),
            new SCOriginal("J4o", 947, 0.405076, 8.906238),
            new SCOriginal("T6o", 877, 0.385581, 8.571955),
            new SCOriginal("97o", 873, 0.384566, 8.570963),
            new SCOriginal("T3s", 1026, 0.415998, 8.415718),
            new SCOriginal("76s", 1045, 0.418616, 8.318417),
            new SCOriginal("95s", 970, 0.403431, 8.261043),
            new SCOriginal("J3o", 1047, 0.415307, 7.914721),
            new SCOriginal("T2s", 1123, 0.425488, 7.538836),
            new SCOriginal("87o", 976, 0.396225, 7.505732),
            new SCOriginal("85s", 1039, 0.406723, 7.239171),
            new SCOriginal("96o", 987, 0.393276, 7.074151),
            new SCOriginal("T5o", 1003, 0.394962, 6.920957),
            new SCOriginal("J2o", 1129, 0.420420, 6.885765),
            new SCOriginal("75s", 1115, 0.414674, 6.594160),
            new SCOriginal("94s", 1063, 0.403925, 6.583641),
            new SCOriginal("T4o", 1097, 0.406874, 6.248512),
            new SCOriginal("65s", 1159, 0.418775, 6.207388),
            new SCOriginal("86o", 1087, 0.402754, 6.099835),
            new SCOriginal("93s", 1121, 0.409454, 6.058991),
            new SCOriginal("84s", 1145, 0.409633, 5.692773),
            new SCOriginal("95o", 1133, 0.406508, 5.650827),
            new SCOriginal("T3o", 1145, 0.406672, 5.480421),
            new SCOriginal("76o", 1164, 0.410142, 5.439126),
            new SCOriginal("92s", 1153, 0.406646, 5.359298),
            new SCOriginal("74s", 1198, 0.412623, 5.109201),
            new SCOriginal("54s", 1225, 0.414534, 4.850294),
            new SCOriginal("T2o", 1149, 0.397258, 4.832254),
            new SCOriginal("85o", 1197, 0.407938, 4.812230),
            new SCOriginal("64s", 1225, 0.413333, 4.769221),
            new SCOriginal("83s", 1201, 0.403003, 4.463809),
            new SCOriginal("94o", 1201, 0.400861, 4.345783),
            new SCOriginal("75o", 1225, 0.405120, 4.269797),
            new SCOriginal("82s", 1207, 0.398164, 4.129509),
            new SCOriginal("73s", 1225, 0.400359, 4.018033),
            new SCOriginal("93o", 1200, 0.393756, 4.000304),
            new SCOriginal("65o", 1225, 0.399443, 3.972305),
            new SCOriginal("53s", 1225, 0.396930, 3.851054),
            new SCOriginal("63s", 1225, 0.395336, 3.777173),
            new SCOriginal("84o", 1225, 0.394468, 3.737896),
            new SCOriginal("92o", 1215, 0.388261, 3.585219),
            new SCOriginal("43s", 1225, 0.386419, 3.402163),
            new SCOriginal("74o", 1225, 0.385498, 3.366747),
            new SCOriginal("72s", 1225, 0.381559, 3.221509),
            new SCOriginal("54o", 1225, 0.381553, 3.221293),
            new SCOriginal("64o", 1225, 0.380105, 3.170312),
            new SCOriginal("52s", 1225, 0.378493, 3.114999),
            new SCOriginal("62s", 1225, 0.376690, 3.054809),
            new SCOriginal("83o", 1225, 0.374838, 2.994827),
            new SCOriginal("42s", 1225, 0.368290, 2.796223),
            new SCOriginal("82o", 1225, 0.368277, 2.795837),
            new SCOriginal("73o", 1225, 0.366023, 2.731972),
            new SCOriginal("53o", 1225, 0.362648, 2.640274),
            new SCOriginal("63o", 1225, 0.360776, 2.591343),
            new SCOriginal("32s", 1225, 0.359844, 2.567461),
            new SCOriginal("43o", 1225, 0.351459, 2.366073),
            new SCOriginal("72o", 1225, 0.345836, 2.243309),
            new SCOriginal("52o", 1225, 0.342846, 2.181602),
            new SCOriginal("62o", 1225, 0.340751, 2.139745),
            new SCOriginal("42o", 1225, 0.331998, 1.976146),
            new SCOriginal("32o", 1225, 0.323032, 1.825374)
#endif
        };
    };


}
