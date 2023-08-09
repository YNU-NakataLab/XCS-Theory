
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
using System.Diagnostics;

namespace xcs_java
{
    class XCS
    {
        private Problem env;
        private XClassifierSet pop;
        private Logger smp;

        double[] correct_ave ;
        double[] sysError_ave;
        double[] popsize_ave;
        double[] swtime_ave;

        /**
         * Constructs the XCS system.
         */
        public XCS(Problem e, Logger smp)
        {
            env = e;
            pop = null;
            this.smp = smp;
            XCSConstants.set_param();
            this.writeLCSConstancs();

            correct_ave = new double[XCSConstants.maxProblems / XCSConstants.moving_average];
            sysError_ave = new double[XCSConstants.maxProblems / XCSConstants.moving_average];
            popsize_ave = new double[XCSConstants.maxProblems / XCSConstants.moving_average];
            swtime_ave = new double[XCSConstants.maxProblems / XCSConstants.moving_average];
        }

        public void runXCS()
        {
            startExperiments();
        }

        private void startExperiments()
        {
            for (int expCounter=0; expCounter < XCSConstants.nrExps; expCounter++)
            {
                XCSConstants.setSeed(expCounter + 1);
                smp.open_Writter_pW(XCSConstants.seed.ToString("00"));
                Console.WriteLine("Experiment Nr." + (expCounter + 1));

                Stopwatch sw = new Stopwatch();
                //Initialize Population
                pop = new XClassifierSet(env.getNrActions());

                doOneSingleStepExperiment(smp.pW, sw);
                smp.close_Writter_pW();

                smp.open_Writter_pW_pop((expCounter + 1).ToString("00"));
                pop.printSet(smp.pW_pop);
                smp.close_Writter_pW_pop();

                smp.pW_sw.WriteLine(expCounter.ToString("00") + "," + (double)sw.ElapsedMilliseconds / 1000);

                pop = null;
            }

            this.writeAveragePerformance(smp.pW_final);
        }

        void doOneSingleStepExperiment(StreamWriter pW, Stopwatch sw)
        {
            int explore = 0;
            int[] correct = new int[XCSConstants.moving_average];
            double[] sysError = new double[XCSConstants.moving_average];
            double[] popsize = new double[XCSConstants.moving_average];

            for (int exploreProbC = 1; exploreProbC <= XCSConstants.maxProblems; exploreProbC += explore)
            {
                explore = (explore + 1) % 2;

                if (explore == 1)
                {
                    sw.Start();
                    String state = env.resetState(false);
                    doOneSingleStepProblemExplore(state, exploreProbC);
                    sw.Stop();
                }
                else
                {
                    String state = env.resetState(true);
                    doOneSingleStepProblemExploit(state, exploreProbC, correct, sysError, popsize);
                }
                if (exploreProbC % XCSConstants.moving_average == 0 && explore == 0 && exploreProbC > 0)
                {
                    writePerformance(pW, correct, sysError, popsize, (double)sw.ElapsedMilliseconds/1000, exploreProbC);
                }
            }
        }

        private void doOneSingleStepProblemExplore(String state, int counter)
        {
            XClassifierSet matchSet = new XClassifierSet(state, pop, counter, env.getNrActions());

            PredictionArray predictionArray = new PredictionArray(matchSet, env.getNrActions());

            int actionWinner = predictionArray.randomActionWinner();

            XClassifierSet actionSet = new XClassifierSet(matchSet, actionWinner);

            double reward = env.executeAction(actionWinner, false);

            actionSet.updateSet(reward);

            actionSet.runGA(counter, state, env.getNrActions());
        }

        private void doOneSingleStepProblemExploit(String state, int counter, int[] correct, double[] sysError, double[] pop_size)
        {
            XClassifierSet matchSet = new XClassifierSet(state, pop, counter, env.getNrActions());

            PredictionArray predictionArray = new PredictionArray(matchSet, env.getNrActions());

            int actionWinner = predictionArray.bestActionWinner();

            double reward = env.executeAction(actionWinner, true);

            if (env.wasCorrect())
                correct[counter % XCSConstants.moving_average] = 1;
            else
                correct[counter % XCSConstants.moving_average] = 0;

            sysError[counter % XCSConstants.moving_average] = Math.Abs(reward - predictionArray.getBestValue());
            pop_size[counter % XCSConstants.moving_average] = pop.getSize();
        }

        private void writePerformance(StreamWriter pW, int[] performance, double[] sysError, double [] pop_size, double swtime, int exploreProbC)
        {
            double perf = 0.0;
            double serr = 0.0;
            double pops = 0.0;
            for (int i = 0; i < XCSConstants.moving_average; i++)
            {
                perf += performance[i];
                serr += sysError[i];
                pops += pop_size[i];
            }
            perf /= (double)(XCSConstants.moving_average);
            serr /= (double)(XCSConstants.moving_average);
            pops /= (double)(XCSConstants.moving_average);
            pW.WriteLine("" + exploreProbC + "," + (float)perf + "," + (float)serr + "," + pops + "," + swtime);
            Console.WriteLine("" + exploreProbC + "," + (float)perf + "," + (float)serr + "," + pops + "," + swtime);

            correct_ave[(int)(exploreProbC / XCSConstants.moving_average) - 1] += perf;
            sysError_ave[(int)(exploreProbC / XCSConstants.moving_average) - 1] += serr;
            popsize_ave[(int)(exploreProbC / XCSConstants.moving_average) - 1] += pops;
            swtime_ave[(int)(exploreProbC / XCSConstants.moving_average) - 1] += swtime;
        }

        private void writeAveragePerformance(StreamWriter pW)
        {
            for (int i = 0; i < (int)(XCSConstants.maxProblems / XCSConstants.moving_average); i++)
            {
                correct_ave[i] /= XCSConstants.nrExps;
                sysError_ave[i] /= XCSConstants.nrExps;
                popsize_ave[i] /= XCSConstants.nrExps;
                swtime_ave[i] /= XCSConstants.nrExps;

                pW.WriteLine(XCSConstants.moving_average * (i + 1) + "," + (float)correct_ave[i] + "," + (float)sysError_ave[i] + "," + popsize_ave[i] + "," + swtime_ave[i]);
            }
        }

        private void writeLCSConstancs()
        {
            smp.open_Writter_pW("_constants");

            smp.pW.WriteLine("Experimental settings");
            smp.pW.WriteLine("maxProblems,"+XCSConstants.maxProblems);
            smp.pW.WriteLine("nrExps," + XCSConstants.nrExps);
            smp.pW.WriteLine("moving_average," + XCSConstants.moving_average);
            smp.pW.WriteLine("seed," + XCSConstants.seed);

            smp.pW.WriteLine();
            smp.pW.WriteLine("Theoritical XCS parameter settings");
            smp.pW.WriteLine("beta," + XCSConstants.beta);
            smp.pW.WriteLine("epsilon_0," + XCSConstants.epsilon_0);
            smp.pW.WriteLine("theta_sub," + XCSConstants.theta_sub);

            smp.pW.WriteLine();
            smp.pW.WriteLine("Other XCS parameter settings");
            smp.pW.WriteLine("maxPopSize," + XCSConstants.maxPopSize);
            smp.pW.WriteLine("alpha," + XCSConstants.alpha);
            smp.pW.WriteLine("delta," + XCSConstants.delta);
            smp.pW.WriteLine("nu," + XCSConstants.nu);
            smp.pW.WriteLine("theta_GA," + XCSConstants.theta_GA);
            smp.pW.WriteLine("theta_del," + XCSConstants.theta_del);
            smp.pW.WriteLine("pX," + XCSConstants.pX);
            smp.pW.WriteLine("pM," + XCSConstants.pM);
            smp.pW.WriteLine("P_dontcare," + XCSConstants.P_dontcare);
            smp.pW.WriteLine("predictionErrorReduction," + XCSConstants.predictionErrorReduction);
            smp.pW.WriteLine("fitnessReduction," + XCSConstants.fitnessReduction);
            smp.pW.WriteLine("doGASubsumption," + XCSConstants.doGASubsumption);
            smp.pW.WriteLine("doActionSetSubsumption," + XCSConstants.doActionSetSubsumption);
            smp.pW.WriteLine("doTSselection," + XCSConstants.doTSselection);
            smp.pW.WriteLine("tsSize," + XCSConstants.tsSize);
            smp.pW.WriteLine("doUniformXover," + XCSConstants.doUniformXover);
            smp.pW.WriteLine("predictionIni," + XCSConstants.predictionIni);
            smp.pW.WriteLine("predictionErrorIni," + XCSConstants.predictionErrorIni);
            smp.pW.WriteLine("fitnessIni," + XCSConstants.fitnessIni);
            smp.pW.WriteLine("dontCare," + XCSConstants.dontCare);

            smp.close_Writter_pW();
        }
    }
}
