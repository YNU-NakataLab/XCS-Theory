
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

    class PredictionArray
    {
        private double[] pa;
        private double[] nr;

        public PredictionArray(XClassifierSet set, int size)
        {
            pa = new double[size];
            nr = new double[size];

            for (int i = 0; i < size; i++)
            {
                pa[i] = 0.0;
                nr[i] = 0.0;
            }
            for (int i = 0; i < set.getSize(); i++)
            {
                XClassifier cl = set.elementAt(i);
                pa[cl.getAction()] += (cl.getPrediction() * cl.getFitness());
                nr[cl.getAction()] += cl.getFitness();
            }
            for (int i = 0; i < size; i++)
            {
                if (nr[i] != 0)
                {
                    pa[i] /= nr[i];
                }
                else
                {
                    pa[i] = 0;
                }
            }
        }

        public double getBestValue()
        {
            int i;
            double max;
            for (i = 1, max = pa[0]; i < pa.Length; i++)
            {
                if (max < pa[i])
                    max = pa[i];
            }
            return max;
        }

        public double getValue(int i)
        {
            if (i >= 0 && i < pa.Length)
                return pa[i];
            return -1.0;
        }

        public int randomActionWinner()
        {
            int ret = 0;
            do
            {
                ret = (int)(XCSConstants.drand() * pa.Length);
            } while (nr[ret] == 0);
            return ret;
        }

        public int bestActionWinner()
        {
            int ret = 0;
            for (int i = 1; i < pa.Length; i++)
            {
                if (pa[ret] < pa[i])
                    ret = i;
            }
            return ret;
        }

        public int rouletteActionWinner()
        {
            double bidSum = 0.0;
            int i;
            for (i = 0; i < pa.Length; i++)
                bidSum += pa[i];

            bidSum *= XCSConstants.drand();
            double bidC = 0.0;
            for (i = 0; bidC < bidSum; i++)
            {
                bidC += pa[i];
            }
            return i;
        }
    }
}
