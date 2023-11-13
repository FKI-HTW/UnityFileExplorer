using System;
using System.Collections.Generic;
using UnityEngine;

namespace CENTIS.UnityFileExplorer.Datastructure
{
	public class FolderNode : TreeNode, IEquatable<FolderNode>
	{
		public List<TreeNode> Children { get; private set; }
		public UINode UIInstance { get; private set; }

		public FolderNode(ExplorerManager manager, NodeInformation info, FolderNode parent, List<TreeNode> children)
			: base(manager, info, parent)
		{
			Children = children;
			UIInstance = GameObject.Instantiate(
				manager.ExplorerConfiguration.FolderPrefab, 
				manager.FileContainer.transform);
			UIInstance.gameObject.name = ToString();
			UIInstance.gameObject.SetActive(false);
			UIInstance.OnSelected += () => manager.SelectNode(this);
			UIInstance.OnDeselected += () => manager.DeselectNode(this);
			UIInstance.OnActivated += () => manager.ActivateNode(this);
		}

		public bool Equals(FolderNode other)
		{
			if (other == null) return false;
			if (other is not FolderNode folderNode) return false;
			return GetHashCode().Equals(folderNode.GetHashCode());
		}

		public override void Show()
		{
			UIInstance.gameObject.SetActive(true);
		}

		public override void Hide()
		{
			UIInstance.gameObject.SetActive(false);
		}

		public override void Unload()
		{
			if (UIInstance != null)
			{
				UIInstance.OnSelected -= () => Manager.SelectNode(this);
				UIInstance.OnDeselected -= () => Manager.DeselectNode(this);
				UIInstance.OnActivated -= () => Manager.ActivateNode(this);
				GameObject.Destroy(UIInstance);
			}

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
