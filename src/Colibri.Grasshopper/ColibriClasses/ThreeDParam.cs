using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Colibri.Grasshopper
{
    public class threeDParam
    {
        public bool IsDefined { get; set; }
        public string JsonSting { get; set; }
        public threeDParam()
        {
            this.IsDefined = false;
        }
        public threeDParam(object JsonObj)
        {
            this.IsDefined = true;
            this.JsonSting = JsonConvert.SerializeObject(JsonObj);
            this.JsonSting = JsonSting.Replace("OOO", "object");

        }

        public override string ToString()
        {
            string outputString = "3D Model for Design Explorer";
            return outputString;
        }
    }
}
