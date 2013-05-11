using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using Crom.Controls;

namespace StickMagik
{
  public struct arcadeStick
  {
    public Matrix mWorld;
    public Mesh model;
    public Material[] materials;
    public Texture[] textures;
    public float radius;
    public string filename;
  }

  public partial class StickMagik : Form
  {
    public float currentTime = 0.0f;                // All of these variables are used for frame counting
    public float elapsedTime = 0.0f;                // and calculating delta time.
    public float previousTime = 0.0f;
    public float frameTimer = 0.0f;
    public int frameCounter = 0;
    public int fps = 0;

    private string assets = "../../Assets/";

    OgreForm of;
    ModelComponentsWindow mf;

    // Functions
    public void InitializeDevice()
    {
      mf = new ModelComponentsWindow();
      of = new OgreForm();;
      Toolbox tb = new Toolbox();

      // Add the form to the dock container
      dockContainer.AddToolWindow(mf);
      mf.Show();
      dockContainer.AddToolWindow(of);
      of.Show();
      dockContainer.AddToolWindow(tb);
      tb.Show();

      of.SetBounds(0, 0, of.Width, of.Height);
      of.Dock = DockStyle.None;
    }

    public StickMagik()
    {
      InitializeComponent();
      InitializeDevice();
    }

    void LoadModel(string path)
    {
      // Load the object into the world
      of.LoadModel(path);

      // Add model to the fragments window
      //mf.AddComponent(
    }

    new public void Update()
    {
      of.Render();
      
      currentTime = Environment.TickCount;
      elapsedTime = (currentTime - previousTime) / 1000;
      previousTime = currentTime;

      frameCounter++;

      if (Environment.TickCount - frameTimer > 1000)
      {
        fps = frameCounter;
        frameCounter = 0;
        frameTimer = Environment.TickCount;

        lblFPS.Text = "FPS: " + fps.ToString();
      }
    }

    public Image DownloadImage(string _URL)
    {
      Image _tmpImage = null;
      try
      {
        // Open a connection
        System.Net.HttpWebRequest _HttpWebRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(_URL);
        _HttpWebRequest.AllowWriteStreamBuffering = true;

        // You can also specify additional header values like the user agent or the referer: (Optional)
        _HttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
        _HttpWebRequest.Referer = "http://www.google.com/";

        // set timeout for 20 seconds (Optional)
        _HttpWebRequest.Timeout = 20000;

        // Request response:
        System.Net.WebResponse _WebResponse = _HttpWebRequest.GetResponse();

        // Open data stream:
        System.IO.Stream _WebStream = _WebResponse.GetResponseStream();

        // convert webstream to image
        _tmpImage = Image.FromStream(_WebStream);

        // Cleanup00
        _WebResponse.Close();
        _WebResponse.Close();
      }
      catch (Exception _Exception)
      {
        // Error
        Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
        return null;
      }
      return _tmpImage;
    }
    private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
    {
      WebClient client = new WebClient();
      string htmlCode;

      char[] delimiters = new char[] { '<', '>' };
      char[] delim = new char[] { '\"' };
      List<string> imageURLs = new List<string>();

      int numPages = 30;
      tsProgress.Maximum = numPages;
      for (int i = 0; i < numPages; i++)
      {
        htmlCode = client.DownloadString("http://www.shoryuken.com/f177/official-street-fighter-iv-te-se-fighstick-template-thread-175293/index" + i.ToString() + ".html");
        // Split up our tokens
        string[] strings = htmlCode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string v in strings)
        {
          // Find our images
          if (v.StartsWith("img src"))
          {
            string[] stuff = v.Split(delim);
            if (stuff.Length > 2)
            {
              // Add the image url
              if (!imageURLs.Exists(delegate(string man) { return man == stuff[1]; }) && !stuff[1].StartsWith("http://shoryuken"))
              {
                imageURLs.Add(stuff[1]);
              }
            }
          }
        }
        //tsLabel.Text = "Parsed Pages " + i.ToString() + '/' + numPages.ToString();
        //tsLabel.Invalidate();
        tsProgress.PerformStep();
      }
      tsProgress.Maximum = imageURLs.Count;
      tsProgress.Value = 0;
      for (int i=0; i<imageURLs.Count; i++)
      {
        Image image = DownloadImage(imageURLs[i]);
        if (image != null)
        {
          // lets save image to disk
          string filename = imageURLs[i].Substring(imageURLs[i].LastIndexOf('/') + 1);
          int qIndex = filename.IndexOf('?');
          if (qIndex != -1)
          {
            filename = filename.Remove(qIndex);
          }
          if (!filename.Equals(""))
          {
            image.Save(filename);
          }
        }
        //tsLabel.Text = "Saved Images " + i.ToString() + '/' + imageURLs.Count.ToString();
        tsProgress.PerformStep();
      }
      return;
      //Bitmap bitmap = new Bitmap(stream);
      //bitmap.Save("Page502", ImageFormat.Bmp);
      //stream.Flush();
      //stream.Close();
    }
  }
}
