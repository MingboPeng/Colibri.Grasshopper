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
            //int connectedCount = sources.Count;

            // Find connected
            if (isAnythingConnected)
            { 
                //if something's connected,and get the first connected
                
                IGH_DocumentObject component = sources[0].Attributes.GetTopLevel.DocObject; //for this connected thing, bring it into the code in a way where we can access its properties
                GH_NumberSlider mySlider = component as GH_NumberSlider; //...then cast (?) it as a slider
                GH_Panel myPanel = component as GH_Panel; // try to cast it as a panel as well

                //of course, if the thing isn't a Slider or Panel, the cast doesn't work, so we get null. let's filter out the nulls
                if (mySlider != null)
                { 
                    selectedInputs.Add("Slider", mySlider.InstanceGuid);
                    changeParamNames(mySlider.NickName, selInput,selOuput);
                }
                else if (myPanel != null)
                {
                    selectedInputs.Add("Panel", myPanel.InstanceGuid);
                    changeParamNames(myPanel.NickName, selInput, selOuput);
                }
                    
            }

            return selectedInputs;
        }

        private void changeParamNames(String newName, IGH_Param SelInputSource, IGH_Param SelOutputSource) {

            SelInputSource.NickName = newName + "newInputs";
            SelOutputSource.NickName = newName + "newOutputs";

        }
    }
}
