using System;
using UnityEngine;

namespace CENTIS.UnityFileExplorer
{
    [Serializable]
    [CreateAssetMenu(fileName = "new FileExplorerConfiguration", menuName = "CENTIS/UnityFileExplorer")]
    public class ExplorerConfiguration : ScriptableObject
    {
        public UINode FolderPrefab { get => _folderPrefab; set => _folderPrefab = value; }
        [SerializeField] private UINode _folderPrefab;

        public UINode FilePrefab { get => _filePrefab; set => _filePrefab = value; }
        [SerializeField] private UINode _filePrefab;

		// TODO : add side column prefabs

		// TODO : add folder path prefabs

		// TODO : add button/arrow prefabs

        // TODO : add cancel/x/choose file prefab

        // TODO : add top bar/bottom bar that contains folder path and buttons?
	}
}
