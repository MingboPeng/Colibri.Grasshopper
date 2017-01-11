using Grasshopper.Kernel.Special;
using GH = Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;

namespace Colibri.Grasshopper
{

    public enum InputType { Slider, Panel, ValueList, Unsupported }

    static class IteratorParam
    {

        public static InputType GetGHType(this IGH_Param RawParam)
        {

            //Check raw param if is null first
            if (RawParam == null)
            {
                return InputType.Unsupported;
            }


            if (RawParam is GH_NumberSlider)
            {
                return InputType.Slider;

            }
            else if (RawParam is GH_Panel)
            {
                return InputType.Panel;
            }
            else if (RawParam is GH_ValueList)
            {
                return InputType.ValueList;
            }
            else
            {
                return InputType.Unsupported;
            }
        }


        /// <summary>
        /// Check if is Slider or Panel, and return the first connected conponent's instance GUID
        /// </summary>

        public static IGH_Param CheckAndGetValidInputSource(IGH_Param SelectedInputSource)
        {
            //var validSourceParam = new List<object>(); //empty list for valid Slider, Panel, or ValueList
            IGH_Param validSourceParam = null;
            // Find the Guid for connected Slide or Panel

            var selInput = SelectedInputSource; //ref for input where sliders are connected to this component
            IList<IGH_Param> sources = selInput.Sources; //list of things connected on this input
            
            // Find connected
            if (sources.Any())
            { 
                //if something's connected,and get the first connected
                
                var component = sources[0].Attributes.GetTopLevel.DocObject as IGH_Param; //for this connected thing, bring it into the code in a way where we can access its properties
                var _type = component.GetGHType();

                component.ExpireSolution(true);

                //of course, if the thing isn't a Slider or Panel, the cast doesn't work, so we get null. let's filter out the nulls
                if (_type == InputType.Slider)
                {
                    validSourceParam = component as GH_NumberSlider;
                
                }
                else if (_type == InputType.Panel)
                {
                    validSourceParam = component as GH_Panel;
                }
                else if (_type == InputType.ValueList)
                {
                    validSourceParam = component as GH_ValueList;
                }
                else {
                    validSourceParam = null;
                }

                if (validSourceParam != null)
                {
                    validSourceParam.NickName = String.IsNullOrEmpty(component.NickName) || validSourceParam.NickName == "List" ? "RenamePlease" : validSourceParam.NickName; //Check if nick name exists
                }
                    
            }

            return validSourceParam;
        }

        //GH_Param<GH_String>

        public static void ChangeParamNickName(IGH_Param ValidSourceParam, IGH_Param InputParam, IGH_Param OutputParam)
        {
            var _validSourceParam = ValidSourceParam;
            var _inputParam = InputParam;
            var _outputParam = OutputParam;

           
            var _type = _validSourceParam.GetGHType();
            IGH_Param _inputSource = null;

            if (_type == InputType.Unsupported)
            {

                _inputSource = _inputParam.Sources[0];

            }
            else
            {
                _inputSource = _validSourceParam as IGH_Param;
            }


            _inputParam.NickName = _type == InputType.Unsupported ? _type.ToString() : _inputSource.NickName;
            _outputParam.NickName = _type == InputType.Unsupported ? _type.ToString() : _inputSource.NickName;
            _outputParam.Description = "This item is one of values from " + _type.ToString() + "_" + _inputParam.NickName;

        }


        public static List<int> GetParamAllStepIndex(IGH_Param ValidSourceParam)
        {
            //only pick the first Input source
            var _validSourceParam = ValidSourceParam;
            var _values = new List<int>();
            var _type = _validSourceParam.GetGHType();
            var _inputSource = _validSourceParam;


            //Slider
            if (_type == InputType.Slider)
            {
                var _mySlider = _inputSource as GH_NumberSlider;
                var _total = _mySlider.TickCount + 1;
                _values.AddRange(Enumerable.Range(0, _total));

            }
            //Panel
            else if (_type == InputType.Panel)
            {
                var _myPanel = _inputSource as GH_Panel;
                //var _stringSeparator = new char[] { '\n' };
                var _panelValues = _myPanel.UserText.Split('\n');

                var _total = _panelValues.Count();
                _values.AddRange(Enumerable.Range(0, _total));


            }
            //ValueList
            else if (_type == InputType.ValueList)
            {
                var _myValueList = _inputSource as GH_ValueList;

                var _total = _myValueList.ListItems.Count();
                _values.AddRange(Enumerable.Range(0, _total));


            }
            else
            {
                _values.Add(-1);
                //_values.Add("Unsupported conponent type! Please use Slider, Panel, or ValueList!");
            }


            return _values;
        }





        
    }
}
