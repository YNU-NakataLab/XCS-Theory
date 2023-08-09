
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
    class Program
    {
        static void Main(string[] args)
        {
            Problem e =
                new RealValuedMultiplexerProb(37);

            Run_XCS(e);
        }

        static void Run_XCS(Problem e)
        {
            Logger smp = new Logger(e.getProblemname());

            XCS xcs = new XCS(e, smp);
            xcs.runXCS();
            smp.close_Writter();
            return;
        }
    }
}
