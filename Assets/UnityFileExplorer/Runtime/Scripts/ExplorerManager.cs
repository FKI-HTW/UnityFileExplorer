using System.Collections.Generic;
using UnityEngine;
using CENTIS.UnityFileExplorer.Datastructure;
using System.IO;
using System;
using UnityEngine.UI;

namespace CENTIS.UnityFileExplorer
{
    public class ExplorerManager : MonoBehaviour
    {
		#region fields

		public ExplorerConfiguration ExplorerConfiguration { get => _explorerConfiguration; set => _explorerConfiguration = value; }
        [SerializeField] private ExplorerConfiguration _explorerConfiguration;

		public GameObject UpperUIBar { get => _upperUIBar; }
		[SerializeField] private GameObject _upperUIBar;

		public GameObject FileContainer { get => _fileContainer; }
		[SerializeField] private GameObject _fileContainer;

		private VirtualFolderNode	_root; // a virtual folder above the disks
		private VirtualFolderNode	_currentFolder;
		private TreeNode			_selectedNode;

		private readonly List<VirtualFolderNode> _lastVisitedNodes = new(); // for back arrow
		private readonly List<VirtualFolderNode> _lastReturnedFromNodes = new(); // for forward arrow

		private readonly HashSet<TreeNode> _hashedNodes = new(); // hashset with all node references for O(1) access

		private Action<string> _fileFoundCallback;

		private Button _exitButton;
		private Button _backButton;
		private Button _forwardButton;

		#endregion

		private void Start()
		{
			LoadAndConfigureCustomPrefabs();
			FindFile(onFilePathFound: Debug.Log); // for testing
		}

		#region public methods

		//Todo - finish method to instantiate and configure personalized ui elements
		public void LoadAndConfigureCustomPrefabs()
        {
			if(_explorerConfiguration.ArrowBackPrefab != null)
            {
				_backButton = Instantiate(_explorerConfiguration.ArrowBackPrefab, _upperUIBar.transform);
				_backButton.onClick.AddListener(GoBack);
			}

			if (_explorerConfiguration.ArrowForwardPrefab != null)
			{
				_forwardButton = Instantiate(_explorerConfiguration.ArrowForwardPrefab, _upperUIBar.transform);
				_forwardButton.onClick.AddListener(GoForward);
			}

			if (_explorerConfiguration.ExitButtonPrefab != null)
			{
				_exitButton = Instantiate(_explorerConfiguration.ExitButtonPrefab, _upperUIBar.transform);
				_exitButton.onClick.AddListener(CloseWindow);
			}

		}

		public void FindFile(string fileExtension = "", Action<string> onFilePathFound = null,
			Environment.SpecialFolder startFolder = Environment.SpecialFolder.UserProfile)
        {
			_fileFoundCallback = onFilePathFound;
			_root = new VirtualFolderNode(this, null, null);
			_currentFolder = _root;

			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach (DriveInfo drive in drives)
			{
				FolderNode diskNode = new(this, drive.GetNodeInformation(), _root);
				_root.AddChild(diskNode);
				_hashedNodes.Add(diskNode);

				diskNode.Show(); // for testing
			}

			/* Commented out for testing purposes, do not remove!
			 * This is used to navigate to the given startFolder and create 
			 * all nodes, that are visited during the navigation.
			 * 
			string startFolderPath = Environment.GetFolderPath(startFolder);
			DirectoryInfo startDir = new(startFolderPath);
			VirtualFolderNode startParent = FindParentRecursive(startDir);
			FolderNode startNode = new(this, startDir.GetNodeInformation(), startParent);
			startParent.AddChild(startNode);
			_hashedNodes.Add(startNode);
			NavigateToNode(startNode);
			*/
		}

		public void CancelFindFile()
		{
			_fileFoundCallback = null;
			// TODO : close explorer ?
		}

		public void SelectNode(TreeNode node)
		{
			if (node == null) return;

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
		}

		public void CloseWindow()
        {
			Application.Quit();

			//for not built application in runtime mode, while package is being developed: 
			UnityEditor.EditorApplication.isPlaying = false;
        }

		#endregion

		#region private methods

		private void NavigateToNode(FolderNode node)
		{
			_currentFolder.NavigateFrom();
			if (!IsFolderLoaded(node))
			{
				AddDirectories(node);
				AddFiles(node);
			}
			node.NavigateTo();
			_lastVisitedNodes.Add(_currentFolder);
			_lastReturnedFromNodes.Clear();
			_currentFolder = node;
		}

		private void ChooseFile(FileNode node)
		{
			_fileFoundCallback?.Invoke(node.ToString());
			// TODO : close explorer ?
		}

		private bool IsFolderLoaded(VirtualFolderNode folder)
		{
			string folderPath = folder.ToString();
			int containedDir = Directory.GetDirectories(folderPath).Length; //Todo - hier fliegt UnauthorizedAccessException -> z.B. C:/Dokumente und Einstellungen - catchen
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
				FileNode fileNode = new(this, file.GetNodeInformation(), folder);
				folder.AddChild(fileNode);
				_hashedNodes.Add(fileNode);
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
