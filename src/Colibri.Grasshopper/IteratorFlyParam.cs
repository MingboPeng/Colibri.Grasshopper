using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel;

namespace Colibri.Grasshopper
{
    public class IteratorFlyParam
    {

        //InputParams objects List
        private List<IGH_Param> inputParams;

        public List<IGH_Param> InputParams
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

        public IteratorFlyParam(List<IGH_Param> InputParams)
        {
            inputParams = InputParams;
            totalIterations = 0;
            IniAllParamsStepLists();
            //set current setp index to 0
            currentStepPositions = Enumerable.Repeat(0, inputParams.Count()).ToList();

        }


        #region Methods

       
        //to get all params' all steps' indexes 
        public void IniAllParamsStepLists()
        {
            var _inputParamsStepLists = new List<List<int>>();
            foreach (var item in inputParams)
            {
                var _inputParamSetpList = new List<int>(IteratorParam.GetParamAllStepIndex(item));
                _inputParamsStepLists.Add(_inputParamSetpList);
            }

            inputParamsStepLists = _inputParamsStepLists;
        }


        public void FlyAll(GH_SolutionEventArgs e)
        {
            
            while (true)
            {

                int _currentParamIndex = 0;

                //move to the next set of slider positions
                if (!MoveToNextPermutation(ref _currentParamIndex))
                {
                    // study is over!
                    e.Document.NewSolution(false);
                    Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
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

            
            if (MoveToIndex >= inputParams.Count)
                return false;

            //System.Windows.Forms.MessageBox.Show(MoveToIndex.ToString());
            IGH_Param currentInputParam = inputParams[MoveToIndex];
            
            
            int _currentStepPosition = currentStepPositions[MoveToIndex];
            List<int> _currentStepIndexes = inputParamsStepLists[MoveToIndex];
            int _currentParamTotalCount = _currentStepIndexes.Count();

            if (_currentStepPosition < _currentParamTotalCount)
            {
                //Figure out which step to fly to...

                //look up the current slider's current sliderStepsPosition and target number
                //int closestTick = calClosestTick();

                //calClosestTick();

                //The current component is already at the maximum value. Reset it back to zero.
                setParamValue(currentInputParam, _currentStepPosition);

                //Increment the current step position
                currentStepPositions[MoveToIndex]++;

                //have we already computed this upcoming combination?  If so, move on to the next one without expiring the solution
                //if (computedValues.Contains(GetSliderVals(sliders)))
                //{
                //    return MoveToNextPermutation(ref _currentInputParamIndex, sliders);
                //}


                return true;
            }
            else
            {


                resetParamValue(currentInputParam);
                ////set our slider steps position back to 0
                //sliderStepsPositions[_currentInputParamIndex] = 0;

                //// Move on to the next slider.
                MoveToIndex++;
                //System.Windows.Forms.MessageBox.Show("index++"+MoveToIndex.ToString());
                //// If we've run out of sliders to modify, we're done permutatin'
                //if (_currentInputParamIndex >= sliders.Count)
                //    return false;

                return MoveToNextPermutation(ref MoveToIndex);
            }
        }




        private void resetParamValue(IGH_Param currentInputParam)
        {
            var _paramType = currentInputParam.GetGHType();

            if (_paramType == InputType.Slider)
            {
                var _slider = currentInputParam as GH_NumberSlider;
                _slider.TickValue = 0;
            }
            else if (_paramType == InputType.Panel)
            {
                
            }
            else if (_paramType == InputType.ValueList)
            {
                var _valueList = currentInputParam as GH_ValueList;
                _valueList.SelectItem(0);
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

        //set curretn param to index
        private void setParamValue(IGH_Param currentInputParam, int SetToStepIndex) {

            var _paramType = currentInputParam.GetGHType();

            if (_paramType == InputType.Slider)
            {
                sliderMoveToNextPermutation(currentInputParam as GH_NumberSlider, SetToStepIndex);
            }
            else if (_paramType == InputType.Panel)
            {
                panelMoveToNextPermutation(currentInputParam as GH_Panel, SetToStepIndex);
            }
            else if (_paramType == InputType.ValueList)
            {
                valueListMoveToNextPermutation(currentInputParam as GH_ValueList, SetToStepIndex);
            }
            
        }


        private bool sliderMoveToNextPermutation(GH_NumberSlider ValidParam, int SetToStepIndex)
        {

            // Increment the slider.
            ValidParam.TickValue = SetToStepIndex;
            return false;
        }

        private bool panelMoveToNextPermutation(GH_Panel ValidParam, int SetToStepIndex)
        {
            return false;
        }

        private bool valueListMoveToNextPermutation(GH_ValueList ValidParam, int SetToStepIndex)
        {
            return false;
        }

        #endregion

    }
}
