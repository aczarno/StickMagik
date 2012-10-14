using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;

namespace StickMagik
{
  public class OBJGroup
  {
    public string name;
    public List<OBJFace> faces;
    public string materialName;

    // Rendering details
    public VertexBuffer vertexBuffer;
    public IndexBuffer indexBuffer;
    public VertexFormats vertexFormat;
    public int numTris;
    public int numVerts;
    public OBJGroup()
    {
      faces = new List<OBJFace>();
    }
  };

  public class OBJFace
  {
    public List<int> vertIndices;
    public List<int> texCoords;
    public List<int> vertNormals;

    public OBJFace()
    {
      vertIndices = new List<int>();
      texCoords = new List<int>();
      vertNormals = new List<int>();
    }
  };

  class ArcadeStick
  {
    Device device;

    //VertexBuffer vertBuffer;
    //IndexBuffer indexBuffer;

    public string name;
    public Matrix mWorld;
    public List<Mesh> models;
    public List<OBJGroup> groups;
    //public Material[] materials;
    public Texture[] textures;
    public float radius;
    public string filename;

    // All vertex data in the file
    private List<Vector3> positions;
    private List<Vector2> texCoords;
    private List<Vector3> normals;

    // Mapping from mesh positions to the complete list of
    // positions for the current mesh
    private int[] positionMap;

    // Indices of vertex channels for the current mesh
    private int textureCoordinateDataIndex;
    private int normalDataIndex;


    // Named materials from all imported MTL files
    private Dictionary<String, Material> materials;

    public ArcadeStick(Device _device)
    {
      device = _device;
    }
    public void LoadOBJ(string _filename)
    {
      filename = _filename;

      // Reset all importer state
      // See field declarations for more information
      positions = new List<Vector3>();
      texCoords = new List<Vector2>();
      normals = new List<Vector3>();
      groups = new List<OBJGroup>();
      models = new List<Mesh>();

      //meshBuilder = null;
      // StartMesh sets positionMap, textureCoordinateDataIndex, normalDataIndex
      materials = new Dictionary<string, Material>();

      // ImportMaterials resets materialContent

      //try
      {
        // Loop over each tokenized line of the OBJ file
        foreach (String[] lineTokens in GetLineTokens(filename))
        {
          ParseObjLine(lineTokens);
        }

        // If the file did not provide a model name (through an 'o' line),
        // then use the file name as a default
        if (name == null)
            name = Path.GetFileNameWithoutExtension(filename);

        // Finish the last group
        FinishGroup();

        fillMeshes();

        // Done with entire model!
        //return rootNode;
      }
      //catch (Exception)
      {
        // InvalidContentExceptions do not need further processing
        //throw;

      }
    }

    void fillPosNormTex(ref OBJGroup group)
    {
      List<CustomVertex.PositionNormalTextured> verts = new List<CustomVertex.PositionNormalTextured>();
      List<int> indicies = new List<int>();

      foreach (OBJFace f in group.faces)
      {
        int numVerts = f.vertIndices.Count;
        for (int i = 0; i < numVerts; i++)
        {
          bool foundMatch = false;
          for (int j = 0; j < verts.Count; j++)
          {
            if (verts[j].Position == positions[f.vertIndices[i]]
              && verts[j].Normal == normals[f.vertNormals[i]]
              && verts[j].Tu == texCoords[f.texCoords[i]].X
              && verts[j].Tv == texCoords[f.texCoords[i]].Y)
            {
              indicies.Add(j);
              foundMatch = true;
              break;
            }
          }
          if (foundMatch)
            continue;

          CustomVertex.PositionNormalTextured vert = new CustomVertex.PositionNormalTextured(positions[f.vertIndices[i]], normals[f.vertNormals[i]], texCoords[f.texCoords[i]].X, texCoords[f.texCoords[i]].Y);
          verts.Add(vert);
          indicies.Add(verts.Count-1);
        }
      }

      group.numVerts = verts.Count;
      group.numTris = indicies.Count / 3;
      group.vertexFormat = CustomVertex.PositionNormalTextured.Format;
      group.vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormalTextured), verts.Count, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionNormalTextured.Format, Pool.Default);
      group.vertexBuffer.SetData(verts.ToArray(), 0, LockFlags.None);
      group.indexBuffer = new IndexBuffer(typeof(int), indicies.Count * sizeof(int), device, Usage.WriteOnly, Pool.Default);
      group.indexBuffer.SetData(indicies.ToArray(), 0, LockFlags.None);
    }

    void fillPosTex(ref OBJGroup group)
    {
      List<CustomVertex.PositionTextured> verts = new List<CustomVertex.PositionTextured>();
      List<int> indicies = new List<int>();

      foreach (OBJFace f in group.faces)
      {
        int numVerts = f.vertIndices.Count;
        for (int i = 0; i < numVerts; i++)
        {
          CustomVertex.PositionTextured vert = new CustomVertex.PositionTextured(positions[f.vertIndices[i]], texCoords[f.texCoords[i] - 1].X, texCoords[f.texCoords[i] - 1].Y);
          verts.Add(vert);
          indicies.Add(indicies.Count);
        }
      }

      group.numVerts = verts.Count;
      group.numTris = indicies.Count / 3;
      group.vertexFormat = CustomVertex.PositionTextured.Format;
      group.vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionTextured), verts.Count, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);
      group.vertexBuffer.SetData(verts.ToArray(), 0, LockFlags.None);
      group.indexBuffer = new IndexBuffer(typeof(int), indicies.Count * sizeof(int), device, Usage.WriteOnly, Pool.Default);
      group.indexBuffer.SetData(indicies.ToArray(), 0, LockFlags.None);
    }

    void fillPosNorm(ref OBJGroup group)
    {
      List<CustomVertex.PositionNormal> verts = new List<CustomVertex.PositionNormal>();
      List<int> indicies = new List<int>();
      foreach (OBJFace f in group.faces)
      {
        int numVerts = f.vertIndices.Count;
        for (int i = 0; i < numVerts; i++)
        {
          bool foundMatch = false;
          for (int j = 0; j < verts.Count; j++)
          {
            // Look for identical verts
            if (verts[j].Position == positions[f.vertIndices[i]]
              && verts[j].Normal == normals[f.vertNormals[i]])
            {
              indicies.Add(j);
              foundMatch = true;
              break;
            }
          }
          if (foundMatch)
            continue;

          CustomVertex.PositionNormal vert = new CustomVertex.PositionNormal(positions[f.vertIndices[i]], normals[f.vertNormals[i]]);
          verts.Add(vert);
          indicies.Add(verts.Count - 1);
        }
      }

      group.numVerts = verts.Count;
      group.numTris = indicies.Count / 3;
      group.vertexFormat = CustomVertex.PositionNormal.Format;
      group.vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormal), verts.Count, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionNormal.Format, Pool.Default);
      group.vertexBuffer.SetData(verts.ToArray(), 0, LockFlags.None);
      group.indexBuffer = new IndexBuffer(typeof(int), indicies.Count * sizeof(int), device, Usage.WriteOnly, Pool.Default);
      group.indexBuffer.SetData(indicies.ToArray(), 0, LockFlags.None);

      /*using (StreamWriter fs = new StreamWriter("output.txt",false))
      {
        for(int k=0; k<verts.Count; k++)
        {
          fs.WriteLine("V"+k+" Pos: ("+
            verts[k].Position.X+
            ", "+verts[k].Position.Y+
            ", "+verts[k].Position.Z+
            ") Norm: ("+
            verts[k].Normal.X+
            ", "+verts[k].Normal.Y+
            ", "+verts[k].Normal.Z+")");
        }
      }*/

      /*using (StreamWriter fs = new StreamWriter("indiciesOut.txt", false))
      {
        for (int k = 0; k < indicies.Count; k++)
        {
          fs.WriteLine(indicies[k]);
        }
      }*/
    }

    void fillPos(ref OBJGroup group)
    {
      List<CustomVertex.PositionOnly> verts = new List<CustomVertex.PositionOnly>();
      List<int> indicies = new List<int>();
      foreach (OBJFace f in group.faces)
      {
        int numVerts = f.vertIndices.Count;
        for (int i = numVerts - 1; i > -1; i--)
        {
          CustomVertex.PositionOnly vert = new CustomVertex.PositionOnly(positions[f.vertIndices[i]]);
          verts.Add(vert);
          indicies.Add(indicies.Count);
        }
      }

      group.numVerts = verts.Count;
      group.numTris = indicies.Count / 3;
      group.vertexFormat = CustomVertex.PositionOnly.Format;
      group.vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionOnly), verts.Count, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionOnly.Format, Pool.Default);
      group.vertexBuffer.SetData(verts.ToArray(), 0, LockFlags.None);
      group.indexBuffer = new IndexBuffer(typeof(int), indicies.Count * sizeof(int), device, Usage.WriteOnly, Pool.Default);
      group.indexBuffer.SetData(indicies.ToArray(), 0, LockFlags.None);
    }

    void fillMeshes()
    {
      for(int i=0; i<groups.Count; i++)
      {
        if (groups[i].faces.Count == 0)
          continue;

        OBJGroup g = groups[i];
        // Position, Normals, and Texture
        if (texCoords.Count > 0 && normals.Count > 0 && positions.Count > 0)
          fillPosNormTex(ref g);
        // Position and Texture
        else if (texCoords.Count > 0 && positions.Count > 0)
          fillPosTex(ref g);
        // Position and Normals
        else if (normals.Count > 0 && positions.Count > 0)
          fillPosNorm(ref g);
        else if (positions.Count > 0)
          fillPos(ref g);
      }

     /* List<CustomVertex.PositionNormal> myVerts = new List<CustomVertex.PositionNormal>();
      myVerts.Add(new CustomVertex.PositionNormal(0, 0, 0, 0, 0, -1));
      myVerts.Add(new CustomVertex.PositionNormal(10, 10, 0, 0, 0, -1));
      myVerts.Add(new CustomVertex.PositionNormal(10, 0, 0, 0, 0, -1));
      myVerts.Add(new CustomVertex.PositionNormal(0, 10, 0, 0, 0, -1));
      myVerts.Add(new CustomVertex.PositionNormal(0, 0, 0, -1, 0, 0));
      myVerts.Add(new CustomVertex.PositionNormal(0, 10, 10, -1, 0, 0));
      myVerts.Add(new CustomVertex.PositionNormal(0, 10, 0, -1, 0, 0));
      myVerts.Add(new CustomVertex.PositionNormal(0, 0, 10, -1, 0, 0));
      myVerts.Add(new CustomVertex.PositionNormal(0, 10, 0, 0, 1, 0));
      myVerts.Add(new CustomVertex.PositionNormal(10, 10, 10, 0, 1, 0));
      myVerts.Add(new CustomVertex.PositionNormal(10, 10, 0, 0, 1, 0));
      myVerts.Add(new CustomVertex.PositionNormal(0, 10, 10, 0, 1, 0));
      myVerts.Add(new CustomVertex.PositionNormal(10, 0, 0, 1, 0, 0));
      myVerts.Add(new CustomVertex.PositionNormal(10, 10, 0, 1, 0, 0));
      myVerts.Add(new CustomVertex.PositionNormal(10, 10, 10, 1, 0, 0));
      myVerts.Add(new CustomVertex.PositionNormal(10, 0, 10, 1, 0, 0));
      myVerts.Add(new CustomVertex.PositionNormal(0, 0, 0, 0, -1, 0));
      myVerts.Add(new CustomVertex.PositionNormal(10, 0, 0, 0, -1, 0));
      myVerts.Add(new CustomVertex.PositionNormal(10, 0, 10, 0, -1, 0));
      myVerts.Add(new CustomVertex.PositionNormal(0, 0, 10, 0, -1, 0));
      myVerts.Add(new CustomVertex.PositionNormal(0, 0, 10, 0, 0, 1));
      myVerts.Add(new CustomVertex.PositionNormal(10, 0, 10, 0, 0, 1));
      myVerts.Add(new CustomVertex.PositionNormal(10, 10, 10, 0, 0, 1));
      myVerts.Add(new CustomVertex.PositionNormal(0, 10, 10, 0, 0, 1));

      List<int> indexes = new List<int>();
      indexes.Add(0);
      indexes.Add(1);
      indexes.Add(2);
      indexes.Add(0);
      indexes.Add(3);
      indexes.Add(1);
      indexes.Add(4);
      indexes.Add(5);
      indexes.Add(6);
      indexes.Add(4);
      indexes.Add(7);
      indexes.Add(5);
      indexes.Add(8);
      indexes.Add(9);
      indexes.Add(10);
      indexes.Add(8);
      indexes.Add(11);
      indexes.Add(9);
      indexes.Add(12);
      indexes.Add(13);
      indexes.Add(14);
      indexes.Add(12);
      indexes.Add(14);
      indexes.Add(15);
      indexes.Add(16);
      indexes.Add(17);
      indexes.Add(18);
      indexes.Add(16);
      indexes.Add(18);
      indexes.Add(19);
      indexes.Add(20);
      indexes.Add(21);
      indexes.Add(22);
      indexes.Add(20);
      indexes.Add(22);
      indexes.Add(23);

      numberVerts = myVerts.Count;
      numberTris = indexes.Count / 3;
      vertBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormal), numberVerts, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionNormal.Format, Pool.Default);
      vertBuffer.SetData(myVerts.ToArray(), 0, LockFlags.None);
      indexBuffer = new IndexBuffer(typeof(int), indexes.Count * sizeof(int), device, Usage.WriteOnly, Pool.Default);
      indexBuffer.SetData(indexes.ToArray(), 0, LockFlags.None);
      --------------------------------------------------------------------------------------------------*/
      /*vertBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormal), 5, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionNormal.Format, Pool.Default);
      
      CustomVertex.PositionNormal[] vertices = new CustomVertex.PositionNormal[5];
      vertices[0].Position = new Vector3(0f, 0f, 0f);
      vertices[0].Normal = new Vector3(-1, 0, 0);
      vertices[1].Position = new Vector3(5f, 0f, 0f);
      vertices[1].Normal = new Vector3(-1, 0, 0);
      vertices[2].Position = new Vector3(10f, 0f, 0f);
      vertices[2].Normal = new Vector3(-1, 0, 0);
      vertices[3].Position = new Vector3(5f, 5f, 0f);
      vertices[3].Normal = new Vector3(-1, 0, 0);
      vertices[4].Position = new Vector3(10f, 5f, 0f);
      vertices[4].Normal = new Vector3(-1, 0, 0);

      vertBuffer.SetData(vertices, 0, LockFlags.None);

      indexBuffer = new IndexBuffer(typeof(int), 6, device, Usage.WriteOnly, Pool.Default);
      int[] indices = new int[6];

      indices[0] = 3;
      indices[1] = 1;
      indices[2] = 0;
      indices[3] = 4;
      indices[4] = 2;
      indices[5] = 1;

      indexBuffer.SetData(indices, 0, LockFlags.None);*/
    }

    public void renderMeshs()
    {
      device.Transform.World = Matrix.Identity;//Translation(-HEIGHT / 2, -WIDTH / 2, 0) * Matrix.RotationZ(angle);
      for (int i = 0; i < groups.Count; i++)
      {

        device.VertexFormat = groups[i].vertexFormat;

        device.SetStreamSource(0, groups[i].vertexBuffer, 0);
        device.Indices = groups[i].indexBuffer;

        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, groups[i].numVerts, 0, groups[i].numTris);
      }

      /*foreach (Mesh m in models)
      {
        int numSubSets = m.GetAttributeTable().Length;
        for (int i = 0; i < numSubSets; ++i)
        {
          m.DrawSubset(i);
        }
      }*/
    }

    /// <summary>
    /// Yields an array of tokens for each line in an OBJ or MTL file.
    /// </summary>
    /// <remarks>
    /// OBJ and MTL files are text based formats of identical structure.
    /// Each line of a OBJ or MTL file is either an instruction or a comment.
    /// Comments begin with # and are effectively ignored.
    /// Instructions are a space dilimited list of tokens. The first token is the
    /// instruction type code. The tokens which follow, are the arguments to that
    /// instruction. The number and format of arguments vary by instruction type.
    /// </remarks>
    /// <param name="filename">Full path of file to read.</param>
    /// <returns>Element 0 is the line type identifier. The remaining elements are
    /// arguments to the identifier's operation.</returns>
    private static IEnumerable<string[]> GetLineTokens(string filename)
    {
      // Open the file
      using (StreamReader reader = new StreamReader(filename))
      {
        int lineNumber = 1;

        // For each line of the file
        while (!reader.EndOfStream)
        {
          // Set the line number to report in case an exception is thrown
          //identity.FragmentIdentifier = lineNumber.ToString();

          // Tokenize line by splitting on 1 more more whitespace character
          string[] lineTokens = Regex.Split(reader.ReadLine().Trim(), @"\s+");

          // Skip blank lines and comments
          if (lineTokens.Length > 0 &&
              lineTokens[0] != String.Empty &&
              !lineTokens[0].StartsWith("#"))
          {
            // Pass off the tokens of this line to be processed
            yield return lineTokens;
          }

          // Done with this line!
          lineNumber++;
        }
      }
    }

    private OBJGroup curGroup = null;
    /// <summary>
    /// Parses and executes an individual line of an OBJ file.
    /// </summary>
    /// <param name="lineTokens">Line to parse as tokens</param>
    private void ParseObjLine(string[] lineTokens)
    {
      // Switch by line type
      switch (lineTokens[0].ToLower())
      {
        // Object
        case "o":
          // The next token is the name of the model
          name = lineTokens[1];
          break;

        // Positions
        case "v":
          positions.Add(ParseVector3(lineTokens));
          break;

        // Texture coordinates
        case "vt":
          {
            // u is required, but v and w are optional
            // Require a Vector2 and ignore the w for the sake of this sample
            Vector2 vt = ParseVector2(lineTokens);

            // Flip the v coordinate
            vt.Y = 1 - vt.Y;

            texCoords.Add(vt);
            break;
          }

        // Normals
        case "vn":
          normals.Add(ParseVector3(lineTokens));
          break;

        // Groups (model meshes)
        case "g":
          // End the current mesh
          if (curGroup != null)
              FinishGroup();

          // Begin a new mesh
          // The next token is an optional name
          if (lineTokens.Length > 1)
            StartGroup(lineTokens[1]);
          else
            StartGroup(null);
          break;

        // Smoothing group
        case "s":
          // Ignore; just use the normals as specified with verticies
          break;

        // Faces (only triangles are supported)
        case "f":
          // Warn about and ignore polygons which are not triangles
          if (lineTokens.Length > 4)
          {
            //importerContext.Logger.LogWarning(null, rootNode.Identity,
            //    "N-sided polygons are not supported; Ignoring face");
            break;
          }

          // If the builder is null, this face is outside of a group
          // Start a new, unnamed group
          //if (meshBuilder == null)
          //    StartMesh(null);
          if (curGroup == null)
            StartGroup(null);

          OBJFace newFace = new OBJFace();

          // For each triangle vertex
          for (int vertexIndex = 1; vertexIndex <= 3; vertexIndex++)
          {
            // Each vertex is a set of three indices:
            // position, texture coordinate, and normal
            // The indices are 1-based, separated by slashes
            // and only position is required.
            string[] indices = lineTokens[vertexIndex].Split('/');

            // Required: Position
            int positionIndex = int.Parse(indices[0],
                CultureInfo.InvariantCulture) - 1;

            // Add index to our face
            newFace.vertIndices.Add(positionIndex);

            if (indices.Length > 1)
            {
              // Optional: Texture coordinate
              int texCoordIndex;
              Vector2 texCoord;
              if (int.TryParse(indices[1], out texCoordIndex))
                texCoord = texCoords[texCoordIndex - 1];
              else
                texCoord = new Vector2(0.0f, 0.0f);

              newFace.texCoords.Add(texCoordIndex);
              // Set channel data for texture coordinate for the following
              // vertex. This must be done before calling AddTriangleVertex
              //meshBuilder.SetVertexChannelData(textureCoordinateDataIndex,
              //    texCoord);
            }

            if (indices.Length > 2)
            {
              // Optional: Normal
              int normalIndex;
              Vector3 normal;
              if (int.TryParse(indices[2], out normalIndex))
                normal = normals[normalIndex - 1];
              else
                normal = new Vector3(0.0f, 0.0f, 0.0f);

              newFace.vertNormals.Add(normalIndex-1);
              // Set channel data for normal for the following vertex.
              // This must be done before calling AddTriangleVertex
              //meshBuilder.SetVertexChannelData(normalDataIndex,
              //    normal);

            }

            // Add the vertex with the vertex data that was just set
            //meshBuilder.AddTriangleVertex(positionMap[positionIndex]);
          }

          // Finally add the new face to our group.
          curGroup.faces.Add(newFace);

          break;

        // Import a material library file
        case "mtllib":
          // Remaining tokens are relative paths to MTL files
          for (int i = 1; i < lineTokens.Length; i++)
          {
            string mtlFileName = lineTokens[i];

            // A full path is needed,
            if (!Path.IsPathRooted(mtlFileName))
            {
              // resolve relative paths
              string directory =
                  Path.GetDirectoryName(filename);
              mtlFileName = Path.GetFullPath(
                  Path.Combine(directory, mtlFileName));
            }

            // By calling AddDependency, we will cause this model
            // to be rebuilt if its associated MTL files change
            //importerContext.AddDependency(mtlFileName);

            // Import and record the new materials
            ImportMaterials(mtlFileName);
          }
          break;

        // Apply a material 
        case "usemtl":
          {
            /*// If the builder is null, OBJ most likely lacks groups
            // Start a new, unnamed group
            if (meshBuilder == null)
              StartMesh(null);

            // Next token is material name
            string materialName = lineTokens[1];

            // Apply the material to the upcoming faces
            MaterialContent material;
            if (materials.TryGetValue(materialName, out material))
              meshBuilder.SetMaterial(material);
            else
            {
              throw new InvalidContentException(String.Format(
                  "Material '{0}' not defined.", materialName),
                  rootNode.Identity);
            }
            */
            break;
          }

        // Unsupported or invalid line types
        default:
          throw new Exception("Unsupported or invalid line type '" + lineTokens[0] + "'" +
              filename);
      }
    }

    /// <summary>
    /// Starts a new mesh and fills it with mesh mapped positions.
    /// </summary>
    /// <param name="name">Name of mesh.</param>
    private void StartGroup(string name)
    {
      curGroup = new OBJGroup();

      if (!String.IsNullOrEmpty(name))
        curGroup.name = name;
      // Obj files need their winding orders swapped
      //meshBuilder.SwapWindingOrder = true;

      // Add additional vertex channels for texture coordinates and normals
      /*textureCoordinateDataIndex = meshBuilder.CreateVertexChannel<Vector2>(
          VertexChannelNames.TextureCoordinate(0));
      normalDataIndex =
          meshBuilder.CreateVertexChannel<Vector3>(VertexChannelNames.Normal());

      // Add each position to this mesh with CreatePosition
      positionMap = new int[positions.Count];
      for (int i = 0; i < positions.Count; i++)
      {
        // positionsMap redirects from the original positions in the order
        // they were read from file to indices returned from CreatePosition
        positionMap[i] = meshBuilder.CreatePosition(positions[i]);
      }*/
    }


    /// <summary>
    /// Finishes building a mesh and adds the resulting MeshContent or
    /// NodeContent to the root model's NodeContent.
    /// </summary>
    private void FinishGroup()
    {
      /*MeshContent meshContent = meshBuilder.FinishMesh();

      // Groups without any geometry are just for transform
      if (meshContent.Geometry.Count > 0)
      {
        // Add the mesh to the model
        rootNode.Children.Add(meshContent);
      }
      else
      {
        // Convert to a general NodeContent
        NodeContent nodeContent = new NodeContent();
        nodeContent.Name = meshContent.Name;

        // Add the transform-only node to the model
        rootNode.Children.Add(nodeContent);
      }

      meshBuilder = null;*/
      groups.Add(curGroup);
      curGroup = null;
    }

    /// <summary>
    /// Parses an MTL file and adds all its materials to the materials collection
    /// </summary>
    /// <param name="filename">Full path of MTL file.</param>
    private void ImportMaterials(string filename)
    {
      /*// Material library identity is tied to the file it is loaded from
      mtlFileIdentity = new ContentIdentity(filename);

      // Reset the current material
      materialContent = null;

      try
      {
        // Loop over each tokenized line of the MTL file
        foreach (String[] lineTokens in
            GetLineTokens(filename, mtlFileIdentity))
        {
          ParseMtlLine(lineTokens);
        }
      }
      catch (InvalidContentException)
      {
        // InvalidContentExceptions do not need further processing
        throw;
      }
      catch (Exception e)
      {
        // Wrap exception with content identity (includes line number)
        throw new InvalidContentException(
            "Unable to parse mtl file. Exception:\n" + e.Message,
            mtlFileIdentity, e);
      }

      // Finish the last material
      if (materialContent != null)
        materials.Add(materialContent.Name, materialContent);*/
    }


    /// <summary>
    /// Parses and executes an individual line of a MTL file.
    /// </summary>
    /// <param name="lineTokens">Line to parse as tokens</param>
    void ParseMtlLine(string[] lineTokens)
    {
      /*// Switch on line type
      switch (lineTokens[0].ToLower())
      {
        // New material
        case "newmtl":
          // Finish the current material
          if (materialContent != null)
            materials.Add(materialContent.Name, materialContent);

          // Start a new material
          materialContent = new BasicMaterialContent();
          materialContent.Identity =
              new ContentIdentity(mtlFileIdentity.SourceFilename);

          // Next token is new material's name
          materialContent.Name = lineTokens[1];
          break;

        // Diffuse color
        case "kd":
          materialContent.DiffuseColor = ParseVector3(lineTokens);
          break;

        // Diffuse texture
        case "map_kd":
          // Reference a texture relative to this MTL file
          materialContent.Texture = new ExternalReference<TextureContent>(
              lineTokens[1], mtlFileIdentity);
          break;

        // Ambient color
        case "ka":
          // Ignore ambient color because it should be set by scene lights
          break;

        // Specular color
        case "ks":
          materialContent.SpecularColor = ParseVector3(lineTokens);
          break;

        // Specular power
        case "ns":
          materialContent.SpecularPower = float.Parse(lineTokens[1],
                  CultureInfo.InvariantCulture);
          break;

        // Emissive color
        case "ke":
          materialContent.EmissiveColor = ParseVector3(lineTokens);
          break;

        // Alpha
        case "d":
          materialContent.Alpha = float.Parse(lineTokens[1],
                  CultureInfo.InvariantCulture);
          break;

        // Illumination mode (0=constant, 1=diffuse, 2=diffuse+specular)
        case "illum":
          // Store as opaque data. This alllows the rendering engine to
          // interpret this data if it likes. For example, it can set
          // constant constant illumination mode by dissabling lighting
          // on the BasicEffect.
          materialContent.OpaqueData.Add("Illumination mode",
              int.Parse(lineTokens[1], CultureInfo.InvariantCulture));
          break;

        // Unsupported or invalid line types
        default:
          throw new InvalidContentException(
              "Unsupported or invalid line type '" + lineTokens[0] + "'",
              mtlFileIdentity);
      }*/
    }

    /// <summary>
    /// Parses a Vector2 from tokens of an OBJ file line.
    /// </summary>
    /// <param name="lineTokens">X and Y coordinates in lineTokens[1 through 2].
    /// </param>
    private static Vector2 ParseVector2(string[] lineTokens)
    {
      return new Vector2(
          float.Parse(lineTokens[1], CultureInfo.InvariantCulture),
          float.Parse(lineTokens[2], CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Parses a Vector3 from tokens of an OBJ file line.
    /// </summary>
    /// <param name="lineTokens">X,Y and Z coordinates in lineTokens[1 through 3].
    /// </param>
    private static Vector3 ParseVector3(string[] lineTokens)
    {
      return new Vector3(
          float.Parse(lineTokens[1], CultureInfo.InvariantCulture),
          float.Parse(lineTokens[2], CultureInfo.InvariantCulture),
          float.Parse(lineTokens[3], CultureInfo.InvariantCulture));
    }
  }
}
