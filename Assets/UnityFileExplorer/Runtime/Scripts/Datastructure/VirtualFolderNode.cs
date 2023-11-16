using System;
using System.Collections.Generic;

namespace CENTIS.UnityFileExplorer.Datastructure
{
	public class VirtualFolderNode : TreeNode, IEquatable<VirtualFolderNode>
	{
		public List<TreeNode> Children { get; private set; }

		public VirtualFolderNode(ExplorerManager manager, NodeInformation info, VirtualFolderNode parent, List<TreeNode> children)
			: base(manager, info, parent)
		{
			Children = children;
		}

		public VirtualFolderNode(ExplorerManager manager, NodeInformation info, VirtualFolderNode parent)
			: this(manager, info, parent, new()) { }

		public bool Equals(VirtualFolderNode other)
		{
			if (other == null) return false;
			if (other is not VirtualFolderNode virtualNode) return false;
			if (!GetHashCode().Equals(virtualNode.GetHashCode())) return false;
			return ToString().Equals(other.ToString());
		}

		public override void Show()
		{
		}

		public override void Hide()
		{
		}

		public override void Unload()
		{
			if (Children != null)
			{
				foreach (TreeNode child in Children)
					child.Unload();
				Children = null;
			}
		}

		public void UpdateChildren(List<TreeNode> children)
		{
			Children = children;
		}

		public void AddChild(TreeNode child)
		{
			Children ??= new();
			Children.Add(child);
		}

		public void NavigateTo()
		{
			if (Children == null)
				throw new NotImplementedException("Folder with unloaded children was opened. Implement loading of children on the fly first!");

			foreach (TreeNode child in Children)
				child.Show();
		}

		public void NavigateFrom()
		{
			if (Children == null)
				throw new NotImplementedException("Folder with unloaded children was opened. Implement loading of children on the fly first!");

			foreach (TreeNode child in Children)
				child.Hide();
		}
	}
}
