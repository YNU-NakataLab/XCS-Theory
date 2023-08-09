
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

    public class XClassifier
    {
        private String[] up_condition;
        private String[] lw_condition;
        private int count;

        private List<double[]> specified_cond;
        private List<int> specified_adds;

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

        public XClassifier(double setSize, int time, String[] situation, int act)
        {
            createMatchingCondition(situation);
            action = act;
            classifierSetVariables(setSize, time);
        }

        public XClassifier(double setSize, int time, int numberOfActions, String[] situation)
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
            this.up_condition = new string[clOld.up_condition.Length];
            this.lw_condition = new string[clOld.lw_condition.Length];

            clOld.up_condition.CopyTo(up_condition, 0);
            clOld.lw_condition.CopyTo(lw_condition, 0);

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
            this.up_condition = new string[condLength];
            this.lw_condition = new string[condLength];

            for (int i = 0; i < condLength; i++)
            {
                if (XCSConstants.drand() < XCSConstants.P_dontcare)
                {
                    this.up_condition[i] = (1.0).ToString();
                    this.lw_condition[i] = (0.0).ToString();
                }
                else
                {
                    double x = XCSConstants.drand();
                    double u = x + XCSConstants.drand() * XCSConstants.s0;
                    double l = x - XCSConstants.drand() * XCSConstants.s0;

                    if (l > u)
                    {
                        double tmp = u;
                        u = l;
                        l = tmp;
                    }

                    if (l < 0.0)
                        l = 0.0;
                    if (u > 1.0)
                        u = 1.0;

                    this.up_condition[i] = u.ToString();
                    this.lw_condition[i] = l.ToString();

                }
            }
        }

        private void createMatchingCondition(String[] cond)
        {
            int condLength = cond.Length;

            this.lw_condition = new string[condLength];
            this.up_condition = new string[condLength];

            this.specified_cond = new List<double[]>();
            this.specified_adds = new List<int>();

            for (int i = 0; i < condLength; i++)
            {

                if (XCSConstants.drand() < XCSConstants.P_dontcare)
                {
                    this.up_condition[i] = (1.0).ToString();
                    this.lw_condition[i] = (0.0).ToString();
                }
                else
                {
                    double x = double.Parse(cond[i]);
                    double u = x + XCSConstants.drand() * XCSConstants.s0;
                    double l = x - XCSConstants.drand() * XCSConstants.s0;

                    if (l > u)
                    {
                        double tmp = u;
                        u = l;
                        l = tmp;
                    }

                    if (l < 0.0)
                        l = 0.0;
                    if (u > 1.0)
                        u = 1.0;

                    this.up_condition[i] = u.ToString();
                    this.lw_condition[i] = l.ToString();

                    if (u-l != 1)
                    {
                        this.specified_cond.Add(new double[] { l, u });
                        this.specified_adds.Add(i);
                    }
                }
            }

            this.count = this.specified_adds.Count();
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

        public bool match(String[] state)
        {
            if (up_condition.Length != state.Length)
                return false;

            for (int i = 0;i<this.count;i++)
            {
                int address = this.specified_adds[i];
                double x = double.Parse(state[address]);

                if(x < this.specified_cond[i][0] || x > this.specified_cond[i][1])
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
                int length = up_condition.Length;
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

                string[] up_cond1 = new string[length];
                string[] lw_cond1 = new string[length];
                string[] up_cond2 = new string[length];
                string[] lw_cond2 = new string[length];

                this.up_condition.CopyTo(up_cond1, 0);
                this.lw_condition.CopyTo(lw_cond1, 0);
                cl.up_condition.CopyTo(up_cond2, 0);
                cl.lw_condition.CopyTo(lw_cond2, 0);

                for (int i = sep1; i < sep2; i++)
                {
                    if (up_cond1[i] != up_cond2[i])
                    {
                        changed = true;
                        string help = up_cond1[i];
                        up_cond1[i] = up_cond2[i];
                        up_cond2[i] = help;
                    }
                    if (lw_cond1[i] != lw_cond2[i])
                    {
                        changed = true;
                        string help = lw_cond1[i];
                        lw_cond1[i] = lw_cond2[i];
                        lw_cond2[i] = help;
                    }

                    if (double.Parse(up_cond1[i]) < double.Parse(lw_cond1[i]))
                    {
                        string help = up_cond1[i];
                        up_cond1[i] = lw_cond1[i];
                        lw_cond1[i] = up_cond1[i];
                    }

                    if (double.Parse(up_cond2[i]) < double.Parse(lw_cond2[i]))
                    {
                        string help = up_cond2[i];
                        up_cond2[i] = lw_cond2[i];
                        lw_cond2[i] = up_cond2[i];
                    }
                }
                if (changed)
                {
                    this.up_condition = up_cond1;
                    this.lw_condition = lw_cond1;
                    cl.up_condition = up_cond2;
                    cl.lw_condition = lw_cond2;
                }
            }

            return changed;
        }

        public bool uniformCrossover(XClassifier cl)
        {
            bool changed = false;
            int length = this.up_condition.Length;

            if (XCSConstants.drand() < XCSConstants.pX)
            {
                string[] up_cond1 = new string[length];
                string[] lw_cond1 = new string[length];
                string[] up_cond2 = new string[length];
                string[] lw_cond2 = new string[length];

                this.up_condition.CopyTo(up_cond1, 0);
                this.lw_condition.CopyTo(lw_cond1, 0);
                cl.up_condition.CopyTo(up_cond2, 0);
                cl.lw_condition.CopyTo(lw_cond2, 0);

                for (int i = 0; i < length; i++)
                {
                    if (XCSConstants.drand() < 0.5)
                    {
                        string help = up_cond1[i];
                        up_cond1[i] = up_cond2[i];
                        up_cond2[i] = help;

                        string tmp2 = lw_cond1[i];
                        lw_cond1[i] = lw_cond2[i];
                        lw_cond2[i] = tmp2;

                        changed = true;
                    }
                }

                this.up_condition = up_cond1;
                this.lw_condition = lw_cond1;
                cl.up_condition = up_cond2;
                cl.lw_condition = lw_cond2;
            }

            return changed;
        }

        public void sbx_average_upper(XClassifier cl)
        {
            double EPS = 0.0001;
            double y1, y2, yl, yu;
            double c1, c2;
            double alpha, beta, betaq, rand;
            double id_cx = 15;
            double eta_c = id_cx;

            List<Attribute> cond1 = new List<Attribute>();
            List<Attribute> cond2 = new List<Attribute>();

            for (int i = 0; i < this.up_condition.Length; i++)
            {
                if (XCSConstants.drand() <= 0.5)
                {
                   

                    if (Math.Abs(double.Parse(this.up_condition[i]) - double.Parse(cl.up_condition[i])) > EPS)
                    {
                        if (double.Parse(this.up_condition[i]) < double.Parse(cl.up_condition[i]))
                        {
                            y1 = double.Parse(this.up_condition[i]);
                            y2 = double.Parse(cl.up_condition[i]);
                        }
                        else
                        {
                            y1 = double.Parse(cl.up_condition[i]);
                            y2 = double.Parse(this.up_condition[i]);
                        }

                        yl = 0.0;
                        yu = 1.0;

                        rand = XCSConstants.drand();
                        beta = 1.0 + (2.0 * (y1 - yl) / (y2 - y1));
                        alpha = 2.0 - Math.Pow(beta, -(eta_c + 1.0));

                        if (rand <= (1.0 / alpha))
                        {
                            betaq = Math.Pow((rand * alpha), (1.0 / (eta_c + 1.0)));
                        }
                        else
                        {
                            betaq = Math.Pow((1.0 / (2.0 - rand * alpha)), (1.0 / (eta_c + 1.0)));
                        }

                        c1 = 0.5 * ((y1 + y2) - betaq * (y2 - y1));
                        beta = 1.0 + (2.0 * (yu - y2) / (y2 - y1));
                        alpha = 2.0 - Math.Pow(beta, -(eta_c + 1.0));

                        if (rand <= (1.0 / alpha))
                        {
                            betaq = Math.Pow((rand * alpha), (1.0 / (eta_c + 1.0)));
                        }
                        else
                        {
                            betaq = Math.Pow((1.0 / (2.0 - rand * alpha)), (1.0 / (eta_c + 1.0)));
                        }

                        c2 = 0.5 * ((y1 + y2) + betaq * (y2 - y1));

                        if (c1 < yl)
                            c1 = yl;
                        if (c2 < yl)
                            c2 = yl;
                        if (c1 > yu)
                            c1 = yu;
                        if (c2 > yu)
                            c2 = yu;

                        if (XCSConstants.drand() < 0.5)
                        {
                            this.up_condition[i] = c2.ToString();
                            cl.up_condition[i] = c1.ToString();
                        }
                        else
                        {
                            this.up_condition[i] = c1.ToString();
                            cl.up_condition[i] = c2.ToString();
                        }
                    }
                }
            }
        }

        public void sbx_average_lower(XClassifier cl)
        {
            double EPS = 0.0001;
            double y1, y2, yl, yu;
            double c1, c2;
            double alpha, beta, betaq, rand;
            double id_cx = 15;
            double eta_c = id_cx;

            List<Attribute> cond1 = new List<Attribute>();
            List<Attribute> cond2 = new List<Attribute>();

            for (int i = 0; i < this.lw_condition.Length; i++)
            {
                if (XCSConstants.drand() <= 0.5)
                {
                    if (Math.Abs(double.Parse(this.lw_condition[i]) - double.Parse(cl.lw_condition[i])) > EPS)
                    {
                        if (double.Parse(this.lw_condition[i]) < double.Parse(cl.lw_condition[i]))
                        {
                            y1 = double.Parse(this.lw_condition[i]);
                            y2 = double.Parse(cl.lw_condition[i]);
                        }
                        else
                        {
                            y1 = double.Parse(cl.lw_condition[i]);
                            y2 = double.Parse(this.lw_condition[i]);
                        }

                        yl = 0.0;
                        yu = 1.0;

                        rand = XCSConstants.drand();
                        beta = 1.0 + (2.0 * (y1 - yl) / (y2 - y1));
                        alpha = 2.0 - Math.Pow(beta, -(eta_c + 1.0));

                        if (rand <= (1.0 / alpha))
                        {
                            betaq = Math.Pow((rand * alpha), (1.0 / (eta_c + 1.0)));
                        }
                        else
                        {
                            betaq = Math.Pow((1.0 / (2.0 - rand * alpha)), (1.0 / (eta_c + 1.0)));
                        }

                        c1 = 0.5 * ((y1 + y2) - betaq * (y2 - y1));
                        beta = 1.0 + (2.0 * (yu - y2) / (y2 - y1));
                        alpha = 2.0 - Math.Pow(beta, -(eta_c + 1.0));

                        if (rand <= (1.0 / alpha))
                        {
                            betaq = Math.Pow((rand * alpha), (1.0 / (eta_c + 1.0)));
                        }
                        else
                        {
                            betaq = Math.Pow((1.0 / (2.0 - rand * alpha)), (1.0 / (eta_c + 1.0)));
                        }

                        c2 = 0.5 * ((y1 + y2) + betaq * (y2 - y1));

                        if (c1 < yl)
                            c1 = yl;
                        if (c2 < yl)
                            c2 = yl;
                        if (c1 > yu)
                            c1 = yu;
                        if (c2 > yu)
                            c2 = yu;

                        if (XCSConstants.drand() < 0.5)
                        {
                            this.lw_condition[i] = c2.ToString();
                            cl.lw_condition[i] = c1.ToString();
                        }
                        else
                        {
                            this.lw_condition[i] = c1.ToString();
                            cl.lw_condition[i] = c2.ToString();
                        }
                    }
                }
            }
        }

        public bool SBCrossover(XClassifier cl)
        {
            if(XCSConstants.drand() < XCSConstants.pX)
                sbx_average_lower(cl);
            if (XCSConstants.drand() < XCSConstants.pX)
                sbx_average_upper(cl);

            for(int i = 0;i<this.up_condition.Length;i++)
            {
                double l = double.Parse(this.lw_condition[i]);
                double u = double.Parse(this.up_condition[i]);


                if (l > u)
                {
                    double tmp = u;
                    u = l;
                    l = tmp;
                }

                if (l < 0.0)
                    l = 0.0;
                if (u > 1.0)
                    u = 1.0;

                this.up_condition[i] = u.ToString();
                this.lw_condition[i] = l.ToString();
            }

            for (int i = 0; i < cl.up_condition.Length; i++)
            {
                double l = double.Parse(cl.lw_condition[i]);
                double u = double.Parse(cl.up_condition[i]);


                if (l > u)
                {
                    double tmp = u;
                    u = l;
                    l = tmp;
                }

                if (l < 0.0)
                    l = 0.0;
                if (u > 1.0)
                    u = 1.0;

                cl.up_condition[i] = u.ToString();
                cl.lw_condition[i] = l.ToString();
            }

            return true;
        }

        public bool applyMutation(String[] state, int numberOfActions)
        {
            bool changed = mutateCondition(state);
            if (mutateAction(numberOfActions))
                changed = true;
            return changed;
        }

        private bool mutateCondition(String[] state)
        {
            bool changed = false;
            int condLength = this.up_condition.Length;

            this.specified_cond = new List<double[]>();
            this.specified_adds = new List<int>();

            for (int i = 0; i < condLength; i++)
            {
                if (XCSConstants.drand() < XCSConstants.pM)
                {
                    double l = double.Parse(this.lw_condition[i]);
                    double u = double.Parse(this.up_condition[i]);

                    if (XCSConstants.drand() < 0.5)
                    {
                        l = l + XCSConstants.drand() * XCSConstants.m0;
                    }
                    else
                    {
                        l = l - XCSConstants.drand() * XCSConstants.m0;
                    }

                    if (XCSConstants.drand() < 0.5)
                    {
                        u = u + XCSConstants.drand() * XCSConstants.m0;
                    }
                    else
                    {
                        u = u - XCSConstants.drand() * XCSConstants.m0;
                    }


                    if (l > u)
                    {
                        double tmp = u;
                        u = l;
                        l = tmp;
                    }

                    if (l < 0.0)
                        l = 0.0;
                    if (u > 1.0)
                        u = 1.0;

                    this.up_condition[i] = u.ToString();
                    this.lw_condition[i] = l.ToString();
                }
            }

            for(int i = 0;i<condLength;i++)
            {
                double lw = double.Parse(this.lw_condition[i]);
                double up = double.Parse(this.up_condition[i]);

                if (up-lw != 1)
                {
                    this.specified_cond.Add(new double[] { lw, up });
                    this.specified_adds.Add(i);
                }
            }

            this.count = this.specified_adds.Count();

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

            for (int i = 0; i < this.count; i++)
            {
                int address = this.specified_adds[i];

                if (this.specified_cond[i][0] != double.Parse(cl.lw_condition[address]) || this.specified_cond[i][1] != double.Parse(cl.up_condition[address]))
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
            if (this.count > cl.count)
                return false;

            for (int i = 0; i < this.count; i++)
            {
                int address = this.specified_adds[i];

                if (this.specified_cond[i][0] > double.Parse(cl.lw_condition[address]) || this.specified_cond[i][1] < double.Parse(cl.up_condition[address]))
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
            Console.WriteLine(string.Join(",",lw_condition) + ","+ string.Join(",", up_condition)+" " + action + "\t" + (float)prediction + "\t" + (float)predictionError + "\t" + (float)fitness +
                       "\t" + numerosity + "\t" + experience + "\t" + (float)actionSetSize + "\t" + timeStamp);
        }

        public void printXClassifier(StreamWriter pW)
        {
            pW.WriteLine(string.Join(",", lw_condition) +","+ string.Join(",", up_condition) + "," + action + "," + (float)prediction + "," + (float)predictionError + "," + (float)fitness +
                   "," + numerosity + "," + experience + "," + (float)actionSetSize + "," + timeStamp);
        }
    }

}
