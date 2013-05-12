namespace StickMagik
{
  partial class Toolbox
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
      this.btnPrimaryColor = new System.Windows.Forms.Panel();
      this.btnSecondaryColor = new System.Windows.Forms.Panel();
      this.colorDialog = new System.Windows.Forms.ColorDialog();
      this.btnMatEdit = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // btnPrimaryColor
      // 
      this.btnPrimaryColor.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
      this.btnPrimaryColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.btnPrimaryColor.Location = new System.Drawing.Point(12, 225);
      this.btnPrimaryColor.Name = "btnPrimaryColor";
      this.btnPrimaryColor.Size = new System.Drawing.Size(25, 25);
      this.btnPrimaryColor.TabIndex = 0;
      this.btnPrimaryColor.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.btnPrimaryColor_MouseDoubleClick);
      // 
      // btnSecondaryColor
      // 
      this.btnSecondaryColor.BackColor = System.Drawing.Color.White;
      this.btnSecondaryColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.btnSecondaryColor.Location = new System.Drawing.Point(43, 225);
      this.btnSecondaryColor.Name = "btnSecondaryColor";
      this.btnSecondaryColor.Size = new System.Drawing.Size(25, 25);
      this.btnSecondaryColor.TabIndex = 0;
      this.btnSecondaryColor.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.btnSecondaryColor_MouseDoubleClick);
      // 
      // btnMatEdit
      // 
      this.btnMatEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnMatEdit.Image = global::StickMagik.Properties.Resources.MatIcon;
      this.btnMatEdit.Location = new System.Drawing.Point(12, 13);
      this.btnMatEdit.Name = "btnMatEdit";
      this.btnMatEdit.Size = new System.Drawing.Size(25, 23);
      this.btnMatEdit.TabIndex = 1;
      this.btnMatEdit.UseVisualStyleBackColor = true;
      this.btnMatEdit.Click += new System.EventHandler(this.btnMatEdit_Click);
      // 
      // Toolbox
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(82, 262);
      this.Controls.Add(this.btnMatEdit);
      this.Controls.Add(this.btnSecondaryColor);
      this.Controls.Add(this.btnPrimaryColor);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "Toolbox";
      this.Text = "Toolbox";
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel btnPrimaryColor;
    private System.Windows.Forms.Panel btnSecondaryColor;
    private System.Windows.Forms.ColorDialog colorDialog;
    private System.Windows.Forms.Button btnMatEdit;
  }
}