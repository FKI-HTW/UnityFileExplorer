using System;
using UnityEngine;

namespace CENTIS.UnityFileExplorer.Datastructure
{
	public class FileNode : TreeNode, IEquatable<FileNode>
	{
		public UINode UIInstance { get; private set; }

		public FileNode(ExplorerManager manager, NodeInformation info, FolderNode parent)
			: base(manager, info, parent)
		{
			UIInstance = GameObject.Instantiate(
				manager.ExplorerConfiguration.FilePrefab, 
				manager.FileContainer.transform);
			UIInstance.gameObject.name = ToString();
			UIInstance.gameObject.SetActive(false);
			UIInstance.OnSelected += () => manager.SelectNode(this);
			UIInstance.OnDeselected += () => manager.DeselectNode(this);
			UIInstance.OnActivated += () => manager.ActivateNode(this);
		}

		public bool Equals(FileNode other)
		{
			if (other == null) return false;
			if (other is not FileNode fileNode) return false;
			if (!GetHashCode().Equals(fileNode.GetHashCode())) return false;
			return ToString().Equals(other.ToString());
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
			if (UIInstance == null) return;

			UIInstance.OnSelected -= () => Manager.SelectNode(this);
			UIInstance.OnDeselected -= () => Manager.DeselectNode(this);
			UIInstance.OnActivated -= () => Manager.ActivateNode(this);
			GameObject.Destroy(UIInstance);
		}
	}
}
