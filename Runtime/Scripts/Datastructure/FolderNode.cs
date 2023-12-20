using System;
using System.Collections.Generic;
using UnityEngine;

namespace CENTIS.UnityFileExplorer.Datastructure
{
	internal class FolderNode : VirtualFolderNode, IEquatable<FolderNode>
	{
		public UINode UIInstance { get; private set; }

		public FolderNode(ExplorerManager manager, NodeInformation info, VirtualFolderNode parent, List<TreeNode> children = null)
			: base(manager, info, parent, children)
		{
			UIInstance = GameObject.Instantiate(
				manager.ExplorerConfiguration.FolderPrefab, 
				manager.NodeContainerPrefab.transform);
			UIInstance.gameObject.name = ToString();
			UIInstance.gameObject.SetActive(false);
			UIInstance.OnSelected += () => manager.SelectNode(this);
			UIInstance.OnDeselected += () => manager.DeselectNode(this);
			UIInstance.OnActivated += () => manager.ActivateNode(this);
			UIInstance.Initialize(info);
		}

		public bool Equals(FolderNode other)
		{
			if (other == null) return false;
			if (other is not FolderNode folderNode) return false;
			if (!GetHashCode().Equals(folderNode.GetHashCode())) return false;
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
			base.Unload();

			if (UIInstance != null)
			{
				UIInstance.OnSelected -= () => Manager.SelectNode(this);
				UIInstance.OnDeselected -= () => Manager.DeselectNode(this);
				UIInstance.OnActivated -= () => Manager.ActivateNode(this);
				GameObject.Destroy(UIInstance);
			}
		}

		public override void MissingPermissions()
		{
			UIInstance.MissingPermissions();
		}
	}
}
