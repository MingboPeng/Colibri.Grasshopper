using Grasshopper.Kernel.Special;
using GH = Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Colibri.Grasshopper;

namespace Colibri.Grasshopper
{
    class IteratorGetSliderPanel
    {
   
        /// <summary>
        /// Check if is Slider or Panel, and return the first connected conponent's instance GUID
        /// </summary>
        public Dictionary<string, Guid> getConnectedSliderOrPanel(IGH_Param SelInputSource, IGH_Param SelOutputSource)
        {
            Dictionary<string, Guid> selectedInputs = new Dictionary<string, Guid>(); //empty list for Slider and Panel's guids

            // Find the Guid for connected Slide or Panel

            var selInput = SelInputSource; //ref for input where sliders are connected to this component
            var selOuput = SelOutputSource;
            IList<IGH_Param> sources = selInput.Sources; //list of things connected on this input
            bool isAnythingConnected = sources.Any(); //is there actually anything connected?
            

            // Find connected
            if (isAnythingConnected)
            { 
                //if something's connected,and get the first connected
                
                var component = sources[0].Attributes.GetTopLevel.DocObject; //for this connected thing, bring it into the code in a way where we can access its properties
                component.NickName = String.IsNullOrEmpty(component.NickName) ? "Rename" : component.NickName;
                var mySlider = component as GH_NumberSlider; //...then cast (?) it as a slider
                var myPanel = component as GH_Panel; // try to cast it as a panel as well
                var myValueList = component as GH_ValueList;
                
                //of course, if the thing isn't a Slider or Panel, the cast doesn't work, so we get null. let's filter out the nulls
                if (mySlider != null)
                {
                    selectedInputs.Add("Slider", mySlider.InstanceGuid);
                    changeParamNames(mySlider.NickName, InputType.Slider, selInput, selOuput);
                }
                else if (myPanel != null)
                {
                    selectedInputs.Add("Panel", myPanel.InstanceGuid);
                    changeParamNames(myPanel.NickName, InputType.Panel, selInput, selOuput);
                }
                else if (myValueList != null)
                {
                    selectedInputs.Add("ValueList", myValueList.InstanceGuid);
                    changeParamNames(myValueList.NickName, InputType.ValueList, selInput, selOuput);
                }
                else {
                    changeParamNames(null, InputType.Invalid, selInput, selOuput);
                }
                    
            }

            return selectedInputs;
        }

        private void changeParamNames(string newName, InputType type, IGH_Param SelInputSource, IGH_Param SelOutputSource)
        {
            var typeName = Enum.GetName(typeof(InputType), type);
            SelInputSource.NickName = typeName;

            SelOutputSource.NickName = newName;

        }

        public enum InputType { Slider, Panel, ValueList, Invalid }
    }
}
