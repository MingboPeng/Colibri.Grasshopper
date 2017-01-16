using Grasshopper.Kernel.Special;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Colibri.Grasshopper
{
    //moved to ColibriBase
    //public enum InputType { Slider, Panel, ValueList, Unsupported }

    static class IteratorParam
    {


        /// <summary>
        /// Check if is Slider or Panel, and return the first connected conponent's instance GUID
        /// </summary>

        public static ColibriParam CheckAndGetValidInputSource(IGH_Param SelectedInputSource)
        {
            //var validSourceParam = new List<object>(); //empty list for valid Slider, Panel, or ValueList
            //ColibriParam validSourceParam = null;
            ColibriParam colibriParam = null;
            // Find the Guid for connected Slide or Panel

            var sources = SelectedInputSource.Sources; //list of things connected on this input
            
            // Find connected
            if (sources.Any())
            { 
                //if something's connected,and get the first connected
                
                var component = sources[0].Attributes.GetTopLevel.DocObject as IGH_Param; //for this connected thing, bring it into the code in a way where we can access its properties

                colibriParam = new ColibriParam(component);

                if (colibriParam.GHType != InputType.Unsupported)
                {
                    component.NickName = String.IsNullOrEmpty(component.NickName) || component.NickName == "List" ? "RenamePlease" : colibriParam.NickName; //Check if nick name exists
                    
                }

                return colibriParam;

            }
            else
            {
                return null;
            }
            
        }

        //GH_Param<GH_String>

        public static void ChangeParamNickName(ColibriParam ValidSourceParam, IGH_Param InputParam, IGH_Param OutputParam)
        {
            
            if (ValidSourceParam != null)
            {
                var validSourceParam = ValidSourceParam;
                var inputParam = InputParam;
                var outputParam = OutputParam;


                var type = validSourceParam.GHType;
                bool isTypeUnsupported = type == InputType.Unsupported ? true : false;
                
                inputParam.NickName = isTypeUnsupported ? type.ToString() : validSourceParam.NickName;
                outputParam.NickName = inputParam.NickName;
                outputParam.Description = "This item is one of values from " + type.ToString() + "_" + inputParam.NickName;
            }
            

        }



    }
}
