namespace ChaosExpert
{
    partial class LoadDataParamForm
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
            this.textBoxDataParams = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnLoadData = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxDataParams
            // 
            this.textBoxDataParams.Location = new System.Drawing.Point(99, 87);
            this.textBoxDataParams.Name = "textBoxDataParams";
            this.textBoxDataParams.Size = new System.Drawing.Size(352, 20);
            this.textBoxDataParams.TabIndex = 0;
            this.textBoxDataParams.Text = "dtp 22.01.2006.16.40-22.12.2006.17.30 dfr classic dci 3 dmc 600 dd ,";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Parameters:";
            // 
            // btnLoadData
            // 
            this.btnLoadData.Location = new System.Drawing.Point(222, 148);
            this.btnLoadData.Name = "btnLoadData";
            this.btnLoadData.Size = new System.Drawing.Size(75, 23);
            this.btnLoadData.TabIndex = 1;
            this.btnLoadData.Text = "Load file";
            this.btnLoadData.UseVisualStyleBackColor = true;
            // 
            // LoadDataParamForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(521, 201);
            this.Controls.Add(this.btnLoadData);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxDataParams);
            this.Name = "LoadDataParamForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "LoadDataParamForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox textBoxDataParams;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Button btnLoadData;
    }
}