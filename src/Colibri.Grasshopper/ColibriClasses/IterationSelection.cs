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

        public List<int> ParamsTakeNumbers
        {
            get { return _paramsTakeNumbers; }
            private set { _paramsTakeNumbers = value; }
        }
        public List<GH_Interval> Domains
        {
            get { return _domains; }
            set { _domains = value; }
        }
        
        public List<List<int>> ParamsSelectedPositions
        {
            get { return _paramsSelectedPositions; }
            private set { }
        }


        private List<int> _paramsTakeNumbers;
        private List<GH_Interval> _domains;
        private List<List<int>> _paramsPositions;
        private List<List<int>> _paramsSelectedPositions;

        //the entire range domain, from 0 the last
        private GH_Interval _totalDomain;

        //User inputs
        public List<GH_Interval> UserDomains { get { return _userDomains; } private set {} }
        private List<GH_Interval> _userDomains;
        public List<int> UserTakes { get { return _userParamsTakeNumbers; } private set {} }
        private List<int> _userParamsTakeNumbers;

        //Construction
        public IteratorSelection()
        {
            this.IsDefinedInSel = false;
        }
        public IteratorSelection(List<int> takeNumbers, List<GH_Interval> domains)
        {
            
            //Both are empty 
            if (takeNumbers.IsNullOrEmpty() && domains.IsNullOrEmpty())
            {
                this.IsDefinedInSel = false;
            }
            else
            {
                this.IsDefinedInSel = true;
                this._userParamsTakeNumbers = takeNumbers;
                this._userDomains = domains;
            }
        }

        
        //Method
        public void MatchSelectionFrom (List<ColibriParam> ColibriParams)
        {

            this.TotalCounts = ColibriBase.CalTotalCounts(ColibriParams);
            this._totalDomain = new GH_Interval(new Rhino.Geometry.Interval(0,this.TotalCounts-1));
            this._domains = iniDomains(this._userDomains, this._totalDomain);

            this._paramsPositions = ColibriBase.AllParamsPositions(ColibriParams);
            this._paramsTakeNumbers = iniTakeNumbers(this._userParamsTakeNumbers, ColibriParams);
            this._paramsSelectedPositions = calParamsSelectedPositions(this._paramsPositions, this._paramsTakeNumbers, ColibriParams);

           
            this.SelectedCounts = calSelectedTotalCount(this._paramsSelectedPositions, this._domains);
        }

        //private void iniColibriParams(List<ColibriParam> ColibriParams)
        //{
            
        //}

        private int calSelectedTotalCount(List<List<int>> allParamsSelectedPositions, List<GH_Interval> domains)
        {
            int selectedTotal = 0;

            //Total count from Domains
            int totalDomainsLength = 0;
            foreach (var item in domains)
            {
                totalDomainsLength += (int)item.Value.Length + 1;

            }

            //Total count from params positions
            int runIterationNumber = 1;
            
            if (allParamsSelectedPositions.Count == this._paramsPositions.Count)
            {
                foreach (var item in allParamsSelectedPositions)
                {
                    runIterationNumber *= item.Count;
                }
            }
            else
            {
                runIterationNumber = this.TotalCounts;
            }

            selectedTotal = Math.Min(totalDomainsLength, runIterationNumber);


            return selectedTotal;
        }

        

        private List<int> iniTakeNumbers(List<int> userTakeNumbers, List<ColibriParam> colibriParams)
        {
            var takeNumbers = new List<int>();
            if (userTakeNumbers.IsNullOrEmpty())
            {
                takeNumbers = Enumerable.Repeat(0, colibriParams.Count()).ToList();
            }
            else if(userTakeNumbers.Count != colibriParams.Count)
            {
                takeNumbers = Enumerable.Repeat(0, colibriParams.Count()).ToList();
            }
            else
            {
                takeNumbers = userTakeNumbers;
            }
            return takeNumbers;
            
        }

        private List<GH_Interval> iniDomains(List<GH_Interval> oldDomains, GH_Interval totalDomain)
        {
            var domains = new List<GH_Interval>();

            var checkedOldDomain = trimDomains(oldDomains, totalDomain);
            
            if (checkedOldDomain.IsNullOrEmpty())
            {
                domains.Add(totalDomain);
            }
            else
            {
                domains = checkedOldDomain;
            }

            
            return domains;

        }

        private List<GH_Interval> trimDomains(List<GH_Interval> oldDomains, GH_Interval totalDomain)
        {
            if (oldDomains.IsNullOrEmpty())
            {
                return null;
            }

            var newDomains = new List<GH_Interval>();
            foreach (var item in oldDomains)
            {
                double min = 0;
                double max = 0;

                //Check the T0 side
                if (item.Value.Min < 0)
                {
                    min = 0;
                }
                else
                {
                    min = item.Value.Min;
                }

                //Check the T1 side
                if (item.Value.Max > totalDomain.Value.Max)
                {
                    max = totalDomain.Value.Max;
                }
                else
                {
                    max = item.Value.Max;
                }

                var newIterval = new Rhino.Geometry.Interval(min, max);

                if (totalDomain.Value.IncludesInterval(newIterval))
                {
                    newDomains.Add(new GH_Interval(newIterval));
                }
                

            }

            return newDomains;


        }

        private List<List<int>> calParamsSelectedPositions(List<List<int>> paramsPositions, List<int> selectedPositionNumbers, List<ColibriParam> ColibriParams)
        {

            int paramCounts = paramsPositions.Count;
            var paramsSelectedPositions = new List<List<int>>();

            //if (selectedPositionNumbers.Count ==1)
            //{
            //    return paramsPositions;
            //}


            
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
                }

                //if 1 to run the current
                else if (positionNumber == 1)
                {
                    thisParamPosition = new List<int>() { ColibriParams[p].Position };
                    paramsSelectedPositions.Add(thisParamPosition);
                }

                //select positions 
                else
                {
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
                
                
            }

            return paramsSelectedPositions;
        }

        //private List<List<int>> calPatamsPositionsOnDomains(List<List<int>> paramsPositions, List<GH_Interval> domains)
        //{
        //    int count = 0;
        //    var rangedParamsPositions = new List<List<int>>();

        //    foreach (var paramPositions in paramsPositions)
        //    {


        //        rangedParamsPositions.Add(new List<int>());
        //        foreach (var position in paramPositions)
        //        {

        //            //check each position in each domain
        //            foreach (var domain in domains)
        //            {
        //                if (domain.Value.IncludesParameter(position))
        //                {
        //                    rangedParamsPositions.Last().Add(position);
        //                }
        //            }



        //            count++;
        //        }





        //    }



        //    return rangedParamsPositions;
        //}



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
            var domains = _userDomains;
            var takes = _userParamsTakeNumbers;

            string outString = "";
            if (takes.Count != 0)
            {
                outString += "Take:\n";
                foreach (var item in takes)
                {
                    outString += " [" + item + "]";
                }
            }

            if (domains.Count != 0)
            {
                outString += "Domain:\n";
                foreach (var item in domains)
                {
                    outString += (int)item.Value.Min + " TO " + (int)item.Value.Max + "\n";
                }
            }

            return outString;

        }
        //this is for the real selection setting that iterator will use for fly
        public string ToString(bool allInfo)
        {
            if (!IsDefinedInSel)
            {
                return null;
            }
            var domains = _domains;
            var takes = _paramsSelectedPositions;

            var userDomains = _userDomains;
            var userTakes = _userParamsTakeNumbers;
            string outString = "";

            if (userTakes.Count != 0)
            {
                outString += "Take\n";
                foreach (var item in takes)
                {
                    outString += " ["+item.Count + "]";
                }
            }

            if (userDomains.Count != 0)
            {
                outString += "Domain\n";
                foreach (var item in domains)
                {
                    int length = (int)item.Value.Length + 1;
                    outString += (int)item.Value.Min + " TO " + (int)item.Value.Max  + " ["+ length +"]\n";
                }
            }

            return outString;

        }

    }
}
