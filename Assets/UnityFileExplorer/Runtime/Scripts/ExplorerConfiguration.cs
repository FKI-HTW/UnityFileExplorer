using System;
using UnityEngine;
using UnityEngine.UI;

namespace CENTIS.UnityFileExplorer
{
	[Serializable]
    public class ExplorerConfiguration
    {
		public GameObject NodeContainer => _nodeContainer;
		[SerializeField] private GameObject _nodeContainer;

		public UINode FolderPrefab => _folderPrefab;
		[SerializeField] private UINode _folderPrefab;

		public UINode FilePrefab => _filePrefab;
		[SerializeField] private UINode _filePrefab;

		public GameObject EmptyFolderPrefab => _emptyFolderPrefab;
		[SerializeField] private GameObject _emptyFolderPrefab;


		public GameObject PathContainer => _pathContainer;
		[SerializeField] private GameObject _pathContainer;

		public UIPathFolder PathFolderPrefab => _pathFolderPrefab;
		[SerializeField] private UIPathFolder _pathFolderPrefab;


		public Button ArrowBackButton => _arrowBackButton;
		[SerializeField] private Button _arrowBackButton;

		public Button ArrowForwardButton => _arrowForwardButton;
		[SerializeField] private Button _arrowForwardButton;

		public Button ExitButton => _exitButton;
		[SerializeField] private Button _exitButton;

		public Button CancelButton => _cancelButton;
		[SerializeField] private Button _cancelButton;

		public Button ConfirmButton => _confirmButton;
		[SerializeField] private Button _confirmButton;
	}
}
