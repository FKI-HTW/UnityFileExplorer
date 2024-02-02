using System;
using UnityEngine;

namespace CENTIS.UnityFileExplorer.Datastructure
{
	internal class EmptyNode : TreeNode, IEquatable<EmptyNode>
	{
		public GameObject UIInstance { get; private set; }

		public EmptyNode(ExplorerConfiguration config, VirtualFolderNode parent)
			: base(config, null, parent)
		{
			UIInstance = GameObject.Instantiate(
				config.EmptyFolderPrefab,
				config.NodeContainer.transform);
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
