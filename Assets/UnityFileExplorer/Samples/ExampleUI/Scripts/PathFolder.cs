using CENTIS.UnityFileExplorer;
using TMPro;
using UnityEngine;

public class PathFolder : UIPathFolder
{
	[SerializeField] private TMP_Text _pathText;

	public override void Initialize(string folderName)
	{
		_pathText.text = folderName;
	}
}
