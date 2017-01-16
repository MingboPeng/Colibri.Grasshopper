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
                var _total = _mySlider.TickCount+1;
                _values = Enumerable.Range(0, _total).ToList();

            }
            //Panel
            else if (_type == InputType.Panel)
            {
                var _myPanel = _inputSource as GH_Panel;
                var _panelValues = _myPanel.UserText.Split('\n');

                var _total = _panelValues.Count();
                _values = Enumerable.Range(0, _total).ToList();

            }
            //ValueList
            else if (_type == InputType.ValueList)
            {
                var _myValueList = _inputSource as GH_ValueList;

                var _total = _myValueList.ListItems.Count();
                _values = Enumerable.Range(0, _total).ToList();


            }
            else
            {
                _values.Add(-1);
                //_values.Add("Unsupported conponent type! Please use Slider, Panel, or ValueList!");
            }


            return _values;
        }



        public static int GetParamTotalCount(IGH_Param ValidSourceParam)
        {
            var validSourceParam = ValidSourceParam;
            var count = 0;
            var type = validSourceParam.GetGHType();
            var inputSource = validSourceParam;


            //Slider
            if (type == InputType.Slider)
            {
                var component = inputSource as GH_NumberSlider;
                count = component.TickCount;

            }
            //Panel
            else if (type == InputType.Panel)
            {
                var component = inputSource as GH_Panel;
                var panelValues = component.UserText.Split('\n');
                count = panelValues.Count();
                
            }
            //ValueList
            else if (type == InputType.ValueList)
            {
                var component = inputSource as GH_ValueList;
                
                count = component.ListItems.Count();

            }
            else
            {
                count = 0;
            }


            return count;
        }






    }
}
