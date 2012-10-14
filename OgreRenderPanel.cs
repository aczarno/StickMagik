using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StickMagik
{
  public partial class OgreRenderPanel : Panel
  {
    public OgreRenderPanel()
    {
      SetStyle(ControlStyles.Selectable, true);
      TabStop = true;
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        this.Focus();
        base.OnMouseDown(e);
    }
    protected override bool IsInputKey(Keys keyData) {
        if (keyData == Keys.Up || keyData == Keys.Down) return true;
        if (keyData == Keys.Left || keyData == Keys.Right) return true;
        return base.IsInputKey(keyData);
    }
    protected override void OnEnter(EventArgs e) {
        this.Invalidate();
        base.OnEnter(e);
    }
    protected override void OnLeave(EventArgs e) {
        this.Invalidate();
        base.OnLeave(e);
    }
    /*protected override void OnPaint(PaintEventArgs pe) {
        base.OnPaint(pe);
        if (this.Focused) {
            var rc = this.ClientRectangle;
            rc.Inflate(-2, -2);
            ControlPaint.DrawFocusRectangle(pe.Graphics, rc);
        }
    }*/
  }
}
