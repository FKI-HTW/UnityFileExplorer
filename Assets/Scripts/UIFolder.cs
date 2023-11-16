using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CENTIS.UnityFileExplorer;
using TMPro;

public class UIFolder : UINode
{
	[SerializeField] private TMP_Text _name;

	public override void Initiate(NodeInformation info)
	{
		_name.text = info.Name;
	}
}
