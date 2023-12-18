using System;
using UnityEngine;

namespace CENTIS.UnityFileExplorer.Datastructure
{
	public class EmptyNode : TreeNode, IEquatable<EmptyNode>
	{
		public GameObject UIInstance { get; private set; }

		public EmptyNode(ExplorerManager manager, VirtualFolderNode parent)
			: base(manager, null, parent)
		{
			UIInstance = GameObject.Instantiate(
				manager.ExplorerConfiguration.NoFilesInfo,
				manager.NodeContainerPrefab.transform);
			UIInstance.name = ToString();
			UIInstance.SetActive(false);
		}

		public override string ToString()
		{
			return $"{Parent}/Empty Directory";
		}

		public bool Equals(EmptyNode other)
		{
			if (other == null) return false;
			if (other is not EmptyNode emptyNode) return false;
			if (!GetHashCode().Equals(emptyNode.GetHashCode())) return false;
			return ToString().Equals(other.ToString());
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
			if (UIInstance == null) return;

			GameObject.Destroy(UIInstance);
		}

		public override void MissingPermissions()
		{
		}
	}
}
