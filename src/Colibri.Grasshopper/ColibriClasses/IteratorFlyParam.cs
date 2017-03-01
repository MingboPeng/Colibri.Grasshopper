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
        private List<ColibriParam> inputParams;

        public List<ColibriParam> InputParams
        {
            get { return inputParams; }
            set { inputParams = value; }
        }


        //List of each input param's all steps index
        private List<List<int>> allParamsSteps;

        //public List<List<int>> InputParamsStepLists
        //{
        //    get { return inputParamsStepLists; }
        //    set { inputParamsStepLists = value; }
        //}

        //current each position
        private List<int> currentStepPositions { get; set; }

        //current counts
        public static int Count { get; private set; }
       
        // Total Iteration number int
        public int TotalIterations { get; private set; }

        private string watchFilePath { get; set; }

        private IteratorSelection selections { get; set; }

        //constructor 
        public IteratorFlyParam(){}

        public IteratorFlyParam(List<ColibriParam> SourceParams, IteratorSelection Selection, string StudyFolder)
        {
            this.inputParams = SourceParams;
            this.selections = Selection == null? new IteratorSelection():Selection;

            this.TotalIterations = ColibriBase.CalTotalCounts(this.inputParams);
            this.allParamsSteps = ColibriBase.AllParamsStepsIndex(this.inputParams);
            this.currentStepPositions = Enumerable.Repeat(0, inputParams.Count()).ToList();
            Count = 0;
            
            createWatchFile(StudyFolder);

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

        public void FlyAll(GH_SolutionEventArgs e)
        {
            
            FirstResetAll();
            
            while (true)
            {

                int currentParamIndex = 0;
                bool isRunning = true;

                //watch the selection
                bool isInSelection = ifInSelection(selections, Count);
                if (isInSelection)
                {
                    //move to the next set of slider positions
                    isRunning = MoveToNextPermutation(ref currentParamIndex);
                    

                    // We've just got a new valid permutation. Solve the new solution.
                    e.Document.NewSolution(false);
                }
                
                Count++;


                isRunning = Count < TotalIterations;
                
                
                
                //Rhino.RhinoDoc.ActiveDoc.Views.Redraw();


                if (!isRunning )
                {
                    // study is over!
                    try
                    {
                       File.Delete(watchFilePath);
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    break;
                }

                //watch the file to stop
                if (!string.IsNullOrEmpty(watchFilePath))
                {
                    if (!File.Exists(watchFilePath))
                    {
                        // watch file was deleted by user
                        break;
                    }
                }


                
                
            }
            
        }
        public void FlyTest(GH_SolutionEventArgs e, int testNumber)
        {
            
            if (TotalIterations<= testNumber)
            {
                FlyAll(e);
                return;
            }

            FirstResetAll();

            int totalParamCount = inputParams.Count;
            var random = new Random();

            var flewID = new HashSet<string>();

            while (true)
            {
                //pick random input param
                int randomParamIndex = random.Next(totalParamCount);
                var currentInputParam = inputParams[randomParamIndex];

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

           
            if (MoveToParamIndex >= inputParams.Count)
                return false;
            
            var currentParam = inputParams[MoveToParamIndex];
            int currentStep = currentStepPositions[MoveToParamIndex];
            //int nextStepPosition = currentStepPositions[MoveToParamIndex] + 1; //old

            int nextStepPosition = allParamsSteps[MoveToParamIndex][currentStep] + 1;
            
            if (nextStepPosition < currentParam.TotalCount)
            {
                //Figure out which step to fly to...

                //look up the current slider's current sliderStepsPosition and target number
                //int closestTick = calClosestTick();

                //calClosestTick();


                //The current component is already at the maximum value. Reset it back to zero.
                currentParam.SetParamTo(nextStepPosition);
                //currentInputParam.SetToNext();

                //Increment the current step position
                this.currentStepPositions[MoveToParamIndex]++;
                
                return true;
            }else
            {

                currentParam.Reset();
                

                ////set our slider steps position back to 0
                this.currentStepPositions[MoveToParamIndex] = 0;

                //// Move on to the next slider.
                MoveToParamIndex++;
                //System.Windows.Forms.MessageBox.Show("index++"+MoveToIndex.ToString());
                //// If we've run out of sliders to modify, we're done permutatin'
                if (MoveToParamIndex >= inputParams.Count)
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

        private bool isInSelection(List<int> FlyIndexID)
        {

            return true;
        }

        #endregion

    }

}
