using System;

namespace CENTIS.UnityFileExplorer.Datastructure
{
	public abstract class TreeNode : IEquatable<TreeNode>
	{
		public ExplorerManager Manager { get; }
		public NodeInformation Info { get; }
        public TreeNode Parent { get; }

		public TreeNode(ExplorerManager manager, NodeInformation info, TreeNode parent)
		{
			Manager = manager;
			Info = info;
			Parent = parent;
		}

		public override string ToString()
		{
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
			return GetHashCode().Equals(treeNode.GetHashCode());
		}

		public abstract void Show();
		public abstract void Hide();
		public abstract void Unload();
	}
}
