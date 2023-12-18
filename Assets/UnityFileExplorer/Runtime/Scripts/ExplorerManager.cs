using System.Collections.Generic;
using CENTIS.UnityFileExplorer.Datastructure;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using TMPro;
using System.Linq;

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
		private Button[] _folderButtons;
		private TextMeshProUGUI[] _folderButtonTexts;
		private GameObject[] _separators;
		private string _startFolderName = "This PC";

		#endregion

		private void Start()
		{
			LoadCustomPrefabs();
			FindFile(onFilePathFound: Debug.Log, null, ".dll"); // for testing - only showing .dll files at the moment
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
			InstatiatePathPrefabs();

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
			UpdateFolderPath(_currentFolder.ToString()); // nullPointer due to treenode toString() line 20 -> not set to an instance of an object.. 
			_noFilesInfoPrefab.SetActive(_currentFolder.Children.Count == 0 
				? true 
				: false); 
			_forwardButton.interactable = true;
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
			UpdateFolderPath(_currentFolder.ToString());
			_noFilesInfoPrefab.SetActive(_currentFolder.Children.Count == 0 
				? true 
				: false);
			_backButton.interactable = true;
			if(_lastReturnedFromNodes.Count == 0)
            {
				_forwardButton.interactable = false;
			}
		}

		#endregion

		#region private methods

		private void InstatiatePathPrefabs()
        {
			_separators = new GameObject[6];
			_folderButtons = new Button[5];
			_folderButtonTexts = new TextMeshProUGUI[5];

			_separators[0] = Instantiate(ExplorerConfiguration.SeperatorPrefab, _pathContainerPrefab.transform);
			// 1 ***
			_folderButtons[0] = Instantiate(ExplorerConfiguration.FolderButtonPrefab, _pathContainerPrefab.transform);
			_folderButtonTexts[0] = _folderButtons[0].GetComponentInChildren<TextMeshProUGUI>();
			_folderButtonTexts[0].SetText(_startFolderName);
			_folderButtons[0].onClick.AddListener(() => OnFolderButtonClick(_folderButtonTexts[0].text)); // check if this works

			_separators[1] = Instantiate(ExplorerConfiguration.SeperatorPrefab, _pathContainerPrefab.transform);
			// 2 ***
			_folderButtons[1] = Instantiate(ExplorerConfiguration.FolderButtonPrefab, _pathContainerPrefab.transform);
			_folderButtonTexts[1] = _folderButtons[1].GetComponentInChildren<TextMeshProUGUI>();
			_folderButtons[1].onClick.AddListener(() => OnFolderButtonClick(_folderButtonTexts[1].text));

			_separators[2] = Instantiate(ExplorerConfiguration.SeperatorPrefab, _pathContainerPrefab.transform);
			// 3 ***
			_folderButtons[2] = Instantiate(ExplorerConfiguration.FolderButtonPrefab, _pathContainerPrefab.transform);
			_folderButtonTexts[2] = _folderButtons[2].GetComponentInChildren<TextMeshProUGUI>();
			_folderButtons[2].onClick.AddListener(() => OnFolderButtonClick(_folderButtonTexts[2].text));

			_separators[3] = Instantiate(ExplorerConfiguration.SeperatorPrefab, _pathContainerPrefab.transform);
			// 4 ***
			_folderButtons[3] = Instantiate(ExplorerConfiguration.FolderButtonPrefab, _pathContainerPrefab.transform);
			_folderButtonTexts[3] = _folderButtons[3].GetComponentInChildren<TextMeshProUGUI>();
			_folderButtons[3].onClick.AddListener(() => OnFolderButtonClick(_folderButtonTexts[3].text));

			_separators[4] = Instantiate(ExplorerConfiguration.SeperatorPrefab, _pathContainerPrefab.transform);
			// 5 ***
			_folderButtons[4] = Instantiate(ExplorerConfiguration.FolderButtonPrefab, _pathContainerPrefab.transform);
			_folderButtonTexts[4] = _folderButtons[4].GetComponentInChildren<TextMeshProUGUI>();
			_folderButtons[4].onClick.AddListener(() => OnFolderButtonClick(_folderButtonTexts[4].text)); 

			_separators[5] = Instantiate(ExplorerConfiguration.SeperatorPrefab, _pathContainerPrefab.transform);

			for (int i = 1; i < _folderButtons.Length; i++)
			{
				_folderButtons[i].gameObject.SetActive(false);
				_separators[i + 1].SetActive(false);
			}
		}

		private void UpdateFolderPath(string currentFolderPath)
		{
			foreach (TextMeshProUGUI texxt in _folderButtonTexts) { texxt.text = ""; } //clear button texts

			currentFolderPath = _startFolderName + "/" + currentFolderPath;
			Debug.Log(currentFolderPath); //todo remove when all works properly
			string[] folders = currentFolderPath.Split(new[] { Path.DirectorySeparatorChar, '/' }, StringSplitOptions.RemoveEmptyEntries);
			if (folders.Length <= _folderButtons.Length)
			{
				for (int i = 0; i < folders.Length; i++)
				{
					_folderButtonTexts[i].SetText(ShortenFilenames(folders[i]));
					_folderButtons[i].gameObject.SetActive(true);
					_separators[i + 1].SetActive(true);
				}

				//dont show buttons with bigger index than folders.length
				for (int i = folders.Length; i < _folderButtons.Length; i++)
				{
					_folderButtons[i].gameObject.SetActive(false);
					_separators[i + 1].SetActive(false);
				}
			}
            //case path has more folderNames than folder buttons -> just name buttons backwards with folder names 'deepest' in file structure
            else
            {
				int j = 1;
				for (int i = 4; i >= 0; i--)
				{
					_folderButtonTexts[i].SetText(ShortenFilenames(folders[folders.Length - j]));
					_folderButtons[i].gameObject.SetActive(true);
					_separators[i + 1].SetActive(true);
					j++;
				}
			}
		}

		private string ShortenFilenames(string inputFolderName)
        {
			int maxLength = 14;
			if (inputFolderName.Length > maxLength)
			{
				return inputFolderName.Substring(0, maxLength - 3) + "...";
			} else { return inputFolderName; }
		}

		private void OnFolderButtonClick(string folderName) //TODO
		{
			Debug.Log($"Navigate to: {folderName}");

			//Frage ist, wie definieren wir den "Sprung" zu einem Ordner ? als select und zu ordner springen, also als gesprungenes forward quasi? 
			// oder als back in den unter back gespeicherten ordnern im verlauf gehen? (der user kann im verlauf ja aber 3 mal an dem ordner angekommen sein,
			//	der nun direkt angeklickt wurde, welchen nimmt man dann? 
			// -> conclusion: jump zum ordner:

			FolderNode destinationNode = null;
			//VirtualFolderNode foundNode = null; **ohne linq
		   VirtualFolderNode foundNode = _lastVisitedNodes.FirstOrDefault(node => node.Info.Name == folderName); //NullPointer - not set to an instance of an obeject

			/*ohne linq:
			 * 
			 * 
			
			foreach (VirtualFolderNode node in _lastVisitedNodes)
			{
				if (node.Info.Name == folderName)
				{
					foundNode = node;
					break;
				}
			}*/

			if (foundNode != null)
			{
				destinationNode = FindParentRecursive(new DirectoryInfo(foundNode.ToString()));
			}
			if(destinationNode != null)
            {
				NavigateToNode(destinationNode);
				UpdateFolderPath(destinationNode.ToString());
				return;
			}
			Debug.Log("Did not find clicked Node.");

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
			_lastReturnedFromNodes.Clear(); //TO DO - understand why list is cleared here ***** FRAGE AN JULIAN *****
			_currentFolder = node;
			if (_currentFolder.Children.Count == 0)
			{
				_noFilesInfoPrefab.SetActive(true);
			}
			UpdateFolderPath(node.ToString());
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
