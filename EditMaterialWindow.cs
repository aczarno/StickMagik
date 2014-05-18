using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mogre;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using Crom.Controls;

namespace StickMagik
{
  public partial class EditMaterialWindow : DockableToolWindow
  {
    Mogre.Material curMat;

    public EditMaterialWindow()
    {
      InitializeComponent();
      Messenger<Material>.AddListener("CurSelectedMaterial", onCurSelectedMaterial);
      Messenger.Broadcast("GetCurSelectedMaterial");
    }

    ~EditMaterialWindow()
    {
      Messenger<Material>.RemoveListener("CurSelectedMaterial", onCurSelectedMaterial);
    }

    void onCurSelectedMaterial(Material _mat)
    {
      curMat = _mat;

      Pass p = curMat.GetBestTechnique().GetPass(0);
      if (p.NumTextureUnitStates > 0)
      {
        //int time = Environment.TickCount & Int32.MaxValue;
        //pbMatPreview.Image = MogreTexturePtrToBitmap(TextureManager.Singleton.GetByName(p.GetTextureUnitState(0).TextureName));
        //time = (Environment.TickCount & Int32.MaxValue) - time;

        //time = Environment.TickCount & Int32.MaxValue;

        String foundPath = p.GetTextureUnitState(0).TextureName;
        FileInfoListPtr fileInfos = ResourceGroupManager.Singleton.FindResourceFileInfo("General", foundPath );
        FileInfoList.Iterator it = fileInfos.Begin();
        if(it != fileInfos.End())
        {
           foundPath = it.Value.archive.Name + "/" + foundPath;
        }
        else
           foundPath = "";
        pbMatPreview.Load(foundPath);
        //time = (Environment.TickCount & Int32.MaxValue) - time;

        //return;
      }
      else
        pbMatPreview.Image = null;
    }

    [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
    private static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);

    public static unsafe void ConvertImageToTexture(Bitmap image, string textureName, Size size)
    {
      

      int width = size.Width;
      int height = size.Height;
      using (ResourcePtr rpt = TextureManager.Singleton.GetByName(textureName))
      {
        using (TexturePtr texture = rpt)
        {
          HardwarePixelBufferSharedPtr texBuffer = texture.GetBuffer();
          texBuffer.Lock(HardwareBuffer.LockOptions.HBL_DISCARD);
          PixelBox pb = texBuffer.CurrentLock;

          BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
          CopyMemory(pb.data, data.Scan0, (uint)(width * height * 4));
          image.UnlockBits(data);

          texBuffer.Unlock();
          texBuffer.Dispose();
        }
      }

      
      return;
    }

    /// <summary>
    /// Converts a TexturePtr to a System.Drawing.Bitmap.
    /// </summary>
    /// <param name="texturePtr">The TexturePtr.</param>
    /// <returns>A System.Drawing.Bitmap.</returns>
    public unsafe static Bitmap MogreTexturePtrToBitmap(TexturePtr texturePtr)
    {
      var bitmap = new Bitmap((int)texturePtr.Width, (int)texturePtr.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      var rgbValues = new byte[texturePtr.Width * 4 * texturePtr.Height];
      fixed (byte* ptr = &rgbValues[0])
      {
        var pixelBox = new PixelBox(texturePtr.Width, texturePtr.Height, 1, Mogre.PixelFormat.PF_A8R8G8B8,
                                    (IntPtr)ptr);

        texturePtr.GetBuffer().BlitToMemory(pixelBox);
      }
      var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, (int)texturePtr.Width, (int)texturePtr.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
      Marshal.Copy(rgbValues, 0, bitmapData.Scan0, rgbValues.Length);
      bitmap.UnlockBits(bitmapData);

      return bitmap;
    }

    private void btnNewMatFile_Click(object sender, EventArgs e)
    {
      if (curMat == null)
        return;

      if (ofdFindMaterial.ShowDialog() == DialogResult.OK)
      {  
        //TextureManager.Singleton.Loa
        curMat.GetBestTechnique().GetPass(0).RemoveAllTextureUnitStates();
        string dir = ofdFindMaterial.FileName.Substring(0, ofdFindMaterial.FileName.Length-ofdFindMaterial.SafeFileName.Length);
        ResourceGroupManager.Singleton.AddResourceLocation(dir, "FileSystem");
        ResourceGroupManager.Singleton.InitialiseAllResourceGroups();

        TextureManager.Singleton.Load(ofdFindMaterial.SafeFileName, "General");
        //ResourceGroupManager.Singleton.AddResourceLocation(ofdFindMaterial.FileName, "Texture", "Default");
        //ResourceGroupManager.Singleton.CreateResource(ofdFindMaterial.FileName, "Default");
        curMat.GetBestTechnique().GetPass(0).CreateTextureUnitState(ofdFindMaterial.SafeFileName);
      }
    }
  }
}
