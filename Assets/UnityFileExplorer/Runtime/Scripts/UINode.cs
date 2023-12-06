using System;
using UnityEngine;

namespace CENTIS.UnityFileExplorer
{
    public abstract class UINode : MonoBehaviour
    {
		public event Action OnSelected;
        public event Action OnDeselected;
        public event Action OnActivated;

        public abstract void Initiate(NodeInformation nodeInformation);
        public abstract void MissingPermissions();

        public void FireOnSelected() => OnSelected?.Invoke();
        public void FireOnDeselected() => OnDeselected?.Invoke();
        public void FireOnActivated() => OnActivated?.Invoke();
    }
}
