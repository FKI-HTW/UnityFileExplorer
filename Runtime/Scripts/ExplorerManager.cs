using System.Collections.Generic;
using CENTIS.UnityFileExplorer.Datastructure;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Security.AccessControl;

namespace CENTIS.UnityFileExplorer
{
    public class ExplorerManager : MonoBehaviour
    {
		#region fields

		public ExplorerConfiguration ExplorerConfiguration 
		{ 
			get
			{
				if (_explorerConfiguration != null)
					return _explorerConfiguration;
				return _explorerConfiguration = (ExplorerConfiguration)Resources.Load("DefaultFileExplorerConfiguration");
			}
			set => _explorerConfiguration = value; 
		}
        [SerializeField] private ExplorerConfiguration _explorerConfiguration;

		public GameObject FileContainer { get => _fileContainer; }
		[SerializeField] private GameObject _fileContainer;

		private VirtualFolderNode	_root; // a virtual folder above the disks
		private VirtualFolderNode	_currentFolder;
		private TreeNode			_selectedNode;

		private readonly List<VirtualFolderNode> _lastVisitedNodes = new(); // for back arrow
		private readonly List<VirtualFolderNode> _lastReturnedFromNodes = new(); // for forward arrow

		private readonly HashSet<TreeNode> _hashedNodes = new(); // hashset with all node references for O(1) access

		private Action<string> _fileFoundCallback;

		private string _fileExtension;
		private bool _certainFilesOnly = false;

		private GameObject _upperUIBar;
		private Button _exitButton;
		private Button _backButton;
		private Button _forwardButton;
		private Button _cancelButton;
		private Button _confirmChoiceButton;
		private GameObject _noFilesInfoPrefab;

		private GameObject _pathContainerPrefab;
		private Button _folderButtonPrefab;
		private GameObject _separatorPrefab;

		private string _currentFolderPath; //check if needed or if _currentFolder can be used

		#endregion

		private void Start()
		{
			LoadCustomPrefabs();
			FindFile(onFilePathFound: Debug.Log, null, ".dll"); // for testing
		}

		#region public methods

		public void LoadCustomPrefabs()
        {
			_upperUIBar = Instantiate(ExplorerConfiguration.UpperUIBar, _fileContainer.transform);

			_backButton = Instantiate(ExplorerConfiguration.ArrowBackPrefab, _upperUIBar.transform);
			_backButton.onClick.AddListener(GoBack);
			_backButton.interactable = false;
			
			_forwardButton = Instantiate(ExplorerConfiguration.ArrowForwardPrefab, _upperUIBar.transform); 
			_forwardButton.onClick.AddListener(GoForward);
			_forwardButton.interactable = false;

			_cancelButton = Instantiate(ExplorerConfiguration.CancelButtonPrefab, _upperUIBar.transform); 
			_cancelButton.onClick.AddListener(CancelFindFile);

			_confirmChoiceButton = Instantiate(ExplorerConfiguration.ChooseFileButtonPrefab, _upperUIBar.transform); 
			_confirmChoiceButton.onClick.AddListener(() => ActivateNode(_selectedNode));

			_exitButton = Instantiate(ExplorerConfiguration.ExitButtonPrefab, _upperUIBar.transform); 
			_exitButton.onClick.AddListener(CancelFindFile);

			_pathContainerPrefab = Instantiate(ExplorerConfiguration.PathContainerPrefab, _fileContainer.transform);

			_noFilesInfoPrefab = Instantiate(ExplorerConfiguration.NoFilesInfo, _fileContainer.transform);
			_noFilesInfoPrefab.SetActive(false);
		}

		public void FindFile(
			Action<string> onFilePathFound = null,
			Environment.SpecialFolder? startFolder = null,
			string fileExtension = ""
        ){
			if (!fileExtension.Equals(""))
			{
				_fileExtension = fileExtension;
				_certainFilesOnly = true;
			}
			_fileFoundCallback = onFilePathFound;
			_root = new VirtualFolderNode(this, null, null);

			// create drive folders beneath virtual root
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach (DriveInfo drive in drives)
			{
				FolderNode diskNode = new(this, drive.GetNodeInformation(), _root);
				_root.AddChild(diskNode);
				_hashedNodes.Add(diskNode);
			}

			if (startFolder == null)
			{
				_currentFolder = _root;
				_root.NavigateTo();
				return;
			}

			// used to navigate to the given startFolder and create all nodes, that are visited during the navigation
			string startFolderPath = Environment.GetFolderPath((Environment.SpecialFolder)startFolder);
			DirectoryInfo startDir = new(startFolderPath);
			VirtualFolderNode startParent = FindParentRecursive(startDir);
			FolderNode startNode = new(this, startDir.GetNodeInformation(), startParent);
			startParent.AddChild(startNode);
			_hashedNodes.Add(startNode);
			_currentFolder = startNode;			
			startNode.NavigateTo();
		}

		public void CancelFindFile()
		{
			_fileFoundCallback = null;
			// TODO : close window ?
		}

		public void SelectNode(TreeNode node)
		{
			if (node == null) return;
			if (node.Equals(_selectedNode))
				ActivateNode(node);
			else
				_selectedNode = node;
		}

		public void DeselectNode(TreeNode node)
		{
			if (_selectedNode == node)
				_selectedNode = null;
		}

		public void ActivateNode(TreeNode node)
		{
			if (node == null) return;

			switch(node)
			{
				case FolderNode folderNode:
					NavigateToNode(folderNode);
					break;
				case VirtualFolderNode virtualFolderNode:
					NavigateToNode(virtualFolderNode);
					break;
				case FileNode fileNode:
					ChooseFile(fileNode);
					break;
			}
		}

		public void GoBack()
		{
			if (_lastVisitedNodes.Count == 0) return;

			VirtualFolderNode targetNode = _lastVisitedNodes[^1];
			_lastVisitedNodes.RemoveAt(_lastVisitedNodes.Count - 1);
			_currentFolder.NavigateFrom();
			targetNode.NavigateTo();
			_lastReturnedFromNodes.Add(_currentFolder);
			_currentFolder = targetNode;
			_noFilesInfoPrefab.SetActive(false);
			_forwardButton.interactable = true;
			UpdateFolderPath(targetNode.ToString());

			if (_lastVisitedNodes.Count == 0)
            {
				_backButton.interactable = false;
			}
		}

		public void GoForward()
		{
			if (_lastReturnedFromNodes.Count == 0) return;

			VirtualFolderNode targetNode = _lastReturnedFromNodes[^1];
			_lastReturnedFromNodes.RemoveAt(_lastReturnedFromNodes.Count - 1);
			_currentFolder.NavigateFrom();
			targetNode.NavigateTo();
			_lastVisitedNodes.Add(_currentFolder);
			_currentFolder = targetNode;
			_backButton.interactable = true;
			UpdateFolderPath(targetNode.ToString());

			if(_lastReturnedFromNodes.Count == 0)
            {
				_forwardButton.interactable = false;
			}
		}

		#endregion

		#region private methods

		private void UpdateFolderPath(string currentFolderPath)
		{
			ClearPath();
			string[] folders = currentFolderPath.Split(Path.DirectorySeparatorChar);

			foreach (string folder in folders)
			{
				CreateFolderButton(folder);
				Instantiate(ExplorerConfiguration.SeperatorPrefab, _pathContainerPrefab.transform);
			}
		}

		private void CreateFolderButton(string folderName)
		{
			Button folderButton = Instantiate(ExplorerConfiguration.FolderButtonPrefab, _pathContainerPrefab.transform);
			Text buttonText = folderButton.GetComponentInChildren<Text>();
			buttonText.text = folderName;
			Button button = folderButton.GetComponent<Button>();
			button.onClick.AddListener(() => OnFolderButtonClick(folderName)); // check if this is best solution
		}

		private void ClearPath() {
			foreach (Transform child in _pathContainerPrefab.transform) 
			{ 
				Destroy(child.gameObject); 
			} 
		}

		private void OnFolderButtonClick(string folderName) //TODO
		{
			// Handle the folder button click (navigate, load contents, etc.)
			Debug.Log($"Navigate to: {folderName}");

			// this path needs to be the currentFolderPath up to the clicked folder -> update lists of lastVisisted etc. 
			//newFolderPath = Path.Combine(currentFolderPath, folderName); //this nonsense needs to be updated
			//UpdateFolderPath(newFolderPath);
		}

		private void NavigateToNode(VirtualFolderNode node)
		{
			try {
				IsFolderLoaded(node);
			} catch (UnauthorizedAccessException) {
				node.MissingPermissions();
				return;
            }

			_currentFolder.NavigateFrom();
			if (!IsFolderLoaded(node))
			{
				AddDirectories(node);
				AddFiles(node);
			}
			node.NavigateTo();
			_lastVisitedNodes.Add(_currentFolder);
			_backButton.interactable = true;
			_lastReturnedFromNodes.Clear(); //TO DO - understand why list is cleared here *****
			_currentFolder = node;
			if (_currentFolder.Children.Count == 0)
			{
				_noFilesInfoPrefab.SetActive(true);
			}
		}

		private void ChooseFile(FileNode node)
		{
			_fileFoundCallback?.Invoke(node.ToString());
			// TODO : close explorer ?
		}

		private bool IsFolderLoaded(VirtualFolderNode folder)
		{
			string folderPath = folder.ToString();
			int containedDir = Directory.GetDirectories(folderPath).Length;
			int containedFiles = Directory.GetFiles(folderPath).Length;
			return folder.Children.Count >= containedDir + containedFiles;
		}

		private void AddDirectories(VirtualFolderNode folder)
		{
			string folderPath = folder.ToString();
			IEnumerable<DirectoryInfo> containedDir = new DirectoryInfo(folderPath).GetDirectories();
			foreach (DirectoryInfo dir in containedDir)
			{
				FolderNode folderNode = new(this, dir.GetNodeInformation(), folder);
				folder.AddChild(folderNode);
				_hashedNodes.Add(folderNode);
			}
		}

		private void AddFiles(VirtualFolderNode folder)
		{
			string folderPath = folder.ToString();
			IEnumerable<FileInfo> containedFiles = new DirectoryInfo(folderPath).GetFiles();
			foreach (FileInfo file in containedFiles)
			{
				if (_certainFilesOnly)
				{
					string fileInfo = file.Name;
					if (fileInfo.EndsWith(_fileExtension))
					{
						FileNode fileNode = new(this, file.GetNodeInformation(), folder);
						folder.AddChild(fileNode);
						_hashedNodes.Add(fileNode);
					}
				}
				else
				{
					FileNode fileNode = new(this, file.GetNodeInformation(), folder);
					folder.AddChild(fileNode);
					_hashedNodes.Add(fileNode);
				}
			}
		}

		private bool TryFindNode(DirectoryInfo userDir, out TreeNode node)
		{
			return _hashedNodes.TryGetValue(new VirtualFolderNode(this, userDir.GetNodeInformation(), null), out node);
		}

		private FolderNode FindParentRecursive(DirectoryInfo userDir)
		{
			if (TryFindNode(userDir.Parent, out TreeNode parent))
				return (FolderNode)parent;

			FolderNode nextParent = FindParentRecursive(userDir.Parent);
			FolderNode newNode = new(this, userDir.Parent.GetNodeInformation(), nextParent);
			nextParent.AddChild(newNode);
			_hashedNodes.Add(newNode);
			return newNode;
		}

		#endregion
	}
}
