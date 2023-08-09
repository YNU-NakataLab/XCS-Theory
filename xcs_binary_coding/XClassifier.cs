
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

    public class XClassifier
    {
        private String condition;

        private List<int> specified_cond;
        private List<int> specified_adds;
        private int count;
        public bool is_match;
        public bool is_subsume;

        private int action;
        private double prediction;
        private double predictionError;
        private double fitness;
        private int numerosity;
        private int experience;
        private double actionSetSize;
        private int timeStamp;

        public XClassifier(double setSize, int time, String situation, int act)
        {
            createMatchingCondition(situation);
            action = act;
            classifierSetVariables(setSize, time);
        }

        public XClassifier(double setSize, int time, int numberOfActions, String situation)
        {
            createMatchingCondition(situation);
            createRandomAction(numberOfActions);
            classifierSetVariables(setSize, time);
        }

        public XClassifier(double setSize, int time, int condLength, int numberOfActions)
        {
            createRandomCondition(condLength);
            createRandomAction(numberOfActions);
            classifierSetVariables(setSize, time);
        }

        public XClassifier(XClassifier clOld)
        {
            condition = String.Copy(clOld.condition);
            action = clOld.action;
            this.prediction = clOld.prediction;
            this.predictionError = clOld.predictionError;
            this.fitness = clOld.fitness / clOld.numerosity;
            this.numerosity = 1;
            this.experience = 0;
            this.actionSetSize = clOld.actionSetSize;
            this.timeStamp = clOld.timeStamp;
        }

        private void createRandomCondition(int condLength)
        {
            char[] condArray = new char[condLength];
            for (int i = 0; i < condLength; i++)
            {
                if (XCSConstants.drand() < XCSConstants.P_dontcare)
                    condArray[i] = XCSConstants.dontCare;
                else
                    if (XCSConstants.drand() < 0.5)
                        condArray[i] = '0';
                    else
                        condArray[i] = '1';
            }
            condition = new String(condArray);
        }

        private void createMatchingCondition(String cond)
        {
            int condLength = cond.Length;
            char[] condArray = new char[condLength];

            this.specified_cond = new List<int>();
            this.specified_adds = new List<int>();

            for (int i = 0; i < condLength; i++)
            {
                if (XCSConstants.drand() < XCSConstants.P_dontcare)
                    condArray[i] = XCSConstants.dontCare;
                else
                {
                    condArray[i] = cond[i];
                    this.specified_cond.Add(cond[i]);
                    this.specified_adds.Add(i);
                }
            }

            condition = new String(condArray);
            this.count = this.specified_cond.Count();
        }

        private void createRandomAction(int numberOfActions)
        {
            action = (int)(XCSConstants.drand() * numberOfActions);
        }

        private void classifierSetVariables(double setSize, int time)
        {
            this.prediction = XCSConstants.predictionIni;
            this.predictionError = XCSConstants.predictionErrorIni;
            this.fitness = XCSConstants.fitnessIni;

            this.numerosity = 1;
            this.experience = 0;
            this.actionSetSize = setSize;
            this.timeStamp = time;
        }

        public bool match(String state)
        {
            if (condition.Length != state.Length)
                return false;

            for (int i = 0; i < this.count; i++)
            {
                int address = this.specified_adds[i];

                if (this.specified_cond[i] != state[address])
                {
                    this.is_match = false;
                    return false;
                }
            }

            this.is_match = true;
            return true;
        }

        public bool twoPointCrossover(XClassifier cl)
        {
            bool changed = false;

            if (XCSConstants.drand() < XCSConstants.pX)
            {
                int length = condition.Length;
                int sep1 = (int)(XCSConstants.drand() * (length));
                int sep2 = (int)(XCSConstants.drand() * (length)) + 1;
                if (sep1 > sep2)
                {
                    int help = sep1;
                    sep1 = sep2;
                    sep2 = help;
                }
                else if (sep1 == sep2)
                {
                    sep2++;
                }
                char[] cond1 = condition.ToCharArray();
                char[] cond2 = cl.condition.ToCharArray();
                for (int i = sep1; i < sep2; i++)
                {
                    if (cond1[i] != cond2[i])
                    {
                        changed = true;
                        char help = cond1[i];
                        cond1[i] = cond2[i];
                        cond2[i] = help;
                    }
                }
                if (changed)
                {
                    condition = new String(cond1);
                    cl.condition = new String(cond2);
                }
            }

            return changed;
        }

        public bool uniformCrossover(XClassifier cl)
        {
            bool changed = false;

            if (XCSConstants.drand() < XCSConstants.pX)
            {
                char[] cond1 = condition.ToCharArray();
                char[] cond2 = cl.condition.ToCharArray();

                for (int i = 0; i < this.condition.Length; i++)
                {
                    if (XCSConstants.drand() < 0.5)
                    {
                        cond1[i] = cl.condition[i];
                        cond2[i] = this.condition[i];

                        changed = true;
                    }
                }

                this.condition = new String(cond1);
                cl.condition = new String(cond2);
            }

            return changed;
        }

        public bool applyMutation(String state, int numberOfActions)
        {
            bool changed = mutateCondition(state);
            if (mutateAction(numberOfActions))
                changed = true;
            return changed;
        }

        private bool mutateCondition(String state)
        {
            bool changed = false;
            int condLength = condition.Length;

            this.specified_cond = new List<int>();
            this.specified_adds = new List<int>();

            for (int i = 0; i < condLength; i++)
            {
                if (XCSConstants.drand() < XCSConstants.pM)
                {
                    char[] cond = condition.ToCharArray();
                    char[] stateC = state.ToCharArray();
                    changed = true;
                    if (cond[i] == XCSConstants.dontCare)
                    {
                        cond[i] = stateC[i];
                    }
                    else
                    {
                        cond[i] = XCSConstants.dontCare;
                    }
                    condition = new String(cond);
                }

                if (condition[i] != XCSConstants.dontCare)
                {
                    this.specified_cond.Add(condition[i]);
                    this.specified_adds.Add(i);
                }
            }

            this.count = this.specified_cond.Count();
            return changed;
        }

        private bool mutateAction(int numberOfActions)
        {
            bool changed = false;

            if (XCSConstants.drand() < XCSConstants.pM)
            {
                int act = 0;
                do
                {
                    act = (int)(XCSConstants.drand() * numberOfActions);
                } while (act == action);
                action = act;
                changed = true;
            }
            return changed;
        }

        public bool equals(XClassifier cl)
        {
            if (cl.action != action || this.count != cl.count)
                return false;

            for (int i = 0;i< this.count;i++)
            {
                int address = this.specified_adds[i];
                if (this.specified_cond[i] != cl.condition[address])
                    return false;
            }

            return true;
        }

        public bool subsumes(XClassifier cl)
        {
            if (cl.action == action)
                if (isSubsumer())
                    if (isMoreGeneral(cl))
                    {
                        this.is_subsume = true;
                        return true;
                    }

            this.is_subsume = false;
            return false;
        }

        public bool isSubsumer()
        {
            if (experience > XCSConstants.theta_sub && predictionError < (double)XCSConstants.epsilon_0)
                return true;
            return false;
        }

        public bool isMoreGeneral(XClassifier cl)
        {
            if (this.count >= cl.count)
                return false;

            for (int i = 0; i < this.count; i++)
            {
                int address = this.specified_adds[i];

                if (this.specified_cond[i] != cl.condition[address])
                    return false;
            }

            return true;
        }


        public double getDelProp(double meanFitness)
        {


            if (fitness / numerosity >= XCSConstants.delta * meanFitness || experience < XCSConstants.theta_del)
                return actionSetSize * numerosity;
            return actionSetSize * numerosity * meanFitness / (fitness / numerosity);
        }

        public double updatePrediction(double P)
        {
            if (XCSConstants.doMAMupdate)
            {
                if ((double)experience < 1.0 / XCSConstants.beta)
                {
                    prediction = (prediction * ((double)experience - 1.0) + P) / (double)experience;
                }
                else
                {
                    prediction += XCSConstants.beta * (P - prediction);
                }
            }
            else
            {
                prediction += XCSConstants.beta * (P - prediction);
            }
            return prediction * numerosity;
        }

        public double updatePreError(double P)
        {
            if (XCSConstants.doMAMupdate)
            {
                if ((double)experience < 1.0 / XCSConstants.beta)
                {
                    predictionError = (predictionError * ((double)experience - 1.0) + Math.Abs(P - prediction)) / (double)experience;
                }
                else
                {
                    predictionError += XCSConstants.beta * (Math.Abs(P - prediction) - predictionError);
                }
            }
            else
            {
                predictionError += XCSConstants.beta * (Math.Abs(P - prediction) - predictionError);
            }
            return predictionError * numerosity;
        }

        public double getAccuracy()
        {
            double accuracy;

            if (predictionError < (double)XCSConstants.epsilon_0)
            {
                accuracy = 1.0;
            }
            else
            {
                accuracy = XCSConstants.alpha * Math.Pow(predictionError / XCSConstants.epsilon_0, -XCSConstants.nu);
            }
            return accuracy;
        }

        public double updateFitness(double accSum, double accuracy)
        {
            fitness += XCSConstants.beta * ((accuracy * numerosity) / accSum - fitness);
            return fitness;
        }

        public double updateActionSetSize(double numerositySum)
        {
            if (experience < 1.0 / XCSConstants.beta)
            {
                actionSetSize = (actionSetSize * (double)(experience - 1) + numerositySum) / (double)experience;
            }
            else
            {
                actionSetSize += XCSConstants.beta * (numerositySum - actionSetSize);
            }
            return actionSetSize * numerosity;
        }

        public int getAction()
        {
            return action;
        }

        public void increaseExperience()
        {
            experience++;
        }

        public double getPrediction()
        {
            return prediction;
        }

        public void setPrediction(double pre)
        {
            prediction = pre;
        }

        public double getPredictionError()
        {
            return predictionError;
        }

        public void setPredictionError(double preE)
        {
            predictionError = preE;
        }

        public double getFitness()
        {
            return fitness;
        }

        public void setFitness(double fit)
        {
            fitness = fit;
        }

        public int getNumerosity()
        {
            return numerosity;
        }

        public void addNumerosity(int num)
        {
            numerosity += num;
        }

        public int getTimeStamp()
        {
            return timeStamp;
        }

        public void setTimeStamp(int ts)
        {
            timeStamp = ts;
        }

        public void printXClassifier()
        {
            Console.WriteLine(condition + " " + action + "\t" + (float)prediction + "\t" + (float)predictionError + "\t" + (float)fitness +
                       "\t" + numerosity + "\t" + experience + "\t" + (float)actionSetSize + "\t" + timeStamp);
        }

        public void printXClassifier(StreamWriter pW)
        {
            pW.WriteLine(condition + "," + action + "," + (float)prediction + "," + (float)predictionError + "," + (float)fitness +
                   "," + numerosity + "," + experience + "," + (float)actionSetSize + "," + timeStamp);
        }
    }

}
