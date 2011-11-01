using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StickMagik
{
  public partial class Toolbox : Form
  {
    private Color primaryColor = new Color();
    private Color secondaryColor = new Color();
    
    public Toolbox()
    {
      InitializeComponent();
    }

    private void btnPrimaryColor_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      colorDialog.Color = primaryColor;
      if (colorDialog.ShowDialog() == DialogResult.OK)
      {
        primaryColor = colorDialog.Color;
        btnPrimaryColor.BackColor = primaryColor;
      }
    }

    private void btnSecondaryColor_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      colorDialog.Color = secondaryColor;
      if (colorDialog.ShowDialog() == DialogResult.OK)
      {
        secondaryColor = colorDialog.Color;
        btnSecondaryColor.BackColor = colorDialog.Color;
      }
    }
  }
}
