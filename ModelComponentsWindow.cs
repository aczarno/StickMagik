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
    bool ignore = false;

    public ModelComponentsWindow()
    {
      InitializeComponent();
      Messenger<ModelComponent>.AddListener("AddFragment", AddComponent);
      Messenger<string>.AddListener("SelectComponent", onSelectComponent);
    }

    ~ModelComponentsWindow()
    {
      Messenger<ModelComponent>.RemoveListener("AddFragment", AddComponent);
      Messenger<string>.RemoveListener("SelectComponent", onSelectComponent);
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
      newGuy.Name = name;
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

    public void onSelectComponent(string name)
    {
      if (!string.IsNullOrEmpty(name))
      {
        ignore = true;
        treeFragments.SelectedNode = GetNode(name);
        treeFragments.Focus();
      }
      else
        treeFragments.SelectedNode = null;
    }

    TreeNode GetNode(string key)
    {
      TreeNode n = null;
      n = GetNode(key, treeFragments.Nodes);
      return n;
    }

    TreeNode GetNode(string key, TreeNodeCollection nodes)
    {
      TreeNode n = null;
      if (nodes.ContainsKey(key))
        n = nodes[key];
      else
      {
        foreach (TreeNode tn in nodes)
        {
          n = GetNode(key, tn.Nodes);
          if (n != null) break;
        }
      }

      return n;
    }

    private void treeFragments_AfterSelect(object sender, TreeViewEventArgs e)
    {
      if (!ignore)
        Messenger<string>.Broadcast("SelectComponent", treeFragments.SelectedNode.Text);
      else
        ignore = false;
    }
  }

  
}
