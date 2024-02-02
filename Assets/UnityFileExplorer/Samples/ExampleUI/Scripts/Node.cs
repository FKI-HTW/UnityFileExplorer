using UnityEngine;
using CENTIS.UnityFileExplorer;
using TMPro;

public class Node : UINode
{
	[SerializeField] private TMP_Text _name;

	public override void Initialize(NodeInformation info)
	{
		_name.text = gameObject.name = info.Name;
	}

	public override void MissingPermissions()
	{
		Debug.LogError("Unauthorized Access!");
	}
}
