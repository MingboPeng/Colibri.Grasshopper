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
        public static Dictionary<InputType, IGH_Param> CheckAndGetValidInputSource(IGH_Param SelectedInputSource)
        {
            var validSourceParam = new Dictionary<InputType, IGH_Param>(); //empty list for valid Slider, Panel, or ValueList

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
                    validSourceParam.Add(InputType.Slider, mySlider);
                }
                else if (myPanel != null)
                {
                    validSourceParam.Add(InputType.Panel, myPanel);
                }
                else if (myValueList != null)
                {
                    validSourceParam.Add(InputType.ValueList, myValueList);
                }
                else {
                    validSourceParam.Add(InputType.Unsupported, null);
                }
                    
            }

            return validSourceParam;
        }

        //GH_Param<GH_String>

        public static void ChangeParamNickName(Dictionary<InputType, IGH_Param> ValidSourceParam, IGH_Param InputParam, IGH_Param OutputParam)
        {
            var validSourceParam = ValidSourceParam;
            var inputParam = InputParam;
            var outputParam = OutputParam;

            
            var type = validSourceParam.Keys.First();
            IGH_Param inputSource = null;
            if (type == InputType.Unsupported) {

                inputSource = inputParam.Sources[0];

            } else {

                inputSource = validSourceParam.Values.First();
            }
            
            
            inputParam.NickName = type == InputType.Unsupported ? type.ToString(): inputSource.NickName;
            outputParam.NickName = type == InputType.Unsupported? inputParam.NickName : inputSource.NickName;
            outputParam.Description = "This item is one of values from " + type.ToString() + "_" + inputParam.NickName;

        }


        public static string GetParamValues(Dictionary<InputType, IGH_Param> ValidSourceParam, IGH_Param InputParam)
        {
            //object currentItem = default(T);
            var validSourceParam = ValidSourceParam;
            var inputParam = InputParam;

            var type = validSourceParam.Keys.First();
            IGH_Param inputSource = null;
            if (type == InputType.Unsupported)
            {

                inputSource = inputParam.Sources[0];

            }
            else
            {

                inputSource = validSourceParam.Values.First();
            }


            return inputSource.ToString();
        }
        
    }
}
