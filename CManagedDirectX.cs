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
      catch (Exception e)
      {
        return;
      }
    }

    private struct Face
    {
      public int[] verticies;
      public int[] texCoords;
      public int[] normals;
    };

    private struct Group
    {
      public string name;
      public int startFaceIndex;
      public int endFaceIndex;
    };

    private struct MatGroup
    {
      public string material;
      public int startFaceIndex;
      public int endFaceIndex;
    };

    public void LoadOBJ(string filename, ref Mesh mesh, ref Material[] meshmaterials, ref Texture[] meshtextures, ref float meshradius)
    {
      char[] delimSpace = new char[] { ' ' };
      char[] delimSlash = new char[] { '/' };
      ArrayList verticies = new ArrayList();
      ArrayList textureCoords = new ArrayList();
      ArrayList vertexNormals = new ArrayList();
      ArrayList groups = new ArrayList();
      ArrayList faces = new ArrayList();
      ArrayList matGroups = new ArrayList();
      
      int groupStartIndex = 0;
      string groupName = "";
      string materialName = "";
      int matGroupStartIndex = 0;

      VertexFormats format = VertexFormats.None;

      if (File.Exists(filename))
      {
        using (StreamReader reader = File.OpenText(filename))
        {
          string[] components;
          string[] faceVerts;
          string s = "";
          while ((s = reader.ReadLine()) != null)
          {
            s.Normalize();
            components = s.Split(delimSpace, StringSplitOptions.RemoveEmptyEntries);
            if (components.Length == 0)
              continue;
            switch (components[0])
            {
              case "#": // Comment
                break;
              case "v": // Vertex 
                verticies.Add(new Vector3((float)Convert.ToDecimal(components[1]), (float)Convert.ToDecimal(components[2]), (float)Convert.ToDecimal(components[3])));
                break;
              case "vt": // Texture coords
                textureCoords.Add(new Vector3((float)Convert.ToDecimal(components[1]), (float)Convert.ToDecimal(components[2]), (float)Convert.ToDecimal(components[3])));
                break;
              case "vn": // Vertex normal
                vertexNormals.Add(new Vector3((float)Convert.ToDecimal(components[1]), (float)Convert.ToDecimal(components[2]), (float)Convert.ToDecimal(components[3])));
                break;
              case "f": // Face
                Face newFace = new Face();
                newFace.verticies = new int[components.Length - 1];
                newFace.texCoords = new int[components.Length - 1];
                newFace.normals = new int[components.Length - 1];
                // Formats: v/vt/vn or v/vt or v//vn
                for (int i = 1; i < components.Length; i++)
                {
                  faceVerts = components[i].Split(delimSlash, StringSplitOptions.None);
                  
                  // Format: v/vt/vn
                  if (faceVerts.Length == 3 && faceVerts[1] != "")
                  {
                    if (format == VertexFormats.None)
                      format = CustomVertex.PositionNormalTextured.Format;
                    newFace.verticies[i-1] = Convert.ToInt32(faceVerts[0])-1;
                    newFace.texCoords[i-1] = Convert.ToInt32(faceVerts[1])-1;
                    newFace.normals[i-1] = Convert.ToInt32(faceVerts[2])-1;
                  }
                  // Format: v/vt
                  else if (faceVerts.Length == 2)
                  {
                    if (format == VertexFormats.None)
                      format = CustomVertex.PositionTextured.Format;
                    newFace.verticies[i-1] = Convert.ToInt32(faceVerts[0])-1;
                    newFace.texCoords[i-1] = Convert.ToInt32(faceVerts[1])-1;
                  }
                  // Format: v//vn
                  else
                  {
                    if (format == VertexFormats.None)
                      format = CustomVertex.PositionNormal.Format;
                    newFace.verticies[i-1] = Convert.ToInt32(faceVerts[0])-1;
                    newFace.normals[i-1] = Convert.ToInt32(faceVerts[2])-1;
                  }
                }
              
                faces.Add(newFace);
                break;
              case "g": // Group for all predicessing faces until next group
                // Need a special case for the last group in the file.
                if (groupName != "")
                {
                  Group newGroup = new Group();
                  newGroup.startFaceIndex = groupStartIndex;
                  newGroup.name = groupName;
                  newGroup.endFaceIndex = faces.Count - 1;
                }
                // Set up for the next group
                groupName = components[1];
                groupStartIndex = faces.Count;
                break;
              case "usemtl": // Material for all predicessing faces until next usemtl
                if (materialName != "")
                {
                  MatGroup newGroup = new MatGroup();
                  newGroup.startFaceIndex = matGroupStartIndex;
                  newGroup.material = materialName;
                  newGroup.endFaceIndex = faces.Count - 1;
                }
                // Set up for the next group
                materialName = components[1];
                matGroupStartIndex = faces.Count;
                break;
              default:
                break;
            } // switch
          } // while
          
          if(groupName != "")
          {
            Group newGroup = new Group();
            newGroup.startFaceIndex = groupStartIndex;
            newGroup.name = groupName;
            newGroup.endFaceIndex = faces.Count - 1;
            groups.Add(newGroup);
          }

          if(materialName != "")
          {
            MatGroup newGroup = new MatGroup();
            newGroup.startFaceIndex = matGroupStartIndex;
            newGroup.material = materialName;
            newGroup.endFaceIndex = faces.Count - 1;
            matGroups.Add(newGroup);
          }
        }

        // Creating our verts for the mesh
        ArrayList customVerts = new ArrayList();
        for (int i = 0; i < verticies.Count; i++)
        {
          switch (format)
          {
            case CustomVertex.PositionNormalTextured.Format:
              customVerts.Add(new CustomVertex.PositionNormalTextured((Vector3)verticies[i], (Vector3)vertexNormals[i], ((Vector3)textureCoords[i]).X, ((Vector3)textureCoords[i]).Y));
              break;
            case CustomVertex.PositionTextured.Format:
              customVerts.Add(new CustomVertex.PositionTextured((Vector3)verticies[i], ((Vector3)textureCoords[i]).X, ((Vector3)textureCoords[i]).Y));
              break;
            case CustomVertex.PositionNormal.Format:
              customVerts.Add(new CustomVertex.PositionNormal((Vector3)verticies[i], (Vector3)vertexNormals[0]));
              break;
          };
        }

        mesh = new Mesh(faces.Count, verticies.Count, MeshFlags.Managed, format, device);

        switch (format)
        {
          case CustomVertex.PositionNormalTextured.Format:
            mesh.SetVertexBufferData((CustomVertex.PositionNormalTextured[])customVerts.ToArray(typeof(CustomVertex.PositionNormalTextured)), LockFlags.None);
            break;
          case CustomVertex.PositionTextured.Format:
            mesh.SetVertexBufferData((CustomVertex.PositionTextured[])customVerts.ToArray(typeof(CustomVertex.PositionTextured)), LockFlags.None);
            break;
          case CustomVertex.PositionNormal.Format:
            mesh.SetVertexBufferData((CustomVertex.PositionNormal[])customVerts.ToArray(typeof(CustomVertex.PositionNormal)), LockFlags.None);
            break;
        };

        // Convert our faces into an index list
        ArrayList indicies = new ArrayList();
        for (int i = 0; i < faces.Count; i++)
        {
          indicies.Add((short)((Face)faces[i]).verticies[0]);
          indicies.Add((short)((Face)faces[i]).verticies[1]);
          indicies.Add((short)((Face)faces[i]).verticies[2]);
        }
        mesh.SetIndexBufferData((short[])indicies.ToArray(typeof(short)), LockFlags.None);

        //meshmaterials = new Material[matGroups.Count];
        //meshtextures = new Texture[1];

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

    public void DrawLineMesh(Mesh mesh)
    {
      
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
      ArrayList verts = new ArrayList();
      for (float x = -groundSize; x < groundSize; x += gridSize)
      {
        CustomVertex.PositionColored vert = new CustomVertex.PositionColored();
        vert.Position = new Vector3(x, groundLevel, groundSize);
        vert.Color = color.ToArgb();
        verts.Add(vert);

        vert = new CustomVertex.PositionColored();
        vert.Position = new Vector3(x, groundLevel, -groundSize);
        vert.Color = color.ToArgb();
        verts.Add(vert);

        vert = new CustomVertex.PositionColored();
        vert.Position = new Vector3(groundSize, groundLevel, x);
        vert.Color = color.ToArgb();
        verts.Add(vert);

        vert = new CustomVertex.PositionColored();
        vert.Position = new Vector3(-groundSize, groundLevel, x);
        vert.Color = color.ToArgb();
        verts.Add(vert);
      }
      CustomVertex.PositionColored[] lines = (CustomVertex.PositionColored[])verts.ToArray(typeof(CustomVertex.PositionColored));
      device.VertexFormat = CustomVertex.PositionColored.Format;
      device.DrawUserPrimitives(PrimitiveType.LineList, (int)((groundSize * 4) / gridSize), lines);
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

    public Vector3 CameraPosition { get { return cameraPosition; } set { cameraPosition = value; } }
    public Vector3 CameraTarget { get { return cameraTarget; } set { cameraTarget = value; } }
    public Vector3 CameraUpVector { get { return cameraUpVector; } set { cameraUpVector = value; } }
    public float Radius { get { return radius; } set { radius = value; } }
    public float MoveDist { get { return moveDist; } set { moveDist = value; } }

    public Camera3D()
    {
    }

    public void SetCamera(Vector3 cPosition, float h, float v)
    {
      cameraPosition = cPosition;
      cameraTarget = new Vector3(0, 0, 0);
      cameraUpVector = new Vector3(0, 0, 0);
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
