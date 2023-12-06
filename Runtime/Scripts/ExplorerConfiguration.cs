using System;
using UnityEngine;
using UnityEngine.UI;

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

        public GameObject ArrowBackPrefab { get => _arrowBackPrefab; set => _arrowBackPrefab = value; }
        [SerializeField] private GameObject _arrowBackPrefab;

        public GameObject ArrowForwardPrefab { get => _arrowForwardPrefab; set => _arrowForwardPrefab = value; }
        [SerializeField] private GameObject _arrowForwardPrefab;

        public GameObject ExitButtonPrefab { get => _exitButtonPrefab; set => _exitButtonPrefab = value; }
        [SerializeField] private GameObject _exitButtonPrefab;

        // TODO : add cancel/choose file prefab
        public GameObject CancelButtonPrefab { get => _cancelButtonPrefab; set => _cancelButtonPrefab = value; }
        [SerializeField] private GameObject _cancelButtonPrefab;

        public GameObject ChooseFileButtonPrefab { get => _chooseFileButtonPrefab; set => _chooseFileButtonPrefab = value; }
        [SerializeField] private GameObject _chooseFileButtonPrefab;


		// TODO : add side column prefabs

		// TODO : add folder path prefabs

        // TODO : add top bar/bottom bar that contains folder path and buttons?
	}
}
