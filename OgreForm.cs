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
        float zoomDelta = (float)System.Math.Pow(2.71828183, -delta/500.0f) * (delta>0?1:-1);
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
      public OgreWindow(Point origin, IntPtr hWnd)
      {
        pos = origin;
        this.hWnd = hWnd;
        Messenger<string>.AddListener("SelectComponent", selectComponent);
      }

      ~OgreWindow()
      {
        Messenger<string>.RemoveListener("SelectComponent", selectComponent);
      }

      public void init()
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
          return;

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
        pointLight.DiffuseColour = ColourValue.Red;
        pointLight.SpecularColour = ColourValue.Red;

        Light directionalLight = sceneMgr.CreateLight("directionalLight");
        directionalLight.Type = Light.LightTypes.LT_DIRECTIONAL;
        directionalLight.DiffuseColour = new ColourValue(.25f, .25f, 0);
        directionalLight.SpecularColour = new ColourValue(.25f, .25f, 0);
        directionalLight.Direction = new Mogre.Vector3(0, -1, 1);

        Light spotLight = sceneMgr.CreateLight("spotLight");
        spotLight.Type = Light.LightTypes.LT_SPOTLIGHT;
        spotLight.DiffuseColour = ColourValue.Blue;
        spotLight.SpecularColour = ColourValue.Blue;
        spotLight.Direction = new Mogre.Vector3(-1, -1, 0);
        spotLight.Position = new Mogre.Vector3(300, 300, 0);
        spotLight.SetSpotlightRange(new Degree(35), new Degree(50));

        // Set up our Input
        root.FrameRenderingQueued += new FrameListener.FrameRenderingQueuedHandler(Input);        
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

        Mesh.Const_SubMeshNameMap map = mainStick.GetMesh().GetSubMeshNameMap();
        ModelComponent frag;
        for (uint i = 0; i < map.Count; i++)
        {
          for(Mesh.Const_SubMeshNameMap.ConstIterator start = map.Begin();
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

      public void injectMouseClick(int x, int y)
      {
        //normalise mouse coordinates to [0,1]
        //we could have used the panel's width/height in pixels instead of viewport's width/height
        float scrx = (float)x / viewport.ActualWidth;
        float scry = (float)y / viewport.ActualHeight;

        Ray ray = camMgr.mainCam.GetCameraToViewportRay(scrx, scry);
        RaySceneQuery query = sceneMgr.CreateRayQuery(ray);
        RaySceneQueryResult results = query.Execute();

        foreach (RaySceneQueryResultEntry entry in results)
        {
          
          entry.movable.ParentSceneNode.ShowBoundingBox = true;
          // Do stuff with the objects that intersect the ray
          continue;
        }

        //if we hit the head then there is 1 result
        //return results.Count > 0;
      }

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

    protected RaySceneQuery raySceneQuery = null;
    private void renderPanel_MouseClick(object sender, MouseEventArgs e)
    {
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
        window.camMgr.Zoom(e.Delta*-0.1f);
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
