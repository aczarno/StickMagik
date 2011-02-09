using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace StickMagik
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            StickMagik newform = new StickMagik();
            newform.Show();
            while (newform.Created)
            {
              newform.Update();
              Application.DoEvents();
            }
        }
    }
}
