using System;
using UnityEngine;
using UnityEngine.UI;

namespace CENTIS.UnityFileExplorer
{
    [Serializable]
    [CreateAssetMenu(fileName = "new FileExplorerConfiguration", menuName = "CENTIS/UnityFileExplorer")]
    public class ExplorerConfiguration : ScriptableObject
    {
        public GameObject UpperUIBar { get => _upperUIBar; set => _upperUIBar = value; }
        [SerializeField] private GameObject _upperUIBar;

        public UINode FolderPrefab { get => _folderPrefab; set => _folderPrefab = value; }
        [SerializeField] private UINode _folderPrefab;

        public UINode FilePrefab { get => _filePrefab; set => _filePrefab = value; }
        [SerializeField] private UINode _filePrefab;

        public Button ArrowBackPrefab { get => _arrowBackPrefab; set => _arrowBackPrefab = value; }
        [SerializeField] private Button _arrowBackPrefab;

        public Button ArrowForwardPrefab { get => _arrowForwardPrefab; set => _arrowForwardPrefab = value; }
        [SerializeField] private Button _arrowForwardPrefab;

        public Button ExitButtonPrefab { get => _exitButtonPrefab; set => _exitButtonPrefab = value; }
        [SerializeField] private Button _exitButtonPrefab;

        public Button CancelButtonPrefab { get => _cancelButtonPrefab; set => _cancelButtonPrefab = value; }
        [SerializeField] private Button _cancelButtonPrefab;

        public Button ChooseFileButtonPrefab { get => _chooseFileButtonPrefab; set => _chooseFileButtonPrefab = value; }
        [SerializeField] private Button _chooseFileButtonPrefab;

        public GameObject NoFilesInfo { get => _noFilesInfoPrefab; set => _noFilesInfoPrefab = value; }
        [SerializeField] private GameObject _noFilesInfoPrefab;

        // folder path prefabs
        public GameObject PathContainerPrefab { get => _pathContainerPrefab; set => _pathContainerPrefab = value; }
        [SerializeField] private GameObject _pathContainerPrefab;

        private Button FolderButtonPrefab { get => _folderButtonPrefab; set => _folderButtonPrefab = value; }
        [SerializeField] private Button _folderButtonPrefab; 

        private GameObject SeperatorPrefab { get => _separatorPrefab; set => _separatorPrefab = value; }
        [SerializeField] private GameObject _separatorPrefab;

        // TODO : add side column prefabs

        // TODO : add top bar/bottom bar that contains folder path and buttons?
    }
}
