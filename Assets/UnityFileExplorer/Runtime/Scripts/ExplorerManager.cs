using System.Collections.Generic;
using UnityEngine;
using CENTIS.UnityFileExplorer.Datastructure;
using System.IO;
using System;
using UnityEngine.UI;
using TMPro;

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
		private Button _cancelButton;
		private Button _confirmChoiceButton;

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
			// TODO : use returned buttons and ternary operator
			if (_explorerConfiguration.ArrowBackPrefab != null)
            {
				_backButton = Instantiate(_explorerConfiguration.ArrowBackPrefab, _upperUIBar.transform);
			}
            else { CreatePlaceholderButton("back"); }
			_backButton.onClick.AddListener(GoBack);

			if (_explorerConfiguration.ArrowForwardPrefab != null)
			{
				_forwardButton = Instantiate(_explorerConfiguration.ArrowForwardPrefab, _upperUIBar.transform);
			}
			else { CreatePlaceholderButton("forward"); }
			_forwardButton.onClick.AddListener(GoForward);
			
			if (_explorerConfiguration.CancelButtonPrefab != null)
			{
				_cancelButton = Instantiate(_explorerConfiguration.CancelButtonPrefab, _upperUIBar.transform);
			}
			else { CreatePlaceholderButton("cancel"); }
			_cancelButton.onClick.AddListener(CancelFindFile);
			
			if (_explorerConfiguration.ChooseFileButtonPrefab != null)
			{
				_confirmChoiceButton = Instantiate(_explorerConfiguration.ChooseFileButtonPrefab, _upperUIBar.transform);
			}
			else { CreatePlaceholderButton("choose file"); }
			_confirmChoiceButton.onClick.AddListener(() => ActivateNode(_selectedNode));

			if (_explorerConfiguration.ExitButtonPrefab != null)
			{
				_exitButton = Instantiate(_explorerConfiguration.ExitButtonPrefab, _upperUIBar.transform);
			}
			else { CreatePlaceholderButton("exit"); }
			_exitButton.onClick.AddListener(CancelFindFile);
		}

		public void FindFile(
			string fileExtension = "", 
			Action<string> onFilePathFound = null,
			Environment.SpecialFolder? startFolder = null)
        {
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

		#endregion

		#region private methods

		private void NavigateToNode(VirtualFolderNode node)
		{
			// TODO : use a better check like Directory.GetAccessControl to check for Access Permission
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

		private void CreatePlaceholderButton(string givenButtonText)
		{
			// TODO : replace with predefined prefabs at some point
			// TODO : return Button
			GameObject newButton = new GameObject(givenButtonText + "Button");
			_backButton = newButton.AddComponent<Button>();
			newButton.transform.SetParent(_upperUIBar.transform, false);
			Image buttonImage = newButton.AddComponent<Image>();
			buttonImage.color = Color.blue;
			RectTransform buttonRect = newButton.GetComponent<RectTransform>();
			buttonRect.sizeDelta = new Vector2(200f, 50f);

			GameObject text = new GameObject("Text");
			TextMeshProUGUI buttonText = text.AddComponent<TextMeshProUGUI>();
			buttonText.text = givenButtonText;
			buttonText.font = Resources.Load<TMP_FontAsset>("LiberationSans SDF ");
			buttonText.fontSize = 24;
			buttonText.alignment = TextAlignmentOptions.Center;
			text.transform.SetParent(newButton.transform, false);
		}

		#endregion
	}
}
