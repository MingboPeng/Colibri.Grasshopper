using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Special;


namespace Colibri.Grasshopper
{
    public class IteratorFly
    {
        //List of input param index that connected with only one Slider, Panel, or ValueList
        private List<int> _validInputParamIndexes;

        public List<int> ValidInputParamIndexes
        {
            get { return _validInputParamIndexes; }
            set { _validInputParamIndexes = value; }
        }


        private List<object> _validInputParams;

        public List<object> ValidInputParams
        {
            get { return _validInputParams; }
            set { _validInputParams = value; }
        }

        private int _totalIterations = 0;





        public bool MoveToNextPermutation(int ValidInputParamIndex, object ValidInputParam)
        {
            int _currentInputParamIndex = ValidInputParamIndex;
            object _validInputParam = ValidInputParam;

            //if (_currentInputParamIndex >= _validInputParams.Count)
            //    return false;

            var _currentParamType= IteratorParam.ConvertParamTypeFormat(_validInputParam);
            if (_currentParamType==InputType.Slider)
            {

            }

            


            GH_NumberSlider slider = sliders[_currentInputParamIndex];
            if (slider.TickValue < slider.TickCount)
            {
                //Figure out which step to fly to...

                //look up the current slider's current sliderStepsPosition and target number
                int totalNumberOfSteps = sliderSteps[index];
                int currentSliderStepsPosition = sliderStepsPositions[index];
                int sliderMidStep = slider.TickCount / 2;
                int numTicksToAddAsInt = slider.TickCount / totalNumberOfSteps;
                double numTicksToAddAsDouble = (double)slider.TickCount / (double)totalNumberOfSteps;

                //find the closest tick
                int closestTick = 0;
                if (currentSliderStepsPosition + numTicksToAddAsInt >= sliderMidStep)
                {
                    closestTick = (int)Math.Ceiling(numTicksToAddAsDouble * currentSliderStepsPosition);
                }
                else
                {
                    closestTick = (int)Math.Floor(numTicksToAddAsDouble * currentSliderStepsPosition);
                }

                // Increment the slider.
                slider.TickValue = closestTick;

                //Increment the current step position
                sliderStepsPositions[_currentInputParamIndex]++;

                //have we already computed this upcoming combination?  If so, move on to the next one without expiring the solution
                if (computedValues.Contains(GetSliderVals(sliders)))
                {
                    return MoveToNextPermutation(ref _currentInputParamIndex, sliders);
                }


                return true;
            }
            else
            {
                // The current slider is already at the maximum value. Reset it back to zero.
                slider.TickValue = 0;

                //set our slider steps position back to 0
                sliderStepsPositions[_currentInputParamIndex] = 0;

                // Move on to the next slider.
                _currentInputParamIndex++;

                // If we've run out of sliders to modify, we're done permutatin'
                if (_currentInputParamIndex >= sliders.Count)
                    return false;

                return MoveToNextPermutation(ref _currentInputParamIndex, sliders);
            }
        }

    }
}
