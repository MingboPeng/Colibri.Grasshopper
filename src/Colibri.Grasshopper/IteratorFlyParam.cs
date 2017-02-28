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
        private List<List<int>> inputParamsStepLists;

        public List<List<int>> InputParamsStepLists
        {
            get { return inputParamsStepLists; }
            set { inputParamsStepLists = value; }
        }

        //current each position
        private List<int> currentStepPositions { get; set; }

        //current counts
        public static int Count { get; private set; }
       
        // Total Iteration number int
        public int TotalIterations { get; private set; }

        private string watchFilePath { get; set; }

        //constructor 
        public IteratorFlyParam(){}

        public IteratorFlyParam(List<ColibriParam> SourceParams, string StudyFolder)
        {
            this.inputParams = SourceParams;
            calTotalIterations();
            this.currentStepPositions = Enumerable.Repeat(0, inputParams.Count()).ToList();
            Count = 0;

            //create a watch file 
            if (!string.IsNullOrEmpty(StudyFolder))
            {
                watchFilePath = StudyFolder + "\\running.txt";
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


        #region Methods

        //to get all params' all steps' indexes 
        private void IniAllParamsStepLists()
        {
            var stepLists = new List<List<int>>();
            foreach (var item in InputParams)
            {
                int totalCount = item.TotalCount;
                if (totalCount >0)
                {
                    var SetpList = Enumerable.Range(0, totalCount).ToList();
                    stepLists.Add(SetpList);
                }
                
            }

            inputParamsStepLists = stepLists;
        }

        private void calTotalIterations()
        {
            
            TotalIterations = 1;
            
            foreach (var item in InputParams)
            {
                int totalCount = item.TotalCount;
                if (totalCount > 0)
                {
                    TotalIterations *= totalCount;
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
                //iterator.GoodToExpire = false;
                //move to the next set of slider positions
                bool isRunning = MoveToNextPermutation(ref currentParamIndex);
                Count++;
                // We've just got a new valid permutation. Solve the new solution.
                bool isTheEnd = Count > TotalIterations;
                
                e.Document.NewSolution(false);
                
                //Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
                //UpdateProgressBar(counter, totalLoops, sw, pbChars);

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
            
            var currentInputParam = inputParams[MoveToParamIndex];
            
            int nextStepPosition = currentStepPositions[MoveToParamIndex] + 1;
            
            if (nextStepPosition < currentInputParam.TotalCount)
            {
                //Figure out which step to fly to...

                //look up the current slider's current sliderStepsPosition and target number
                //int closestTick = calClosestTick();

                //calClosestTick();
                

                //The current component is already at the maximum value. Reset it back to zero.
                currentInputParam.SetParamTo(nextStepPosition);
                //currentInputParam.SetToNext();

                //Increment the current step position
                this.currentStepPositions[MoveToParamIndex]++;
                
                return true;
            }else
            {
                
                currentInputParam.Reset();
                

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


        #endregion

    }

}
