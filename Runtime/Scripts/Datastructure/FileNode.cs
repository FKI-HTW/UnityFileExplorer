using System;
using UnityEngine;

namespace CENTIS.UnityFileExplorer.Datastructure
{
	public class FileNode : TreeNode, IEquatable<FileNode>
	{
		public GameObject UIInstance { get; private set; }

		public FileNode(ExplorerManager manager, NodeInformation info, FolderNode parent)
			: base(manager, info, parent)
		{
			UIInstance = GameObject.Instantiate(
				manager.ExplorerConfiguration.FilePrefab, 
				manager.FileContainer.transform);
			UIInstance.name = $"{Info.Path}/{Info.Name}";
			UIInstance.SetActive(false);
		}

		public bool Equals(FileNode other)
		{
			if (other == null) return false;
			if (other is not FileNode fileNode) return false;
			return GetHashCode().Equals(fileNode.GetHashCode());
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
		}
	}
}
