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
        private List<object> _InputParams;

        public List<object> InputParams
        {
            get { return _InputParams; }
            set { _InputParams = value; }
        }


        //List of each input param's all steps index
        private List<List<int>> _InputParamsStepIndexes;

        public List<List<int>> InputParamsStepIndexes
        {
            get { return _InputParamsStepIndexes; }
            set { _InputParamsStepIndexes = value; }
        }


        // Total Iteration number int 
        private int _totalIterations;

        public int TotalIterations
        {
            get { return _totalIterations; }
            set { _totalIterations = value; }
        }

        //constructor 
        public IteratorFlyParam()
        {
            _totalIterations = 0;

            foreach (var item in _InputParams)
            {
                var _inputParamSetpIndex = new List<int>();
                _inputParamSetpIndex = IteratorParam.GetParamAllStepIndex(_InputParams);
                _InputParamsStepIndexes.Add(_inputParamSetpIndex);
            }
           
        }




        //#region Methods


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


        //#endregion

    }
}
