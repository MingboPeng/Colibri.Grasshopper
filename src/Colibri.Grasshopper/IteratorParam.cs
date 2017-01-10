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

        


        /// <summary>
        /// Check if is Slider or Panel, and return the first connected conponent's instance GUID
        /// </summary>
        public static object CheckAndGetValidInputSource(IGH_Param SelectedInputSource)
        {
            //var validSourceParam = new List<object>(); //empty list for valid Slider, Panel, or ValueList
            object validSourceParam = null;
            // Find the Guid for connected Slide or Panel

            var selInput = SelectedInputSource; //ref for input where sliders are connected to this component
            IList<IGH_Param> sources = selInput.Sources; //list of things connected on this input
            bool isAnythingConnected = sources.Any(); //is there actually anything connected?
            
            
            // Find connected
            if (isAnythingConnected)
            { 
                //if something's connected,and get the first connected
                
                var component = sources[0].Attributes.GetTopLevel.DocObject; //for this connected thing, bring it into the code in a way where we can access its properties
                component.NickName = String.IsNullOrEmpty(component.NickName) || component.NickName=="List" ? "RenamePlease" : component.NickName; //Check if nick name exists
                component.ExpireSolution(true);
                //is there any way to detect the type instead of cast?????
                var mySlider = component as GH_NumberSlider; //...then cast (?) it as a slider
                var myPanel = component as GH_Panel; // try to cast it as a panel as well
                var myValueList = component as GH_ValueList;
                
                
                //of course, if the thing isn't a Slider or Panel, the cast doesn't work, so we get null. let's filter out the nulls
                if (mySlider != null)
                {
                    validSourceParam = mySlider;
                
                }
                else if (myPanel != null)
                {
                    validSourceParam = myPanel;
                }
                else if (myValueList != null)
                {
                    validSourceParam = myValueList;
                }
                else {
                    validSourceParam = InputType.Unsupported;
                }
                    
            }

            return validSourceParam;
        }

        //GH_Param<GH_String>

        public static void ChangeParamNickName(object ValidSourceParam, IGH_Param InputParam, IGH_Param OutputParam)
        {
            var _validSourceParam = ValidSourceParam;
            var _inputParam = InputParam;
            var _outputParam = OutputParam;

           
            var _type = ConvertParamTypeFormat(_validSourceParam);
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


        public static List<int> GetParamAllStepIndex(object ValidSourceParam)
        {
            //only pick the first Input source
            var _validSourceParam = ValidSourceParam;
            var _values = new List<int>();
            var _type = ConvertParamTypeFormat(_validSourceParam);
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
        public static InputType ConvertParamTypeFormat(object RawParam)
        {
            var _rawType = RawParam.GetType();

            if (_rawType.Equals(typeof(GH_NumberSlider)))
            {
                return InputType.Slider;

            } else if (_rawType.Equals(typeof(GH_Panel)))
            {
                return InputType.Panel;
            } else if (_rawType.Equals(typeof(GH_ValueList)))
            {
                return InputType.ValueList;
            }
            else
            {
                return InputType.Unsupported;
            }
            
        }
    }
}
