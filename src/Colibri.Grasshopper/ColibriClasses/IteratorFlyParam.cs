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
            this._allPositions = ColibriBase.AllParamsPositions(this._inputParams);
            this._allSelectedPositions = _selections.ParamsSelectedPositions == null? this._allPositions : _selections.ParamsSelectedPositions;
            this.currentPositionsIndex = Enumerable.Repeat(0, _inputParams.Count()).ToList();
            Count = 0;
            
            createWatchFile(studyFolder);

        }


        #region Methods
        
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

                
                //move to the next set of slider positions
                isRunning = MoveToNextPermutation(ref currentParamIndex);

                Count++;

                //watch the selection
                bool isInSelection = ifInSelectionDomains(_selections, Count);
                if (isInSelection)
                {

                    // We've just got a new valid permutation. Solve the new solution.
                    e.Document.NewSolution(false);
                    
                }

                //todo: check the First 0 position which would be execulted at the end.

                
                //isRunning = Count < _selectedCounts;


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
                    e.Document.NewSolution(false);
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
            {
                return false;
            }

            //System.Windows.Forms.MessageBox.Show(Count.ToString());
            

            var currentParam = _inputParams[MoveToParamIndex];
            var thisSelectedPositions = _allSelectedPositions[MoveToParamIndex];
            //int currentStepPosition = currentStepPositionsIndex[MoveToParamIndex];

            int nextPositionIndex = currentPositionsIndex[MoveToParamIndex]+1; 
            

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
                //System.Windows.Forms.MessageBox.Show("index++" + MoveToParamIndex.ToString());
                //// If we've run out of sliders to modify, we're done permutatin'
                if (MoveToParamIndex >= _inputParams.Count)
                {
                    return false;
                }
                    
                
                return MoveToNextPermutation(ref MoveToParamIndex);
            }


        }


        //private static int calClosestTick()
        //{
        //    //int totalNumberOfSteps = sliderSteps[index];
        //    //int currentSliderStepsPosition = sliderStepsPositions[index];
        //    //int sliderMidStep = slider.TickCount / 2;
        //    //int numTicksToAddAsInt = slider.TickCount / totalNumberOfSteps;
        //    //double numTicksToAddAsDouble = (double)slider.TickCount / (double)totalNumberOfSteps;

        //    ////find the closest tick
        //    //int closestTick = 0;
        //    //if (currentSliderStepsPosition + numTicksToAddAsInt >= sliderMidStep)
        //    //{
        //    //    closestTick = (int)Math.Ceiling(numTicksToAddAsDouble * currentSliderStepsPosition);
        //    //}
        //    //else
        //    //{
        //    //    closestTick = (int)Math.Floor(numTicksToAddAsDouble * currentSliderStepsPosition);
        //    //}

        //    //return closestTick;
        //    return 0;
        //}

        private bool ifInSelectionDomains(IteratorSelection Selections, int CurrentCount)
        {
            //Selections undefined, so all is in seleciton
            if (!Selections.IsDefinedInSel)
            {
                return true;
            }

            var currentCount = (double)CurrentCount;
            bool isInSelection = true;

            if (Selections.Domains.Any())
            {
                foreach (var domain in Selections.Domains)
                { //todo: bug, the following will override the isInselection if the current count is inclued in previous domain.
                    isInSelection = domain.Value.IncludesParameter(currentCount);
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
