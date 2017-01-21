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

        //current each position
        private List<int> currentStepPositions { get; set; }

        //current counts
        public int Count { get; private set; }
       
        // Total Iteration number int 
        public int TotalIterations { get; private set; }
        

        //constructor 
        public IteratorFlyParam(){}

        public IteratorFlyParam(List<ColibriParam> SourceParams)
        {
            inputParams = SourceParams;
            calTotalIterations();
            //set current setp index to 0
            currentStepPositions = Enumerable.Repeat(0, inputParams.Count()).ToList();
            //currentStepPositions[0] = 1;

            Count = 0;
        }


        #region Methods

        //to get all params' all steps' indexes 
        public void IniAllParamsStepLists()
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

        private void ResetAll()
        {
            foreach (var item in InputParams)
            {
                item.Reset();
            }
        }

        public void FlyAll(GH_SolutionEventArgs e)
        {

            ResetAll();
            e.Document.NewSolution(false);

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
                this.Count++;

                e.Document.NewSolution(false);
                Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
                //UpdateProgressBar(counter, totalLoops, sw, pbChars);
            }
            
            
        }




        private bool MoveToNextPermutation(ref int MoveToParamIndex)
        {

            // watch the folder : RUN file

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

                //Increment the current step position
                this.currentStepPositions[MoveToParamIndex]++;

                //have we already computed this upcoming combination?  If so, move on to the next one without expiring the solution
                //if (computedValues.Contains(GetSliderVals(sliders)))
                //{
                //    return MoveToNextPermutation(ref _currentInputParamIndex, sliders);
                //}
                return true;
            }else
            {
                ////set our slider steps position back to 0
                this.currentStepPositions[MoveToParamIndex] = 0;

                //// Move on to the next slider.
                MoveToParamIndex++;
                //System.Windows.Forms.MessageBox.Show("index++"+MoveToIndex.ToString());
                //// If we've run out of sliders to modify, we're done permutatin'

                if (MoveToParamIndex >= inputParams.Count)
                    return false;

                currentInputParam.Reset();
                
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
