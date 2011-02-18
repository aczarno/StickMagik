using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.IO;

namespace CDXWrapper
{
  class CManagedDirectX
  {
    public struct tPrimativeMesh
    {
      public Vector3 position;
      public Material material;
      public Mesh mesh;
      public Vector3 rotation;
      public Vector3 scale;
    }

    public struct tVertex
    {
      public float XVal;
      public float YVal;
      public float ZVal;
      public float fPointSize;
      public int nColor;

      public tVertex(float _fXVal, float _fYVal, float _fZVal, float _pSize, int _iColor)
      {
        XVal = _fXVal;
        YVal = _fYVal;
        ZVal = _fZVal;
        fPointSize = _pSize;
        nColor = _iColor;
      }
    }
    #region Members
    private Device device = null;
    // thread-safe singleton
    static readonly CManagedDirectX instance = new CManagedDirectX();
    Microsoft.DirectX.Direct3D.Sprite sprite = null;
    Microsoft.DirectX.Direct3D.Line line = null;
    #endregion
    #region Properties
    public Device Device { get { return device; } set { device = value; } }
    public static CManagedDirectX Instance { get { return CManagedDirectX.instance; } }
    public Microsoft.DirectX.Direct3D.Sprite Sprite { get { return sprite; } }
    #endregion
    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static CManagedDirectX()
    {
    }

    CManagedDirectX()
    {
    }

    public void InitD3D(System.Windows.Forms.Control renderWindow, int screenWidth, int screenHeight, bool isWindowed, bool vsync)
    {
      PresentParameters presentParams = new PresentParameters();
      presentParams.BackBufferWidth = screenWidth;
      presentParams.BackBufferHeight = screenHeight;
      presentParams.BackBufferFormat = (isWindowed) ? Format.Unknown : Format.R5G6B5;
      presentParams.BackBufferCount = 1;
      presentParams.SwapEffect = SwapEffect.Discard;
      presentParams.AutoDepthStencilFormat = DepthFormat.D16;
      presentParams.EnableAutoDepthStencil = true;
      presentParams.DeviceWindow = renderWindow;
      presentParams.Windowed = isWindowed;
      presentParams.FullScreenRefreshRateInHz = 0;
      presentParams.PresentationInterval = (vsync) ? PresentInterval.Default : PresentInterval.Immediate;
      device = new Device(0, DeviceType.Hardware, renderWindow, CreateFlags.SoftwareVertexProcessing, presentParams);

      device.RenderState.Lighting = true;

      device.Lights[0].Type = LightType.Directional;
      device.Lights[0].Diffuse = Color.White;
      device.Lights[0].Direction = new Vector3(1, 1, -1);
      device.Lights[0].Update();
      device.Lights[0].Enabled = true;

      device.Lights[1].Type = LightType.Directional;
      device.Lights[1].Diffuse = Color.White;
      device.Lights[1].Direction = new Vector3(-1, -1, -1);
      device.Lights[1].Update();
      device.Lights[1].Enabled = true;

      device.SamplerState[0].MinFilter = TextureFilter.Anisotropic;
      device.SamplerState[0].MagFilter = TextureFilter.Anisotropic;

      device.SamplerState[0].AddressU = TextureAddress.Mirror;
      device.SamplerState[0].AddressV = TextureAddress.Mirror;

      device.RenderState.PointSpriteEnable = true;
      device.RenderState.PointScaleEnable = true;
      device.RenderState.PointScaleA = 0f;
      device.RenderState.PointScaleB = 0f;
      device.RenderState.PointScaleC = 100f;

      device.RenderState.SourceBlend = Blend.One;
      device.RenderState.DestinationBlend = Blend.One;

      device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, (float)screenWidth / (float)screenHeight, 0.3f, 500f);
      device.Transform.View = Matrix.LookAtLH(new Vector3(0, -5f, 0.0f), new Vector3(0, 0, 0), new Vector3(0, 0, 1));

      try
      {
        sprite = new Microsoft.DirectX.Direct3D.Sprite(device);
        line = new Microsoft.DirectX.Direct3D.Line(device);
      }
      catch (Exception)
      {
        DialogResult r = MessageBox.Show("Failed to Create the Sprite object", "ManagedDirect3D::Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //return false;
      }
    }

    public void LoadOBJ(string filename, ref Mesh[] meshs, ref Material[] materials, ref Texture[] textures, ref float radius)
    {
      ArrayList verts = new ArrayList();
      ArrayList norms = new ArrayList();
      ArrayList texcoords = new ArrayList();

      StreamReader model = new StreamReader(filename);
      string currentLine;

      while (!model.EndOfStream)
      {
        currentLine = model.ReadLine();
        if (currentLine[0] == 'v')
        {
          string[] vert = currentLine.Split(' ');
          verts.Add(new Vector3((float)Convert.ToDouble(vert[1]), (float)Convert.ToDouble(vert[2]), (float)Convert.ToDouble(vert[3])));
        }
        else if (currentLine.StartsWith("vn"))
        {
          string[] normal = currentLine.Split(' ');
          norms.Add(new Vector3((float)Convert.ToDouble(normal[1]), (float)Convert.ToDouble(normal[2]), (float)Convert.ToDouble(normal[3])));
        }
        else if (currentLine.StartsWith("vt"))
        {
          string[] texture = currentLine.Split(' ');
          texcoords.Add(new Vector3((float)Convert.ToDouble(texture[1]), (float)Convert.ToDouble(texture[2]), (float)Convert.ToDouble(texture[3]))); 
        }
        else if (currentLine[0] == 'f')
        {
        }
        else if (currentLine[0] == 'g')
        {
        }
        else if (currentLine.StartsWith("usemtl"))
        {
        }
        else if (currentLine[0] == '#')
        {
        }
      }
      
    }

    public void LoadMesh(string filename, ref Mesh mesh, ref Material[] meshmaterials, ref Texture[] meshtextures, ref float meshradius)
    {
      ExtendedMaterial[] materialarray;

      try
      {
        
        mesh = Mesh.FromFile(filename, MeshFlags.Managed, device, out materialarray);
      
      if ((materialarray != null) && (materialarray.Length > 0))
      {
        meshmaterials = new Material[materialarray.Length];
        meshtextures = new Texture[materialarray.Length];

        for (int i = 0; i < materialarray.Length; i++)
        {
          meshmaterials[i] = materialarray[i].Material3D;
          meshmaterials[i].Ambient = meshmaterials[i].Diffuse;

          if ((materialarray[i].TextureFilename != null) && (materialarray[i].TextureFilename != string.Empty))
          {
            meshtextures[i] = TextureLoader.FromFile(device, materialarray[i].TextureFilename);
          }
        }
      }

      mesh = mesh.Clone(mesh.Options.Value, CustomVertex.PositionNormalTextured.Format, device);
      mesh.ComputeNormals();

      VertexBuffer vertices = mesh.VertexBuffer;
      GraphicsStream stream = vertices.Lock(0, 0, LockFlags.None);
      Vector3 meshcenter;
      meshradius = Geometry.ComputeBoundingSphere(stream, mesh.NumberVertices, mesh.VertexFormat, out meshcenter) * 0.0005f; //* scale; // HACK
      }
      catch(Exception e)
      {
        return;
      }
    }

    public void DrawMesh(Mesh mesh, Material[] meshmaterials, Texture[] meshtextures)
    {
      for (int i = 0; i < meshmaterials.Length; i++)
      {
        device.Material = meshmaterials[i];
        device.SetTexture(0, meshtextures[i]);
        mesh.DrawSubset(i);
      }
    }

    public void DrawPrimativeMesh(tPrimativeMesh mesh)
    {
      device.Material = mesh.material;
      device.SetTexture(0, null);
      device.Transform.World = Matrix.Scaling(mesh.scale) * Matrix.Translation(mesh.position) * Matrix.RotationX(mesh.rotation.X) * Matrix.RotationY(mesh.rotation.Y) * Matrix.RotationZ(mesh.rotation.Z);
      mesh.mesh.DrawSubset(0);
    }
    /// <summary>
    /// Begins the Sprite (MUST be called between DeviceBegin() and DeviceEnd()!).
    /// </summary>
    /// <returns>true if successful, false otherwise.</returns>
    public bool SpriteBegin()
    {
      if (sprite == null)
        return false;

      try
      {
        sprite.Begin(SpriteFlags.AlphaBlend);
      }
      catch (Exception)
      {
        DialogResult r = MessageBox.Show("Failed to begin sprite scene.", "ManagedDirect3D::Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return false;
      }

      return true;
    }

    public bool SpriteEnd()
    {
      if (sprite == null)
        return false;

      try
      {
        sprite.End();
      }
      catch (Exception)
      {
        DialogResult r = MessageBox.Show("Failed to end sprite scene.", "ManagedDirect3D::Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return false;
      }

      return true;
    }

    public bool Draw2DLine(int x1, int y1, int x2, int y2, Int32 red, Int32 green, Int32 blue)
    {
      if (line == null)
        return false;

      Vector2[] verts = new Vector2[2];

      verts[0].X = x1;
      verts[0].Y = y1;

      verts[1].X = x2;
      verts[1].Y = y2;

      line.Draw(verts, System.Drawing.Color.FromArgb(red, green, blue));

      return true;
    }

    public void DrawGround(float groundSize, float gridSize, float groundLevel, Color color)
    {
      /*line.GlLines = true;
      //line.
      line.DrawTransform
      glBegin(GL_LINES);

      for (float x = -groundSize; x <= groundSize; x += gridSize)
      {
          glVertex3f(x, groundLevel, groundSize);
          glVertex3f(x, groundLevel, -groundSize);

          glVertex3f(groundSize, groundLevel, x);
          glVertex3f(-groundSize, groundLevel, x);
      }

      glEnd();
      */
      ArrayList verts = new ArrayList();
      for(float x = -groundSize; x<groundSize; x+=gridSize)
      {
        CustomVertex.PositionColored vert = new CustomVertex.PositionColored();
        vert.Position = new Vector3(x, groundLevel, groundSize);
        //vert.Position = new Vector3(x, groundSize, groundLevel);
        vert.Color = color.ToArgb();
        verts.Add(vert);

        vert = new CustomVertex.PositionColored();
        vert.Position = new Vector3(x, groundLevel, -groundSize);
        //vert.Position = new Vector3(x, -groundSize, groundLevel);
        vert.Color = color.ToArgb();
        verts.Add(vert);

        vert = new CustomVertex.PositionColored();
        vert.Position = new Vector3(groundSize, groundLevel, x);
        //vert.Position = new Vector3(groundSize, x, groundLevel);
        vert.Color = color.ToArgb();
        verts.Add(vert);

        vert = new CustomVertex.PositionColored();
        vert.Position = new Vector3(-groundSize, groundLevel, x);
        //vert.Position = new Vector3(-groundSize, x, groundLevel);
        vert.Color = color.ToArgb();
        verts.Add(vert);
	    }
      CustomVertex.PositionColored[] lines = (CustomVertex.PositionColored[])verts.ToArray(typeof(CustomVertex.PositionColored));
      device.VertexFormat = CustomVertex.PositionColored.Format;
      device.DrawUserPrimitives(PrimitiveType.LineList, (int)((groundSize * 4) / gridSize), lines);
      //device.
      /*CustomVertex.TransformedColored[] lines = new CustomVertex.TransformedColored[2];
      lines[0].Position = new Vector4(200, 0, 200, 1.0f);
      lines[0].Color = Color.FromArgb(0, 255, 0).ToArgb();
      lines[1].Position = new Vector4(250, 0, 250, 1.0f);
      lines[1].Color = Color.FromArgb(0, 255, 0).ToArgb();*/

      //device.VertexFormat = CustomVertex.TransformedColored.Format;
      //device.DrawUserPrimitives(PrimitiveType.LineList, 1, lines);
    }
    public void FillQuad(ref CustomVertex.PositionNormalTextured[] verticesarray, float width, float height)
    {
      ArrayList verts = new ArrayList();
      verts.Add(new CustomVertex.PositionNormalTextured(new Vector3(-(width * 0.5f), (height * 0.5f), 0.0f), new Vector3(0.0f, 0.0f, 1.0f), 0, 1));
      verts.Add(new CustomVertex.PositionNormalTextured(new Vector3((width * 0.5f), (height * 0.5f), 0.0f), new Vector3(0.0f, 0.0f, 1.0f), 1, 1));
      verts.Add(new CustomVertex.PositionNormalTextured(new Vector3(-(width * 0.5f), -(height * 0.5f), 0.0f), new Vector3(0.0f, 0.0f, 1.0f), 0, 0));
      verts.Add(new CustomVertex.PositionNormalTextured(new Vector3((width * 0.5f), (height * 0.5f), 0.0f), new Vector3(0.0f, 0.0f, 1.0f), 1, 1));
      verts.Add(new CustomVertex.PositionNormalTextured(new Vector3((width * 0.5f), -(height * 0.5f), 0.0f), new Vector3(0.0f, 0.0f, 1.0f), 1, 0));
      verts.Add(new CustomVertex.PositionNormalTextured(new Vector3(-(width * 0.5f), -(height * 0.5f), 0.0f), new Vector3(0.0f, 0.0f, 1.0f), 0, 0));

      verts.Add(new CustomVertex.PositionNormalTextured(new Vector3(-(width * 0.5f), -(height * 0.5f), 0.0f), new Vector3(0.0f, 0.0f, 1.0f), 0, 0));
      verts.Add(new CustomVertex.PositionNormalTextured(new Vector3((width * 0.5f), (height * 0.5f), 0.0f), new Vector3(0.0f, 0.0f, 1.0f), 1, 1));
      verts.Add(new CustomVertex.PositionNormalTextured(new Vector3(-(width * 0.5f), (height * 0.5f), 0.0f), new Vector3(0.0f, 0.0f, 1.0f), 0, 1));
      verts.Add(new CustomVertex.PositionNormalTextured(new Vector3(-(width * 0.5f), -(height * 0.5f), 0.0f), new Vector3(0.0f, 0.0f, 1.0f), 0, 0)); // d
      verts.Add(new CustomVertex.PositionNormalTextured(new Vector3((width * 0.5f), -(height * 0.5f), 0.0f), new Vector3(0.0f, 0.0f, 1.0f), 1, 0));  // c
      verts.Add(new CustomVertex.PositionNormalTextured(new Vector3((width * 0.5f), (height * 0.5f), 0.0f), new Vector3(0.0f, 0.0f, 1.0f), 1, 1));   // b

      verticesarray = (CustomVertex.PositionNormalTextured[])verts.ToArray(typeof(CustomVertex.PositionNormalTextured));
    }
    public void DrawQuad(tPrimativeMesh mesh, CustomVertex.PositionNormalTextured[] verticesarray)
    {
      device.Transform.World = Matrix.Scaling(mesh.scale) * Matrix.Translation(mesh.position) * Matrix.RotationX(mesh.rotation.X) * Matrix.RotationY(mesh.rotation.Y) * Matrix.RotationZ(mesh.rotation.Z);
      device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
      device.Material = mesh.material;
      device.SetTexture(0, null);
      device.DrawUserPrimitives(PrimitiveType.TriangleList, 4, verticesarray);
    }
  }
  public class Camera3D
  {
    private Vector3 cameraPosition;   
    private Vector3 cameraTarget;    
    private Vector3 cameraUpVector;

    private float radius = 1;
    private float moveDist = 1f;

    private float hRadians;
    private float vRadians;

    public Vector3 CameraPosition
    {
      get { return cameraPosition; }
      set { cameraPosition = value; }
    }
    public Vector3 CameraTarget
    {
      get { return cameraTarget; }
      set { cameraTarget = value; }
    }
    public Vector3 CameraUpVector
    {
      get { return cameraUpVector; }
      set { cameraUpVector = value; }
    }
    public float Radius
    {
      get { return radius; }
      set { radius = value; }
    }
    public float MoveDist
    {
      get { return moveDist; }
      set { moveDist = value; }
    }

    public Camera3D() 
    { 
    }

    public void SetCamera(Vector3 cPosition, float h, float v)
    {
      cameraPosition = cPosition;
      cameraTarget = new Vector3(0,0,0);
      cameraUpVector = new Vector3(0,0,0);
      hRadians = h;
      vRadians = v;

      RotateCamera(0, 0);
    }

    public void RotateCamera(float h, float v)
    {
      hRadians += h;
      vRadians += v;

      cameraTarget.Y = cameraPosition.Y + (float)(radius * Math.Sin(vRadians));
      cameraTarget.X = cameraPosition.X + (float)(radius * Math.Cos(vRadians) * Math.Cos(hRadians));
      cameraTarget.Z = cameraPosition.Z + (float)(radius * Math.Cos(vRadians) * Math.Sin(hRadians));

      cameraUpVector.X = cameraPosition.X - cameraTarget.X;
      cameraUpVector.Y = Math.Abs(cameraPosition.Y + (float)(radius * Math.Sin(vRadians + Math.PI / 2)));
      cameraUpVector.Z = cameraPosition.Z - cameraTarget.Z;
    }

    public void SlideCamera(float h, float v)
    {
      cameraPosition.Y += v * moveDist;
      cameraPosition.X += h * moveDist * (float)Math.Cos(hRadians + Math.PI / 2);
      cameraPosition.Z += h * moveDist * (float)Math.Sin(hRadians + Math.PI / 2);
      RotateCamera(0, 0);
    }

    public void MoveCamera(float d)
    {
      cameraPosition.Y += d * moveDist * (float)Math.Sin(vRadians);
      cameraPosition.X += d * moveDist * (float)(Math.Cos(vRadians) * Math.Cos(hRadians));
      cameraPosition.Z += d * moveDist * (float)(Math.Cos(vRadians) * Math.Sin(hRadians));
      RotateCamera(0, 0);
    }
  }
}
