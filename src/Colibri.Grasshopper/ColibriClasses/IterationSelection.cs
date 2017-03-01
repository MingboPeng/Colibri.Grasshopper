using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colibri.Grasshopper
{
    public class IteratorSelection
    {
        public bool IsDefined { get; private set; }
        public List<int> Steps { get; private set; }
        public List<GH_Interval> Domains { get; private set; }
        public int SelectedCounts { get; private set; }
        public int TotalCounts { get; private set; }

        private List<List<int>> allParamsSteps;

        //Construction
        public IteratorSelection()
        {
            this.IsDefined = false;
        }
        public IteratorSelection(List<int> Steps, List<GH_Interval> Domains)
        {
            this.IsDefined = true;
            this.Steps = Steps;
            this.Domains = Domains;
            //this.SelectedCounts = calSelectedCounts(Steps, Domains);
        }

        //Method
        public int MatchSelectionFrom (List<ColibriParam> ColibriParams)
        {
            iniColibriParams(ColibriParams);
            this.Steps = iniSteps(this.Steps, ColibriParams);

            var fullSteps = new List<int>();
            foreach (var item in this.allParamsSteps)
            {
                fullSteps.Add(item.Count);

            }


            int runIterationNumber = 1;

            if (this.Steps.Count == fullSteps.Count)
            {
                

                for (int i = 0; i < fullSteps.Count; i++)
                {
                    //todo: cal steps to match  ColibriParams numbers
                    
                    //cal run numbers
                    int step = Steps[i];
                    int fullStep = fullSteps[i];

                    if (step < fullStep && step > 0)
                    {
                        runIterationNumber *= step;
                    }
                    else
                    {
                        runIterationNumber *= fullStep;
                    }
                        
                }
            }
            else
            {
                runIterationNumber = this.TotalCounts;
            }

            return runIterationNumber;

        }

        private void iniColibriParams(List<ColibriParam> ColibriParams)
        {
            this.TotalCounts = ColibriBase.CalTotalCounts(ColibriParams);
            this.allParamsSteps = ColibriBase.AllParamsStepsIndex(ColibriParams);
            
        }

        private List<int> iniSteps(List<int> Steps, List<ColibriParam> ColibriParams)
        {
            var steps = new List<int>();
            if (Steps == null)
            {
                steps = Enumerable.Repeat(0, ColibriParams.Count()).ToList();
            }
            else
            {
                steps = Steps;
            }
            return steps;
            
        }

        //private int calSelectedCounts(List<int> Steps, List<GH_Interval> Domains)
        //{
        //    int totalIterations = 1;

        //    foreach (var item in Steps)
        //    {
        //        int totalCount = item.TotalCount;
        //        if (totalCount > 0)
        //        {
        //            totalIterations *= totalCount;
        //        }
        //    }
        //    return 0;
        //}

        //override
        public override string ToString()
        {
            if (!IsDefined)
            {
                return null;
            }

            string outString = "";
            if (Steps.Count != 0)
            {
                outString += "Take:" + Steps.Count + "\n";
            }

            if (Domains.Count != 0)
            {
                outString += "Domain:" + Domains.Count + "\n"; ;
            }

            return outString;

        }

    }
}
