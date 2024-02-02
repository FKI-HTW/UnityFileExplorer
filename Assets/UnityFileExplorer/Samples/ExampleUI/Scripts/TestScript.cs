using CENTIS.UnityFileExplorer;
using UnityEngine;

public class TestScript : MonoBehaviour
{
	[SerializeField] private ExplorerManager _manager;

	private void Start()
	{
		_manager.FindFile(onFilePathFound: Debug.Log, null, ".dll"); // for testing
	}
}
