using System.Collections;
using UnityEngine;

namespace Match3Simple.Managers
{
    public abstract class ManagerBase<T> : MonoBehaviour where T : ManagerBase<T>
    {
        protected static T Instance { get; private set; }

        protected virtual void OnEnable()
        {
            if (Instance != null) return;

            Instance = (T)this;
        }

        protected virtual void OnDisable()
        {
            if (Instance != this) return;

            Instance = null;
        }
    }
}