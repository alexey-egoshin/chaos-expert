using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ChaosExpert
{
    public partial class VariationIndexParamsForm : Form
    {
        public string[] param;
        public VariationIndexParamsForm()
        {
            InitializeComponent();
        }

        private void varIndStartbutton_Click(object sender, EventArgs e)
        {
            param = varIndParamsTextBox.Text.Split();            
        }
    }
}