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
                currentValue = "PanelValue";
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

        public override string ToString()
        {
            string currentValue = GHType.ToString()+ CurrentValue();

            return currentValue;
        }

    }
}
