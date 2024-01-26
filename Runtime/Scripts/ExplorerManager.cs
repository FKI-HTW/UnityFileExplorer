using CENTIS.UnityFileExplorer.Datastructure;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace CENTIS.UnityFileExplorer
{
    public class ExplorerManager : MonoBehaviour
    {
		#region ui references

		public GameObject FileExplorerContainer { get => _fileExplorerContainer; set => _fileExplorerContainer = value; }
		[SerializeField] private GameObject _fileExplorerContainer;

		public Button ArrowBackButton { get => _arrowBackButton; set => _arrowBackButton = value; }
		[SerializeField] private Button _arrowBackButton;

		public Button ArrowForwardButton { get => _arrowForwardButton; set => _arrowForwardButton = value; }
		[SerializeField] private Button _arrowForwardButton;

		public GameObject PathContainer { get => _pathContainer; set => _pathContainer = value; }
		[SerializeField] private GameObject _pathContainer;

		public UIPathFolder PathFolderPrefab { get => _folderButtonPrefab; set => _folderButtonPrefab = value; }
		[SerializeField] private UIPathFolder _folderButtonPrefab;

		public Button ExitButton { get => _exitButton; set => _exitButton = value; }
		[SerializeField] private Button _exitButton;

		public GameObject NodeContainer { get => _nodeContainer; set => _nodeContainer = value; }
		[SerializeField] private GameObject _nodeContainer;

		public UINode FolderPrefab { get => _folderPrefab; set => _folderPrefab = value; }
		[SerializeField] private UINode _folderPrefab;

		public UINode FilePrefab { get => _filePrefab; set => _filePrefab = value; }
		[SerializeField] private UINode _filePrefab;

		public GameObject EmptyFolderPrefab { get => _emptyFolderPrefab; set => _emptyFolderPrefab = value; }
		[SerializeField] private GameObject _emptyFolderPrefab;

		public Button CancelButton { get => _cancelButton; set => _cancelButton = value; }
		[SerializeField] private Button _cancelButton;

		public Button ChooseFileButton { get => _chooseFileButton; set => _chooseFileButton = value; }
		[SerializeField] private Button _chooseFileButton;

		#endregion

		#region fields

		private VirtualFolderNode	_root; // a virtual folder above the disks
		private VirtualFolderNode	_currentFolder;
		private TreeNode			_selectedNode;

		private readonly List<VirtualFolderNode> _lastVisitedNodes = new(); // for back arrow
		private readonly List<VirtualFolderNode> _lastReturnedFromNodes = new(); // for forward arrow

		private readonly Dictionary<string, TreeNode> _hashedNodes = new();

		private readonly List<PathNode> _pathNodes = new();

		private Action<string> _fileFoundCallback;

		private string _fileExtension;
		private bool _certainFilesOnly = false;

		#endregion

		private void Start()
		{
			LoadCustomPrefabs();
			FindFile(onFilePathFound: Debug.Log, null, ".dll"); // for testing - only showing .dll files at the moment
		}

		#region public methods

		public virtual void FindFile(
			Action<string> onFilePathFound = null,
			Environment.SpecialFolder? startFolder = null,
			string fileExtension = null
        ){
			if (!string.IsNullOrEmpty(fileExtension))
			{
				_fileExtension = fileExtension;
				_certainFilesOnly = true;
			}

			_fileFoundCallback = onFilePathFound;
			_root = new VirtualFolderNode(this, new() { Name = "This PC" }, null);
			_hashedNodes.Add(_root.ToString(), _root);
			InitFileExplorer();

			// create drive folders beneath virtual root
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach (DriveInfo drive in drives)
			{
				FolderNode diskNode = new(this, drive.GetNodeInformation(), _root);
				_root.AddChild(diskNode);
				_hashedNodes.Add(diskNode.ToString(), diskNode);
			}

			if (startFolder == null)
			{
				_currentFolder = _root;
				_root.NavigateTo();
				UpdatePath();
				return;
			}

			// used to navigate to the given startFolder and create all nodes, that are visited during the navigation
			string startFolderPath = Environment.GetFolderPath((Environment.SpecialFolder)startFolder);
			DirectoryInfo startDir = new(startFolderPath);
			VirtualFolderNode startParent = FindParentRecursive(startDir);
			FolderNode startNode = new(this, startDir.GetNodeInformation(), startParent);
			startParent.AddChild(startNode);
			_hashedNodes.Add(startNode.ToString(), startNode);
			_currentFolder = startNode;
			startNode.NavigateTo();
			UpdatePath();
		}

		public virtual void CancelFindFile()
		{
			_fileFoundCallback = null;
			// TODO : close window ?
		}

		#endregion

		#region private methods

		private void InitFileExplorer()
		{
			if (ArrowBackButton != null)
			{
				ArrowBackButton.onClick.RemoveListener(GoBack);
				ArrowBackButton.onClick.AddListener(GoBack);
				ArrowBackButton.interactable = false;
			}

			if (ArrowForwardButton != null)
			{
				ArrowForwardButton.onClick.RemoveListener(GoForward);
				ArrowForwardButton.onClick.AddListener(GoForward);
				ArrowForwardButton.interactable = false;
			}

			if (ExitButton != null)
			{
				ExitButton.onClick.RemoveListener(CancelFindFile);
				ExitButton.onClick.AddListener(CancelFindFile);
			}

			if (CancelButton != null)
			{
				CancelButton.onClick.RemoveListener(CancelFindFile);
				CancelButton.onClick.AddListener(CancelFindFile);
			}

			if (ChooseFileButton != null)
			{
				ChooseFileButton.onClick.RemoveListener(ChooseSelectedNode);
				ChooseFileButton.onClick.AddListener(ChooseSelectedNode);
			}
		}

		internal void SelectNode(TreeNode node)
		{
			if (node == null) return;
			if (node.Equals(_selectedNode))
				ActivateNode(_selectedNode);
			else
				_selectedNode = node;
		}

		internal void DeselectNode(TreeNode node)
		{
			if (_selectedNode == node)
				_selectedNode = null;
		}

		internal void ActivateNode(TreeNode node)
		{
			if (node == null) return;

			_selectedNode = null;
			switch (node)
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
				case EmptyNode:
				default:
					throw new Exception("How did this happen?");
			}
		}

		private void ChooseSelectedNode()
		{
			ActivateNode(_selectedNode);
		}

		// TODO : optimize this
		private void UpdatePath()
		{
			if (PathContainer == null) return;

			foreach (PathNode node in _pathNodes)
				GameObject.Destroy(node.UIInstance.gameObject);
			_pathNodes.Clear();

			VirtualFolderNode folderNode = _currentFolder;
			while (folderNode != null)
			{
				string name = folderNode.Info.Name.Replace("\\", "").Replace("/", "");
				_pathNodes.Add(new() { FolderNode = folderNode, Name = name });
				folderNode = folderNode.Parent;
			}

			for (int i = _pathNodes.Count - 1; i >= 0; i--)
			{
				PathNode node = _pathNodes[i];
				UIPathFolder uiInstance = Instantiate(PathFolderPrefab, PathContainer.transform);
				uiInstance.Initialize(node.Name);
				uiInstance.OnActivated += () => ActivateNode(node.FolderNode);
				node.UIInstance = uiInstance;
			}
		}

		private void GoBack()
		{
			if (_lastVisitedNodes.Count == 0) return;

			VirtualFolderNode targetNode = _lastVisitedNodes[^1];
			_lastVisitedNodes.RemoveAt(_lastVisitedNodes.Count - 1);
			_lastReturnedFromNodes.Add(_currentFolder);

			if (ArrowForwardButton != null)
				ArrowForwardButton.interactable = true;
			if (ArrowBackButton != null)
				ArrowBackButton.interactable = _lastVisitedNodes.Count != 0;

			_currentFolder.NavigateFrom();
			_currentFolder = targetNode;
			_currentFolder.NavigateTo();

			UpdatePath();
		}

		private void GoForward()
		{
			if (_lastReturnedFromNodes.Count == 0) return;

			VirtualFolderNode targetNode = _lastReturnedFromNodes[^1];
			_lastReturnedFromNodes.RemoveAt(_lastReturnedFromNodes.Count - 1);
			_lastVisitedNodes.Add(_currentFolder);

			if (ArrowBackButton != null)
				ArrowBackButton.interactable = true;
			if (ArrowForwardButton != null)
				ArrowForwardButton.interactable = _lastReturnedFromNodes.Count != 0;

			_currentFolder.NavigateFrom();
			_currentFolder = targetNode;
			_currentFolder.NavigateTo();

			UpdatePath();
		}

		private void NavigateToNode(VirtualFolderNode node)
		{
			try {
				if (!node.IsFolderLoaded)
				{
					AddDirectories(node);
					AddFiles(node);
					if (!node.IsFolderLoaded || node.Children.Count == 0)
					{
						EmptyNode emptyNode = new(this, node);
						node.AddChild(emptyNode);
					}
				}
			} catch (UnauthorizedAccessException) {
				node.MissingPermissions();
				return;
            }

			_lastVisitedNodes.Add(_currentFolder);
			_lastReturnedFromNodes.Clear();
			if (ArrowBackButton != null)
				ArrowBackButton.interactable = true;
			if (ArrowForwardButton != null)
				ArrowForwardButton.interactable = false;

			_currentFolder.NavigateFrom();
			_currentFolder = node;
			_currentFolder.NavigateTo();

			UpdatePath();
		}

		private void ChooseFile(FileNode node)
		{
			_fileFoundCallback?.Invoke(node.ToString());
			CancelFindFile();
		}

		private void AddDirectories(VirtualFolderNode folder)
		{
			string folderPath = folder.ToString();
			IEnumerable<DirectoryInfo> containedDir = new DirectoryInfo(folderPath).GetDirectories();
			foreach (DirectoryInfo dir in containedDir)
			{
				FolderNode folderNode = new(this, dir.GetNodeInformation(), folder);
				folder.AddChild(folderNode);
				_hashedNodes.Add(folderNode.ToString(), folderNode);
			}
		}

		private void AddFiles(VirtualFolderNode folder)
		{
			string folderPath = folder.ToString();
			IEnumerable<FileInfo> containedFiles = new DirectoryInfo(folderPath).GetFiles();
			foreach (FileInfo file in containedFiles)
			{
				if (_certainFilesOnly && !file.Name.EndsWith(_fileExtension)) continue;

				FileNode fileNode = new(this, file.GetNodeInformation(), folder);
				folder.AddChild(fileNode);
				_hashedNodes.Add(fileNode.ToString(), fileNode);
			}
		}

		private FolderNode FindParentRecursive(DirectoryInfo userDir)
		{
			if (_hashedNodes.TryGetValue(userDir.Parent.ToString(), out TreeNode parent))
				return (FolderNode)parent;

			FolderNode nextParent = FindParentRecursive(userDir.Parent);
			FolderNode newNode = new(this, userDir.Parent.GetNodeInformation(), nextParent);
			nextParent.AddChild(newNode);
			_hashedNodes.Add(newNode.ToString(), newNode);
			return newNode;
		}

		#endregion
	}
}
