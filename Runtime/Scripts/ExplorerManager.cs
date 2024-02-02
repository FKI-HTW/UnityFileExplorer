using CENTIS.UnityFileExplorer.Datastructure;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace CENTIS.UnityFileExplorer
{
    public class ExplorerManager : MonoBehaviour
    {
		#region fields

		[SerializeField] private ExplorerConfiguration _config;
		public ExplorerConfiguration ExplorerConfiguration 
		{ 
			get => _config; 
			set => _config = value; 
		}

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

		#region public methods

		public virtual void FindFile(
			Action<string> onFilePathFound = null,
			Environment.SpecialFolder? startFolder = null,
			string fileExtension = null
        ){
			InitFileExplorer();
			if (!string.IsNullOrEmpty(fileExtension))
			{
				_fileExtension = fileExtension;
				_certainFilesOnly = true;
			}

			_fileFoundCallback = onFilePathFound;
			_currentFolder = _root = new VirtualFolderNode(_config, new() { Name = "This PC" }, null);
			_hashedNodes.Add(_root.ToString(), _root);

			// create drive folders beneath virtual root
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach (DriveInfo drive in drives)
			{
				FolderNode diskNode = new(_config, drive.GetNodeInformation(), _root);
				diskNode.OnSelected += SelectNode;
				diskNode.OnDeselected += DeselectNode;
				diskNode.OnActivated += ActivateNode;
				_root.AddChild(diskNode);
				_hashedNodes.Add(diskNode.ToString(), diskNode);
			}

			if (startFolder == null)
			{
				_root.NavigateTo();
				UpdatePath();
				return;
			}

			// used to navigate to the given startFolder and create all nodes, that are visited during the navigation
			string startFolderPath = Environment.GetFolderPath((Environment.SpecialFolder)startFolder);
			DirectoryInfo startDir = new(startFolderPath);
			VirtualFolderNode startParent = FindParentRecursive(startDir);
			FolderNode startNode = new(_config, startDir.GetNodeInformation(), startParent);
			startNode.OnSelected += SelectNode;
			startNode.OnDeselected += DeselectNode;
			startNode.OnActivated += ActivateNode;
			startParent.AddChild(startNode);
			_hashedNodes.Add(startNode.ToString(), startNode);
			NavigateToNode(startNode);
			UpdatePath();
		}

		public virtual void CancelFindFile()
		{
			_fileFoundCallback = null;
			_root?.Unload();
			_selectedNode = _currentFolder = _root = null;
			_hashedNodes.Clear();
			_lastVisitedNodes.Clear();
			_lastReturnedFromNodes.Clear();
			foreach (PathNode node in _pathNodes)
				GameObject.Destroy(node.UIInstance.gameObject);
			_pathNodes.Clear();
			_fileExtension = string.Empty;
			_certainFilesOnly = false;
		}

		#endregion

		#region private methods

		private void InitFileExplorer()
		{
			if (_config.ArrowBackButton != null)
			{
				_config.ArrowBackButton.onClick.RemoveListener(GoBack);
				_config.ArrowBackButton.onClick.AddListener(GoBack);
				_config.ArrowBackButton.interactable = false;
			}

			if (_config.ArrowForwardButton != null)
			{
				_config.ArrowForwardButton.onClick.RemoveListener(GoForward);
				_config.ArrowForwardButton.onClick.AddListener(GoForward);
				_config.ArrowForwardButton.interactable = false;
			}

			if (_config.ExitButton != null)
			{
				_config.ExitButton.onClick.RemoveListener(CancelFindFile);
				_config.ExitButton.onClick.AddListener(CancelFindFile);
			}

			if (_config.CancelButton != null)
			{
				_config.CancelButton.onClick.RemoveListener(CancelFindFile);
				_config.CancelButton.onClick.AddListener(CancelFindFile);
			}

			if (_config.ConfirmButton != null)
			{
				_config.ConfirmButton.onClick.RemoveListener(ChooseSelectedNode);
				_config.ConfirmButton.onClick.AddListener(ChooseSelectedNode);
			}
		}

		private void SelectNode(TreeNode node)
		{
			if (node == null) return;
			if (node.Equals(_selectedNode))
				ActivateNode(_selectedNode);
			else
				_selectedNode = node;
		}

		private void DeselectNode(TreeNode node)
		{
			if (_selectedNode == node)
				_selectedNode = null;
		}

		private void ActivateNode(TreeNode node)
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

		private void ChooseFile(FileNode node)
		{
			_fileFoundCallback?.Invoke(node.ToString());
			CancelFindFile();
		}

		private void ChooseSelectedNode()
		{
			ActivateNode(_selectedNode);
		}

		// TODO : optimize this
		private void UpdatePath()
		{
			if (_config.PathContainer == null) return;

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
				UIPathFolder uiInstance = Instantiate(_config.PathFolderPrefab, _config.PathContainer.transform);
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

			if (_config.ArrowForwardButton != null)
				_config.ArrowForwardButton.interactable = true;
			if (_config.ArrowBackButton != null)
				_config.ArrowBackButton.interactable = _lastVisitedNodes.Count != 0;

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

			if (_config.ArrowBackButton != null)
				_config.ArrowBackButton.interactable = true;
			if (_config.ArrowForwardButton != null)
				_config.ArrowForwardButton.interactable = _lastReturnedFromNodes.Count != 0;

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
						EmptyNode emptyNode = new(_config, node);
						node.AddChild(emptyNode);
					}
				}
			} catch (UnauthorizedAccessException) {
				node.MissingPermissions();
				return;
            }

			_lastVisitedNodes.Add(_currentFolder);
			_lastReturnedFromNodes.Clear();
			if (_config.ArrowBackButton != null)
				_config.ArrowBackButton.interactable = true;
			if (_config.ArrowForwardButton != null)
				_config.ArrowForwardButton.interactable = false;

			_currentFolder.NavigateFrom();
			_currentFolder = node;
			_currentFolder.NavigateTo();

			UpdatePath();
		}

		private void AddDirectories(VirtualFolderNode folder)
		{
			string folderPath = folder.ToString();
			IEnumerable<DirectoryInfo> containedDir = new DirectoryInfo(folderPath).GetDirectories();
			foreach (DirectoryInfo dir in containedDir)
			{
				FolderNode folderNode = new(_config, dir.GetNodeInformation(), folder);
				folderNode.OnSelected += SelectNode;
				folderNode.OnDeselected += DeselectNode;
				folderNode.OnActivated += ActivateNode;
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

				FileNode fileNode = new(_config, file.GetNodeInformation(), folder);
				fileNode.OnSelected += SelectNode;
				fileNode.OnDeselected += DeselectNode;
				fileNode.OnActivated += ActivateNode;
				folder.AddChild(fileNode);
				_hashedNodes.Add(fileNode.ToString(), fileNode);
			}
		}

		private FolderNode FindParentRecursive(DirectoryInfo userDir)
		{
			if (_hashedNodes.TryGetValue(userDir.Parent.ToString(), out TreeNode parent))
				return (FolderNode)parent;

			FolderNode nextParent = FindParentRecursive(userDir.Parent);
			FolderNode newNode = new(_config, userDir.Parent.GetNodeInformation(), nextParent);
			newNode.OnSelected += SelectNode;
			newNode.OnDeselected += DeselectNode;
			newNode.OnActivated += ActivateNode;
			nextParent.AddChild(newNode);
			_hashedNodes.Add(newNode.ToString(), newNode);
			return newNode;
		}

		#endregion
	}
}
