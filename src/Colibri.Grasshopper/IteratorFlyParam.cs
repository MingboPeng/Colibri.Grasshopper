using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;

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


        private List<int> currentStepPositions { get; set; }

       
        // Total Iteration number int 
        private int totalIterations;

        public int TotalIterations
        {
            get { return totalIterations; }
            set { totalIterations = value; }
        }

        //constructor 
        public IteratorFlyParam(){}

        public IteratorFlyParam(List<ColibriParam> SourceParams)
        {
            inputParams = SourceParams;
            totalIterations = 0;
            IniAllParamsStepLists();
            //set current setp index to 0
            currentStepPositions = Enumerable.Repeat(0, inputParams.Count()).ToList();
        }


        #region Methods

        //to get all params' all steps' indexes 
        public void IniAllParamsStepLists()
        {
            var stepLists = new List<List<int>>();
            foreach (var item in inputParams)
            {
                int totalCount = item.StepCount();
                if (totalCount >0)
                {
                    var SetpList = Enumerable.Range(0, totalCount).ToList();
                    stepLists.Add(SetpList);
                }
                
                item.ResetValue();
                
            }

            inputParamsStepLists = stepLists;
        }


        public void FlyAll(GH_SolutionEventArgs e)
        {

            
            while (true)
            {

                int currentParamIndex = 0;
                
                //move to the next set of slider positions
                if (!MoveToNextPermutation(ref currentParamIndex))
                {
                    // study is over!
                    e.Document.NewSolution(false);
                    break;
                }

                // We've just got a new valid permutation. Solve the new solution.
                //counter++;
                e.Document.NewSolution(false);
                Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
                //UpdateProgressBar(counter, totalLoops, sw, pbChars);
            }
            
            
        }




        private bool MoveToNextPermutation(ref int MoveToIndex)
        {

            // watch the folder : RUN file

            if (MoveToIndex >= inputParams.Count)
                return false;

            //Increment the current step position
            currentStepPositions[MoveToIndex]++;

            var currentInputParam = inputParams[MoveToIndex];
            
            int currentStepPosition = currentStepPositions[MoveToIndex];
            //List<int> currentStepIndexes = inputParamsStepLists[MoveToIndex];
            //int currentParamTotalCount = currentStepIndexes.Count();
            
            if (currentStepPosition < currentInputParam.StepCount())
            {
                //Figure out which step to fly to...

                //look up the current slider's current sliderStepsPosition and target number
                //int closestTick = calClosestTick();

                //calClosestTick();

                //The current component is already at the maximum value. Reset it back to zero.
                currentInputParam.SetParamTo(currentStepPosition);
                
                //have we already computed this upcoming combination?  If so, move on to the next one without expiring the solution
                //if (computedValues.Contains(GetSliderVals(sliders)))
                //{
                //    return MoveToNextPermutation(ref _currentInputParamIndex, sliders);
                //}
                return true;
            }else
            {

                currentInputParam.ResetValue();
                ////set our slider steps position back to 0
                currentStepPositions[MoveToIndex]=0;

                //// Move on to the next slider.
                MoveToIndex++;
                //System.Windows.Forms.MessageBox.Show("index++"+MoveToIndex.ToString());
                //// If we've run out of sliders to modify, we're done permutatin'

                //if (MoveToIndex >= inputParams.Count)
                //    return false;

                return MoveToNextPermutation(ref MoveToIndex);
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
