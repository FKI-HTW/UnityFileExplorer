using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CENTIS.UnityFileExplorer.Datastructure;

namespace CENTIS.UnityFileExplorer
{
    public class ExplorerManager : MonoBehaviour
    {
        public ExplorerConfiguration ExplorerConfiguration { get => _explorerConfiguration; set => _explorerConfiguration = value; }
        [SerializeField] private ExplorerConfiguration _explorerConfiguration;

		public GameObject FileContainer { get => _fileContainer; }
		[SerializeField] private GameObject _fileContainer;

		// TODO : add side column reference

		// TODO : add explorer path reference

		private TreeNode _root; // a virtual folder above C: / E: etc.
		private TreeNode _selectedNode;
		private FolderNode _currentFolder;

        public void OpenFileExplorer(string fileExtension = "")
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

		public void Continue()
		{
			// TODO : called upon pressing continue button
			//		if the selected node is a file it's chosen
			//		if the selected node is a folder it's navigated to
		}

		public void ChooseFile()
		{
			// TODO : this will be called from the file node on double clicks or by a "continue" button
			// for the currently selected treenode if said node is a file
		}
	}
}
