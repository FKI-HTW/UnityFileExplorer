using System;

namespace CENTIS.UnityFileExplorer.Datastructure
{
	internal abstract class TreeNode : IEquatable<TreeNode>
	{
		public ExplorerManager Manager { get; }
		public NodeInformation Info { get; }
        public VirtualFolderNode Parent { get; }

		public TreeNode(ExplorerManager manager, NodeInformation info, VirtualFolderNode parent)
		{
			Manager = manager;
			Info = info;
			Parent = parent;
		}

		public override string ToString()
		{
			if (string.IsNullOrEmpty(Info?.Path)) 
				return Info.Name;
			return $"{Info.Path}/{Info.Name}";
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public bool Equals(TreeNode other)
		{
			if (other == null) return false;
			if (other is not TreeNode treeNode) return false;
			if (!GetHashCode().Equals(treeNode.GetHashCode())) return false;
			return ToString().Equals(other.ToString());
		}

		public abstract void Show();
		public abstract void Hide();
		public abstract void Unload();
		public abstract void MissingPermissions();
	}
}
