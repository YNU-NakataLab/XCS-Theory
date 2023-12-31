# XCS Classifier System with Theoretical Parameter Settings for C#

This is an open source code of the XCS [learning classifier system](https://en.wikipedia.org/wiki/Learning_classifier_system) for binary- and real-valued inputs (i.e., ternary alphabet and ordered-bound hyperrectangular representation are employed as a rule antecedent, respectively) written in C# using theoretical parameter settings. This code is based on XCS-Java given by Martin V. Butz:

``
Butz, M. V. (2000). XCSJava 1.0: An Implementation of the XCS classi£ er system in Java. Technical Report 2000027, Illinois Genetic Algorithms Laboratory
``

Note that, for speeding up, this code employs the "messy-coding" like rule-matching process with paralell computing, which returns the same result of normal matching process.

You should receive the GNU General Public License. This code can be distributed WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABLILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

## What is XCS?

Learning Classifier Systems (LCSs), a paradigm of evolutionary rule-based machine learning, have been applied to modern issues in the machine learning field. Wilson’s XCS classifier system is the most successful and best-studied LCS algorithm to date. 

XCS stands for Accuracy-Based Learning Classifier System (or eXtended Classifier System), a classification framework based on a reinforcement learning technique. It seeks to form accurate, maximally general rules that together classify the state space of a given domain. The framework uses an accuracy-based fitness to provide a natural pressure to produce maximally accurate rules. In contrast, a rule-reduction method, i.e., the subsumption operator, allows pressure to extract the maximally general rules. The cooperative cycle with both pressures enables XCS to eventually retain the maximally accurate and general rules if the evolutionary computation methods can generate them. 

## The Power of Our Learning Optimality Theory


We have derived a theoretical approach that mathematically guarantees that XCS identifies maximally accurate rules in the fewest iterations possible, which also returns a theoretically valid hyperparameter setting. We also experimentally show that our theoretical setting enables XCS to easily solve several challenging problems where it had previously struggled. This contribution has been published on IEEE Transactions on Evolutionary Computation.



## How to Run

 1. Open "xcs_coding.sln" with Microsoft Visual Studio.

 2. Decide which problem to solve from "xcs_binary_coding/Program.cs" for binary-valued inputs or "xcs_realvalue_coding/Program.cs" for real-valued inputs.

 3. Run with or without debug.

## Copyright
> The copyright belongs to Masaya Nakata at Yokohama National University, Japan. You are free to use this code for research purposes. Please refer the following article: "Masaya Nakata and Will N. Browne. Learning optimality theory for accuracy-based learning classifier systems. IEEE Transactions on Evolutionary Computation 25.1 (2020): 61-74."

```
@article{nakata2020learning,
title={Learning optimality theory for accuracy-based learning classifier systems},
author={Nakata, Masaya and Browne, Will N},
journal={IEEE Transactions on Evolutionary Computation},
volume={25},
number={1},
pages={61--74},
year={2020},
publisher={IEEE}
}
```
## References
S. W. Wilson, "**Classifier Fitness Based on Accuracy**," Evolutionary Computation, Vol.3, No.2, pp. 149-175, June 1995. https://doi.org/10.1162/evco.1995.3.2.149  
<sub> The original paper of XCS for binary-valued inputs using ternary alphabet representation.</sub>

M. V. Butz, S. W. Wilson, "**An Algorithmic Description of XCS**," S. Soft Computing, Volume 6, Issue 3-4, pp. 144-153, June 2002. https://doi.org/10.1007/s005000100111  
<sub>The tutorial for the XCS implementation.</sub>

S. W. Wilson, "**Get Real! XCS with Continuous-Valued Inputs**," Learning Classifier Systems, IWLCS 1999, Lecture Notes in Computer Science, vol 1813, pp. 209-219, July 2000. https://doi.org/10.1007/3-540-45027-0_11  
<sub>The original paper of XCSR (XCS for real-valued inputs).</sub>  
<sub>The Center-Spread hyperrectangular representation is proposed in this paper.</sub>

S. W. Wilson, "**Mining Oblique Data with XCS**," Advances in Learning Classifier Systems, IWLCS 2000, Lecture Notes in Computer Science, vol 1996, pp. 158-174, August 2001. https://doi.org/10.1007/3-540-44640-0_11  
<sub>The original paper of XCSI (XCS for integer-valued inputs).</sub>

C. Stone, L. Bull, "**For Real! XCS with Continuous-Valued Inputs**," Evolutionary Computation, Vol. 11, No.3, pp.299-336, September 2003. https://doi.org/10.1162/106365603322365315  
<sub>The Ordered Bound hyperrectangular representation is proposed based on the interval-based integer representation in XCSI. </sub>