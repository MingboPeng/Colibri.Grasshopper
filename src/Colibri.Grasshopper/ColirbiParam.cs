using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colibri.Grasshopper
{
    //this class is created for containing Slider, Pannel, ValueList
    public class ColibriParam
    {
        //Properties
        public InputType GHType { get; private set; }


        private string nickName;

        public string NickName
        {
            get { return nickName; }
            set
            {
                nickName = value;
                Param.NickName = nickName;
            }
        }

        // for now is used for tracking the panel values positon only
        private int position;

        public int Position
        {
            get { return position; }
            private set { position = value; }
        }

        

        private List<string> panelValues = new List<string>();

        public IGH_Param Param { get; private set; }

        //Constructor
        public ColibriParam()
        {

        }
        public ColibriParam(IGH_Param RawParam)
        {
            Param = RawParam;
            GHType = GetGHType();
            NickName = Param.NickName;

            if (GHType == InputType.Panel)
            {
                var panel = this.Param as GH_Panel;
                panelValues = panel.UserText.Split('\n').ToList();
            }

        }
        
        //Methods
        private InputType GetGHType()
        {
            var rawParam = this.Param;
            //Check raw param if is null first
            if (rawParam == null)
            {
                return InputType.Unsupported;
            }
            else if (rawParam is GH_NumberSlider)
            {
                return InputType.Slider;

            }
            else if (rawParam is GH_Panel)
            {
                return InputType.Panel;
            }
            else if (rawParam is GH_ValueList)
            {
                return InputType.ValueList;
            }
            else
            {
                return InputType.Unsupported;
            }
        }

        public string CurrentValue() {

            var rawParam = this.Param;
            string currentValue = string.Empty;

            if (GHType == InputType.Slider)
            {
                var slider = rawParam as GH_NumberSlider;
                currentValue = slider.CurrentValue.ToString();
            }
            else if (GHType == InputType.Panel)
            {
                currentValue = panelValues[position];
            }
            else if (GHType == InputType.ValueList)
            {
                var valueList = rawParam as GH_ValueList;
                currentValue = valueList.SelectedItems.First().Value.ToString();
            }
            else
            {
                currentValue = "Please use Slider, Panel, or ValueList!";
            }

            return currentValue;

        }

        public int StepCount()
        {
            
            var param = this.Param;
            var count = 0;


            //Slider
            if (GHType == InputType.Slider)
            {
                var mySlider = param as GH_NumberSlider;
                count = mySlider.TickCount + 1;

            }

            //Panel
            else if (GHType == InputType.Panel)
            {
                
                count = panelValues.Count();

            }

            //ValueList
            else if (GHType == InputType.ValueList)
            {
                var myValueList = param as GH_ValueList;

                count = myValueList.ListItems.Count();


            }
            else
            {
                count = 0;
            }


            return count;
        }

        public void SetParamTo(int SetToStepIndex)
        {

            var param = this.Param;

            if (GHType != InputType.Unsupported)
            {
                position = SetToStepIndex;
            }


            if (GHType == InputType.Slider)
            {
                var slider = param as GH_NumberSlider;
                slider.TickValue = SetToStepIndex;
            }
            else if (GHType == InputType.Panel)
            {
                CurrentValue();
            }
            else if (GHType == InputType.ValueList)
            {
                var valueList = param as GH_ValueList;
                valueList.SelectItem(SetToStepIndex);
            }


        }

        public void ResetValue()
        {
            var param = this.Param;

            if (GHType == InputType.Slider)
            {
                var slider = param as GH_NumberSlider;
                slider.TickValue = 0;
            }
            else if (GHType == InputType.Panel)
            {
                //do nothing
            }
            else if (GHType == InputType.ValueList)
            {
                var valueList = param as GH_ValueList;
                valueList.SelectItem(0);
            }

            position = 0;

        }


        // Override method
        public override string ToString()
        {
            string currentValue = CurrentValue();

            return currentValue;
        }

    }
}
