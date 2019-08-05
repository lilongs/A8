namespace CallSystem
{
    partial class frmReset
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
            this.btnAC = new DevExpress.XtraEditors.SimpleButton();
            this.btnCC = new DevExpress.XtraEditors.SimpleButton();
            this.btnFC01 = new DevExpress.XtraEditors.SimpleButton();
            this.btnFC02 = new DevExpress.XtraEditors.SimpleButton();
            this.btnCCPrint = new DevExpress.XtraEditors.SimpleButton();
            this.btnWS3Print = new DevExpress.XtraEditors.SimpleButton();
            this.SuspendLayout();
            // 
            // btnAC
            // 
            this.btnAC.Location = new System.Drawing.Point(73, 33);
            this.btnAC.Name = "btnAC";
            this.btnAC.Size = new System.Drawing.Size(120, 81);
            this.btnAC.TabIndex = 2;
            this.btnAC.Text = "AC探针次数重置";
            this.btnAC.Click += new System.EventHandler(this.btnAC_Click);
            // 
            // btnCC
            // 
            this.btnCC.Location = new System.Drawing.Point(247, 33);
            this.btnCC.Name = "btnCC";
            this.btnCC.Size = new System.Drawing.Size(120, 81);
            this.btnCC.TabIndex = 2;
            this.btnCC.Text = "CC探针次数重置";
            this.btnCC.Click += new System.EventHandler(this.btnCC_Click);
            // 
            // btnFC01
            // 
            this.btnFC01.Location = new System.Drawing.Point(421, 33);
            this.btnFC01.Name = "btnFC01";
            this.btnFC01.Size = new System.Drawing.Size(120, 81);
            this.btnFC01.TabIndex = 2;
            this.btnFC01.Text = "FC01探针次数重置";
            this.btnFC01.Click += new System.EventHandler(this.btnFC01_Click);
            // 
            // btnFC02
            // 
            this.btnFC02.Location = new System.Drawing.Point(73, 175);
            this.btnFC02.Name = "btnFC02";
            this.btnFC02.Size = new System.Drawing.Size(120, 81);
            this.btnFC02.TabIndex = 2;
            this.btnFC02.Text = "FC02探针次数重置";
            this.btnFC02.Click += new System.EventHandler(this.btnFC02_Click);
            // 
            // btnCCPrint
            // 
            this.btnCCPrint.Location = new System.Drawing.Point(247, 175);
            this.btnCCPrint.Name = "btnCCPrint";
            this.btnCCPrint.Size = new System.Drawing.Size(120, 81);
            this.btnCCPrint.TabIndex = 2;
            this.btnCCPrint.Text = "CC打印次数重置";
            this.btnCCPrint.Click += new System.EventHandler(this.btnCCPrint_Click);
            // 
            // btnWS3Print
            // 
            this.btnWS3Print.Location = new System.Drawing.Point(421, 175);
            this.btnWS3Print.Name = "btnWS3Print";
            this.btnWS3Print.Size = new System.Drawing.Size(120, 81);
            this.btnWS3Print.TabIndex = 2;
            this.btnWS3Print.Text = "WS3打印次数重置";
            this.btnWS3Print.Click += new System.EventHandler(this.btnWS3Print_Click);
            // 
            // frmReset
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 279);
            this.Controls.Add(this.btnWS3Print);
            this.Controls.Add(this.btnCCPrint);
            this.Controls.Add(this.btnFC02);
            this.Controls.Add(this.btnFC01);
            this.Controls.Add(this.btnCC);
            this.Controls.Add(this.btnAC);
            this.Name = "frmReset";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "耗材重置";
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btnAC;
        private DevExpress.XtraEditors.SimpleButton btnCC;
        private DevExpress.XtraEditors.SimpleButton btnFC01;
        private DevExpress.XtraEditors.SimpleButton btnFC02;
        private DevExpress.XtraEditors.SimpleButton btnCCPrint;
        private DevExpress.XtraEditors.SimpleButton btnWS3Print;
    }
}