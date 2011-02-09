using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Design;

namespace TestTool
{
  // This will make it so in a designer this control will be considered a container.
  [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
  public partial class InnerWindow : UserControl
  {

    [DescriptionAttribute("Title of the window.")]
    public string Title
    {
      get { return lblTitle.Text; }
      set { lblTitle.Text = value; }
    }

    private bool isMouseDown = false;
    private Point offset;

    public InnerWindow()
    {
      InitializeComponent();
    }

    private void tsTitleBar_MouseDown(object sender, MouseEventArgs e)
    {
      offset = new Point(e.X, e.Y);
      isMouseDown = true;
    }

    private void tsTitleBar_MouseUp(object sender, MouseEventArgs e)
    {
      isMouseDown = false;
    }

    private void tsTitleBar_MouseMove(object sender, MouseEventArgs e)
    {
      if (isMouseDown == true)
      {
        this.Location = new Point(this.Location.X - offset.X + e.X, this.Location.Y - offset.Y + e.Y);
      }
    }

    private void tsTitleBar_MouseLeave(object sender, EventArgs e)
    {
      isMouseDown = false;
    }

    private void btnCollapse_Click(object sender, EventArgs e)
    {

    }

    private void tsTitleBar_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
    {
      return;
    }
  }
}
