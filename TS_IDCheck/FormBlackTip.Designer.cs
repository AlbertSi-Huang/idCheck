namespace TS_IDCheck
{
    partial class FormBlackTip
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
            this.labTip = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labTip
            // 
            this.labTip.Font = new System.Drawing.Font("微软雅黑", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labTip.Location = new System.Drawing.Point(0, 0);
            this.labTip.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.labTip.Name = "labTip";
            this.labTip.Size = new System.Drawing.Size(498, 369);
            this.labTip.TabIndex = 0;
            this.labTip.Text = "请联系工作人员";
            this.labTip.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormBlackTip
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(21F, 46F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Red;
            this.ClientSize = new System.Drawing.Size(1024, 600);
            this.Controls.Add(this.labTip);
            this.Font = new System.Drawing.Font("微软雅黑", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ForeColor = System.Drawing.Color.Blue;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(10, 11, 10, 11);
            this.Name = "FormBlackTip";
            this.Text = "FormBlackTip";
            this.Load += new System.EventHandler(this.FormBlackTip_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labTip;
    }
}