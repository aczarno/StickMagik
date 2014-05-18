using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
using System.Drawing;

namespace StickMagik
{
  // Types of fragments, mainly used for icons
  public enum FragmentType
  {
    MODEL,
    JOYSTICK,
    BUTTON
  }

  // Custom data class for Model frags to place in the tree
  public class ModelComponent
  {
    string name = "";
    FragmentType type = FragmentType.MODEL;
    string parent = "";
    TexturePtr texture = null;
    Bitmap textureImage = null;
    Color color = Color.White;
    SubEntity subEntity = null;

    public string Name { get { return name; } set { name = value; } }
    public FragmentType Type { get { return type; } set { type = value; } }
    public string Parent { get { return parent; } set { parent = value; } }
    public TexturePtr Texture { get { return texture; } set { texture = value; } }
    public Bitmap TextureImage { get { return textureImage; } set { textureImage = value; } }
    public Color Color { get { return color; } set { color = value; } }
    public SubEntity SubEntity { get { return subEntity; } set { subEntity = value; } }

    public ModelComponent(string _name)
    {
      name = _name;
      type = FragmentType.MODEL;
      parent = "";
    }

    public ModelComponent(string _name, FragmentType _type)
    {
      name = _name;
      type = _type;
      parent = "";
    }

    public ModelComponent(string _name, string _parent)
    {
      name = _name;
      type = FragmentType.MODEL;
      parent = _parent;
    }

    public ModelComponent(string _name, FragmentType _type, string _parent)
    {
      name = _name;
      type = _type;
      parent = _parent;
    }

    public override string ToString()
    {
      return name;
    }
  }

  public class ArcadeStickData
  {
    static readonly ArcadeStickData instance = new ArcadeStickData();
    public static ArcadeStickData Instance { get { return instance; } }
    static ArcadeStickData() {}
    private ArcadeStickData() {}

    static string filename;
    static Entity entity;

    static List<ModelComponent> components;

    public string Filename { get { return filename; } }
    public Entity Entity { get { return entity; } }
    public List<ModelComponent> Components { get { return components; } }
    public int NumComponents { get { return components.Count; } }

    public ModelComponent GetComponent(int index)
    {
      return components[index];
    }

    public ModelComponent GetComponent(string name)
    {
      return components.Find(delegate(ModelComponent m) { return m.Name == name; });
    }
    
    public void FillData(string _filename, ref Entity _entity)
    {
      filename = _filename;
      entity = _entity;
      components = new List<ModelComponent>();

      ModelComponent m;
      Pass p;
      String foundPath = "";
      FileInfoListPtr fileInfos;
      FileInfoList.Iterator it;

      Mesh.Const_SubMeshNameMap map = entity.GetMesh().GetSubMeshNameMap();
      for (uint i = 0; i < map.Count; i++)
      {
        for (Mesh.Const_SubMeshNameMap.ConstIterator start = map.Begin();
          start != map.End(); start++)
        {
          if (start.Value == i)
          {
            // Name
            m = new ModelComponent(start.Key);

            // SubEntity
            m.SubEntity = entity.GetSubEntity(start.Key);
         
            // Type (Button, Joystick, Body?)
            if(start.Key.Contains("Button") ||
              start.Key.Contains("Plunger") ||
              start.Key.Contains("Cylinder"))
              m.Type = FragmentType.BUTTON;
            else if(start.Key.Contains("Balltop") ||
              start.Key.Contains("Shaft") ||
              start.Key.Contains("Dust"))
              m.Type = FragmentType.JOYSTICK;
            else
              m.Type = FragmentType.MODEL;

            //m.Parent =;

            // Texture
            p = m.SubEntity.GetMaterial().GetBestTechnique().GetPass(0);
            if(p.NumTextureUnitStates > 0)
            {
              // Set the texture
              m.Texture = TextureManager.Singleton.GetByName(p.GetTextureUnitState(0).TextureName);
              // Get a bitmap version to display
              foundPath = m.Texture.Name;
              fileInfos = ResourceGroupManager.Singleton.FindResourceFileInfo("General", foundPath );
              it = fileInfos.Begin();
              if(it != fileInfos.End())
                 foundPath = it.Value.archive.Name + "/" + foundPath;
              else
                 foundPath = "";
              m.TextureImage = new Bitmap(foundPath);
            }
            else
            {
              m.Texture = null;
              // TODO: Put in a no texture image
              m.TextureImage = null;
            }

            // Color
            m.Color = Color.White;

            components.Add(m);
            break;
          }
        }
      }
    }

  }
}
