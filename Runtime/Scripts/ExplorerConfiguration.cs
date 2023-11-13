using System;
using UnityEngine;

namespace CENTIS.UnityFileExplorer
{
    [Serializable]
    [CreateAssetMenu(fileName = "new FileExplorerConfiguration", menuName = "CENTIS/UnityFileExplorer")]
    public class ExplorerConfiguration : ScriptableObject
    {
        public GameObject FolderPrefab { get => _folderPrefab; set => _folderPrefab = value; }
        [SerializeField] private GameObject _folderPrefab; // TODO : change to folder class

        public GameObject FilePrefab { get => _filePrefab; set => _filePrefab = value; }
        [SerializeField] private GameObject _filePrefab; // TODO : change to file class

        // TODO : add side column prefabs

        // TODO : add path prefabs?

        // TODO : add button/arrow prefabs?
    }
}
