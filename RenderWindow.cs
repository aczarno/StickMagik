using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

// DX wrapper and texture manager
using CDXWrapper;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;

namespace StickMagik
{
  public partial class RenderWindow : Form
  {
    private enum MouseButtons
    {
      MOUSE_LEFT = 0,
      MOUSE_RIGHT,
      MOUSE_MIDDLE,
    }

    public ManagedTextureManager tm;                    // An instance of the texture manager.
    private CManagedDirectX d3d;
    private Microsoft.DirectX.DirectInput.Device keyboard;
    private Microsoft.DirectX.DirectInput.Device mouse;
    public Microsoft.DirectX.Direct3D.Blend curSrcBlend = Microsoft.DirectX.Direct3D.Blend.BothSourceAlpha;   // Currently selected source blend mode.
    public Microsoft.DirectX.Direct3D.Blend curDestBlend = Microsoft.DirectX.Direct3D.Blend.InvSourceColor;  // Currently selected destination blend mode.
    private Camera3D cam;
    private int mouseX;
    private int mouseY;

    //private CDXWrapper.CManagedDirectX.tPrimativeMesh primativeMesh;

    private arcadeStick TEStick;
    
    public RenderWindow()
    {
      InitializeComponent();
      InitializeDevice();
      InitializeInput();
    }

    public void InitializeDevice()
    {
      d3d = CManagedDirectX.Instance;
      d3d.InitD3D(display, display.Width, display.Height, true, false);

      d3d.Device.DeviceResizing += new CancelEventHandler(OnD3DDeviceResize);
      //d3d.InitD3D(display, display.Width, display.Height, true, false);
      tm = ManagedTextureManager.Instance;
      tm.InitManagedTextureManager(d3d.Device, d3d.Sprite);

      // Set up our camera
      cam = new Camera3D();
      cam.SetCamera(new Vector3(0, 0, 10), (float)Math.PI * 3f / 2f, 0);
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

    public void LoadArcadeStick(string filename)
    {
      TEStick.filename = filename;
      d3d.LoadMesh(filename, ref TEStick.model, ref TEStick.materials, ref TEStick.textures, ref TEStick.radius);
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
    }

    public void LoadArcadeStick(string filename, Matrix initialRotation)
    {
      LoadArcadeStick(filename);
      TEStick.mWorld = initialRotation;
    }

    public void Update()
    {
      updateInput();

      // Updates Here -------------------------------------------------------------------------------------------


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

      d3d.DrawGround(10, 1, 0, Color.Blue);

      d3d.Device.Transform.World = TEStick.mWorld;
      d3d.DrawMesh(TEStick.model, TEStick.materials, TEStick.textures);
      //d3d.DrawPrimativeMesh(primativeMesh);
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

      // ---------------
      // Camera here
      d3d.Device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, display.Width / display.Height, 1f, 500f);
      d3d.Device.Transform.View = Matrix.LookAtLH(cam.CameraPosition, cam.CameraTarget, cam.CameraUpVector);

      d3d.Device.EndScene();
      d3d.Device.Present();
    }

    private void updateInput()
    {
      KeyboardState keys = keyboard.GetCurrentKeyboardState();
      MouseState clicks = mouse.CurrentMouseState;


      //tbXY.Text = clicks.ToString();

      //if (keys[Key.W])
      //  cam.moveCameraIn(1f);
      //if (keys[Key.S])
      //  cam.moveCameraIn(-1f);
      //if (keys[Key.A])
      //  cam.moveCameraRight(-1f);
      //if (keys[Key.D])
      //  cam.moveCameraRight(1f);*
      //Capture Buttons.
      byte[] buttons = clicks.GetMouseButtons();
      if (buttons[(int)MouseButtons.MOUSE_RIGHT] != 0 && (keys[Key.LeftAlt] || keys[Key.RightAlt]))
      {
        cam.SlideCamera(clicks.X * 0.1f, clicks.Y * 0.1f);
        //cam.moveCamTargetRight(clicks.X*0.1f);
        //cam.moveCamTargetUp(clicks.Y*0.1f);
      }
      else if (buttons[(int)MouseButtons.MOUSE_LEFT] != 0 && (keys[Key.LeftAlt] || keys[Key.RightAlt]))
      {
        //cam.rotateCam(clicks.X, clicks.Y, 1);
        cam.RotateCamera(clicks.X * -0.01f, clicks.Y * -0.01f);
      }
      else if (buttons[(int)MouseButtons.MOUSE_MIDDLE] != 0 && (keys[Key.LeftAlt] || keys[Key.RightAlt]))
      {
        cam.MoveCamera(clicks.Y * 0.1f);
      }
      else if (buttons[(int)MouseButtons.MOUSE_LEFT] != 0)
      {
        if (meshPick(TEStick, mouseX, mouseY) == true)
        {
          return;
        }
        else
        {
          return;
        }

      }
    }

    private bool meshPick(arcadeStick mesh, int x, int y)
    {
      Vector3 v = new Vector3(0, 0, 0);
      v.X = (((2.0f * x) / this.Width) - 1) / d3d.Device.GetTransform(TransformType.Projection).M11;
      v.Y = -(((2.0f * y) / this.Height) - 1) / d3d.Device.GetTransform(TransformType.Projection).M22;
      v.Z = 1.0f;

      Matrix m = d3d.Device.GetTransform(TransformType.View);
      Vector3 rayOrigin, rayDir;

      m.Invert();
      rayDir.X = v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31;
      rayDir.Y = v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32;
      rayDir.Z = v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33;
      rayOrigin.X = m.M41;
      rayOrigin.Y = m.M42;
      rayOrigin.Z = m.M43;

      Matrix matInverse;
      matInverse = mesh.mWorld;
      matInverse.Invert();

      rayOrigin.TransformCoordinate(matInverse);
      rayDir.TransformNormal(matInverse);
      rayDir.Normalize();


      //IntersectInformation info;
      return mesh.model.Intersect(rayOrigin, rayDir);
    }

    private void OnD3DDeviceResize(object caller, CancelEventArgs args)
    {
      d3d.presentParams.BackBufferHeight = display.Height;
      d3d.presentParams.BackBufferWidth = display.Width;

      d3d.Device.Reset(d3d.presentParams);

      d3d.Device.RenderState.Lighting = true;

      d3d.Device.Lights[0].Type = LightType.Directional;
      d3d.Device.Lights[0].Diffuse = Color.White;
      d3d.Device.Lights[0].Direction = new Vector3(1, 1, -1);
      d3d.Device.Lights[0].Update();
      d3d.Device.Lights[0].Enabled = true;

      d3d.Device.Lights[1].Type = LightType.Directional;
      d3d.Device.Lights[1].Diffuse = Color.White;
      d3d.Device.Lights[1].Direction = new Vector3(-1, -1, -1);
      d3d.Device.Lights[1].Update();
      d3d.Device.Lights[1].Enabled = true;

      d3d.Device.SamplerState[0].MinFilter = TextureFilter.Anisotropic;
      d3d.Device.SamplerState[0].MagFilter = TextureFilter.Anisotropic;

      d3d.Device.SamplerState[0].AddressU = TextureAddress.Mirror;
      d3d.Device.SamplerState[0].AddressV = TextureAddress.Mirror;

      d3d.Device.RenderState.PointSpriteEnable = true;
      d3d.Device.RenderState.PointScaleEnable = true;
      d3d.Device.RenderState.PointScaleA = 0f;
      d3d.Device.RenderState.PointScaleB = 0f;
      d3d.Device.RenderState.PointScaleC = 100f;

      d3d.Device.RenderState.SourceBlend = Blend.One;
      d3d.Device.RenderState.DestinationBlend = Blend.One;

      d3d.Device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, (float)display.Width / (float)display.Height, 0.3f, 500f);
      d3d.Device.Transform.View = Matrix.LookAtLH(new Vector3(0, -5f, 0.0f), new Vector3(0, 0, 0), new Vector3(0, 0, 1));
      /*d3d.Device.RenderState.FillMode = FillMode.Solid;
      d3d.Device.RenderState.CullMode = Cull.None;

      LoadArcadeStick(TEStick.filename, TEStick.mWorld);

      cam.SetCamera(new Vector3(0, 0, 10), (float)Math.PI * 3f / 2f, 0);*/
    }

  }
}
