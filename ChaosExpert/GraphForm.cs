using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ChaosExpert
{
    public partial class GraphForm : Form
    {
        public GraphForm()
        {
            InitializeComponent();
        }

        public void CreateZedGraphControl()
        {
            ZedGraph.ZedGraphControl zedGraphControl2 = new ZedGraph.ZedGraphControl();            
            zedGraphControl2.Dock = System.Windows.Forms.DockStyle.Fill;            
            zedGraphControl2.IsEnableSelection = true;
            zedGraphControl2.Location = new System.Drawing.Point(0, 0);
            zedGraphControl2.Name = "zedGraphControl";
            zedGraphControl2.ScrollGrace = 0;
            zedGraphControl2.ScrollMaxX = 0;
            zedGraphControl2.ScrollMaxY = 0;
            zedGraphControl2.ScrollMaxY2 = 0;
            zedGraphControl2.ScrollMinX = 0;
            zedGraphControl2.ScrollMinY = 0;
            zedGraphControl2.ScrollMinY2 = 0;
            zedGraphControl2.Size = new System.Drawing.Size(766, 250);
            zedGraphControl2.TabIndex = 3;
            zedGraphControl2.IsShowPointValues = true;
        }
    }
}