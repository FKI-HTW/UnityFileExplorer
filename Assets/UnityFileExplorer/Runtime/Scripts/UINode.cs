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

        protected void FireOnSelected() => OnSelected?.Invoke();
        protected void FireOnDeselected() => OnDeselected?.Invoke();
        protected void FireOnActivated() => OnActivated?.Invoke();
    }
}
