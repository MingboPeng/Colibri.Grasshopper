using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel;

namespace Colibri.Grasshopper
{
    public class IteratorFlyParam
    {

        //InputParams objects List
        private List<object> inputParams;

        public List<object> InputParams
        {
            get { return inputParams; }
            set { inputParams = value; }
        }


        //List of each input param's all steps index
        private List<List<int>> inputParamsStepIndexes;

        public List<List<int>> InputParamsStepIndexes
        {
            get { return inputParamsStepIndexes; }
            set { inputParamsStepIndexes = value; }
        }


        // Total Iteration number int 
        private int totalIterations;

        public int TotalIterations
        {
            get { return totalIterations; }
            set { totalIterations = value; }
        }

        //constructor 
        public IteratorFlyParam(){}

        public IteratorFlyParam(List<object> InputParams)
        {
            inputParams = InputParams;
            
        }





        #region Methods

       
        //to get all params' all steps' indexes 
        public void SetAllParamsStepIndexes()
        {
            var _inputParamsStepIndexes = new List<List<int>>();
            foreach (var item in inputParams)
            {
                var _inputParamSetpIndex = new List<int>();
                _inputParamSetpIndex = IteratorParam.GetParamAllStepIndex(item);
                _inputParamsStepIndexes.Add(_inputParamSetpIndex);
            }

            inputParamsStepIndexes = _inputParamsStepIndexes;
        }

        //doesn't work now
        public int RunThrough() {
            //put first for test here, will remove it later
            foreach (var item in inputParamsStepIndexes.First())
            {
                
                System.Threading.Thread.Sleep(1000);
                return item;

            }
            return -1;
            
        }

        //public bool FlyAll(GH_SolutionEventArgs e) {
        //    while (true)
        //    {
        //        int _currentInputParamIndex = 0;

        //        //move to the next set of slider positions
        //        if (!MoveToNextPermutation(ref _currentInputParamIndex, _InputParams))
        //        {
        //            // study is over!
        //            e.Document.NewSolution(false);
        //            Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
        //            break;
        //        }

        //        // We've just got a new valid permutation. Solve the new solution.
        //        counter++;
        //        e.Document.NewSolution(false);
        //        Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
        //        UpdateProgressBar(counter, totalLoops, sw, pbChars);
        //    }



        //    return true;
        //}




        //private bool MoveToNextPermutation(int ValidInputParamIndex, object ValidInputParam)
        //{
        //    int _currentInputParamIndex = ValidInputParamIndex;
        //    object _validInputParam = ValidInputParam;

        //    //if (_currentInputParamIndex >= _validInputParams.Count)
        //    //    return false;

        //    var _currentParamType= IteratorParam.ConvertParamTypeFormat(_validInputParam);
        //    if (_currentParamType==InputType.Slider)
        //    {

        //    }




        //    GH_NumberSlider slider = sliders[_currentInputParamIndex];
        //    if (slider.TickValue < slider.TickCount)
        //    {
        //        //Figure out which step to fly to...

        //        //look up the current slider's current sliderStepsPosition and target number
        //        int totalNumberOfSteps = sliderSteps[index];
        //        int currentSliderStepsPosition = sliderStepsPositions[index];
        //        int sliderMidStep = slider.TickCount / 2;
        //        int numTicksToAddAsInt = slider.TickCount / totalNumberOfSteps;
        //        double numTicksToAddAsDouble = (double)slider.TickCount / (double)totalNumberOfSteps;

        //        //find the closest tick
        //        int closestTick = 0;
        //        if (currentSliderStepsPosition + numTicksToAddAsInt >= sliderMidStep)
        //        {
        //            closestTick = (int)Math.Ceiling(numTicksToAddAsDouble * currentSliderStepsPosition);
        //        }
        //        else
        //        {
        //            closestTick = (int)Math.Floor(numTicksToAddAsDouble * currentSliderStepsPosition);
        //        }

        //        // Increment the slider.
        //        slider.TickValue = closestTick;

        //        //Increment the current step position
        //        sliderStepsPositions[_currentInputParamIndex]++;

        //        //have we already computed this upcoming combination?  If so, move on to the next one without expiring the solution
        //        if (computedValues.Contains(GetSliderVals(sliders)))
        //        {
        //            return MoveToNextPermutation(ref _currentInputParamIndex, sliders);
        //        }


        //        return true;
        //    }
        //    else
        //    {
        //        // The current slider is already at the maximum value. Reset it back to zero.
        //        slider.TickValue = 0;

        //        //set our slider steps position back to 0
        //        sliderStepsPositions[_currentInputParamIndex] = 0;

        //        // Move on to the next slider.
        //        _currentInputParamIndex++;

        //        // If we've run out of sliders to modify, we're done permutatin'
        //        if (_currentInputParamIndex >= sliders.Count)
        //            return false;

        //        return MoveToNextPermutation(ref _currentInputParamIndex, sliders);
        //    }
        //}


        #endregion

    }
}
