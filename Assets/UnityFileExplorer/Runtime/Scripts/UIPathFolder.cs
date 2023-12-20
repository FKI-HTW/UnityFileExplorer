using System;
using UnityEngine;

namespace CENTIS.UnityFileExplorer
{
    public abstract class UIPathFolder : MonoBehaviour
    {
		public event Action OnActivated;

		public abstract void Initialize(string folderName);

		public void Activate() => OnActivated?.Invoke();
	}
}
