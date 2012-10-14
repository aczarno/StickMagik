namespace StickMagik
{
  partial class OpenGLWindow
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
      this.glWindow = new Tao.Platform.Windows.SimpleOpenGlControl();
      this.SuspendLayout();
      // 
      // glWindow
      // 
      this.glWindow.AccumBits = ((byte)(0));
      this.glWindow.AutoCheckErrors = false;
      this.glWindow.AutoFinish = false;
      this.glWindow.AutoMakeCurrent = true;
      this.glWindow.AutoSwapBuffers = true;
      this.glWindow.BackColor = System.Drawing.Color.Black;
      this.glWindow.ColorBits = ((byte)(32));
      this.glWindow.DepthBits = ((byte)(16));
      this.glWindow.Dock = System.Windows.Forms.DockStyle.Fill;
      this.glWindow.Location = new System.Drawing.Point(0, 0);
      this.glWindow.Name = "glWindow";
      this.glWindow.Size = new System.Drawing.Size(876, 491);
      this.glWindow.StencilBits = ((byte)(0));
      this.glWindow.TabIndex = 0;
      this.glWindow.Paint += new System.Windows.Forms.PaintEventHandler(this.glWindow_Paint);
      // 
      // OpenGLWindow
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(876, 491);
      this.Controls.Add(this.glWindow);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "OpenGLWindow";
      this.Text = "OpenGLWindow";
      this.Load += new System.EventHandler(this.OpenGLWindow_Load);
      this.ResumeLayout(false);

    }

    #endregion

    private Tao.Platform.Windows.SimpleOpenGlControl glWindow;
  }
}