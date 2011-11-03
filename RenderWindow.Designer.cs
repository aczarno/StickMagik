namespace StickMagik
{
  partial class RenderWindow
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
      this.display = new System.Windows.Forms.PictureBox();
      this.button1 = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.display)).BeginInit();
      this.SuspendLayout();
      // 
      // display
      // 
      this.display.Dock = System.Windows.Forms.DockStyle.Fill;
      this.display.Location = new System.Drawing.Point(0, 0);
      this.display.Name = "display";
      this.display.Size = new System.Drawing.Size(882, 522);
      this.display.TabIndex = 0;
      this.display.TabStop = false;
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(795, 12);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "button1";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // RenderWindow
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(882, 522);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.display);
      this.DoubleBuffered = true;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "RenderWindow";
      this.Text = "RenderWindow";
      ((System.ComponentModel.ISupportInitialize)(this.display)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox display;
    private System.Windows.Forms.Button button1;
  }
}