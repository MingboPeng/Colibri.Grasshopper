using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;
using System.IO;

namespace Colibri.Grasshopper
{
    public class IteratorFlyParam
    {

        //InputParams objects List
        private List<ColibriParam> _inputParams;

        public List<ColibriParam> InputParams
        {
            get { return _inputParams; }
            set { _inputParams = value; }
        }


        //List of each input param's all steps index
        private List<List<int>> _allPositions;
        private List<List<int>> _allSelectedPositions;
        private int _selectedCounts;
        //public List<List<int>> InputParamsStepLists
        //{
        //    get { return inputParamsStepLists; }
        //    set { inputParamsStepLists = value; }
        //}

        //current each position
        private List<int> currentPositionsIndex { get; set; }

        //current counts
        public static int Count { get; private set; }
       
        // Total Iteration number int
        public int _totalCounts { get; private set; }
        

        private string watchFilePath { get; set; }

        private IteratorSelection _selections { get; set; }

        //constructor 
        public IteratorFlyParam(){}

        public IteratorFlyParam(List<ColibriParam> sourceParams, IteratorSelection selections, string studyFolder)
        {
            this._inputParams = sourceParams;
            this._selections = selections == null? new IteratorSelection(): selections;

            this._totalCounts = ColibriBase.CalTotalCounts(this._inputParams);
            this._selectedCounts = _selections.SelectedCounts > 0 ? _selections.SelectedCounts : _totalCounts;
            this._allPositions = ColibriBase.AllParamsStepsIndex(this._inputParams);
            this._allSelectedPositions = _selections.AllParamsSelectedSteps == null? this._allPositions : _selections.AllParamsSelectedSteps;
            this.currentPositionsIndex = Enumerable.Repeat(0, _inputParams.Count()).ToList();
            Count = 0;
            
            createWatchFile(studyFolder);

        }


        #region Methods

        //to get all params' all steps' indexes 
        //private List<List<int>> iniAllParamsStepsList(List<ColibriParam> ColibriParams)
        //{
        //    var stepsList = new List<List<int>>();
        //    foreach (var item in ColibriParams)
        //    {
        //        int totalCount = item.TotalCount;
        //        if (totalCount >0)
        //        {
        //            var SetpList = Enumerable.Range(0, totalCount).ToList();
        //            stepsList.Add(SetpList);
        //        }
        //    }
        //    return stepsList;
        //    //inputParamsStepLists = stepLists;
        //}

        //private int calTotalIterations(List<ColibriParam> ColibriParams)
        //{
            
        //    int countSum = 1;
            
        //    foreach (var item in ColibriParams)
        //    {
        //        int totalCount = item.TotalCount;
        //        if (totalCount > 0)
        //        {
        //            countSum *= totalCount;
        //        }
        //    }

        //    return countSum;

        //}
        
        //create a watch file 
        private void createWatchFile(string FolderPath)
        {
            if (!string.IsNullOrEmpty(FolderPath))
            {
                watchFilePath = FolderPath + "\\running.txt";
                try
                {
                    File.WriteAllText(watchFilePath, "running");
                }
                catch (Exception)
                {
                    throw;
                }

            }
        }
        
        private void FirstResetAll()
        {
            foreach (var item in InputParams)
            {
               item.Reset();
            }
        }

        

        public void FlyAll(GH_SolutionEventArgs e)
        {
            
            FirstResetAll();
            
            while (true)
            {

                int currentParamIndex = 0;
                bool isRunning = true;

                //watch the selection
                bool isInSelection = ifInSelection(_selections, Count);
                if (isInSelection)
                {
                    //move to the next set of slider positions
                    isRunning = MoveToNextPermutation(ref currentParamIndex);
                    

                    // We've just got a new valid permutation. Solve the new solution.
                    e.Document.NewSolution(false);
                }
                
                Count++;


                isRunning = Count < _selectedCounts;



                //Rhino.RhinoDoc.ActiveDoc.Views.Redraw();

                //watch the file to stop
                if (!string.IsNullOrEmpty(watchFilePath))
                {
                    if (!File.Exists(watchFilePath))
                    {
                        // watch file was deleted by user
                        isRunning = false;
                    }
                }

                if (!isRunning )
                {
                    // study is over!
                    
                    if (File.Exists(watchFilePath))
                    {
                        File.Delete(watchFilePath);
                    }

                    break;
                }
                
            }
            
        }
        public void FlyTest(GH_SolutionEventArgs e, int testNumber)
        {
            
            if (_totalCounts <= testNumber)
            {
                FlyAll(e);
                return;
            }

            FirstResetAll();

            int totalParamCount = _inputParams.Count;
            var random = new Random();

            var flewID = new HashSet<string>();

            while (true)
            {
                //pick random input param
                int randomParamIndex = random.Next(totalParamCount);
                var currentInputParam = _inputParams[randomParamIndex];

                //pick random step of param
                int randomStepIndex = random.Next(currentInputParam.TotalCount);
                var flyID = randomParamIndex + "_" + randomStepIndex;

                //test if the same flyID has already ran
                if (!flewID.Contains(flyID))
                {
                    
                    flewID.Add(flyID);
                    currentInputParam.SetParamTo(randomStepIndex);
                    Count++;

                    e.Document.NewSolution(false);

                    if (flewID.Count >= testNumber)
                    {
                        // study is over!
                        break;
                    }

                }
            }
        }
        
       
        private bool MoveToNextPermutation(ref int MoveToParamIndex)
        {

           
            if (MoveToParamIndex >= _inputParams.Count)
                return false;
            
            var currentParam = _inputParams[MoveToParamIndex];
            var thisSelectedPositions = _allSelectedPositions[MoveToParamIndex];
            //int currentStepPosition = currentStepPositionsIndex[MoveToParamIndex];

            int nextPositionIndex = currentPositionsIndex[MoveToParamIndex] + 1; 
            

            if (nextPositionIndex < thisSelectedPositions.Count)
            {
                //Figure out which step to fly to...

                //look up the current slider's current sliderStepsPosition and target number
                //int closestTick = calClosestTick();

                //calClosestTick();

                int nextPosition = thisSelectedPositions[nextPositionIndex];
                //The current component is already at the maximum value. Reset it back to zero.
                currentParam.SetParamTo(nextPosition);
                //currentInputParam.SetToNext();

                //Increment the current step position
                this.currentPositionsIndex[MoveToParamIndex]++ ;
                
                return true;
            }else
            {
                //currentParam.Reset();
                currentParam.SetParamTo(thisSelectedPositions.First());

                ////set our slider steps position back to 0
                this.currentPositionsIndex[MoveToParamIndex] = 0;

                //// Move on to the next slider.
                MoveToParamIndex++;
                //System.Windows.Forms.MessageBox.Show("index++"+MoveToIndex.ToString());
                //// If we've run out of sliders to modify, we're done permutatin'
                if (MoveToParamIndex >= _inputParams.Count)
                    return false;
                
                return MoveToNextPermutation(ref MoveToParamIndex);
            }
        }


        private static int calClosestTick()
        {
            //int totalNumberOfSteps = sliderSteps[index];
            //int currentSliderStepsPosition = sliderStepsPositions[index];
            //int sliderMidStep = slider.TickCount / 2;
            //int numTicksToAddAsInt = slider.TickCount / totalNumberOfSteps;
            //double numTicksToAddAsDouble = (double)slider.TickCount / (double)totalNumberOfSteps;

            ////find the closest tick
            //int closestTick = 0;
            //if (currentSliderStepsPosition + numTicksToAddAsInt >= sliderMidStep)
            //{
            //    closestTick = (int)Math.Ceiling(numTicksToAddAsDouble * currentSliderStepsPosition);
            //}
            //else
            //{
            //    closestTick = (int)Math.Floor(numTicksToAddAsDouble * currentSliderStepsPosition);
            //}

            //return closestTick;
            return 0;
        }

        private bool ifInSelection(IteratorSelection Selections, int CurrentCount)
        {
            //Selections undefined, so all is in seleciton
            if (!Selections.IsDefined)
            {
                return true;
            }

            var currentCount = (double)CurrentCount;
            bool isInSelection = true;

            if (Selections.Domains.Any())
            {
                foreach (var item in Selections.Domains)
                {
                    isInSelection = item.Value.IncludesParameter(currentCount);
                }
            }
            return isInSelection;

        }

        //private bool isInSelection(List<int> FlyIndexID)
        //{

        //    return true;
        //}

        #endregion

    }

}
