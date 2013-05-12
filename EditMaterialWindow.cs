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
  public partial class EditMaterialWindow : Form
  {
    public EditMaterialWindow()
    {
      
      InitializeComponent();
      Messenger.Broadcast("GetCurMaterial");
    }

    void getCurrentMat()
    {
    }
  }
}
