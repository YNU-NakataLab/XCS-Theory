
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

    class XClassifierSet
    {
        private int numerositySum;
        private XClassifierSet parentSet;
        private XClassifier[] clSet;
        private int cllSize;

        public XClassifierSet(int numberOfActions)
        {
            numerositySum = 0;
            cllSize = 0;
            parentSet = null;
            clSet = new XClassifier[XCSConstants.maxPopSize + numberOfActions];
        }

        public XClassifierSet(String[] state, XClassifierSet pop, int time, int numberOfActions)
        {
            parentSet = pop;
            numerositySum = 0;
            cllSize = 0;
            clSet = new XClassifier[pop.cllSize + numberOfActions];

            bool[] actionCovered = new bool[numberOfActions];
            for (int i = 0; i < actionCovered.Length; i++)
                actionCovered[i] = false;

            Parallel.For(0, pop.cllSize, i =>
            {
                pop.clSet[i].match(state);
            });

            for (int i = 0; i < pop.cllSize; i++)
            {
                if (pop.clSet[i].is_match)
                {
                    addClassifier(pop.clSet[i]);
                    actionCovered[pop.clSet[i].getAction()] = true;
                }
            }

            bool again;
            do
            {
                again = false;
                for (int i = 0; i < actionCovered.Length; i++)
                {
                    if (!actionCovered[i])
                    {
                        XClassifier newCl = new XClassifier(numerositySum + 1, time, state, i);

                        addClassifier(newCl);
                        pop.addClassifier(newCl);
                    }
                }
                while (pop.numerositySum > XCSConstants.maxPopSize)
                {
                    XClassifier cdel = pop.deleteFromPopulation();
                    int pos = 0;
                    if (cdel != null && (pos = containsClassifier(cdel)) != -1)
                    {
                        numerositySum--;
                        if (cdel.getNumerosity() == 0)
                        {
                            removeClassifier(pos);
                            if (!isActionCovered(cdel.getAction()))
                            {
                                again = true;
                                actionCovered[cdel.getAction()] = false;
                            }
                        }
                    }
                }
            } while (again);
        }

        public XClassifierSet(XClassifierSet matchSet, int action)
        {
            parentSet = matchSet;
            numerositySum = 0;
            cllSize = 0;
            clSet = new XClassifier[matchSet.cllSize];

            for (int i = 0; i < matchSet.cllSize; i++)
            {
                if (matchSet.clSet[i].getAction() == action)
                {
                    addClassifier(matchSet.clSet[i]);
                }
            }
        }

        private int containsClassifier(XClassifier cl)
        {
            for (int i = 0; i < cllSize; i++)
                if (clSet[i] == cl)
                    return i;
            return -1;
        }

        private void match(XClassifier cl, string[] state)
        {
            cl.is_match = cl.match(state);
        }

        private bool isActionCovered(int action)
        {
            for (int i = 0; i < cllSize; i++)
            {
                if (clSet[i].getAction() == action)
                    return true;
            }
            return false;
        }

        public void updateSet(double reward)
        {

            double P = reward;

            for (int i = 0; i < cllSize; i++)
            {
                clSet[i].increaseExperience();
                clSet[i].updatePreError(P);
                clSet[i].updatePrediction(P);
                clSet[i].updateActionSetSize(numerositySum);
            }
            updateFitnessSet();

            if (XCSConstants.doActionSetSubsumption)
                doActionSetSubsumption();
        }

        private void updateFitnessSet()
        {
            double accuracySum = 0.0;
            double[] accuracies = new double[cllSize];

            for (int i = 0; i < cllSize; i++)
            {
                accuracies[i] = clSet[i].getAccuracy();
                accuracySum += accuracies[i] * clSet[i].getNumerosity();
            }

            for (int i = 0; i < cllSize; i++)
            {
                clSet[i].updateFitness(accuracySum, accuracies[i]);
            }
        }

        public void runGA(int time, String[] state, int numberOfActions)
        {
            if (cllSize == 0 || time - getTimeStampAverage() < XCSConstants.theta_GA)
                return;

            setTimeStamps(time);

            double fitSum = getFitnessSum();

            XClassifier cl1P;
            XClassifier cl2P;

            if (!XCSConstants.doTSselection)
            {
                cl1P = selectXClassifierRW(fitSum);
                cl2P = selectXClassifierRW(fitSum);
            }
            else
            {
                cl1P = selectOffspringTS();
                cl2P = selectOffspringTS();
            }

            XClassifier cl1 = new XClassifier(cl1P);
            XClassifier cl2 = new XClassifier(cl2P);

            bool is_changed_x = false;
            if (!XCSConstants.doUniformXover)
            {
                is_changed_x = cl1.twoPointCrossover(cl2);
            }
            else
            {
                is_changed_x = cl1.uniformCrossover(cl2);
            }

            cl1.applyMutation(state, numberOfActions);
            cl2.applyMutation(state, numberOfActions);

            if (is_changed_x)
            {
                cl1.setPrediction((cl1.getPrediction() + cl2.getPrediction()) / 2.0);
                cl1.setPredictionError(XCSConstants.predictionErrorReduction * (cl1.getPredictionError() + cl2.getPredictionError()) / 2.0);
                cl1.setFitness(XCSConstants.fitnessReduction * (cl1.getFitness() + cl2.getFitness()) / 2.0);
                cl2.setPrediction(cl1.getPrediction());
                cl2.setPredictionError(cl1.getPredictionError());
                cl2.setFitness(cl1.getFitness());
            }
            else
            {
                cl1.setPredictionError(XCSConstants.predictionErrorReduction * cl1.getPredictionError());
                cl1.setFitness(XCSConstants.fitnessReduction * cl1.getFitness());
                cl2.setPredictionError(XCSConstants.predictionErrorReduction * cl2.getPredictionError());
                cl2.setFitness(XCSConstants.fitnessReduction * cl2.getFitness());
            }

            insertDiscoveredXClassifiers(cl1, cl2, cl1P, cl2P);
        }

        private XClassifier selectXClassifierRW(double fitSum)
        {
            double choiceP = XCSConstants.drand() * fitSum;
            int i = 0;
            double sum = clSet[i].getFitness();
            while (choiceP > sum)
            {
                i++;
                sum += clSet[i].getFitness();
            }

            return clSet[i];
        }

        private XClassifier selectOffspringTS()
        {
            XClassifier cl = null;
            double maxFitness;

            while (cl == null)
            {
                maxFitness = 0;
                foreach (XClassifier c in this.clSet)
                {
                    if (c != null)
                        if (c.getFitness() / (double)(c.getNumerosity()) > maxFitness)
                            for (int i = 0; i < c.getNumerosity(); i++)
                                if (XCSConstants.drand() < XCSConstants.tsSize)
                                {
                                    cl = c;
                                    maxFitness = c.getFitness() / c.getNumerosity();
                                    break;
                                }
                }
            }
            return cl;
        }

        private void insertDiscoveredXClassifiers(XClassifier cl1, XClassifier cl2, XClassifier cl1P, XClassifier cl2P)
        {
            XClassifierSet pop = this;
            while (pop.parentSet != null)
                pop = pop.parentSet;

            if (XCSConstants.doGASubsumption)
            {
                subsumeXClassifier(cl1, cl1P, cl2P);
                subsumeXClassifier(cl2, cl1P, cl2P);
            }
            else
            {
                pop.addXClassifierToPopulation(cl1);
                pop.addXClassifierToPopulation(cl2);
            }

            while (pop.numerositySum > XCSConstants.maxPopSize)
                pop.deleteFromPopulation();
        }

        private void subsumeXClassifier(XClassifier cl, XClassifier cl1P, XClassifier cl2P)
        {
            if (cl1P != null && cl1P.subsumes(cl))
            {
                increaseNumerositySum(1);
                cl1P.addNumerosity(1);
            }
            else if (cl2P != null && cl2P.subsumes(cl))
            {
                increaseNumerositySum(1);
                cl2P.addNumerosity(1);
            }
            else
            {
                subsumeXClassifier(cl);
            }
        }

        private void subsumeXClassifier(XClassifier cl)
        {
            List<XClassifier> choices = new List<XClassifier>();

            for (int i = 0; i < cllSize; i++)
            {
                if (clSet[i].subsumes(cl))
                    choices.Add(clSet[i]);
            }

            Parallel.For(0, cllSize, i =>
            {
                clSet[i].subsumes(cl);
            });

            for (int i = 0; i < cllSize; i++)
                if (cl.is_subsume)
                    choices.Add(clSet[i]);

            if (choices.Count() > 0)
            {
                int choice = (int)((double)XCSConstants.drand() * choices.Count());
                choices[choice].addNumerosity(1);
                increaseNumerositySum(1);
                return;
            }
            addXClassifierToPopulation(cl);
        }

        private void doActionSetSubsumption()
        {
            XClassifierSet pop = this;
            while (pop.parentSet != null)
                pop = pop.parentSet;

            XClassifier subsumer = null;
            for (int i = 0; i < cllSize; i++)
            {
                if (clSet[i].isSubsumer())
                    if (subsumer == null || clSet[i].isMoreGeneral(subsumer))
                        subsumer = clSet[i];
            }

            if (subsumer != null)
            {
                for (int i = 0; i < cllSize; i++)
                {
                    if (subsumer.isMoreGeneral(clSet[i]) && subsumer != clSet[i])
                    {
                        int num = clSet[i].getNumerosity();
                        subsumer.addNumerosity(num);
                        clSet[i].addNumerosity((-1) * num);
                        pop.removeClassifier(clSet[i]);
                        removeClassifier(i);
                        i--;
                    }
                }
            }
        }

        private void addXClassifierToPopulation(XClassifier cl)
        {
            XClassifierSet pop = this;
            while (pop.parentSet != null)
                pop = pop.parentSet;

            XClassifier oldcl = null;
            if ((oldcl = pop.getIdenticalClassifier(cl)) != null)
            {
                oldcl.addNumerosity(1);
                increaseNumerositySum(1);
            }
            else
            {
                pop.addClassifier(cl);
            }
        }

        private XClassifier getIdenticalClassifier(XClassifier newCl)
        {
            for (int i = 0; i < cllSize; i++)
                if (newCl.equals(clSet[i]))
                    return clSet[i];
            return null;
        }

        private XClassifier deleteFromPopulation()
        {
            double meanFitness = getFitnessSum() / (double)numerositySum;
            double sum = 0.0;
            for (int i = 0; i < cllSize; i++)
            {
                sum += clSet[i].getDelProp(meanFitness);
            }

            double choicePoint = sum * XCSConstants.drand();
            sum = 0.0;
            for (int i = 0; i < cllSize; i++)
            {
                sum += clSet[i].getDelProp(meanFitness);
                if (sum > choicePoint)
                {
                    XClassifier cl = clSet[i];
                    cl.addNumerosity(-1);
                    numerositySum--;
                    if (cl.getNumerosity() == 0)
                    {
                        removeClassifier(i);
                    }
                    return cl;
                }
            }
            return null;
        }

        public void confirmClassifiersInSet()
        {
            int copyStep = 0;
            numerositySum = 0;
            int i;
            for (i = 0; i < cllSize - copyStep; i++)
            {
                if (clSet[i + copyStep].getNumerosity() == 0)
                {
                    copyStep++;
                    i--;
                }
                else
                {
                    if (copyStep > 0)
                    {
                        clSet[i] = clSet[i + copyStep];
                    }
                    numerositySum += clSet[i].getNumerosity();
                }
            }
            for (; i < cllSize; i++)
            {
                clSet[i] = null;
            }
            cllSize -= copyStep;
        }


        private void setTimeStamps(int time)
        {
            for (int i = 0; i < cllSize; i++)
                clSet[i].setTimeStamp(time);
        }

        private void addClassifier(XClassifier classifier)
        {
            clSet[cllSize] = classifier;
            addValues(classifier);
            cllSize++;
        }

        private void addValues(XClassifier cl)
        {
            numerositySum += cl.getNumerosity();
        }

        private void increaseNumerositySum(int nr)
        {
            numerositySum += nr;
            if (parentSet != null)
                parentSet.increaseNumerositySum(nr);
        }

        private bool removeClassifier(XClassifier classifier)
        {
            int i;
            for (i = 0; i < cllSize; i++)
                if (clSet[i] == classifier)
                    break;
            if (i == cllSize)
            {
                return false;
            }
            for (; i < cllSize - 1; i++)
                clSet[i] = clSet[i + 1];
            clSet[i] = null;

            cllSize--;

            return true;
        }

        private bool removeClassifier(int pos)
        {
            int i;
            for (i = pos; i < cllSize - 1; i++)
                clSet[i] = clSet[i + 1];
            clSet[i] = null;
            cllSize--;

            return true;
        }

        private double getPredictionSum()
        {
            double sum = 0.0;

            for (int i = 0; i < cllSize; i++)
            {
                sum += clSet[i].getPrediction() * clSet[i].getNumerosity();
            }
            return sum;
        }

        private double getFitnessSum()
        {
            double sum = 0.0;

            for (int i = 0; i < cllSize; i++)
                sum += clSet[i].getFitness();
            return sum;
        }

        private double getTimeStampSum()
        {
            double sum = 0.0;

            for (int i = 0; i < cllSize; i++)
            {
                sum += clSet[i].getTimeStamp() * clSet[i].getNumerosity();
            }
            return sum;
        }

        public int getNumerositySum()
        {
            return numerositySum;
        }

        public XClassifier elementAt(int i)
        {
            return clSet[i];
        }

        public int getSize()
        {
            return cllSize;
        }

        private double getTimeStampAverage()
        {
            double sum = 0.0;
            double num_sum = (double)(this.getNumerositySum());

            for (int i = 0; i < cllSize; i++)
            {
                sum += clSet[i].getTimeStamp() / num_sum * clSet[i].getNumerosity();
            }

            return sum;
        }

        public void printSet()
        {
            Console.WriteLine("Averages:");
            Console.WriteLine("Pre: " + (getPredictionSum() / numerositySum) + " Fit: " + (getFitnessSum() / numerositySum) + " Tss: " + (getTimeStampSum() / numerositySum) + " Num: " + numerositySum);
            for (int i = 0; i < cllSize; i++)
            {
                clSet[i].printXClassifier();
            }
        }

        public void printSet(StreamWriter pW)
        {
            for (int i = 0; i < cllSize; i++)
            {
                clSet[i].printXClassifier(pW);
            }
        }
    }
}
