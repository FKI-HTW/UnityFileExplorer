using System;
using UnityEngine;

namespace CENTIS.UnityFileExplorer
{
    public abstract class UINode : MonoBehaviour
    {
		public event Action OnSelected;
        public event Action OnDeselected;
        public event Action OnActivated;

        public abstract void Initialize(NodeInformation nodeInformation);
        public abstract void OnFailedToLoad(ENodeFailedToLoad reason);

        public void Select() => OnSelected?.Invoke();
        public void Deselect() => OnDeselected?.Invoke();
        public void Activate() => OnActivated?.Invoke();
    }
}
