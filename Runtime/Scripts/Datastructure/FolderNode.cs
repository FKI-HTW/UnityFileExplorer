using System;
using System.Collections.Generic;
using UnityEngine;

namespace CENTIS.UnityFileExplorer.Datastructure
{
	public class FolderNode : TreeNode, IEquatable<FolderNode>
	{
		public List<TreeNode> Children { get; private set; }
		public GameObject UIInstance { get; private set; }

		public FolderNode(ExplorerManager manager, NodeInformation info, FolderNode parent, List<TreeNode> children)
			: base(manager, info, parent)
		{
			Children = children;
			UIInstance = GameObject.Instantiate(
				manager.ExplorerConfiguration.FolderPrefab, 
				manager.FileContainer.transform);
			UIInstance.name = $"{Info.Path}/{Info.Name}";
			UIInstance.SetActive(false);
		}

		public bool Equals(FolderNode other)
		{
			if (other == null) return false;
			if (other is not FolderNode folderNode) return false;
			return GetHashCode().Equals(folderNode.GetHashCode());
		}

		public override void Show()
		{
			UIInstance.SetActive(true);
		}

		public override void Hide()
		{
			UIInstance.SetActive(false);
		}

		public override void Unload()
		{
			if (UIInstance != null)
				GameObject.Destroy(UIInstance);

			if (Children == null)
				return;

			foreach (TreeNode child in Children)
				child.Unload();
			Children = null;
		}

		public void UpdateChildren(List<TreeNode> children)
		{
			Children = children;
		}

		public void NavigateTo()
		{
			((FolderNode)Parent).NavigateFrom();

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
