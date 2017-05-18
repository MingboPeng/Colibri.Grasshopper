using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colibri.Grasshopper
{
    public class ImgParam
    {
        public bool IsDefined { get; set; }
        public string SaveName { get; set; }
        public List<string> ViewNames { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public ImgParam()
        {
            this.IsDefined = false;
        }
        public ImgParam(string SaveName, List<string> ViewNames, int Width = 600, int Height = 600)
        {
            this.IsDefined = true;
            this.SaveName = SaveName;
            this.ViewNames = ViewNames;
            this.Width = Width;
            this.Height = Height;
        }

        public override string ToString()
        {

            string output = "SaveName:" + SaveName + ";\n";
            if (ViewNames.Count == 0)
            {
                ViewNames.Add("ActiveView");
            }
            string vName = "ViewNames:" + string.Join(",", ViewNames) + ";\n";

            output += vName;
            output += "Width:" + Width.ToString() + ";\n";
            output += "Height:" + Height.ToString() + ";";

            return output;
        }
    }
}
