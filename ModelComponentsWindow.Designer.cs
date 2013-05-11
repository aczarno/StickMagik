namespace StickMagik
{
  partial class ModelComponentsWindow
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelComponentsWindow));
      this.treeFragments = new System.Windows.Forms.TreeView();
      this.imgListFragmentIcons = new System.Windows.Forms.ImageList(this.components);
      this.SuspendLayout();
      // 
      // treeFragments
      // 
      this.treeFragments.Dock = System.Windows.Forms.DockStyle.Top;
      this.treeFragments.ImageIndex = 0;
      this.treeFragments.ImageList = this.imgListFragmentIcons;
      this.treeFragments.Location = new System.Drawing.Point(0, 0);
      this.treeFragments.Name = "treeFragments";
      this.treeFragments.SelectedImageIndex = 0;
      this.treeFragments.Size = new System.Drawing.Size(294, 245);
      this.treeFragments.TabIndex = 1;
      this.treeFragments.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeFragments_AfterSelect);
      // 
      // imgListFragmentIcons
      // 
      this.imgListFragmentIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListFragmentIcons.ImageStream")));
      this.imgListFragmentIcons.TransparentColor = System.Drawing.Color.Transparent;
      this.imgListFragmentIcons.Images.SetKeyName(0, "2169924-kappa.png");
      // 
      // ModelComponentsWindow
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(294, 276);
      this.Controls.Add(this.treeFragments);
      this.Name = "ModelComponentsWindow";
      this.Text = "Model Components";
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TreeView treeFragments;
    private System.Windows.Forms.ImageList imgListFragmentIcons;


  }
}