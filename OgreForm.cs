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

    public class CamManager
    {
      protected List<Camera> cams = new List<Camera>();
      protected SceneManager sceneMgr = null;

      public Camera mainCam = null;
      SceneNode camNode = null;
      /// <summary>
      /// More or less the cam position, might want to just remove and use position
      /// </summary>
      protected Mogre.Vector3 eyePoint = Mogre.Vector3.ZERO;
      /// <summary>
      /// Distance to the lookAt point.
      /// </summary>
      protected float centerOfInterest;

      //protected Mogre.Vector3 mU;	// Right vector
      //protected Mogre.Vector3 mV;	// Readjust up-vector
      //protected Mogre.Vector3 mW;	// Negative view direction

      public bool Initialize(ref SceneManager _sceneMgr)
      {
        sceneMgr = _sceneMgr;

        // Create the camera
        mainCam = sceneMgr.CreateCamera("MainCamera");

        //mainCam.Position = new Mogre.Vector3(0, 10, 100);
        setEyePoint(new Mogre.Vector3(0, 10, 100));
        setCenterOfInterestPoint(Mogre.Vector3.ZERO);
        //mainCam.LookAt(Mogre.Vector3.ZERO);
        mainCam.NearClipDistance = 5;

        camNode = sceneMgr.RootSceneNode.CreateChildSceneNode("camNode", new Mogre.Vector3(0, 0, 0));
        camNode.AttachObject(mainCam);
        cams.Add(mainCam);

        return true;
      }

      void setEyePoint(Mogre.Vector3 _eyePoint)
      {
        eyePoint = _eyePoint;
        calcModelView();
      }

      void setCenterOfInterestPoint(Mogre.Vector3 point)
      {
        float distance = Mogre.Math.Sqrt(Mogre.Math.Sqr(eyePoint.x-point.x) + Mogre.Math.Sqr(eyePoint.y-point.y) + Mogre.Math.Sqr(eyePoint.z-point.z));
        centerOfInterest = distance;
        mainCam.LookAt(point);
      }

      Mogre.Vector3 getCenterOfInterestPoint()
      {
        return eyePoint + mainCam.Direction * centerOfInterest;
      }

      void calcModelView()
      {
        // Row0
        Mogre.Vector3 right = mainCam.Orientation * Mogre.Vector3.UNIT_X;
        // Row1
        Mogre.Vector3 up = mainCam.Orientation * Mogre.Vector3.UNIT_Y;
        // Row2
        Mogre.Vector3 back = -mainCam.Direction;

        // u right
        // v up
        // w back

        Mogre.Vector3 d = new Mogre.Vector3(eyePoint.DotProduct(right), eyePoint.DotProduct(up), eyePoint.DotProduct(back));
        /*mainCam.ViewMatrix.m00 = mU.x; mainCam.ViewMatrix.m10 = mU.y; mainCam.ViewMatrix.m20 = mU.z; mainCam.ViewMatrix.m30 = d.x;
        mainCam.ViewMatrix.m01 = mV.x; mainCam.ViewMatrix.m11 = mV.y; mainCam.ViewMatrix.m21 = mV.z; mainCam.ViewMatrix.m31 = d.y;
        mainCam.ViewMatrix.m02 = mW.x; mainCam.ViewMatrix.m12 = mW.y; mainCam.ViewMatrix.m22 = mW.z; mainCam.ViewMatrix.m32 = d.z;
        mainCam.ViewMatrix.m03 = 0.0f; mainCam.ViewMatrix.m13 = 0.0f; mainCam.ViewMatrix.m23 = 0.0f; mainCam.ViewMatrix.m33 = 1.0f;*/

        mainCam.ViewMatrix.m00 = right.x; mainCam.ViewMatrix.m01 = right.y; mainCam.ViewMatrix.m02 = right.z;
        mainCam.ViewMatrix.m10 = up.x; mainCam.ViewMatrix.m11 = up.y; mainCam.ViewMatrix.m12 = up.z;
        mainCam.ViewMatrix.m20 = -back.x; mainCam.ViewMatrix.m21 = -back.y; mainCam.ViewMatrix.m22 = -back.z;
        mainCam.Position = d;
       
        //mainCam.SetCustomViewMatrix(true, 
      }

      public void Pan(float dx, float dz)
      {

        Mogre.Vector3 v = mainCam.RealOrientation * ((Mogre.Vector3.UNIT_Y * dz) + (Mogre.Vector3.UNIT_X * -dx));
        camNode.Translate(v.x, v.y, 0);

			  //Vec3f mW = mInitialCam.getViewDirection().normalized();
        //Mogre.Vector3 mW = getViewDirection().NormalisedCopy;
        //Mogre.Vector3 mW = -mainCam.Direction;
			  //Vec3f mU = Vec3f::yAxis().cross( mW ).normalized();
        //Mogre.Vector3 mU = Mogre.Vector3.UNIT_Y.CrossProduct(mW).NormalisedCopy;

			  //Vec3f mV = mW.cross( mU ).normalized();
        //Mogre.Vector3 mV = mW.CrossProduct(mU).NormalisedCopy;

			  //mCurrentCam.setEyePoint( mInitialCam.getEyePoint() + mU * deltaX + mV * deltaY );
        //setEyePoint(eyePoint + mU * dx + mV * dz);
      }

      public void Tumble(float dx, float dy)
      {
        dx /= 100.0f;
        dy /= 100.0f;
        camNode.Pitch(new Radian(-dy), Node.TransformSpace.TS_LOCAL);
        camNode.Yaw(new Radian(-dx), Node.TransformSpace.TS_WORLD);
        /*//float deltaY = ( mousePos.y - mInitialMousePos.y ) / 100.0f;
			  //float deltaX = ( mousePos.x - mInitialMousePos.x ) / -100.0f;
        dy /= 100f;
        dx /= -100f;

        //Vec3f mW = mInitialCam.getViewDirection().normalized();
			  //bool invertMotion = ( mInitialCam.getOrientation() * Vec3f::yAxis() ).y < 0.0f;
			  //Vec3f mU = Vec3f::yAxis().cross( mW ).normalized();
			  //Vec3f mV = mW.cross( mU ).normalized();
  			Mogre.Vector3 mW = mainCam.Direction.NormalisedCopy;
        bool invertMotion = (mainCam.Orientation*Mogre.Vector3.UNIT_Y).y < 0.0f;
        Mogre.Vector3 mU = Mogre.Vector3.UNIT_Y.CrossProduct(mW).NormalisedCopy;
        Mogre.Vector3 mV = mW.CrossProduct(mU).NormalisedCopy;

			  if(invertMotion) 
        {
				  dx = -dx;
				  dy = -dy;
			  }
  			
			  //Vec3f rotatedVec = Quatf( mU, deltaY ) * ( mInitialCam.getEyePoint() - mInitialCam.getCenterOfInterestPoint() );
			  //rotatedVec = Quatf( Vec3f::yAxis(), deltaX ) * rotatedVec;
  	    Mogre.Vector3 rotatedVec = new Quaternion(dy, mU) * (eyePoint - getCenterOfInterestPoint());
        rotatedVec = new Quaternion(dx, Mogre.Vector3.UNIT_Y) * rotatedVec;

			  //mCurrentCam.setEyePoint( mInitialCam.getCenterOfInterestPoint() + rotatedVec );
			  //mCurrentCam.setOrientation( mInitialCam.getOrientation() * Quatf( mU, deltaY ) * Quatf( Vec3f::yAxis(), deltaX ) );
        setEyePoint(getCenterOfInterestPoint() + rotatedVec);
        Quaternion newOrient = mainCam.Orientation * new Quaternion(dy, mU) * new Quaternion(dx, Mogre.Vector3.UNIT_Y);
        newOrient.Normalise();
        mainCam.Orientation = newOrient;
        mainCam.Direction = newOrient * Mogre.Vector3.NEGATIVE_UNIT_Z;
        //calcModelView();*/
      }

      public void Zoom(float delta)
      {
        /*float newCOI = powf(2.71828183f, -mouseDelta / 500.0f) * mInitialCam.getCenterOfInterest();
        Vec3f oldTarget = mInitialCam.getCenterOfInterestPoint();
        Vec3f newEye = oldTarget - mInitialCam.getViewDirection() * newCOI;
        mCurrentCam.setEyePoint(newEye);
        mCurrentCam.setCenterOfInterest(newCOI);*/
        float newCOI = (float)System.Math.Pow(2.71828183, -delta / 500.0) * centerOfInterest;
        //float newCOI = centerOfInterest + delta;
        Mogre.Vector3 oldTar = getCenterOfInterestPoint();
        Mogre.Vector3 newEye = oldTar - mainCam.Direction * newCOI;
        //mainCam.Position = mainCam.Position + (delta * mainCam.Direction);
        setEyePoint(newEye);
        centerOfInterest = newCOI;
      }
    }

    public class OgreWindow
    {
      public Root root;
      public SceneManager sceneMgr;

      public CamManager camMgr;
      protected Viewport viewport;
      protected RenderWindow window;
      protected Point pos;
      protected IntPtr hWnd;

      // Input Variables
      protected InputManager inputMgr;
      protected Keyboard keyboard;
      protected Mouse mouse;

      public OgreWindow(Point origin, IntPtr hWnd)
      {
        pos = origin;
        this.hWnd = hWnd;
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

        Entity stick = sceneMgr.CreateEntity("ogre", "TEStick.mesh");
        stick.CastShadows = true;
        SceneNode node = sceneMgr.RootSceneNode.CreateChildSceneNode("ogreNode");
        node.AttachObject(stick);
        node.Position = new Mogre.Vector3(0, 10, 0);
        node.Rotate(Mogre.Vector3.UNIT_X, Mogre.Math.PI);
        node.Rotate(Mogre.Vector3.UNIT_Y, Mogre.Math.PI);

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

        /*ParamList param = new ParamList();
        IntPtr windowHnd;
        window.GetCustomAttribute("WINDOW", out windowHnd); // window is your RenderWindow!
        param.Insert("WINDOW", windowHnd.ToString());
        inputMgr = InputManager.CreateInputSystem(param);
     
        try
        {
          keyboard = (MOIS.Keyboard)inputMgr.CreateInputObject(MOIS.Type.OISKeyboard, false);
          mouse = (MOIS.Mouse)inputMgr.CreateInputObject(MOIS.Type.OISMouse, false);
        }
        catch (SEHException ex)
        {
          //if (OgreException.IsThrown)
          //  MessageBox.Show(OgreException.LastException.Description);
          //else
            MessageBox.Show(ex.Message);
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);
        }   */
        
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
      /*raySceneQuery = window.sceneMgr.CreateRayQuery(new Ray());
      Ray mouseRay = window.camMgr.mainCam.GetCameraToViewportRay(e.X, e.Y);
      raySceneQuery.Ray = mouseRay;

      RaySceneQueryResult result = raySceneQuery.Execute();
      //RaySceneQueryResult.Iterator iter = result.GetEnumerator();

      for (int i = 0; i < result.Count; i++)
      {
        continue;
        //result[i].movable.bound
      }*/
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
        window.camMgr.Zoom(e.Delta*0.1f);
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
