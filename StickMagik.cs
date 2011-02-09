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

// DX wrapper and texture manager
using CDXWrapper;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;

namespace StickMagik
{
  public partial class StickMagik : Form
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

    private enum MouseButtons
    {
      MOUSE_LEFT = 0,
      MOUSE_RIGHT,
      MOUSE_MIDDLE,
    }
    // Private members
    public ManagedTextureManager tm;                    // An instance of the texture manager.
    private CManagedDirectX d3d;
    private Microsoft.DirectX.DirectInput.Device keyboard;
    private Microsoft.DirectX.DirectInput.Device mouse;
    public float currentTime = 0.0f;                // All of these variables are used for frame counting
    public float elapsedTime = 0.0f;                // and calculating delta time.
    public float previousTime = 0.0f;
    public float frameTimer = 0.0f;
    public int frameCounter = 0;
    public int fps = 0;
    public Microsoft.DirectX.Direct3D.Blend curSrcBlend = Microsoft.DirectX.Direct3D.Blend.BothSourceAlpha;   // Currently selected source blend mode.
    public Microsoft.DirectX.Direct3D.Blend curDestBlend = Microsoft.DirectX.Direct3D.Blend.InvSourceColor;  // Currently selected destination blend mode.
    private Camera3D cam;

    private CDXWrapper.CManagedDirectX.tPrimativeMesh primativeMesh;

    arcadeStick TEStick;

    // Functions
    public void InitializeDevice()
    {
      d3d = CManagedDirectX.Instance;
      //this.BackgroundImage
      d3d.InitD3D(panel1, 1024, 768, true, false);
      //test = Mesh.Sphere(d3d.Device, 100.0f, 12, 12);
      tm = ManagedTextureManager.Instance;
      tm.InitManagedTextureManager(d3d.Device, d3d.Sprite);

      primativeMesh = new CDXWrapper.CManagedDirectX.tPrimativeMesh();
      primativeMesh.position = new Vector3(0f, 0f, 0f);
      primativeMesh.scale = new Vector3(1.0f, 1.0f, 1.0f);
      primativeMesh.rotation = new Vector3(0.0f, 0.0f, 0.0f);
      primativeMesh.mesh = Mesh.Sphere(d3d.Device, 0.5f, 12, 12);
      Material newmaterial = new Material();
      newmaterial.Diffuse = Color.Blue;
      newmaterial.Ambient = Color.Blue;
      primativeMesh.material = newmaterial;

      TEStick.filename = "TEStick.x";
      d3d.LoadMesh("TEStick.x", ref TEStick.model, ref TEStick.materials, ref TEStick.textures, ref TEStick.radius);
      TEStick.mWorld = Matrix.Identity;

      TEStick.materials[0].Diffuse = Color.Blue;
      TEStick.materials[1].Diffuse = Color.Red;
      TEStick.materials[2].Diffuse = Color.Green;
      TEStick.materials[3].Diffuse = Color.Yellow;
      TEStick.materials[4].Diffuse = Color.Purple;
      TEStick.materials[5].Diffuse = Color.Orange;
      TEStick.materials[6].Diffuse = Color.White;
      TEStick.materials[7].Diffuse = Color.Black;
      TEStick.materials[8].Diffuse = Color.Turquoise;

      TEStick.mWorld.RotateYawPitchRoll((float)Math.PI, (float)Math.PI / 2f, 0);
      // Set up our camera
      cam = new Camera3D();
      cam.SetCamera(new Vector3(0, 0, 10), (float)Math.PI*3f/2f, 0);
    }

    public void InitializeInput()
    {
      keyboard = new Microsoft.DirectX.DirectInput.Device(SystemGuid.Keyboard);
      keyboard.SetCooperativeLevel(this, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
      keyboard.Acquire();

      mouse = new Microsoft.DirectX.DirectInput.Device(SystemGuid.Mouse);
      mouse.SetCooperativeLevel(this, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
      mouse.Acquire();
    }

    public StickMagik()
    {
      InitializeComponent();
      InitializeDevice();
      InitializeInput();
      Toolbox tb = new Toolbox();
      
      AddOwnedForm(tb);
      tb.Show();

    }

    public void Update()
    {
      updateInput();

      currentTime = Environment.TickCount;
      elapsedTime = (currentTime - previousTime) / 1000;
      previousTime = currentTime;

      frameCounter++;

      if (Environment.TickCount - frameTimer > 1000)
      {
        fps = frameCounter;
        frameCounter = 0;
        frameTimer = Environment.TickCount;

        tbFPS.Text = fps.ToString();
      }

      // Updates Here -------------------------------------------------------------------------------------------

      // ---------------
      // Camera here
      d3d.Device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, this.Width / this.Height, 1f, 500f);
      d3d.Device.Transform.View = Matrix.LookAtLH(cam.CameraPosition, cam.CameraTarget, cam.CameraUpVector);
      // --------------
      d3d.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Gray, 1.0f, 0);
      // Begin Rendering
      d3d.Device.BeginScene();
      // Start with a clean world
      d3d.Device.Transform.World = Matrix.Identity;
      // Draw 3D Here -~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-
      d3d.Device.SetRenderState(Microsoft.DirectX.Direct3D.RenderStates.AlphaBlendEnable, true);
      d3d.Device.SetRenderState(Microsoft.DirectX.Direct3D.RenderStates.AlphaTestEnable, true);
      d3d.Device.RenderState.AlphaBlendOperation = BlendOperation.Add;
      d3d.Device.RenderState.AlphaSourceBlend = Blend.SourceAlpha;
      d3d.Device.RenderState.DestinationBlend = Blend.InvSourceAlpha;

      //d3d.DrawPrimativeMesh(primativeMesh);
      d3d.Device.Transform.World = TEStick.mWorld;
      d3d.DrawMesh(TEStick.model, TEStick.materials, TEStick.textures);
      // -~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~-~

      // Reset the world matrix
      d3d.Device.Transform.World = Matrix.Identity;
      #region Sprite Rendering
      d3d.SpriteBegin();
      d3d.Device.SetRenderState(Microsoft.DirectX.Direct3D.RenderStates.ZBufferWriteEnable, false);
      d3d.Device.SetRenderState(Microsoft.DirectX.Direct3D.RenderStates.AlphaBlendEnable, true);
      d3d.Device.SetRenderState(Microsoft.DirectX.Direct3D.RenderStates.AlphaTestEnable, true);

      d3d.Device.SetRenderState(Microsoft.DirectX.Direct3D.RenderStates.SourceBlend,
          Convert.ToInt32(curSrcBlend));
      d3d.Device.SetRenderState(Microsoft.DirectX.Direct3D.RenderStates.DestinationBlend,
          Convert.ToInt32(curDestBlend));

      // Render Sprites Here --------------------------------------------------------------------------------------------

      // --------------------------------------------------------------------------------------------------------
      d3d.SpriteEnd();
      d3d.Device.SetRenderState(Microsoft.DirectX.Direct3D.RenderStates.ZBufferWriteEnable, true);
      #endregion

      d3d.Device.EndScene();
      d3d.Device.Present();
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
        tsLabel.Text = "Parsed Pages " + i.ToString() + '/' + numPages.ToString();
        tsLabel.Invalidate();
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
        tsLabel.Text = "Saved Images " + i.ToString() + '/' + imageURLs.Count.ToString();
        tsProgress.PerformStep();
      }
      return;
      //Bitmap bitmap = new Bitmap(stream);
      //bitmap.Save("Page502", ImageFormat.Bmp);
      //stream.Flush();
      //stream.Close();
    }

    private void updateInput()
    {
      KeyboardState keys = keyboard.GetCurrentKeyboardState();
      MouseState clicks = mouse.CurrentMouseState;

     
      /*if (keys[Key.W])
        cam.moveCameraIn(1f);
      if (keys[Key.S])
        cam.moveCameraIn(-1f);
      if (keys[Key.A])
        cam.moveCameraRight(-1f);
      if (keys[Key.D])
        cam.moveCameraRight(1f);*/
      //Capture Buttons.
      byte[] buttons = clicks.GetMouseButtons();
      if (buttons[(int)MouseButtons.MOUSE_RIGHT] != 0 && (keys[Key.LeftAlt] || keys[Key.RightAlt]))
      {
        cam.SlideCamera(clicks.X*0.1f, clicks.Y*0.1f);
        //cam.moveCamTargetRight(clicks.X*0.1f);
        //cam.moveCamTargetUp(clicks.Y*0.1f);
      }
      else if (buttons[(int)MouseButtons.MOUSE_LEFT] != 0 && (keys[Key.LeftAlt] || keys[Key.RightAlt]))
      {
        //cam.rotateCam(clicks.X, clicks.Y, 1);
        cam.RotateCamera(clicks.X*-0.01f, clicks.Y*-0.01f);
      }
      else if (buttons[(int)MouseButtons.MOUSE_MIDDLE] != 0 && (keys[Key.LeftAlt] || keys[Key.RightAlt]))
      {
        cam.MoveCamera(clicks.Y*0.1f);
      }
    }
  }
}
