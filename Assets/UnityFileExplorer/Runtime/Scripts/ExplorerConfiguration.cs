using System;
using UnityEngine;
using UnityEngine.UI;

namespace CENTIS.UnityFileExplorer
{
    [Serializable]
    [CreateAssetMenu(fileName = "new FileExplorerConfiguration", menuName = "CENTIS/UnityFileExplorer")]
    public class ExplorerConfiguration : ScriptableObject
    {
        //"Container Prefabs"
        public GameObject UpperUIBar { get => _upperUIBar; set => _upperUIBar = value; }
        [SerializeField] private GameObject _upperUIBar;

        public GameObject PathContainerPrefab { get => _pathContainerPrefab; set => _pathContainerPrefab = value; }
        [SerializeField] private GameObject _pathContainerPrefab;

        public GameObject NodeContainerPrefab { get => _nodeContainerPrefab; set => _nodeContainerPrefab = value; }
        [SerializeField] private GameObject _nodeContainerPrefab;

        public GameObject BottomUIBar { get => _bottomUIBar; set => _bottomUIBar = value; }
        [SerializeField] private GameObject _bottomUIBar;

        //UI Buttons
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

        //Folder Path
        public Button FolderButtonPrefab { get => _folderButtonPrefab; set => _folderButtonPrefab = value; }
        [SerializeField] private Button _folderButtonPrefab;

        public GameObject SeperatorPrefab { get => _separatorPrefab; set => _separatorPrefab = value; }
        [SerializeField] private GameObject _separatorPrefab;

        //Data Structure
        public UINode FolderPrefab { get => _folderPrefab; set => _folderPrefab = value; }
        [SerializeField] private UINode _folderPrefab;

        public UINode FilePrefab { get => _filePrefab; set => _filePrefab = value; }
        [SerializeField] private UINode _filePrefab;

        public GameObject NoFilesInfo { get => _noFilesInfoPrefab; set => _noFilesInfoPrefab = value; }
        [SerializeField] private GameObject _noFilesInfoPrefab;

        // TODO : add side column prefabs
    }
}
