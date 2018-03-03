namespace ChaosExpert
{
    partial class VariationIndexParamsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.varIndParamsTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.varIndStartbutton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // varIndParamsTextBox
            // 
            this.varIndParamsTextBox.Location = new System.Drawing.Point(66, 30);
            this.varIndParamsTextBox.Name = "varIndParamsTextBox";
            this.varIndParamsTextBox.Size = new System.Drawing.Size(409, 20);
            this.varIndParamsTextBox.TabIndex = 0;
            this.varIndParamsTextBox.Text = "psi 0 pei 0 psl 10 pel 100 pwl 200";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Params: ";
            // 
            // varIndStartbutton
            // 
            this.varIndStartbutton.Location = new System.Drawing.Point(206, 115);
            this.varIndStartbutton.Name = "varIndStartbutton";
            this.varIndStartbutton.Size = new System.Drawing.Size(162, 23);
            this.varIndStartbutton.TabIndex = 2;
            this.varIndStartbutton.Text = "Compute variation index";
            this.varIndStartbutton.UseVisualStyleBackColor = true;
            this.varIndStartbutton.Click += new System.EventHandler(this.varIndStartbutton_Click);
            this.varIndStartbutton.Parent = this;
            // 
            // VariationIndexParamsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 205);
            this.Controls.Add(this.varIndStartbutton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.varIndParamsTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "VariationIndexParamsForm";
            this.Text = "Procedure: Variation Index";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox varIndParamsTextBox;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Button varIndStartbutton;
    }
}