
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

    public class DV1Prob : Problem
    {
        public int conLength;
        private int maxPayoff;
        private int posBits;
        private char[] currentState;
        private bool correct;
        private bool reset;
        private int nrActions = 2;
        private int[] sigma = 
        { 1, 2, 3, 8, 9,10,11,13,14,24,25,26,27,28,
         30,40,41,42,43,46,47,56,57,58,59,61,65,66,67,69,70,
         71,72,73,74,75,77,78,79,81,82,83,85,86,88,89,90,91,
         93,94,95,97,98,99,101,102,103,104,105,106,107,109,
         110,113,114,115,117,118,121,122,123,125,126,127};

        public DV1Prob()
        {
            conLength = 7;
            currentState = new char[conLength];
            maxPayoff = 1000;
            correct = false;
            reset = false;
        }

        public string getProblemname()
        {
            return "dv1";
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
            int val = 0;

            for (int i = 0; i < conLength; i++)
            {
                if (currentState[i] == '1')
                {
                    val += (int)(Math.Pow(2.0, (double)(conLength - 1 - i)));
                }
            }

            int answer;
            if (sigma.Contains(val))
                answer = 1;
            else
                answer = 0;

            double ret;
            if (answer == action)
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
