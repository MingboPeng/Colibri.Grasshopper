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
    static class IteratorParam
    {

        public enum InputType { Slider, Panel, ValueList, Unsupported }


        /// <summary>
        /// Check if is Slider or Panel, and return the first connected conponent's instance GUID
        /// </summary>
        public static List<object> CheckAndGetValidInputSource(IGH_Param SelectedInputSource)
        {
            var validSourceParam = new List<object>(); //empty list for valid Slider, Panel, or ValueList

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
                    validSourceParam.Add(mySlider);
                }
                else if (myPanel != null)
                {
                    validSourceParam.Add(myPanel);
                }
                else if (myValueList != null)
                {
                    validSourceParam.Add(myValueList);
                }
                else {
                    validSourceParam.Add(InputType.Unsupported);
                }
                    
            }

            return validSourceParam;
        }

        //GH_Param<GH_String>

        public static void ChangeParamNickName(List<object> ValidSourceParam, IGH_Param InputParam, IGH_Param OutputParam)
        {
            var _validSourceParam = ValidSourceParam.First();
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


        public static List<string> GetParamValues(List<object> ValidSourceParam, IGH_Param InputParam)
        {
            
            var _validSourceParam = ValidSourceParam.First();
            var inputParam = InputParam;

            var _values = new List<string>();
            var _type = ConvertParamTypeFormat(_validSourceParam);
            var inputSource = _validSourceParam;
           

            //Slider
            if (_type==InputType.Slider)
            {
                var mySlider = inputSource as GH_NumberSlider;
                _values.Add(mySlider.CurrentValue.ToString());
                    
            }
            //Panel
            else if (_type == InputType.Panel)
            {
                var myPanel = inputSource as GH_Panel;
                var _stringSeparator = new char[] { '\n' };
                var _panelValues = myPanel.UserText.Split('\n');

                if (_panelValues.Any())
                {
                    foreach (var item in _panelValues)
                    {
                        _values.Add(item);
                    }
                        
                }
                    
            }
            //ValueList
            else if (_type == InputType.ValueList)
            {
                var myValueList = inputSource as GH_ValueList;
                if (myValueList.SelectedItems.Any())
                {
                    foreach (var item in myValueList.SelectedItems)
                    {
                        _values.Add(item.Value.ToString());
                    }

                }
                    
            }
            else
            {
                _values.Add("Unsupported conponent type! Please use Slider, Panel, or ValueList!");
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
