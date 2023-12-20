using UnityEngine;
using CENTIS.UnityFileExplorer;
using TMPro;

public class Node : UINode
{
	[SerializeField] private TMP_Text _name;

	public override void Initialize(NodeInformation info)
	{
		_name.text = info.Name;
	}

	public override void MissingPermissions()
	{
		//Todo - check if unauthorized text has been added already before adding it again
		_name.text += " --- Unauthorized Access!";
	}
}
