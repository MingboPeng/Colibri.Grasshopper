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
        public bool IsDefinedInSel { get; private set; }

        
        public int SelectedCounts { get; private set; }
        public int TotalCounts { get; private set; }

        public List<int> PositionNumbers
        {
            get { return _positionNumbers; }
            private set { _positionNumbers = value; }
        }
        public List<GH_Interval> Domains
        {
            get { return _domains; }
            private set { _domains = value; }
        }
        public List<List<int>> ParamsSelectedPositions
        {
            get { return _paramsSelectedPositions; }
            private set { }
        }

        private List<int> _positionNumbers;
        private List<GH_Interval> _domains;
        private List<List<int>> _paramsPositions;
        private List<List<int>> _paramsSelectedPositions;

        //Construction
        public IteratorSelection()
        {
            this.IsDefinedInSel = false;
        }
        public IteratorSelection(List<int> takeNumbers, List<GH_Interval> domains)
        {
            //is defined in Selection component
            this.IsDefinedInSel = true;
            this._positionNumbers = takeNumbers;
            this._domains = domains;
        }

        
        //Method

        
        public void MatchSelectionFrom (List<ColibriParam> ColibriParams)
        {
            this.TotalCounts = ColibriBase.CalTotalCounts(ColibriParams);

            this._paramsPositions = ColibriBase.AllParamsStepsIndex(ColibriParams);
            this._positionNumbers = iniPositionNumbers(this._positionNumbers, ColibriParams);
            this._paramsSelectedPositions = calParamsSelectedPositions(this._paramsPositions, this._positionNumbers, ColibriParams);

            this.SelectedCounts = calSelectedTotalCount(this._paramsSelectedPositions);
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

            if (selAllParamsSteps.Count == this._paramsPositions.Count)
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

        private List<List<int>> calParamsSelectedPositions(List<List<int>> paramsPositions, List<int> selectedPositionNumbers, List<ColibriParam> ColibriParams)
        {
            int paramCounts = paramsPositions.Count;
            var paramsSelectedPositions = new List<List<int>>();

            for (int p = 0; p < paramCounts; p++)
            {
                //var selStepIndex = new List<int>();
                
                
                var positionList = paramsPositions[p];
                int totalPositionNumber = positionList.Count;
                int positionNumber = Math.Min(selectedPositionNumbers[p], totalPositionNumber);
                var thisParamPosition = new List<int>();


                //if 0 to run all
                if (positionNumber == 0)
                {
                    paramsSelectedPositions.Add(positionList);
                    continue;
                }

                //if 1 to run the current
                if (positionNumber == 1)
                {
                    thisParamPosition = new List<int>() { ColibriParams[p].Position };
                    paramsSelectedPositions.Add(thisParamPosition);
                    continue;
                }

                //select positions 
                int positionMax = totalPositionNumber - 1;
                int positionMid = positionMax / 2;
                double numAdd = (double)positionMax / (positionNumber - 1);

                thisParamPosition = new List<int>(positionNumber);
                thisParamPosition.Add(0);

                for (int i = 0; i < positionNumber - 1; i++)
                {
                    int nextPosition = 0;
                    if (thisParamPosition[i] + numAdd >= positionMid)
                    {
                        nextPosition = (int)Math.Floor(numAdd + thisParamPosition[i]);
                    }
                    else
                    {
                        nextPosition = (int)Math.Ceiling(numAdd + thisParamPosition[i]);
                    }

                    thisParamPosition.Add(nextPosition);

                }

                thisParamPosition[positionNumber - 1] = positionMax;

                paramsSelectedPositions.Add(thisParamPosition);
            }

            return paramsSelectedPositions;
        }

        private List<int> iniPositionNumbers(List<int> oldPositionNumbers, List<ColibriParam> colibriParams)
        {
            var positionNumbers = new List<int>();
            if (oldPositionNumbers == null || !oldPositionNumbers.Any())
            {
                positionNumbers = Enumerable.Repeat(0, colibriParams.Count()).ToList();
            }
            else
            {
                positionNumbers = oldPositionNumbers;
            }
            return positionNumbers;
            
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
            if (!IsDefinedInSel)
            {
                return null;
            }

            string outString = "";
            if (_positionNumbers.Count != 0)
            {
                outString += "Take:" + _positionNumbers.Count + "\n";
            }

            if (Domains.Count != 0)
            {
                outString += "Domain:" + _domains.Count + "\n"; ;
            }

            return outString;

        }

    }
}
