
//Note:This is an open source code of XCS classifier system(C#) using theoretical parameter settings for binary coding. 
//     This code is based on XCS-Java given by Martin V. Butz:
//          "Butz, M. V. (2000). XCSJava 1.0: An Implementation of the XCS classi£ er system in Java. Technical Report 2000027, Illinois Genetic Algorithms Laboratory"
//     For speeding up, this code employs the "messy-coding" like rule-matching process with paralell computing, which returns the same result of normal matching process.
//     You should receive the GNU General Public License. This code can be distributed WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABLILITY or FITNESS FOR A PARTICULAR PURPOSE.See the GNU General Public License for more details.
//Author: Masaya Nakata, Yokohama National University, Kanagawa, Japan
//Created:08/26/2019


using System;

namespace xcs_java
{
    class XCSConstants
    {
        public static int maxProblems = 5000000;
        public static int nrExps = 30;
        public static int moving_average = 5000;

        public static bool doMAMupdate = false;
        public static int theta_sub = 255; //See the class "TheoreticalSetting" to use a theoretical value for a given problem
        public static double epsilon_0; //is automatically set when theta_sub is determined 
        public static double beta; //is automatically set when theta_sub is determined

        public static int maxPopSize = 100000;
        public static double alpha = 0.1;
        public static double delta = 0.1;
        public static double nu = 5.0;
        public static int theta_GA = 25;
        public static int theta_del = 20;
        public static double pX = 0.8;
        public static double pM = 0.01;
        public static double P_dontcare = 1.0;
        public static double predictionErrorReduction = 1;
        public static double fitnessReduction = 0.1;
        public static bool doGASubsumption = true;
        public static bool doActionSetSubsumption = true;
        public static bool doTSselection = true;
        public static double tsSize = 0.4;
        public static bool doUniformXover = true;
        public static double predictionIni = 0.0;
        public static double predictionErrorIni = 0.0;
        public static double fitnessIni = 0.01;
        public static char dontCare = '#';
        public static long seed = 1;

        private static Random rand;


        public static void set_param()
        {
            bool was_set = false;
            for (int i = 0; i < TheoreticalSetting.setting.GetLength(0); i++)
            {
                if (TheoreticalSetting.setting[i,0] == XCSConstants.theta_sub)
                {
                    XCSConstants.beta = TheoreticalSetting.setting[i, 1];
                    XCSConstants.epsilon_0 = TheoreticalSetting.setting[i, 2];
                    was_set = true;
                    break;
                }
            }

            if (!was_set)
            {
                Console.WriteLine("theta_sub does not match any implimented theoretical setting: see TheoreticalSetting ");
                Console.ReadLine();
                System.Environment.Exit(0);
            }

            return;
        }

        public static void setSeed(int s)
        {
            XCSConstants.seed = s;
            XCSConstants.rand = new Random(s);
        }

        public static double drand()
        {
            return XCSConstants.rand.NextDouble();
        }
    }

    class TheoreticalSetting
    {
        //Ideal setting
        public static double[,] setting = new double[,]
        {
            //Ideal setting
            { 31,  0.13870090, 58.52695551 }, //DV1
            { 63,  0.08199827, 30.23281083 }, //37-RMUx, 3x3CMUX, 37-AMUX
            { 127, 0.04739000, 15.36777000 }, //70-MUX
            { 255, 0.02686679, 7.748196720 }, //135-MUX
            
            //Adjustment setting
            { 72,  0.07391850, 26.78241081 }, //DV1 Pa = 0.1
            { 145, 0.04255840, 13.58822963 }, //DV1 Pa = 0.01
            { 146, 0.04233210, 13.48189305 }, //37-RMUX Pa = 0.1, 3x3CMUX Pa = 0.1, 37-AMUX Pa = 0.1
            { 293, 0.02393600,  6.76300696 }, //70-MUX Pa = 0.1
            { 588, 0.01330713,  3.38740679 }, //135-MUX Pa = 0.1
        };
    }
}
