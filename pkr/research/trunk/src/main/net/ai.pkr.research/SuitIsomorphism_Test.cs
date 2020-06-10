/* Copyright 2010-2012 Ivan Alles.
   Licensed under the MIT License (see file LICENSE). */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Diagnostics;
using ai.pkr.stdpoker;
using ai.pkr.metagame;
using ai.lib.algorithms;


namespace ai.pkr.metastrategy.nunit
{
    /// <summary>
    /// Calculates number of nodes in some chance trees.
    /// </summary>
    [TestFixture]
    public class SuitIsomorphism_Test
    {
        NormSuit _sn = new NormSuit();
        NormSuit _sn2 = new NormSuit();
        NormSuit _snFlop = new NormSuit();


        int _pocketCount = 0;
        private CardSet _normPocket;

        private CardSet _maxNormPreflopPocket = new CardSet();
        private int _normPreflopCount = 0;

        private CardSet _maxNormFlopPocket = new CardSet();
        private int _normFlopCount = 0;

        [Test]
        public void Holdem()
        {
            Reset();
            Console.WriteLine("Calculate number of chance nodes from 1 player perspective for Holdem");

            CardEnum.Combin(StdDeck.Descriptor, 2, CardSet.Empty, CardSet.Empty, SuitIsomorphismPreflopHoldem);
            Console.WriteLine("Preflop Chance Nodes: colored: {0}, color-isomorphic: {1} {2:0.##}%",
                _pocketCount, _normPreflopCount, 100.0 * _normPreflopCount / _pocketCount);
            int flopCount = (int)(EnumAlgos.CountCombin(52, 2)*EnumAlgos.CountCombin(50, 3));
            Console.WriteLine("Flop Chance Nodes: colored: {0}, color-isomorphic: {1} {2:0.##}%",
                              flopCount, _normFlopCount, 100.0 * _normFlopCount / flopCount);
            Assert.AreEqual(EnumAlgos.CountCombin(52, 2), _pocketCount);
            Assert.AreEqual(169, _normPreflopCount);
            // Value calculated by this test by many different implementations.
            Assert.AreEqual(1348620, _normFlopCount);
        }

        [Test]
        public void Omaha()
        {
            Reset();
            Console.WriteLine("Calculate number of chance nodes from 1 player perspective for Omaha");

            CardEnum.Combin(StdDeck.Descriptor, 4, CardSet.Empty, CardSet.Empty, SuitIsomorphismPreflopOmaha);
            Console.WriteLine("Preflop Chance Nodes: colored: {0}, color-isomorphic: {1} {2:0.##}%",
                _pocketCount, _normPreflopCount, 100.0 * _normPreflopCount / _pocketCount);
            Assert.AreEqual(EnumAlgos.CountCombin(52, 4), _pocketCount);
            // Value verified in Wiki, etc.
            Assert.AreEqual(16432, _normPreflopCount);
        }

        private void Reset()
        {
            _pocketCount = 0;
            _maxNormPreflopPocket = new CardSet();
            _normPreflopCount = 0;
            _normFlopCount = 0;
        }

        private void SuitIsomorphismPreflopOmaha(ref CardSet pocket)
        {
            _pocketCount++;
            _normPocket = _sn.Convert(pocket);
            if (_maxNormPreflopPocket.bits < _normPocket.bits)
            {
                _normPreflopCount++;
                _maxNormPreflopPocket = _normPocket;
            } 
            _sn.Reset();
        }

        private void SuitIsomorphismPreflopHoldem(ref CardSet pocket)
        {
            _pocketCount++;
            _normPocket = _sn.Convert(pocket);
            if(_maxNormPreflopPocket.bits < _normPocket.bits)
            {
                _normPreflopCount++;
                _maxNormPreflopPocket = _normPocket;

                _sn2.Reset();
                _sn2.Convert(_normPocket);
                _maxNormFlopPocket.Clear();

                CardEnum.Combin(StdDeck.Descriptor, 3, CardSet.Empty, _normPocket, SuitIsomorphismFlopHoldem);
            }
            _sn.Reset();
            
        }

        private void SuitIsomorphismFlopHoldem(ref CardSet flop)
        {
            _snFlop.CopyFrom(_sn2);
            CardSet isoFlop = _snFlop.Convert(flop);
            Assert.AreEqual(CardSet.Empty, _normPocket & isoFlop);
            if (_maxNormFlopPocket.bits < isoFlop.bits)
            {
                _normFlopCount++;
                _maxNormFlopPocket = isoFlop;
            }
        }
    }
}
