using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesignExplorer_Grasshopper
{
    public partial class LoginDemo : Form
    {
        public LoginDemo()
        {
            InitializeComponent();
        }

        public string passwordValue = null;

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            passwordValue = textBox2.Text;
        }
    }
}
