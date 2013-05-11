using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Crom.Controls;

namespace StickMagik
{
  public partial class ModelComponentsWindow : DockableToolWindow
  {
    public ModelComponentsWindow()
    {
      InitializeComponent();
      Messenger<ModelComponent>.AddListener("AddFragment", AddComponent);
    }

    ~ModelComponentsWindow()
    {
      Messenger<ModelComponent>.RemoveListener("AddFragment", AddComponent);
    }

    public void AddComponent(ModelComponent frag)
    {
      AddComponent(frag.Name, frag.Type, frag.Parent);
    }

    public void AddComponent(string name)
    {
      AddComponent(name, FragmentType.MODEL, "");
    }

    public void AddComponent(string name, FragmentType type)
    {
      AddComponent(name, type, "");
    }

    public void AddComponent(string name, string parent)
    {
      AddComponent(name, FragmentType.MODEL, parent);
    }

    public void AddComponent(string name, FragmentType type, string parent)
    {
      TreeNode newGuy = new TreeNode(name);
      // TODO: Change icon based on FragmentType
      if (!string.IsNullOrEmpty(parent))
      {
        foreach (TreeNode t in treeFragments.Nodes)
        {
          if (t.Name == parent)
            t.Nodes.Add(newGuy);
        }
      }
      else
        treeFragments.Nodes.Add(newGuy);
    }

    private void treeFragments_AfterSelect(object sender, TreeViewEventArgs e)
    {
      Messenger<string>.Broadcast("SelectComponent", treeFragments.SelectedNode.Text);
    }
  }

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
    string name;
    FragmentType type;
    string parent;

    public string Name { get { return name; } set { name = value; } }
    public FragmentType Type { get { return type; } set { type = value; } }
    public string Parent { get { return parent; } set { parent = value; } }

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
  }
}
