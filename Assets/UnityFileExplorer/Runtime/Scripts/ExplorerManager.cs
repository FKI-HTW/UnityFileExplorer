using System.Collections.Generic;
using UnityEngine;
using CENTIS.UnityFileExplorer.Datastructure;

namespace CENTIS.UnityFileExplorer
{
    public class ExplorerManager : MonoBehaviour
    {
		#region fields

		public ExplorerConfiguration ExplorerConfiguration { get => _explorerConfiguration; set => _explorerConfiguration = value; }
        [SerializeField] private ExplorerConfiguration _explorerConfiguration;

		public GameObject FileContainer { get => _fileContainer; }
		[SerializeField] private GameObject _fileContainer;

		// TODO : add side column reference

		// TODO : add explorer path reference

		private TreeNode _root; // a virtual folder above C: / E: etc.
		private TreeNode _selectedNode;
		private FolderNode _currentFolder;

		private readonly List<FolderNode> _lastVisitedNodes = new(); // for back arrow
		private readonly List<FolderNode> _lastReturnedFromNodes = new(); // for forward arrow

		#endregion

		#region public methods

		public void OpenFileExplorer(string fileExtension = "") // TODO : add action callback for when file was chosen
        {
			// TODO :
			// 1. instantiate the main explorer windows and save references in configuration
			//    OR require the user to define the UI and references beforehand
			//		We still need to discuss how atomic the individual parts of the window should i.e. 
			//		if buttons, back arrow etc. are all seperate prefabs or are part of the container class prefab
			// 2. Load Information from windows' current user
			// 3. Load Root > Main > Users > profile
			// 4. Whenever a folder is loaded whose contents are not yet loaded (parent or children null, or maybe add an additional flag),
			//		load them first and update the structure accordingly

			// navigating to a path can be done by either calling NavigateTo of the parent/child, or if the target
			// is not a direct neighbor, recursevly find the node using the hashcodes
		}

		public void CloseFileExplorer()
		{

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
					_currentFolder.NavigateFrom();
					folderNode.NavigateTo();
					_lastVisitedNodes.Add(_currentFolder);
					_lastReturnedFromNodes.Clear();
					_currentFolder = folderNode;
					break;
				case FileNode fileNode:
					ChooseFile(fileNode);
					break;
			}
		}

		public void ChooseFile()
		{
			if (_selectedNode == null) return;
			ChooseFile(_selectedNode);
		}

		public void GoBack()
		{
			if (_lastVisitedNodes.Count == 0) return;

			FolderNode targetNode = _lastVisitedNodes[^1];
			_lastVisitedNodes.RemoveAt(_lastVisitedNodes.Count - 1);
			_currentFolder.NavigateFrom();
			targetNode.NavigateTo();
			_lastReturnedFromNodes.Add(_currentFolder);
			_currentFolder = targetNode;
		}

		public void GoForward()
		{
			if (_lastReturnedFromNodes.Count == 0) return;

			FolderNode targetNode = _lastReturnedFromNodes[^1];
			_lastReturnedFromNodes.RemoveAt(_lastReturnedFromNodes.Count - 1);
			_currentFolder.NavigateFrom();
			targetNode.NavigateTo();
			_lastVisitedNodes.Add(_currentFolder);
			_currentFolder = targetNode;
		}

		#endregion

		#region private methods

		private void ChooseFile(TreeNode node)
		{

		}

		#endregion
	}
}
