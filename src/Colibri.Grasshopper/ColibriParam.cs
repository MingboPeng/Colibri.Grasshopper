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

        private int totalCount;

        public int TotalCount
        {
            get { return totalCount; }
            set { totalCount = value; }
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
            nickName = Param.NickName;
            
            if (GHType == InputType.Panel)
            {
                var panel = this.Param as GH_Panel;
                panelValues = getPanelValue(panel);

            }

            TotalCount = CountSteps();

            //check slider's Implied Nickname
            if (GHType == InputType.Slider)
            {
                var slider = this.Param as GH_NumberSlider;
                nickName = String.IsNullOrEmpty(slider.NickName)&& slider.ImpliedNickName !="Input"? slider.ImpliedNickName: slider.NickName;

            }

            CalIniPosition();


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

        private List<string> getPanelValue(GH_Panel panel)
        {
            var panelValues = new List<string>();

            panelValues = panel.UserText.Split('\n').ToList();
            if (panelValues.Count ==1 && panelValues[0].Contains("Double click to edit panel content"))
            {
                panelValues = panel.VolatileData.AllData(true).Select(_=>_.ToString()).ToList();
            }
            
            return panelValues;
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
                currentValue = panelValues[Position];
                
            }
            else if (GHType == InputType.ValueList)
            {
                var valueList = rawParam as GH_ValueList;
                currentValue = valueList.FirstSelectedItem.Value.ToString();
            }
            else
            {
                currentValue = "Please use Slider, Panel, or ValueList!";
            }

            return currentValue;

        }
        private void CalIniPosition()
        {

            var rawParam = this.Param;

            if (GHType == InputType.Slider)
            {
                var slider = rawParam as GH_NumberSlider;
                this.Position = slider.TickValue;
            }
            else if (GHType == InputType.ValueList)
            {
                var valueList = rawParam as GH_ValueList;
                for (int i = 0; i < valueList.ListItems.Count(); i++)
                {
                    var item = valueList.ListItems[i];
                    if (item.Selected)
                    {
                        this.Position = i;
                    }
                }
            }
            else
            {
                this.Position = 0;
            }
            

        }

        private int CountSteps()
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

            
            this.Position = SetToStepIndex;

            if (GHType == InputType.Slider)
            {
                var slider = param as GH_NumberSlider;
                slider.TickValue = SetToStepIndex;

            }
            else if (GHType == InputType.Panel)
            {
                this.Param.ExpireSolution(false);
            }
            else if (GHType == InputType.ValueList)
            {
                var valueList = param as GH_ValueList;
                valueList.SelectItem(SetToStepIndex);
                //valueList.ToggleItem(SetToStepIndex);


            }


        }

        public void Reset()
        {
            if (GHType == InputType.Panel)
            {
                SetParamTo(0);
            }
            
            else
            {
                
                if (Position == 0)
                {
                    Param.ExpireSolution(true);
                }
                else
                {
                    SetParamTo(0);
                }
            }
        }
            


        // Override method
        public override string ToString()
        {
            string currentValue =CurrentValue();

            return currentValue;
        }

        public string ToString (bool withNames)
        {

            string currentValue = "[" +nickName+","+ CurrentValue()+ "]";

            return currentValue;
        }



    }
}
