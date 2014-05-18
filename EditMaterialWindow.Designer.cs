namespace StickMagik
{
  partial class EditMaterialWindow
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
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.btnNewMatFile = new System.Windows.Forms.Button();
      this.pbMatPreview = new System.Windows.Forms.PictureBox();
      this.ofdFindMaterial = new System.Windows.Forms.OpenFileDialog();
      ((System.ComponentModel.ISupportInitialize)(this.pbMatPreview)).BeginInit();
      this.SuspendLayout();
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(12, 230);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(230, 20);
      this.textBox1.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 211);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(84, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Current Material:";
      // 
      // btnNewMatFile
      // 
      this.btnNewMatFile.Image = global::StickMagik.Properties.Resources.FileIcon;
      this.btnNewMatFile.Location = new System.Drawing.Point(248, 230);
      this.btnNewMatFile.Name = "btnNewMatFile";
      this.btnNewMatFile.Size = new System.Drawing.Size(24, 23);
      this.btnNewMatFile.TabIndex = 2;
      this.btnNewMatFile.UseVisualStyleBackColor = true;
      this.btnNewMatFile.Click += new System.EventHandler(this.btnNewMatFile_Click);
      // 
      // pbMatPreview
      // 
      this.pbMatPreview.Location = new System.Drawing.Point(13, 13);
      this.pbMatPreview.Name = "pbMatPreview";
      this.pbMatPreview.Size = new System.Drawing.Size(259, 195);
      this.pbMatPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pbMatPreview.TabIndex = 3;
      this.pbMatPreview.TabStop = false;
      // 
      // ofdFindMaterial
      // 
      this.ofdFindMaterial.FileName = "*.png";
      this.ofdFindMaterial.Filter = "PNG|*.png|JPEG|*.jpg|All files|*.*";
      // 
      // EditMaterialWindow
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 262);
      this.Controls.Add(this.pbMatPreview);
      this.Controls.Add(this.btnNewMatFile);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.textBox1);
      this.Name = "EditMaterialWindow";
      this.Text = "Edit Material";
      ((System.ComponentModel.ISupportInitialize)(this.pbMatPreview)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnNewMatFile;
    private System.Windows.Forms.PictureBox pbMatPreview;
    private System.Windows.Forms.OpenFileDialog ofdFindMaterial;
  }
}