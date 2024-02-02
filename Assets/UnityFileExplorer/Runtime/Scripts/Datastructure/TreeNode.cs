using System;

namespace CENTIS.UnityFileExplorer.Datastructure
{
	internal abstract class TreeNode : IEquatable<TreeNode>
	{
		public ExplorerConfiguration Config { get; }
		public NodeInformation Info { get; }
        public VirtualFolderNode Parent { get; }

		public event Action<TreeNode> OnSelected;
		public event Action<TreeNode> OnDeselected;
		public event Action<TreeNode> OnActivated;

		public TreeNode(ExplorerConfiguration config, NodeInformation info, VirtualFolderNode parent)
		{
			Config = config;
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

		protected void Select(TreeNode node) => OnSelected?.Invoke(node);
		protected void Deselect(TreeNode node) => OnDeselected?.Invoke(node);
		protected void Activate(TreeNode node) => OnActivated?.Invoke(node);
	}
}
