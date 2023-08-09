
//Note:This is an open source code of XCSI classifier system(C#) using theoretical parameter settings for real-valued coding. 
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

namespace xcs
{

    public class RealValuedMultiplexerProb : Problem
    {
        public int conLength;
        private int maxPayoff;
        private int posBits;
        private string[] currentState;
        private bool correct;
        private bool reset;
        private int nrActions = 2;

        public RealValuedMultiplexerProb(int length)
        {
            conLength = length;
            double i;
            for (i = 1.0; i + Math.Pow(2.0, i) <= conLength; i++) ;
            posBits = (int)(i - 1);

            if (posBits + Math.Pow(2.0, (double)posBits) != conLength)
                Console.WriteLine("There are additonally " + ((int)(conLength - (posBits + Math.Pow(2.0, (double)posBits)))) + " irrelevant Bits!");

            currentState = new string[conLength];
            maxPayoff = 1000;
            correct = false;
            reset = false;
        }

        public string getProblemname()
        {
            return conLength.ToString() + "-rmux";
        }

        public string[] resetState(bool is_test)
        {
            for (int i = 0; i < conLength; i++)
            {
                currentState[i] = XCSConstants.drand().ToString();
            }

            reset = false;
            return currentState;
        }

        public double executeAction(int action, bool is_test)
        {
            int place = posBits;
            for (int i = 0; i < posBits; i++)
            {
                if (double.Parse(currentState[i]) >=0.5)
                {
                    place += (int)(Math.Pow(2.0, (double)(posBits - 1 - i)));
                }
            }
            int ret = 0;
            int ans;

            if (double.Parse(currentState[place]) >= 0.5)
                ans = 1;
            else
                ans = 0;

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

        public string[] getCurrentState()
        {
            return currentState;
        }
    }
}
