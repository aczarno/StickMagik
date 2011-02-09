namespace TestTool
{
  partial class InnerWindow
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InnerWindow));
      this.tsTitleBar = new System.Windows.Forms.ToolStrip();
      this.lblTitle = new System.Windows.Forms.ToolStripLabel();
      this.btnCollapse = new System.Windows.Forms.ToolStripButton();
      this.tsTitleBar.SuspendLayout();
      this.SuspendLayout();
      // 
      // tsTitleBar
      // 
      this.tsTitleBar.BackColor = System.Drawing.SystemColors.WindowFrame;
      this.tsTitleBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.tsTitleBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblTitle,
            this.btnCollapse});
      this.tsTitleBar.Location = new System.Drawing.Point(0, 0);
      this.tsTitleBar.Name = "tsTitleBar";
      this.tsTitleBar.Padding = new System.Windows.Forms.Padding(0);
      this.tsTitleBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
      this.tsTitleBar.Size = new System.Drawing.Size(270, 25);
      this.tsTitleBar.TabIndex = 0;
      this.tsTitleBar.Text = "toolStrip1";
      this.tsTitleBar.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.tsTitleBar_QueryContinueDrag);
      this.tsTitleBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tsTitleBar_MouseUp);
      this.tsTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tsTitleBar_MouseDown);
      this.tsTitleBar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tsTitleBar_MouseMove);
      this.tsTitleBar.MouseLeave += new System.EventHandler(this.tsTitleBar_MouseLeave);
      // 
      // lblTitle
      // 
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.Size = new System.Drawing.Size(77, 22);
      this.lblTitle.Text = "Window Title";
      this.lblTitle.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tsTitleBar_MouseUp);
      this.lblTitle.MouseLeave += new System.EventHandler(this.tsTitleBar_MouseLeave);
      this.lblTitle.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tsTitleBar_MouseMove);
      this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tsTitleBar_MouseDown);
      // 
      // btnCollapse
      // 
      this.btnCollapse.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.btnCollapse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnCollapse.Image = ((System.Drawing.Image)(resources.GetObject("btnCollapse.Image")));
      this.btnCollapse.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.btnCollapse.Name = "btnCollapse";
      this.btnCollapse.Size = new System.Drawing.Size(23, 22);
      this.btnCollapse.Text = "toolStripButton1";
      this.btnCollapse.Click += new System.EventHandler(this.btnCollapse_Click);
      // 
      // InnerWindow
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoScroll = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Controls.Add(this.tsTitleBar);
      this.Location = new System.Drawing.Point(0, 10);
      this.Name = "InnerWindow";
      this.Size = new System.Drawing.Size(270, 291);
      this.tsTitleBar.ResumeLayout(false);
      this.tsTitleBar.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ToolStrip tsTitleBar;
    private System.Windows.Forms.ToolStripLabel lblTitle;
    private System.Windows.Forms.ToolStripButton btnCollapse;
  }
}
