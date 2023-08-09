
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
    class Logger
    {
        public StreamWriter pW = null;
        public StreamWriter pW_final = null;
        public StreamWriter pW_pop = null;
        public StreamWriter pW_sw = null;
        public string directly_name = null;

        public Logger(string prob_name)
        {
            DateTime dt = new DateTime();
            dt = DateTime.Now;
            directly_name = dt.ToString("hhmmss") + "_" + prob_name + "_xcs_theory_"+XCSConstants.maxPopSize;
            Directory.CreateDirectory(directly_name);
            pW_final = new StreamWriter(directly_name + "/" + "log_average.csv");
            pW_sw = new StreamWriter(directly_name + "/" + "log_sw.csv");
        }

        public void close_Writter()
        {
            this.pW_final.Close();
            this.pW_sw.Close();
        }

        public void open_Writter_pW(string tag)
        {
            this.pW = new StreamWriter(directly_name + "/" + "log"+tag+".csv");
        }

        public void close_Writter_pW()
        {
            this.pW.Close();
        }

        public void open_Writter_pW_pop(string tag)
        {
            this.pW_pop = new StreamWriter(directly_name + "/" + "log_pop" + tag + ".csv");
        }

        public void close_Writter_pW_pop()
        {
            this.pW_pop.Close();
        }


    }
}
