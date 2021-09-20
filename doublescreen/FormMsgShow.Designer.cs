namespace doublescreen
{
    partial class FormMsgShow
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
            this.lbMsg = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbMsg
            // 
            this.lbMsg.Font = new System.Drawing.Font("宋体", 16F);
            this.lbMsg.ForeColor = System.Drawing.Color.Black;
            this.lbMsg.Location = new System.Drawing.Point(12, 50);
            this.lbMsg.Name = "lbMsg";
            this.lbMsg.Size = new System.Drawing.Size(288, 89);
            this.lbMsg.TabIndex = 0;
            this.lbMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormMsgShow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.ClientSize = new System.Drawing.Size(312, 205);
            this.Controls.Add(this.lbMsg);
            this.Location = new System.Drawing.Point(1024, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMsgShow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "消息提示";
            this.Load += new System.EventHandler(this.FormMsgShow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbMsg;
    }
}