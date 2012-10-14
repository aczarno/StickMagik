using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
//using Tao.OpenGl;
using Crom.Controls;

namespace StickMagik
{
  public partial class OpenGLWindow : DockableToolWindow
  {
    ObjMesh objMesh;
    //ObjMesh mesh;
    public OpenGLWindow()
    {
      InitializeComponent();
    }

    private void OpenGLWindow_Load(object sender, EventArgs e)
    {
      objMesh = new ObjMesh("../../Assets/TestStick.obj");
      glWindow.InitializeContexts();
      //ObjMeshLoader.Load(
    }

    private void glWindow_Paint(object sender, PaintEventArgs e)
    {
      GL.ClearColor(0.0f, 0.0f, 1.0f, 1.0f);
      GL.Clear(ClearBufferMask.ColorBufferBit);

      //GL.MatrixMode(MatrixMode.Projection); //GL_PROJECTION);
      //GL.LoadIdentity();
      //GL.Frustum(100, 100, 100, 100, 1, 1000);// glFrustum(fovy, aspect, znear, zfar);

      // start fresh
      GL.MatrixMode(MatrixMode.Modelview); // glMatrixMode(GL_MODELVIEW);
      GL.LoadIdentity(); // glLoadIdentity();

      // set up camera
      GL.Rotate(0, 1, 0, 90); // glRotatef(0, 1, 0, -camHeading);
      GL.Rotate(1, 0, 0, -90); // glRotatef(1, 0, 0, -camPitch);
      GL.Translate(-3, 0,0); // glTranslatef(-camPos.x, -camPos.y, -camPos.z);


      // save the camera matrix
      GL.PushMatrix(); // glPushMatrix();
        GL.Translate(0, 0, 0);
        
        objMesh.Render();
      // restore the camera matrix
      GL.PopMatrix();

     
      /*GL.Begin(BeginMode.Triangles);
        GL.Color3(1, 0, 0);
        GL.Vertex2(-1.0f, -1.0f);

        GL.Color3(0, 1, 0);
        GL.Vertex2(1.0f, -1.0f);
        GL.Color3(0, 0, 1);
        GL.Vertex2(0f, 1.0f);
      GL.End();*/
    }
  }
}
