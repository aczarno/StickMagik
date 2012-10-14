namespace StickMagik
{
  partial class OgreForm
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
      this.renderWindow = new OgreRenderPanel();
      //((System.ComponentModel.ISupportInitialize)(this.renderWindow)).BeginInit();
      this.SuspendLayout();
      // 
      // renderWindow
      // 
      this.renderWindow.Dock = System.Windows.Forms.DockStyle.Fill;
      this.renderWindow.Location = new System.Drawing.Point(0, 0);
      this.renderWindow.Name = "renderWindow";
      this.renderWindow.Size = new System.Drawing.Size(822, 651);
      this.renderWindow.TabIndex = 1;
      this.renderWindow.TabStop = false;
      this.renderWindow.MouseMove += new System.Windows.Forms.MouseEventHandler(this.renderPanel_MouseMove);
      this.renderWindow.Resize += new System.EventHandler(this.renderPanel_Resize);
      this.renderWindow.MouseClick += new System.Windows.Forms.MouseEventHandler(this.renderPanel_MouseClick);
      this.renderWindow.Paint += new System.Windows.Forms.PaintEventHandler(this.renderPanel_Paint);
      this.renderWindow.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.renderPanel_MouseMove);
      // 
      // OgreForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(822, 651);
      this.Controls.Add(this.renderWindow);
      this.Name = "OgreForm";
      this.Text = "OgreForm";
      //((System.ComponentModel.ISupportInitialize)(this.renderWindow)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private OgreRenderPanel renderWindow;


  }
}