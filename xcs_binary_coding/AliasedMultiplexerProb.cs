
//Note:This is an open source code of XCS classifier system(C#) using theoretical parameter settings for binary coding. 
//     This code is based on XCS-Java given by Martin V. Butz:
//          "Butz, M. V. (2000). XCSJava 1.0: An Implementation of the XCS classi£ er system in Java. Technical Report 2000027, Illinois Genetic Algorithms Laboratory"
//     For speeding up, this code employs the "messy-coding" like rule-matching process with paralell computing, which returns the same result of normal matching process.
//     You should receive the GNU General Public License. This code can be distributed WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABLILITY or FITNESS FOR A PARTICULAR PURPOSE.See the GNU General Public License for more details.
//Author: Masaya Nakata, Yokohama National University, Kanagawa, Japan
//Created:08/26/2019


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace xcs_java
{

    public class AliasedMultiplexerProb : Problem
    {
        public int conLength;
        private int maxPayoff;
        private int posBits;
        private char[] currentState;
        private bool correct;
        private bool reset;
        private int nrActions = 2;
        private double aliasingRate;

        public AliasedMultiplexerProb(int length, double _aliasingRate)
        {
            conLength = length;
            double i;
            for (i = 1.0; i + Math.Pow(2.0, i) <= conLength; i++) ;
            posBits = (int)(i - 1);

            if (posBits + Math.Pow(2.0, (double)posBits) != conLength)
                Console.WriteLine("There are additonally " + ((int)(conLength - (posBits + Math.Pow(2.0, (double)posBits)))) + " irrelevant Bits!");

            currentState = new char[conLength];
            maxPayoff = 1000;
            correct = false;
            reset = false;
            aliasingRate = _aliasingRate;
        }

        public string getProblemname()
        {
            return conLength.ToString() + "-amux"+aliasingRate;
        }

        public String resetState(bool is_test)
        {
            for (int i = 0; i < conLength; i++)
            {
                if (XCSConstants.drand() < 0.5)
                {
                    currentState[i] = '0';
                }
                else
                {
                    currentState[i] = '1';
                }
            }
            reset = false;
            return (new String(currentState));
        }

        public double executeAction(int action, bool is_test)
        {
            int place = posBits;
            for (int i = 0; i < posBits; i++)
            {
                if (currentState[i] == '1')
                {
                    place += (int)(Math.Pow(2.0, (double)(posBits - 1 - i)));
                }
            }
            int ret = 0;
            int ans = int.Parse(currentState[place].ToString());

            if (action == ans)
            {
                correct = true;
                ret = maxPayoff;
            }
            else
            {
                correct = false;
                ret = 0;
            }
            reset = true;

            if (!is_test && XCSConstants.drand() < aliasingRate)
            {
                if (ret == 1000)
                    ret = 0;
                else
                    ret = 1000;
            }

            return (double)ret;
        }

        public bool wasCorrect()
        {
            return correct;
        }

        public bool doReset()
        {
            return reset;
        }

        public int getConditionLength()
        {
            return conLength;
        }

        public int getMaxPayoff()
        {
            return maxPayoff;
        }

        public int getNrActions()
        {
            return nrActions;
        }

        public String getCurrentState()
        {
            return new String(currentState);
        }
    }
}
