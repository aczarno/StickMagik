using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mogre;
using Crom.Controls;
using MOIS;
using System.Runtime.InteropServices;

namespace StickMagik
{
  public partial class OgreForm : DockableToolWindow
  {
    protected OgreWindow window;

    public OgreForm()
    {
      InitializeComponent();
      //this.Size = new Size(800, 600);
      this.Disposed += new EventHandler(OgreForm_Disposed);
      //this.Resize += new EventHandler(OgreForm_Resize);

      window = new OgreWindow(new Point(100, 30), renderWindow.Handle);
      window.init();
    }

    void OgreForm_Disposed(object sender, EventArgs e)
    {
      window.Dispose();
    }

    public void Render()
    {
      window.Render();
    }

    public void LoadModel(string path)
    {
      window.LoadModel(path);
    }

    public class CamManager
    {
      protected List<Camera> cams = new List<Camera>();
      protected SceneManager sceneMgr = null;

      public Camera mainCam = null;
      SceneNode camNode = null;

      protected float distanceToCenter = 0f;

      public bool Initialize(ref SceneManager _sceneMgr)
      {
        sceneMgr = _sceneMgr;

        // Create the camera
        mainCam = sceneMgr.CreateCamera("MainCamera");
        mainCam.NearClipDistance = 5;

        camNode = sceneMgr.RootSceneNode.CreateChildSceneNode("camNode", new Mogre.Vector3(0, 0, 0));
        camNode.AttachObject(mainCam);
        camNode.Position = new Mogre.Vector3(0, 20, 100);
        cams.Add(mainCam);

        return true;
      }

      public void Pan(float dx, float dz)
      {
        Mogre.Vector3 v = mainCam.RealOrientation * ((Mogre.Vector3.UNIT_Y * dz) + (Mogre.Vector3.UNIT_X * -dx));
        camNode.Translate(v.x, v.y, 0);
      }

      public void Tumble(float dx, float dy)
      {
        dx /= 100.0f;
        dy /= 100.0f;
        camNode.Pitch(new Radian(-dy), Node.TransformSpace.TS_LOCAL);
        camNode.Yaw(new Radian(-dx), Node.TransformSpace.TS_WORLD);
      }

      public void Zoom(float delta)
      {
        float zoomDelta = (float)System.Math.Pow(2.71828183, -delta / 500.0f) * (delta > 0 ? 1 : -1);
        Mogre.Vector3 translateVec = camNode.Orientation.ZAxis * zoomDelta;
        camNode.Translate(translateVec);
      }
    }

    // Our main rendering class that handles all Ogre related buisness
    public class OgreWindow
    {
      public Root root;
      public SceneManager sceneMgr;

      public CamManager camMgr;
      protected Viewport viewport;
      protected RenderWindow window;
      protected Point pos;
      protected IntPtr hWnd;

      protected Entity mainStick;
      // Input Variables
      protected InputManager inputMgr;
      protected Keyboard keyboard;
      protected Mouse mouse;

      protected SubEntity curComponent;
      protected MaterialPtr restoreMat;

      protected RaySceneQuery raySceneQuery;

      public OgreWindow(Point origin, IntPtr hWnd)
      {
        pos = origin;
        this.hWnd = hWnd;
        Messenger<string>.AddListener("SelectComponent", selectComponent);
        Messenger.AddListener("GetCurSelectedMaterial", onGetCurSelectedMat);
      }

      ~OgreWindow()
      {
        Messenger<string>.RemoveListener("SelectComponent", selectComponent);
        Messenger.RemoveListener("GetCurSelectedMaterial", onGetCurSelectedMat);
      }

      public bool init()
      {
        // Start with a new root to get this party started
        root = new Root();

        // Configuration buisness
        ConfigFile config = new ConfigFile();
        config.Load("resources.cfg", "\t:=", true);

        // Go through all our configuration settings
        ConfigFile.SectionIterator itor = config.GetSectionIterator();
        string secName, typeName, archName;

        // Move through all of the sections
        while (itor.MoveNext())
        {
          secName = itor.CurrentKey;
          ConfigFile.SettingsMultiMap settings = itor.Current;
          foreach (KeyValuePair<string, string> pair in settings)
          {
            typeName = pair.Key;
            archName = pair.Value;
            ResourceGroupManager.Singleton.AddResourceLocation(archName, typeName, secName);
          }
        }

        // Configure our window and set up the RenderSystem
        bool found = false;
        foreach (RenderSystem rs in root.GetAvailableRenderers())
        {
          root.RenderSystem = rs;
          string rname = root.RenderSystem.Name;
          if (rname == "Direct3D9 Rendering Subsystem")
          {
            found = true;
            break;
          }
        }

        // If we can't find the DirectX rendering system somethign is seriously wrong
        if (!found)
          return false;

        root.RenderSystem.SetConfigOption("Full Screen", "No");
        root.RenderSystem.SetConfigOption("Video Mode", "640 x 480 @ 32-bit colour");

        root.Initialise(false);
        NameValuePairList misc = new NameValuePairList();
        misc["externalWindowHandle"] = hWnd.ToString();
        window = root.CreateRenderWindow("Simple Mogre Form Window", 0, 0, false, misc);
        ResourceGroupManager.Singleton.InitialiseAllResourceGroups();

        // Create our SceneManager
        sceneMgr = root.CreateSceneManager(SceneType.ST_GENERIC, "SceneMgr");
        sceneMgr.AmbientLight = new ColourValue(0.5f, 0.5f, 0.5f);
        sceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_STENCIL_ADDITIVE;

        // Create the camera
        camMgr = new CamManager();
        camMgr.Initialize(ref sceneMgr);

        viewport = window.AddViewport(camMgr.mainCam);
        viewport.BackgroundColour = new ColourValue(0, 0, 0, 1);

        // Load our stick here
        LoadModel("TEStick.mesh");

        // Set up ground
        Plane plane = new Plane(Mogre.Vector3.UNIT_Y, 0);
        MeshManager.Singleton.CreatePlane("ground", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, plane,
          1500, 1500, 20, 20, true, 1, 5, 5, Mogre.Vector3.UNIT_Z);
        Entity ground = sceneMgr.CreateEntity("GroundEnt", "ground");
        sceneMgr.RootSceneNode.CreateChildSceneNode().AttachObject(ground);

        ground.SetMaterialName("Examples/Rockwall");
        ground.CastShadows = false;

        // Set up some lights
        Light pointLight = sceneMgr.CreateLight("pointLight");
        pointLight.Type = Light.LightTypes.LT_POINT;
        pointLight.Position = new Mogre.Vector3(0, 150, 250);
        pointLight.DiffuseColour = ColourValue.White;
        pointLight.SpecularColour = ColourValue.White;

        Light directionalLight = sceneMgr.CreateLight("directionalLight");
        directionalLight.Type = Light.LightTypes.LT_DIRECTIONAL;
        directionalLight.DiffuseColour = new ColourValue(.25f, .25f, 0);
        directionalLight.SpecularColour = new ColourValue(.25f, .25f, 0);
        directionalLight.Direction = new Mogre.Vector3(0, -1, 1);

        Light spotLight = sceneMgr.CreateLight("spotLight");
        spotLight.Type = Light.LightTypes.LT_SPOTLIGHT;
        spotLight.DiffuseColour = ColourValue.White;
        spotLight.SpecularColour = ColourValue.White;
        spotLight.Direction = new Mogre.Vector3(-1, -1, 0);
        spotLight.Position = new Mogre.Vector3(300, 300, 0);
        spotLight.SetSpotlightRange(new Degree(35), new Degree(50));

        // Set up our Input
        root.FrameRenderingQueued += new FrameListener.FrameRenderingQueuedHandler(Input);

        // Set up for picking
        raySceneQuery = sceneMgr.CreateRayQuery(new Ray(), SceneManager.WORLD_GEOMETRY_TYPE_MASK);
        if (null == raySceneQuery)
          return false;
        raySceneQuery.SetSortByDistance(true);

        return true;
      }

      public bool LoadModel(string modelPath)
      {
        mainStick = sceneMgr.CreateEntity("Stick", modelPath);
        mainStick.CastShadows = true;
        SceneNode node = sceneMgr.RootSceneNode.CreateChildSceneNode("StickNode");
        node.AttachObject(mainStick);
        node.Position = new Mogre.Vector3(0, 10, 0);
        //node.Rotate(Mogre.Vector3.UNIT_X, Mogre.Math.PI);
        //node.Rotate(Mogre.Vector3.UNIT_Y, Mogre.Math.PI);

        ArcadeStickData.Instance.FillData(modelPath, ref mainStick);
        Mesh.Const_SubMeshNameMap map = mainStick.GetMesh().GetSubMeshNameMap();
        ModelComponent frag;
        for (uint i = 0; i < map.Count; i++)
        {
          for (Mesh.Const_SubMeshNameMap.ConstIterator start = map.Begin();
            start != map.End(); start++)
          {
            if (start.Value == i)
            {
              frag = new ModelComponent(start.Key);
              Messenger<ModelComponent>.Broadcast("AddFragment", frag);
              break;
            }
          }

        }
        return true;
      }

      public void selectComponent(string name)
      {
        // Restore old material to previously selected component
        if (curComponent != null)
          curComponent.SetMaterial(restoreMat);

        if (string.IsNullOrEmpty(name))
        {
          curComponent = null;
          return;
        }

        // Set the new current component and add a wireframe highlight to indicated selection
        curComponent = mainStick.GetSubEntity(name);
        restoreMat = curComponent.GetMaterial();
        MaterialPtr newMat = MaterialManager.Singleton.Create("Highlight", "General");
        Pass highlight = newMat.GetTechnique(0).GetPass(0);
        highlight.LightingEnabled = false;
        highlight.PolygonMode = PolygonMode.PM_WIREFRAME;
        highlight = newMat.GetTechnique(0).CreatePass();
        highlight = curComponent.Technique.GetPass(0);

        curComponent.SetMaterial(newMat);

        Messenger<Material>.Broadcast("CurSelectedMaterial", restoreMat);
      }

      void onGetCurSelectedMat()
      {
        if (curComponent != null)
          Messenger<Material>.Broadcast("CurSelectedMaterial", curComponent.GetMaterial());
      }

      public bool HighlightComponent(string name)
      {
        return true;
      }

      // Here is where we can handle input.
      bool Input(FrameEvent evt)
      {
        /*keyboard.Capture();
        mouse.Capture();

        //if(keyboard.IsKeyDown(KeyCode.KC_T))
        //  do shit
        if (mouse.MouseState.ButtonDown(MouseButtonID.MB_Left))
        {
          Ray mouseRay = camMgr.mainCam.GetCameraToViewportRay(mouse.MouseState.X.rel, mouse.MouseState.Y.rel);
          raySceneQuery.Ray = mouseRay;

          RaySceneQueryResult result = raySceneQuery.Execute();
          //RaySceneQueryResult.Iterator iter = result.GetEnumerator();

          for (int i = 0; i < result.Count; i++)
          {
            continue;
            //result[i].movable.bound
          }

        }*/

        return true;
      }

      public string GetSubmeshName(SubMesh _mesh)
      {
        Mesh.Const_SubMeshNameMap map = mainStick.GetMesh().GetSubMeshNameMap();

        for (uint i = 0; i < map.Count; i++)
        {
          if(_mesh == mainStick.GetMesh().GetSubMesh((ushort)i))
          {
            for (Mesh.Const_SubMeshNameMap.ConstIterator start = map.Begin();
            start != map.End(); start++)
            {
              if (start.Value == i)
                return start.Key;
            }
          }
        }

        return "";
      }

      public void injectMouseClick(int x, int y)
      {
        //normalise mouse coordinates to [0,1]
        //we could have used the panel's width/height in pixels instead of viewport's width/height
        float scrx = (float)x / viewport.ActualWidth;
        float scry = (float)y / viewport.ActualHeight;

        Ray ray = camMgr.mainCam.GetCameraToViewportRay(scrx, scry);
        //RaySceneQuery query = sceneMgr.CreateRayQuery(ray);
        //RaySceneQueryResult results = query.Execute();
        
        SubMesh m = GetSubmeshFromRay(ray);
        Messenger<string>.Broadcast("SelectComponent", GetSubmeshName(m));
      }

      public SubMesh GetSubmeshFromRay(Mogre.Vector3 point, Mogre.Vector3 normal)
      {
        Ray ray = new Ray(point, normal);
        return GetSubmeshFromRay(ray);
      }

      public SubMesh GetSubmeshFromRay(Ray ray)
      {
        // Check we are initialised
        if (raySceneQuery != null)
        {
          // create a query object
          raySceneQuery.Ray = ray;

          // execute the query, returns a vector of hits
          RaySceneQueryResult rayresult = raySceneQuery.Execute();
          if (rayresult.Count <= 0)
            // raycast did not hit an objects bounding box
            return null;
        }
        else
          return null;

        SubMesh resultSub = null;
        // at this point we have raycast to a series of different objects bounding boxes.
        // we need to test these different objects to see which is the first polygon hit.
        // there are some minor optimizations (distance based) that mean we wont have to
        // check all of the objects most of the time, but the worst case scenario is that
        // we need to test every triangle of every object.
        float closest_distance = -1.0f;
        Mogre.Vector3 closest_result = Mogre.Vector3.ZERO;
        Mogre.Vector3 vNormal = Mogre.Vector3.ZERO;
        RaySceneQueryResult query_result = raySceneQuery.GetLastResults();

        foreach (RaySceneQueryResultEntry this_result in query_result)
        {
          // stop checking if we have found a raycast hit that is closer
          // than all remaining entities
          if ((closest_distance >= 0.0f) &&
              (closest_distance < this_result.distance))
            break;

          // only check this result if its a hit against an entity
          if ((this_result.movable != null) &&
              (this_result.movable.MovableType == "Entity"))
          {
            // get the entity to check
            Entity pentity = (Entity)this_result.movable;

            // mesh data to retrieve 
            List<uint> vertex_count = new List<uint>((int)pentity.NumSubEntities);
            List<uint> index_count = new List<uint>((int)pentity.NumSubEntities);
            List<Mogre.Vector3[]> vertices = new List<Mogre.Vector3[]>((int)pentity.NumSubEntities);
            List<UInt64[]> indices = new List<UInt64[]>((int)pentity.NumSubEntities);

            int ncf = -1; // new_closest_found

            for (ushort sm = 0; sm < pentity.NumSubEntities; sm++)
            {
              // get the mesh information
              /*
              GetMeshInformation(pentity.GetMesh().GetSubMesh(sm),
                  ref vertex_count[sm], ref vertices[sm], ref index_count[sm], ref indices[sm],
                  pentity.ParentNode._getDerivedPosition(),    // WorldPosition
                  pentity.ParentNode._getDerivedOrientation(), // WorldOrientation
                  pentity.ParentNode.GetScale());*/
              vertex_count.Add(new uint());
              uint vcount_ref = vertex_count[sm];
              vertices.Add(new Mogre.Vector3[0]);
              Mogre.Vector3[] v_ref = vertices[sm];
              index_count.Add(new uint());
              uint icount_ref = index_count[sm];
              indices.Add(new UInt64[0]);
              UInt64[] i_ref = indices[sm];
              GetSubMeshInformation(pentity.GetMesh(), ref vcount_ref, ref v_ref, ref icount_ref, ref i_ref,
                pentity.ParentNode._getDerivedPosition(),   // World Position
                pentity.ParentNode._getDerivedOrientation(),  // WorldOrientation
                pentity.ParentNode.GetScale(), sm);

              // test for hitting individual triangles on the mesh
              for (int i = 0; i < (int)icount_ref; i += 3)
              {
                // check for a hit against this triangle
                Mogre.Pair<bool, float> hit = Mogre.Math.Intersects(ray, v_ref[i_ref[i]],
                    v_ref[i_ref[i + 1]], v_ref[i_ref[i + 2]], true, false);

                // if it was a hit check if its the closest
                if (hit.first)
                {
                  if ((closest_distance < 0.0f) ||
                      (hit.second < closest_distance))
                  {
                    // this is the closest so far, save it off
                    closest_distance = hit.second;
                    ncf = i;
                    resultSub = pentity.GetMesh().GetSubMesh(sm);
                  }
                }
              }
            }

            if (ncf > -1)
            {
              closest_result = ray.GetPoint(closest_distance);
              // if you don't need the normal, comment this out; you'll save some CPU cycles.
              //Mogre.Vector3 v1 = vertices[indices[ncf]] - vertices[indices[ncf + 1]];
              //Mogre.Vector3 v2 = vertices[indices[ncf + 2]] - vertices[indices[ncf + 1]];
              //vNormal = v1.CrossProduct(v2);
            }

            // free the verticies and indicies memory
            for(int i=0; i<vertices.Count; i++)
            {
              vertices[i] = null;
            }
            vertices.Clear();

            for (int i = 0; i < indices.Count; i++)
            {
              indices[i] = null;
            }
            indices.Clear();
          }
        }

        return resultSub;
      }

      public bool RaycastFromPoint(Mogre.Vector3 point, Mogre.Vector3 normal, ref Mogre.Vector3 result,
          ref Mogre.Vector3 resNormal)
      {
        Ray ray = new Ray(point, normal);
        return RaycastFromPoint(ray, ref result, ref resNormal);
      }

      // raycast from a point in to the scene.
      // returns success or failure.
      // on success the point is returned in the result.
      public bool RaycastFromPoint(Ray ray, ref Mogre.Vector3 result, ref Mogre.Vector3 resNormal)
      {
        // check we are initialised
        if (raySceneQuery != null)
        {
          // create a query object
          raySceneQuery.Ray = ray;

          // execute the query, returns a vector of hits
          RaySceneQueryResult rayresult = raySceneQuery.Execute();
          if (rayresult.Count <= 0)
            // raycast did not hit an objects bounding box
            return false;
        }
        else
          return false;

        // at this point we have raycast to a series of different objects bounding boxes.
        // we need to test these different objects to see which is the first polygon hit.
        // there are some minor optimizations (distance based) that mean we wont have to
        // check all of the objects most of the time, but the worst case scenario is that
        // we need to test every triangle of every object.
        float closest_distance = -1.0f;
        Mogre.Vector3 closest_result = Mogre.Vector3.ZERO;
        Mogre.Vector3 vNormal = Mogre.Vector3.ZERO;
        RaySceneQueryResult query_result = raySceneQuery.GetLastResults();

        foreach (RaySceneQueryResultEntry this_result in query_result)
        {
          // stop checking if we have found a raycast hit that is closer
          // than all remaining entities
          if ((closest_distance >= 0.0f) &&
              (closest_distance < this_result.distance))
            break;

          // only check this result if its a hit against an entity
          if ((this_result.movable != null) &&
              (this_result.movable.MovableType == "Entity"))
          {
            // get the entity to check
            Entity pentity = (Entity)this_result.movable;

            // mesh data to retrieve 
            uint vertex_count = 0;
            uint index_count = 0;
            Mogre.Vector3[] vertices = new Mogre.Vector3[0];
            UInt64[] indices = new UInt64[0];

            // get the mesh information
            GetMeshInformation(pentity.GetMesh(),
                ref vertex_count, ref vertices, ref index_count, ref indices,
                pentity.ParentNode._getDerivedPosition(),    // WorldPosition
                pentity.ParentNode._getDerivedOrientation(), // WorldOrientation
                pentity.ParentNode.GetScale());

            int ncf = -1; // new_closest_found

            // test for hitting individual triangles on the mesh
            for (int i = 0; i < (int)index_count; i += 3)
            {
              // check for a hit against this triangle
              Mogre.Pair<bool, float> hit = Mogre.Math.Intersects(ray, vertices[indices[i]],
                  vertices[indices[i + 1]], vertices[indices[i + 2]], true, false);

              // if it was a hit check if its the closest
              if (hit.first)
              {
                if ((closest_distance < 0.0f) ||
                    (hit.second < closest_distance))
                {
                  // this is the closest so far, save it off
                  closest_distance = hit.second;
                  ncf = i;
                }
              }
            }

            if (ncf > -1)
            {
              closest_result = ray.GetPoint(closest_distance);
              // if you don't need the normal, comment this out; you'll save some CPU cycles.
              Mogre.Vector3 v1 = vertices[indices[ncf]] - vertices[indices[ncf + 1]];
              Mogre.Vector3 v2 = vertices[indices[ncf + 2]] - vertices[indices[ncf + 1]];
              vNormal = v1.CrossProduct(v2);
            }

            // free the verticies and indicies memory
            vertices = null;
            indices = null;
          }
        }

        // if we found a new closest raycast for this object, update the
        // closest_result before moving on to the next object.
        if (closest_distance >= 0.0f)
        {
          result = new Mogre.Vector3();
          result.x = closest_result.x; result.y = closest_result.y; result.z = closest_result.z;
          resNormal = vNormal / vNormal.Normalise();


          // this visualizes the 'result' position 
          if (!sceneMgr.HasSceneNode("marker"))
          {
            SceneNode node = sceneMgr.CreateSceneNode("marker");
            Entity ent = sceneMgr.CreateEntity("marker", "Cube.mesh");
            node.AttachObject(ent);
            node.Position = result;
            node.Scale(0.003f, 0.003f, 0.003f);
            sceneMgr.RootSceneNode.AddChild(node);
          }
          else
            sceneMgr.GetSceneNode("marker").Position = result;


          // raycast success
          return true;
        }
        else
          // raycast failed
          return false;
      } // RayCastFromPoint

      public unsafe void GetSubMeshInformation(MeshPtr mesh,
                                               ref uint vertex_count,
                                               ref Mogre.Vector3[] vertices,
                                               ref uint index_count,
                                               ref UInt64[] indices,
                                               Mogre.Vector3 position,
                                               Quaternion orientation,
                                               Mogre.Vector3 scale,
                                               ushort subMeshIndex)
      {
        bool added_shared = false;
        uint current_offset = 0;
        uint shared_offset = 0;
        uint next_offset = 0;
        uint index_offset = 0;

        vertex_count = index_count = 0;

        SubMesh submesh = mesh.GetSubMesh(subMeshIndex);

        // We only need to add the shared vertices once
        if (submesh.useSharedVertices)
        {
          if (!added_shared)
          {
            vertex_count += mesh.sharedVertexData.vertexCount;
            added_shared = true;
          }
        }
        else
        {
          vertex_count += submesh.vertexData.vertexCount;
        }

        // Add the indices
        index_count += submesh.indexData.indexCount;

        // Allocate space for the vertices and indices
        vertices = new Mogre.Vector3[vertex_count];
        indices = new UInt64[index_count];
        added_shared = false;

        // Run through the submesh again, adding the data into the arrays
        VertexData vertex_data = submesh.useSharedVertices ? mesh.sharedVertexData : submesh.vertexData;

        if (!submesh.useSharedVertices || (submesh.useSharedVertices && !added_shared))
        {
          if (submesh.useSharedVertices)
          {
            added_shared = true;
            shared_offset = current_offset;
          }

          VertexElement posElem = vertex_data.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.VES_POSITION);
          HardwareVertexBufferSharedPtr vbuf = vertex_data.vertexBufferBinding.GetBuffer(posElem.Source);

          byte* vertex = (byte*)vbuf.Lock(HardwareBuffer.LockOptions.HBL_READ_ONLY);
          float* pReal;

          // There is _no_ baseVertexPointerToElement() which takes an Ogre::Real or a double
          //  as second argument. So make it float, to avoid trouble when Ogre::Real will
          //  be comiled/typedefed as double:
          //      Ogre::Real* pReal;
          for (int j = 0; j < vertex_data.vertexCount; ++j, vertex += vbuf.VertexSize)
          {
            posElem.BaseVertexPointerToElement(vertex, &pReal);
            Mogre.Vector3 pt = new Mogre.Vector3(pReal[0], pReal[1], pReal[2]);
            vertices[current_offset + j] = (orientation * (pt * scale)) + position;
          }
          // |!| Important: VertexBuffer Unlock() + Dispose() avoids memory corruption
          vbuf.Unlock();
          vbuf.Dispose();
          next_offset += vertex_data.vertexCount;
        }

        IndexData index_data = submesh.indexData;
        uint numTris = index_data.indexCount / 3;
        HardwareIndexBufferSharedPtr ibuf = index_data.indexBuffer;

        // UNPORTED line of C++ code (because ibuf.IsNull() doesn't exist in C#)
        // if( ibuf.isNull() ) continue
        // need to check if index buffer is valid (which will be not if the mesh doesn't have triangles like a pointcloud)

        bool use32bitindexes = (ibuf.Type == HardwareIndexBuffer.IndexType.IT_32BIT);

        uint* pLong = (uint*)ibuf.Lock(HardwareBuffer.LockOptions.HBL_READ_ONLY);
        ushort* pShort = (ushort*)pLong;
        uint offset = submesh.useSharedVertices ? shared_offset : current_offset;
        if (use32bitindexes)
        {
          for (int k = 0; k < index_data.indexCount; ++k)
            indices[index_offset++] = (UInt64)pLong[k] + (UInt64)offset;
        }
        else
        {
          for (int k = 0; k < index_data.indexCount; ++k)
            indices[index_offset++] = (UInt64)pShort[k] + (UInt64)offset;
        }
        // |!| Important: IndexBuffer Unlock() + Dispose() avoids memory corruption
        ibuf.Unlock();
        ibuf.Dispose();
        current_offset = next_offset;

        // |!| Important: MeshPtr Dispose() avoids memory corruption
        mesh.Dispose(); // This dispose the MeshPtr, not the Mesh
      }

      // Get the mesh information for the given mesh.
      // Code found in Wiki: www.ogre3d.org/wiki/index.php/RetrieveVertexData
      public unsafe void GetMeshInformation(MeshPtr mesh,
                                            ref uint vertex_count,
                                            ref Mogre.Vector3[] vertices,
                                            ref uint index_count,
                                            ref UInt64[] indices,
                                            Mogre.Vector3 position,
                                            Quaternion orientation,
                                            Mogre.Vector3 scale)
      {
        bool added_shared = false;
        uint current_offset = 0;
        uint shared_offset = 0;
        uint next_offset = 0;
        uint index_offset = 0;

        vertex_count = index_count = 0;

        // Calculate how many vertices and indices we're going to need
        for (ushort i = 0; i < mesh.NumSubMeshes; ++i)
        {
          SubMesh submesh = mesh.GetSubMesh(i);

          // We only need to add the shared vertices once
          if (submesh.useSharedVertices)
          {
            if (!added_shared)
            {
              vertex_count += mesh.sharedVertexData.vertexCount;
              added_shared = true;
            }
          }
          else
          {
            vertex_count += submesh.vertexData.vertexCount;
          }

          // Add the indices
          index_count += submesh.indexData.indexCount;
        }

        // Allocate space for the vertices and indices
        vertices = new Mogre.Vector3[vertex_count];
        indices = new UInt64[index_count];
        added_shared = false;

        // Run through the submeshes again, adding the data into the arrays
        for (ushort i = 0; i < mesh.NumSubMeshes; ++i)
        {
          SubMesh submesh = mesh.GetSubMesh(i);
          VertexData vertex_data = submesh.useSharedVertices ? mesh.sharedVertexData : submesh.vertexData;

          if (!submesh.useSharedVertices || (submesh.useSharedVertices && !added_shared))
          {
            if (submesh.useSharedVertices)
            {
              added_shared = true;
              shared_offset = current_offset;
            }

            VertexElement posElem =
                vertex_data.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.VES_POSITION);
            HardwareVertexBufferSharedPtr vbuf =
                vertex_data.vertexBufferBinding.GetBuffer(posElem.Source);

            byte* vertex = (byte*)vbuf.Lock(HardwareBuffer.LockOptions.HBL_READ_ONLY);
            float* pReal;

            // There is _no_ baseVertexPointerToElement() which takes an Ogre::Real or a double
            //  as second argument. So make it float, to avoid trouble when Ogre::Real will
            //  be comiled/typedefed as double:
            //      Ogre::Real* pReal;
            for (int j = 0; j < vertex_data.vertexCount; ++j, vertex += vbuf.VertexSize)
            {
              posElem.BaseVertexPointerToElement(vertex, &pReal);
              Mogre.Vector3 pt = new Mogre.Vector3(pReal[0], pReal[1], pReal[2]);
              vertices[current_offset + j] = (orientation * (pt * scale)) + position;
            }
            // |!| Important: VertexBuffer Unlock() + Dispose() avoids memory corruption
            vbuf.Unlock();
            vbuf.Dispose();
            next_offset += vertex_data.vertexCount;
          }

          IndexData index_data = submesh.indexData;
          uint numTris = index_data.indexCount / 3;
          HardwareIndexBufferSharedPtr ibuf = index_data.indexBuffer;

          // UNPORTED line of C++ code (because ibuf.IsNull() doesn't exist in C#)
          // if( ibuf.isNull() ) continue
          // need to check if index buffer is valid (which will be not if the mesh doesn't have triangles like a pointcloud)

          bool use32bitindexes = (ibuf.Type == HardwareIndexBuffer.IndexType.IT_32BIT);

          uint* pLong = (uint*)ibuf.Lock(HardwareBuffer.LockOptions.HBL_READ_ONLY);
          ushort* pShort = (ushort*)pLong;
          uint offset = submesh.useSharedVertices ? shared_offset : current_offset;
          if (use32bitindexes)
          {
            for (int k = 0; k < index_data.indexCount; ++k)
            {
              indices[index_offset++] = (UInt64)pLong[k] + (UInt64)offset;
            }
          }
          else
          {
            for (int k = 0; k < index_data.indexCount; ++k)
            {
              indices[index_offset++] = (UInt64)pShort[k] + (UInt64)offset;
            }
          }
          // |!| Important: IndexBuffer Unlock() + Dispose() avoids memory corruption
          ibuf.Unlock();
          ibuf.Dispose();
          current_offset = next_offset;
        }

        // |!| Important: MeshPtr Dispose() avoids memory corruption
        mesh.Dispose(); // This dispose the MeshPtr, not the Mesh

      } // GetMeshInformation

      public void Render()
      {
        root.RenderOneFrame();
      }

      public void Dispose()
      {
        if (root != null)
        {
          root.Dispose();
          root = null;
        }
      }

      public void Resize()
      {
        window.WindowMovedOrResized();
      }
    }


    private void renderPanel_MouseClick(object sender, MouseEventArgs e)
    {
      if(e.Clicks == 1)
        window.injectMouseClick(e.X, e.Y);
    }

    int prevX = 0;
    int prevY = 0;

    private void renderPanel_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left)
      {
        window.camMgr.Pan(e.X - prevX, e.Y - prevY);
      }
      if (e.Delta != 0)
      {
        window.camMgr.Zoom(e.Delta * -0.1f);
      }
      if (e.Button == MouseButtons.Right)
      {
        window.camMgr.Tumble(e.X - prevX, e.Y - prevY);
      }

      prevX = e.X;
      prevY = e.Y;
    }

    private void renderPanel_Paint(object sender, PaintEventArgs e)
    {
      window.Render();
    }

    private void renderPanel_Resize(object sender, EventArgs e)
    {
      window.Resize();
    }
  }
}
