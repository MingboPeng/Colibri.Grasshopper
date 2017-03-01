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

        private List<int> _positions;
        private List<List<int>> allParamsSteps;
        private List<List<int>> selParamsSteps;


        public List<int> Positions
        {
            get { return _positions; }
            private set { _positions = value; }
        }
        public List<List<int>> AllParamsSelectedSteps
        {
            get { return selParamsSteps; }
            private set { }
        }


        public List<GH_Interval> Domains { get; private set; }
        public int SelectedCounts { get; private set; }
        public int TotalCounts { get; private set; }
        

        //Construction
        public IteratorSelection()
        {
            this.IsDefined = false;
        }
        public IteratorSelection(List<int> Steps, List<GH_Interval> Domains)
        {
            this.IsDefined = true;
            this._positions = Steps;
            this.Domains = Domains;
            //this.SelectedCounts = calSelectedCounts(Steps, Domains);
        }

        

        //Method

        //this.Steps checked
        //this.SelectedCounts checked
        public void MatchSelectionFrom (List<ColibriParam> ColibriParams)
        {
            this.TotalCounts = ColibriBase.CalTotalCounts(ColibriParams);
            this.allParamsSteps = ColibriBase.AllParamsStepsIndex(ColibriParams);
            this._positions = iniSteps(this._positions, ColibriParams);
            this.selParamsSteps = calSelectedParamsSteps(this.allParamsSteps, this._positions, ColibriParams);

            this.SelectedCounts = calSelectedTotalCount(this.selParamsSteps);

        }

        //private void iniColibriParams(List<ColibriParam> ColibriParams)
        //{
            
        //}

        private int calSelectedTotalCount(List<List<int>> SelAllParamsSteps)
        {
            var selAllParamsSteps = SelAllParamsSteps;
            //var fullSteps = new List<int>();
            //foreach (var item in this.allParamsSteps)
            //{
            //    fullSteps.Add(item.Count);

            //}

            int runIterationNumber = 1;

            //OLd

            //if (Steps.Count == fullSteps.Count)
            //{

            //    for (int i = 0; i < fullSteps.Count; i++)
            //    {
            //        //todo: cal steps to match  ColibriParams numbers

            //        //cal run numbers
            //        int step = Steps[i];
            //        int fullStep = fullSteps[i];

            //        if (step < fullStep && step > 0)
            //        {
            //            runIterationNumber *= step;
            //        }
            //        else
            //        {
            //            runIterationNumber *= fullStep;
            //        }

            //    }
            //}
            //else
            //{
            //    runIterationNumber = this.TotalCounts;
            //}

            if (selAllParamsSteps.Count == this.allParamsSteps.Count)
            {
                foreach (var item in selAllParamsSteps)
                {
                    runIterationNumber *= item.Count;
                }
            }
            else
            {
                runIterationNumber = this.TotalCounts;
            }



            return runIterationNumber;
        }

        private List<List<int>> calSelectedParamsSteps(List<List<int>> ParamsPositions, List<int> Positions, List<ColibriParam> ColibriParams)
        {
            int paramCounts = ParamsPositions.Count;
            var selParamsStepIndex = new List<List<int>>();

            for (int p = 0; p < paramCounts; p++)
            {
                //var selStepIndex = new List<int>();
                int step = Positions[p];
                var stepIndexList = ParamsPositions[p];
                int totalSteps = stepIndexList.Count;

                //if 0 to run all
                if (step ==0)
                {
                    selParamsStepIndex.Add(ParamsPositions[p]);
                    continue;
                }

                //if 1 to run the current
                if (step == 1)
                {
                    var currentPositon = new List<int>() { ColibriParams[p].Position };
                    selParamsStepIndex.Add(currentPositon);
                    continue;
                }

                int stepMax = totalSteps - 1;
                int stepMid = stepMax / 2;
                double numAdd = (double)stepMax / (step - 1);

                var thisStepIndex = new List<int>(step);
                thisStepIndex.Add(0);

                for (int i = 0; i < step - 1; i++)
                {
                    int nextPosition = 0;
                    if (thisStepIndex[i] + numAdd >= stepMid)
                    {
                        nextPosition = (int)Math.Ceiling(numAdd + thisStepIndex[i]);
                    }
                    else
                    {
                        nextPosition = (int)Math.Floor(numAdd + thisStepIndex[i]);
                    }

                    thisStepIndex.Add(nextPosition);

                }

                thisStepIndex[step - 1] = stepMax;

                selParamsStepIndex.Add(thisStepIndex);
            }

            return selParamsStepIndex;
        }

        private List<int> iniSteps(List<int> positions, List<ColibriParam> colibriParams)
        {
            var steps = new List<int>();
            if (positions == null || !positions.Any())
            {
                steps = Enumerable.Repeat(0, colibriParams.Count()).ToList();
            }
            else
            {
                steps = positions;
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
            if (_positions.Count != 0)
            {
                outString += "Take:" + _positions.Count + "\n";
            }

            if (Domains.Count != 0)
            {
                outString += "Domain:" + Domains.Count + "\n"; ;
            }

            return outString;

        }

    }
}
